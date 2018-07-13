/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatBot.cs
* 
* Copyright (c) 2016-2018 John Davis
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

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Represents the Ninja Cat bot.
    /// </summary>
    public partial class NinjaCatBot
    {
        #region Private variables


        private NinjaCatDiscordClient client;

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
        private async Task Start()
        {
            // Create Discord client.
            client = new NinjaCatDiscordClient();

            // Create command service and map.
            var commands = new CommandService();
            var commandMap = new ServiceCollection();

            // Load commands from assembly.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

#if !PRIVATE
            // Certain things are to be done when the bot joins a guild.
            client.JoinedGuild += async (guild) =>
            {
                // Pause for 5 seconds.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Check to see if this is a bot farm.
                if (await CheckBotGuild(guild))
                    return;

                // Update server count.
                await UpdateSiteServerCountAsync();

                // Dev began Oct 2. 2016.
            };

            // Update count on guild leave.
            client.LeftGuild += async (guild) => await UpdateSiteServerCountAsync();
#endif

            // Listen for messages.
            client.MessageReceived += async (message) =>
            {
                // Get the message and check to see if it is a user message.
                var msg = message as IUserMessage;
                if (msg == null)
                    return;

                // Keeps track of where the command begins.
                var pos = 0;

                // Attempt to parse a command.
                if (msg.HasStringPrefixLower(Constants.CommandPrefix, ref pos))
                {
                    var result = await commands.ExecuteAsync(new CommandContext(client, msg), msg.Content.Substring(pos));
                    if (!result.IsSuccess)
                    {
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
            // Check for bot guilds.
            foreach (var shard in client.Shards)
            {
#pragma warning disable 4014
                shard.Connected += async () =>
                {
                    foreach (var guild in shard.Guilds)
                        CheckBotGuild(guild);
                    await Task.CompletedTask;
                };
#pragma warning restore 4014
            }
#endif

            // Create HTTP client.
            var httpClient = new HttpClient();

            // Start checking for new builds.
            var buildThread = new Thread(new ThreadStart(async () =>
            {
                while (true)
                {
                    // Wait 5 minutes.
                    await Task.Delay(TimeSpan.FromMinutes(5));

                    client.LogOutput($"In blog post loop");
                    BlogEntry post = null;
                    try
                    {
                        // Get latest post.
                        var doc = XDocument.Parse(await httpClient.GetStringAsync($"https://blogs.windows.com/windowsexperience/tag/windows-insider-program/feed"));
                        var entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                                      select new BlogEntry()
                                      {
                                          Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                                          Title = item.Elements().First(i => i.Name.LocalName == "title").Value,
                                          Desc = item.Elements().First(i => i.Name.LocalName == "description").Value
                                      };
                        post = entries.ToList().Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build")).FirstOrDefault();
                    }
                    catch (HttpRequestException ex)
                    {
                        client.LogOutput($"ERROR GETTING NEW POST: {ex}");
                        continue;
                    }

                    // Check if post is a valid insider post.
                    if (post != null)
                    {
                        // Have we ever seen a post yet? This prevents false announcements if the bot has never seen a post before.
                        if (string.IsNullOrWhiteSpace(client.CurrentUrl))
                        {
                            client.CurrentUrl = post.Link;
                            client.SaveSettings();
                            client.LogOutput($"SAVED POST AS LATEST: {post.Link}");
                            continue;
                        }

                        // Is the latest post the same? If so, no need to announce it.
                        if (client.CurrentUrl == post.Link)
                            continue;

                        // Get build numbers. If empty, ignore the post.
                        var build = Regex.Match(post.Link, @"\d{5,}").Value;
                        var buildM = Regex.Match(post.Link, @"\d{5,}", RegexOptions.RightToLeft).Value;
                        if (string.IsNullOrWhiteSpace(build))
                            continue;

                        // Log post.
                        client.LogOutput($"POST CONFIRMED: NEW BUILD");

                        // Save post.
                        client.CurrentUrl = post.Link;
                        client.SaveSettings();

                        // Create variables.
                        var ring = string.Empty;
                        var platform = string.Empty;

                        // Check for fast or slow, or both.
                        if (post.Link.ToLowerInvariant().Contains("skip-ahead"))
                        {
                            ring = " to the Skip Ahead ring";
                        }
                        else
                        {
                            if (post.Desc.ToLowerInvariant().Contains("fast") && post.Desc.ToLowerInvariant().Contains("slow"))
                                ring = " to both the Fast and Slow rings";
                            else if (post.Desc.ToLowerInvariant().Contains("fast"))
                                ring = " to the Fast ring";
                            else if (post.Desc.ToLowerInvariant().Contains("slow"))
                                ring = " to the Slow ring";
                        }

                        // Check for PC or mobile, or both.
                        if (post.Link.ToLowerInvariant().Contains("pc") && post.Link.ToLowerInvariant().Contains("mobile") && post.Link.ToLowerInvariant().Contains("server"))
                            platform = " for PC, Server, and Mobile";
                        else if (post.Link.ToLowerInvariant().Contains("pc") && post.Link.ToLowerInvariant().Contains("mobile"))
                            platform = " for both PC and Mobile";
                        else if (post.Link.ToLowerInvariant().Contains("pc") && post.Link.ToLowerInvariant().Contains("server"))
                            platform = " for both PC and Server";
                        else if (post.Link.ToLowerInvariant().Contains("mobile") && post.Link.ToLowerInvariant().Contains("server"))
                            platform = " for both Server and Mobile";
                        else if (post.Link.ToLowerInvariant().Contains("pc"))
                            platform = " for PC";
                        else if (post.Link.ToLowerInvariant().Contains("mobile"))
                            platform = " for Mobile";
                        else if (post.Link.ToLowerInvariant().Contains("server"))
                            platform = " for Server";

                        // Send build to guilds.
                        foreach (var shard in client.Shards)
                            SendNewBuildToShard(shard, build, buildM, ring + platform, post.Link);

                        // Update game.
                        await client.UpdateGameAsync();
                    }
                }
            }));
            buildThread.Start();

            // Wait a minute for bot to start up.
            await Task.Delay(TimeSpan.FromMinutes(1));

            // Create thread for POSTing server count and updating game.
            var serverCountThread = new Thread(new ThreadStart(async () =>
            {
                while (true)
                {
                    // Update count and game.
#if !PRIVATE
                    await UpdateSiteServerCountAsync();
#endif
                    await client.UpdateGameAsync();

                    // Wait an hour.
                    await Task.Delay(TimeSpan.FromHours(1));
                }
            }));
            serverCountThread.Start();

            // Wait forever.
            await Task.Delay(-1);
        }

#if !PRIVATE
        /// <summary>
        /// Updates the site server count.
        /// </summary>
        private async Task UpdateSiteServerCountAsync()
        {
            try
            {
                // Get current user.
                var user = client.Shards?.FirstOrDefault()?.CurrentUser;
                if (user == null)
                    return;

                // Create request.
                var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://bots.discord.pw/api/bots/{user.Id}/stats");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["Authorization"] = Credentials.BotApiToken;

                using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
                {
                    streamWriter.Write($"{{\"server_count\":{client.Guilds.Count}}}");
                    streamWriter.Flush();
                }

                await httpWebRequest.GetResponseAsync();
            }
            catch (Exception ex)
            {
                // Log error.
                client.LogOutput($"FAILED UPDATING SERVER COUNT: {ex}");
            }
        }
#endif

        private async void SendNewBuildToShard(DiscordSocketClient shard, string build, string buildM, string type, string url)
        {
            // Announce in the specified channel of each guild.
            foreach (var guild in shard.Guilds)
            {
                // Get channel.
                var channel = client.GetSpeakingChannelForSocketGuild(guild);

                // If the channel is null, continue on to the next guild.
                if (channel == null)
                {
                    client.LogOutput($"ROLLING OVER SERVER (NO SPEAKING) ({shard.ShardId}/{client.Shards.Count - 1}): {guild.Name}");
                    continue;
                }

                // Verify we have permission to speak.
                if (guild.CurrentUser?.GetPermissions(channel).SendMessages != true)
                {
                    client.LogOutput($"ROLLING OVER SERVER (NO PERMS) ({shard.ShardId}/{client.Shards.Count - 1}): {guild.Name}");
                    continue;
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

                try
                {
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
                    if (build != buildM)
                    {
                        switch (client.GetRandomNumber(3))
                        {
                            default:
                                await channel.SendMessageAsync($"{roleText}Yay! Windows 10 Insider Preview Build {build} for PC and Build {buildM} for Mobile has just been released{type}! :mailbox_with_mail: :smiley_cat:\n{url}");
                                break;

                            case 1:
                                await channel.SendMessageAsync($"{roleText}Windows 10 Insider Preview Build {build} for PC and Build {buildM} for Mobile has just been released{type}! Yes! :mailbox_with_mail: :smiley_cat:\n{url}");
                                break;

                            case 2:
                                await channel.SendMessageAsync($"{roleText}Better check for updates now! Windows 10 Insider Preview Build {build} for PC and Build {buildM} for Mobile has just been released{type}! :mailbox_with_mail: :smiley_cat:\n{url}");
                                break;
                        }
                    }
                    else
                    {
                        switch (client.GetRandomNumber(3))
                        {
                            default:
                                await channel.SendMessageAsync($"{roleText}Yay! Windows 10 Insider Preview Build {build} has just been released{type}! :mailbox_with_mail: :smiley_cat:\n{url}");
                                break;

                            case 1:
                                await channel.SendMessageAsync($"{roleText}Windows 10 Insider Preview Build {build} has just been released{type}! Yes! :mailbox_with_mail: :smiley_cat:\n{url}");
                                break;

                            case 2:
                                await channel.SendMessageAsync($"{roleText}Better check for updates now! Windows 10 Insider Preview Build {build} has just been released{type}! :mailbox_with_mail: :smiley_cat:\n{url}");
                                break;
                        }
                    }

                    // Revert mentionable setting.
                    if (mentionable == false && guild.CurrentUser.GuildPermissions.ManageRoles && guild.CurrentUser.Hierarchy > role.Position)
                        await role.ModifyAsync((e) => e.Mentionable = false);
                }
                catch (Exception ex)
                {
                    client.LogOutput($"FAILURE IN SPEAKING FOR {guild.Name} ({shard.ShardId}/{client.Shards.Count - 1}): {ex}");
                }

                // Log server.
                client.LogOutput($"SPOKEN IN SERVER: {guild.Name} ({shard.ShardId}/{client.Shards.Count - 1})");
            }
        }

        private async Task<bool> CheckBotGuild(SocketGuild guild)
        {
            // If the server is the Discord bots server, ignore.
            if (guild.Id == Constants.BotsGuildId)
                return false;

            // Ensure guild is updated.
            if (guild.Users.Count != guild.MemberCount)
                await guild.DownloadUsersAsync();

            // Is this a bot guild?
            if (guild.MemberCount >= 50 && (guild.Users.Count(u => u.IsBot) / (double)guild.MemberCount) >= 0.9)
            {
                client.LogOutput($"LEAVING BOT SERVER: {guild.Name}");
                try
                {
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
