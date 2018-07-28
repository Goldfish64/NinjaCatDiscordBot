/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: BotCommands.cs
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
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Contains commands for the bot.
    /// </summary>
    public sealed partial class CommandModule : ModuleBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandModule"/> class.
        /// </summary>
        public CommandModule() { }

        #endregion

        #region Methods

        private async Task<NinjaCatDiscordClient> StartTypingAndGetClient()
        {
            // Bot is typing, with added pause for realism.
            await Context.Channel.TriggerTypingAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            return Context.Client as NinjaCatDiscordClient;
        }

        /// <summary>
        /// Gets the about message.
        /// </summary>
        [Command(Constants.AboutCommand)]
        private async Task GetAboutAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Create variable for speaking channel mention.
            var speakingChannel = string.Empty;
            var speakingRole = string.Empty;

            // Get guild. If null, ignore it.
            var guild = (Context.Channel as IGuildChannel)?.Guild as SocketGuild;
            if (guild != null)
            {
                // Get speaking channel.
                var channel = client.GetSpeakingChannelForSocketGuild(guild);

                // Get the mention if speaking is enabled.
                if (channel != null)
                    speakingChannel = channel.Mention;

                // Get ping role.
                var role = client.GetSpeakingRoleForIGuild(guild);

                // Get name of role if enabled.
                if (role != null)
                    speakingRole = role.Name;
            }

            // Dev began Oct 2. 2016.

            // Create speaking channel string.
            var channelText = string.Empty;
            if (!string.IsNullOrEmpty(speakingChannel))
                channelText = $"\n\nI'm currently speaking in {speakingChannel}, but that can be changed with the **{Constants.CommandPrefix}{Constants.SetChannelCommand}** command.";

            // Create role string.
            var roleText = string.Empty;
            if (!string.IsNullOrEmpty(speakingRole))
                roleText = $"\n\nWhen a new build releases, I will ping the **{speakingRole}** role, but that can be changed with the **{Constants.CommandPrefix}{Constants.SetRoleCommand}** command.";

            // Select and send message.
            switch (client.GetRandomNumber(2))
            {
                default:
                    await ReplyAsync($"{Constants.AboutMessage1}" + channelText + roleText);
                    break;

                case 1:
                    await ReplyAsync($"{Constants.AboutMessage2}" + channelText + roleText);
                    break;
            }
        }

        /// <summary>
        /// Gets help.
        /// </summary>
        [Command(Constants.HelpCommand)]
        private async Task GetHelpAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Select and send message.
            switch (client.GetRandomNumber(2))
            {
                default:
                    await ReplyAsync($"So you need help huh? You've come to the right place. :cat::question:\n\n" +
                        $"My set of commands include:\n" +
                        Constants.HelpBody);
                    break;

                case 1:
                    await ReplyAsync($"You need help? Why didn't you just say so? :cat::question:\n\n" +
                        $"My set of commands are as follows:\n" +
                        Constants.HelpBody);
                    break;
            }
        }

        /// <summary>
        /// Gets the homepage URL.
        /// </summary>
        [Command(Constants.HomeCommand)]
        private async Task GetHomeAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Select and send message with URL.
            switch (client.GetRandomNumber(3))
            {
                default:
                    await ReplyAsync($"My source code is here:\n{Constants.HomeCommandUrl}");
                    break;

                case 1:
                    await ReplyAsync($"Here are where my source code is stored:\n{Constants.HomeCommandUrl}");
                    break;

                case 2:
                    await ReplyAsync($"My source:\n{Constants.HomeCommandUrl}");
                    break;
            }
        }

        /// <summary>
        /// Gets the invite URL.
        /// </summary>
        [Command(Constants.InviteCommand)]
        private async Task GetInviteAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Select and send message with invite URL.
            switch (client.GetRandomNumber(3))
            {
                default:
                    await ReplyAsync($"This link will let me be on *your* server:\n{Constants.InviteCommandUrl}");
                    break;

                case 1:
                    await ReplyAsync($"So you want me in *your* server huh? Use this link:\n{Constants.InviteCommandUrl}");
                    break;

                case 2:
                    await ReplyAsync($"Use this link to add me to *your* server:\n{Constants.InviteCommandUrl}");
                    break;
            }
        }

        /// <summary>
        /// Gets the T-Rex.
        /// </summary>
        [Command(Constants.TrexCommand)]
        private async Task GetTrexAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();
            await ReplyAsync("<a:trexa:393897398881222656>");
        }

        /// <summary>
        /// Jumbo.
        /// </summary>
        [Command(Constants.JumboCommand)]
        private async Task SendJumboAsync(params string[] emotes) {
            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Check perms if on server.
            if (Context.Guild != null) {
                var role = client.GetJumboRoleForIGuild(Context.Guild);
                var user = Context.User as SocketGuildUser;

                if (role != null && !user.Roles.Contains(role))
                    return;
            } 

            // Create web client.
            var webClient = new WebClient();

            // Get first emote.
            foreach (var emote in emotes) {
                if (emote.LastIndexOf(':') > 0) {
                    var idStr = emote.Substring(emote.LastIndexOf(':') + 1);
                    idStr = idStr.Substring(0, idStr.Length - 1);
                    if (ulong.TryParse(idStr, out ulong emoteId)) {
                        var isGif = emote.StartsWith("<a:");
                        var data = webClient.DownloadData($"https://cdn.discordapp.com/emojis/{emoteId}" + (isGif ? ".gif" : ".png"));
                        await Context.Channel.SendFileAsync(new MemoryStream(data), "emote" + (isGif ? ".gif" : ".png"));
                        return;
                    }
                }
            }        
        }

        /// <summary>
        /// Gets the latest Insider PC build.
        /// </summary>
        [Command(Constants.LatestBuildCommand)]
        private async Task GetLatestBuildAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get build.
            var data = await client.GetLatestBuildNumberAsync();
            if (data == null)
            {
                await ReplyAsync($"The latest Windows 10 build for PCs couldn't be found. :crying_cat_face: :computer:");
                return;
            }

            // Send.
            await ReplyAsync($"The latest Windows 10 build for PCs is **{data.Item1}**. :cat: :computer:\n{data.Item2}");
        }

        /// <summary>
        /// Gets the latest Insider mobile build.
        /// </summary>
        [Command(Constants.LatestMobileBuildCommand)]
        private async Task GetLatestMobileBuildAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get build.
            var data = await client.GetLatestBuildNumberAsync(BuildType.Mobile);
            if (data == null)
            {
                await ReplyAsync($"The latest Windows 10 Mobile build couldn't be found. :crying_cat_face: :telephone:");
                return;
            }

            // Send.
            await ReplyAsync($"The latest Windows 10 Mobile build is **{data.Item1}**. :cat: :telephone:\n{data.Item2}");
        }

        /// <summary>
        /// Gets the latest Insider server build.
        /// </summary>
        [Command(Constants.LatestServerBuildCommand)]
        private async Task GetLatestServerBuildAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get build.
            var data = await client.GetLatestBuildNumberAsync(BuildType.Server);
            if (data == null)
            {
                await ReplyAsync($"The latest Windows Server build couldn't be found. :crying_cat_face: :desktop:");
                return;
            }

            // Send.
            await ReplyAsync($"The latest Windows Server build is **{data.Item1}**. :cat: :desktop:\n{data.Item2}");
        }

        /// <summary>
        /// Gets the latest Insider skip-ahead build.
        /// </summary>
        [Command(Constants.LatestSkipAheadBuildCommand)]
        private async Task GetLatestSkipAheadBuildAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get build.
            var data = await client.GetLatestBuildNumberAsync(BuildType.SkipAheadPc);
            if (data == null)
            {
                await ReplyAsync($"The latest Windows 10 Skip Ahead build couldn't be found. :crying_cat_face: :desktop:");
                return;
            }

            // Send.
            await ReplyAsync($"The latest Windows 10 Skip Ahead build is **{data.Item1}**. :cat: :fast_forward:\n{data.Item2}");
        }

        /// <summary>
        /// Replies with the bot's info.
        /// </summary>
        [Command(Constants.BotInfoCommand)]
        private async Task GetBotInfoAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get passed time.
            var timeSpan = DateTime.Now.ToLocalTime() - client.StartTime.ToLocalTime();

            // Create string. From http://stackoverflow.com/questions/1138723/timespan-to-friendly-string-library-c.
            var parts = new[]
                {
                    Tuple.Create("day", timeSpan.Days),
                    Tuple.Create("hour", timeSpan.Hours),
                    Tuple.Create("minute", timeSpan.Minutes),
                    Tuple.Create("second", timeSpan.Seconds)
                }.SkipWhile(i => i.Item2 <= 0);
            var timeString = string.Join(", ", parts.Select(p => string.Format("{0} {1}{2}", p.Item2, p.Item1, p.Item2 > 1 ? "s" : string.Empty)));

            // Build embed.
            var embed = new EmbedBuilder();
            embed.Author = new EmbedAuthorBuilder();
            embed.Author.IconUrl = client.CurrentUser.GetAvatarUrl();

            // If in a guild, make color.
            if (Context.Guild != null)
            {
                // Get current user.
                var guildUser = await Context.Guild.GetCurrentUserAsync();

                // Get highest role with color.
                var highestrole = Context.Guild.EveryoneRole;
                foreach (var role in guildUser.RoleIds)
                {
                    var newRole = Context.Guild.GetRole(role);
                    if (newRole.Position > highestrole.Position && newRole.Color.RawValue != 0)
                        highestrole = newRole;
                }

                // Set color, username, and join date.
                embed.Color = highestrole.Color;
                embed.Author.Name = guildUser.Nickname ?? guildUser.Username;
                embed.AddField((e) => { e.Name = "Join date"; e.Value = guildUser.JoinedAt?.ToLocalTime().ToString("d"); e.IsInline = true; });
            }
            else
            {
                // Set username.
                embed.Author.Name = client.CurrentUser.Username;
            }

            // Add final fields.
            var shardId = Context.Guild != null ? (client.GetShardIdFor(Context.Guild) + 1) : 0;
            embed.AddField((e) => { e.Name = "Servers"; e.Value = client.Guilds.Count.ToString(); e.IsInline = true; });
            embed.AddField((e) => { e.Name = "Shard"; e.Value = $"{shardId.ToString()} of {client.Shards.Count.ToString()}"; e.IsInline = true; });
            embed.AddField((e) => { e.Name = "Uptime"; e.Value = timeString; });

            // Select and send message with embed.
            switch (client.GetRandomNumber(3))
            {
                default:
                    await ReplyAsync("Here are my stats:", embed: embed);
                    break;

                case 1:
                    await ReplyAsync("Here you go:", embed: embed);
                    break;

                case 2:
                    await ReplyAsync("My information awaits:", embed: embed);
                    break;
            }
        }

        #endregion
    }
}
