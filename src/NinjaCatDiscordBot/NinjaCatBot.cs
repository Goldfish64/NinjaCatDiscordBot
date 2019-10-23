/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatBot.cs
* 
* Copyright (c) 2016-2019 John Davis
*
* Permission is hereby granted, free of charge, to any person obtaining a
* copy of this software and associated documentation files (the "Software"),
* to deal in the Software without restriction, including without limitation
* the rights to use, copy, modify, merge, publish, distribute, sublicense,
* and/or sell copies of the Software, and to permit persons to whom the
* Software is furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
* IN THE SOFTWARE.
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NinjaCatDiscordBot {
    /// <summary>
    /// Represents the Ninja Cat bot.
    /// </summary>
    public partial class NinjaCatBot {
        #region Private variables

        private NinjaCatDiscordClient client;
        private Timer timerBuild;

        #endregion

        #region Entry method

        /// <summary>
        /// Main method.
        /// </summary>
        public static void Main(string[] args) => new NinjaCatBot().Start().GetAwaiter().GetResult();

        #endregion

        #region Methods

        /// <summary>
        /// Starts the bot.
        /// </summary>
        private async Task Start() {
            // Create Discord client.
            client = new NinjaCatDiscordClient();

            // Create command service and map.
            var commands = new CommandService();

            // Load commands from assembly.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

#if !PRIVATE
            // Certain things are to be done when the bot joins a guild.
            client.JoinedGuild += async (guild) => {
                // Pause for 5 seconds.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Check to see if this is a bot farm.
                if (await CheckBotGuild(guild))
                    return;

                // Update server count.
                // await UpdateSiteServerCountAsync();

                // Dev began Oct 2. 2016.
            };

            // Update count on guild leave.
            // client.LeftGuild += async (guild) => await UpdateSiteServerCountAsync();
#endif

            // Listen for messages.
            client.MessageReceived += async (message) => {
                // Get the message and check to see if it is a user message.
                var msg = message as IUserMessage;
                if (msg == null)
                    return;

                // Keeps track of where the command begins.
                var pos = 0;

                // Attempt to parse a command.
                if (msg.HasStringPrefixLower(Constants.CommandPrefix, ref pos)) {
                    var result = await commands.ExecuteAsync(new CommandContext(client, msg), msg.Content.Substring(pos), null);
                    if (!result.IsSuccess) {
                        // Is the command just unknown? If so, return.
                        if (result.Error == CommandError.UnknownCommand)
                            return;

                        // Bot is typing.
                        await msg.Channel.TriggerTypingAsync();

                        // Pause for realism and send message.
                        await Task.Delay(TimeSpan.FromSeconds(0.75));

                        await msg.Channel.SendMessageAsync($"I'm sorry, but something happened. Error: {result.ErrorReason}\n\nIf there are spaces in a parameter, make sure to surround it with quotes.");
                    }
                    return;
                }
            };

            // Log in to Discord. Token is stored in the Credentials class.
            await client.LoginAsync(TokenType.Bot, Credentials.DiscordToken);
            await client.StartAsync();

#if !PRIVATE
            /*// Check for bot guilds.
            foreach (var shard in client.Shards) {
#pragma warning disable 4014
                shard.Connected += async () => {
                    foreach (var guild in shard.Guilds)
                        CheckBotGuild(guild);
                    await Task.CompletedTask;
                };
#pragma warning restore 4014
            }*/
#endif

            // Create HTTP client.
            var httpClient = new HttpClient();

            // Start checking for new builds.
            timerBuild = new Timer(async (s) => {
                client.LogInfo($"Checking for build...");

                // Attempt to get the latest post and skip if we cannot.
                BlogEntry post = null;
                try {
                    // Get latest post.
                    var doc = XDocument.Parse(await httpClient.GetStringAsync($"https://blogs.windows.com/windowsexperience/tag/windows-insider-program/feed"));
                    var entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                                  select new BlogEntry() {
                                      Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                                      Title = item.Elements().First(i => i.Name.LocalName == "title").Value,
                                      Desc = item.Elements().First(i => i.Name.LocalName == "description").Value
                                  };
                    post = entries.ToList().Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build")).FirstOrDefault();
                }
                catch (HttpRequestException ex) {
                    client.LogError($"Exception when getting post: {ex}");
                    return;
                }
                if (post == null) {
                    client.LogError($"Unable to get new post");
                    return;
                }

                // Have we ever seen a post yet? This prevents false announcements if the bot has never seen a post before.
                if (string.IsNullOrWhiteSpace(client.CurrentUrl)) {
                    client.CurrentUrl = post.Link;
                    client.SaveSettings();
                    client.LogInfo($"Saved post as new latest build: {post.Link}");
                    return;
                }

                // Is the latest post the same? If so, no need to announce it.
                if (client.CurrentUrl == post.Link)
                    return;

                // Get build numbers. If empty, ignore the post.
                var build = Regex.Match(post.Link, @"\d{5,}").Value;
                if (string.IsNullOrWhiteSpace(build)) {
                    client.LogError($"Post build number is blank");
                    return;
                }

                // Stop timer.
                timerBuild.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

                // Log post.
                client.LogInfo($"New build received");

                // Save post.
                client.CurrentUrl = post.Link;
                client.SaveSettings();

                // Get first sentence of post. We'll parse the ring out of this.
                var description = post.Desc.ToLowerInvariant().Split('.').FirstOrDefault();
                if (description == null) {
                    client.LogError($"Post description is blank");
                    return;
                }

                // Determine ring.
                var ring = string.Empty;
                var platform = string.Empty;
                if (description.Contains("skip ahead")) {
                    // Skip ahead takes priority over other rings.
                    ring = " to the Skip Ahead ring";
                }
                else {
                    if (description.Contains("fast") && description.Contains("slow"))
                        ring = " to both the Fast and Slow rings";
                    else if (description.Contains("fast"))
                        ring = " to the Fast ring";
                    else if (description.Contains("slow"))
                        ring = " to the Slow ring";
                }

                // Determine build platform.
                if (post.Link.ToLowerInvariant().Contains("pc") && post.Link.ToLowerInvariant().Contains("server"))
                    platform = " for both PC and Server";
                else if (post.Link.ToLowerInvariant().Contains("pc"))
                    platform = " for PC";
                else if (post.Link.ToLowerInvariant().Contains("server"))
                    platform = " for Server";

                // Send build to guilds.
                foreach (var shard in client.Shards)
                    SendNewBuildToShard(shard, build, ring + platform, post.Link);

                // Update game.
                await client.UpdateGameAsync();

                // Restart timer.
                timerBuild.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            // Wait a minute for bot to start up.
            await Task.Delay(TimeSpan.FromMinutes(1));

            // Create thread for updating game.
            var serverCountThread = new Thread(new ThreadStart(async () => {
                while (true) {
                    // Update game.
                    await client.UpdateGameAsync();

                    // Wait an hour.
                    await Task.Delay(TimeSpan.FromHours(1));
                }
            }));
            serverCountThread.Start();

            // Wait forever.
            await Task.Delay(-1);
        }

        private async Task SendBuildToGuild(DiscordSocketClient shard, SocketGuild guild, string build, string type, string url) {
            // Get channel.
            var channel = client.GetSpeakingChannelForSocketGuild(guild);

            // If the channel is null, continue on to the next guild.
            if (channel == null) {
                client.LogInfo($"Rolling over {guild.Name} (disabled) ({shard.ShardId}/{client.Shards.Count - 1})");
                return;
            }

            // Verify we have permission to speak.
            if (guild.CurrentUser?.GetPermissions(channel).SendMessages != true) {
                client.LogInfo($"Rolling over {guild.Name} (no perms) ({shard.ShardId}/{client.Shards.Count - 1})");
                return;
            }

            // Get ping role.
            var role = client.GetSpeakingRoleForIGuild(guild);
            var roleSkip = client.GetSpeakingRoleSkipForIGuild(guild);
            if (type.ToLowerInvariant().Contains("skip ahead") && roleSkip != null)
                role = roleSkip;
            var roleText = string.Empty;

            // Does the role exist, and should we ping?
            if (!type.ToLowerInvariant().Contains("server") && role != null)
                roleText = $"{role.Mention} ";

            try {
                // Check if the role is mentionable.
                // If not, attempt to make it mentionable, and revert the setting after the message is sent.
                var mentionable = role?.IsMentionable;
                if (mentionable == false && guild.CurrentUser.GuildPermissions.ManageRoles && guild.CurrentUser.Hierarchy > role.Position)
                    await role.ModifyAsync((e) => e.Mentionable = true);

                // Wait a second.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Send typing message.
                await channel.TriggerTypingAsync();

                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Select and send message.
                switch (client.GetRandomNumber(3)) {
                    default:
                        await channel.SendMessageAsync($"{roleText}Windows 10 Insider Preview Build {build} has just been released{type}! :mailbox_with_mail: :smiley_cat:\n{url}");
                        break;

                    case 1:
                        await channel.SendMessageAsync($"{roleText}Windows 10 Insider Preview Build {build} has just been released{type}! Yes! :mailbox_with_mail: :smiley_cat:\n{url}");
                        break;

                    case 2:
                        await channel.SendMessageAsync($"{roleText}Better check for updates now! Windows 10 Insider Preview Build {build} has just been released{type}! :mailbox_with_mail: :smiley_cat:\n{url}");
                        break;
                }

                // Revert mentionable setting.
                if (mentionable == false && guild.CurrentUser.GuildPermissions.ManageRoles && guild.CurrentUser.Hierarchy > role.Position)
                    await role.ModifyAsync((e) => e.Mentionable = false);
            }
            catch (Exception ex) {
                client.LogError($"Failed to speak in {guild.Name} ({shard.ShardId}/{client.Shards.Count - 1}): {ex}");
            }

            // Log server.
            client.LogInfo($"Spoke in {guild.Name} ({shard.ShardId}/{client.Shards.Count - 1})");
        }

        private async void SendNewBuildToShard(DiscordSocketClient shard, string build, string type, string url) {
            // If the MS server is in this shard, announce there first.
            var msGuild = shard.Guilds.SingleOrDefault(g => g.Id == Constants.MsGuildId);
            if (msGuild != null)
                await SendBuildToGuild(shard, msGuild, build, type, url);

            // Announce in the specified channel of each guild.
            foreach (var guild in shard.Guilds) {
                // Skip MS guild.
                if (guild.Id == Constants.MsGuildId)
                    continue;

                // Send to guild.
                await SendBuildToGuild(shard, guild, build, type, url);
            }
        }

        private async Task<bool> CheckBotGuild(SocketGuild guild) {
            // If the server is the Discord bots server, ignore.
            if (guild.Id == Constants.BotsGuildId)
                return false;

            // Ensure guild is updated.
            if (guild.Users.Count != guild.MemberCount)
                await guild.DownloadUsersAsync();

            // Is this a bot guild?
            if (guild.MemberCount >= 50 && (guild.Users.Count(u => u.IsBot) / (double)guild.MemberCount) >= 0.9) {
                client.LogInfo($"Leaving bot server {guild.Name}");
                try {
                    // Bot is typing in default channel.
                    await guild.DefaultChannel.TriggerTypingAsync();

                    // Pause for realism.
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    // Send notice.
                    await guild.DefaultChannel.SendMessageAsync($"It looks like this server is a bot farm, so I'll show myself out. If this is a legitimate server, contact *{Constants.OwnerName}*.");
                }
                catch { }

                // Wait 2 seconds, then leave.
                await Task.Delay(TimeSpan.FromSeconds(2));
                await guild.LeaveAsync();

                // This was a bot server.
                return true;
            }

            // This is not a bot server.
            return false;
        }

        #endregion
    }
}
