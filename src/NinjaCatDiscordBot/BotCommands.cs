/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: BotCommands.cs
* 
* Copyright (c) 2016 - 2020 John Davis
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
using NinjaCatDiscordBot.Properties;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot {
    /// <summary>
    /// Contains commands for the bot.
    /// </summary>
    public partial class BotCommandsModule : CommandModuleBase {
        private HttpClient _client = new HttpClient();

        #region Command methods

        /// <summary>
        /// Gets help.
        /// </summary>
        [Command(Constants.HelpCommand)]
        [Summary("show help")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetHelpAsync() {
            var sb = new StringBuilder();

            // Select and send message.
            switch (Context.Client.GetRandomNumber(2)) {
                default:
                    sb.AppendLine("So you need help huh? You've come to the right place. :cat::question:\n\n" +
                        $"__My set of commands include:__");
                    break;

                case 1:
                    sb.AppendLine("You need help? Why didn't you just say so? :cat::question:\n\n" +
                        $"__My set of commands are as follows:__");
                    break;
            }

            // General commands.
            foreach (var c in Context.Client.Commands.Commands.Where(p => p.Remarks == Constants.RemarkGeneral).OrderBy(p => p.Name)) {
                sb.Append($"**{Constants.CommandPrefix}{c.Name}**");
                foreach (var p in c.Parameters) {
                    sb.Append($" [{p.Name}]");
                }
                sb.AppendLine($": {c.Summary}");
            }

            // Admin commands.
            sb.AppendLine("\n__Admin commands:__");
            foreach (var c in Context.Client.Commands.Commands.Where(p => p.Remarks == Constants.RemarkAdmin).OrderBy(p => p.Name)) {
                sb.Append($"**{Constants.CommandPrefix}{c.Name}**");
                foreach (var p in c.Parameters) {
                    sb.Append($" [{p.Name}]");
                }
                sb.AppendLine($": {c.Summary}");
            }

            await ReplyAsync(sb.ToString());
        }

        /// <summary>
        /// Gets the homepage URL.
        /// </summary>
        [Command("source")]
        [Alias("home")]
        [Summary("go to my source code")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetHomeAsync() {
            await ReplyRandomAsync(null,
                $"My source code is here:\n{Constants.AppUrl}",
                $"Here is where my source code is stored:\n{Constants.AppUrl}",
                $"My source:\n{Constants.AppUrl}"
            );
        }

        /// <summary>
        /// Gets the invite URL.
        /// </summary>
        [Command("invite")]
        [Summary("invite me to your server")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetInviteAsync() {
            var inviteUrl = string.Format(Constants.InviteUrl, Context.Client.CurrentUser.Id);
            await ReplyRandomAsync(null,
                $"This link will let me be on *your* server:\n{inviteUrl}",
                $"So you want me in *your* server huh? Use this link:\n{inviteUrl}",
                $"Use this link to add me to *your* server:\n{inviteUrl}"
            );
        }

        /// <summary>
        /// Gets the T-Rex.
        /// </summary>
        [Command("trex")]
        [Summary("shows the Windows 10 Skype emoticon")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetTrexAsync() {
            using (var stream = new MemoryStream(Resources.trex))
                await Context.Channel.SendFileAsync(stream, "trex.gif");
        }

        /// <summary>
        /// Jumbo.
        /// </summary>
        [Command("jumbo")]
        [Alias("j")]
        [Summary("jumboify emotes")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task SendJumboAsync(params string[] emotes) {
            // Check perms if on server.
            if (Context.Guild != null) {
                var role = Context.Client.GetRoleForIGuild(Context.Guild, RoleType.Jumbo);
                var user = Context.User as SocketGuildUser;

                if (role != null && !user.Roles.Contains(role))
                    return;
            }

            if (emotes.Length == 0) {
                await ReplyAsync("No emotes passed!");
                return;
            }

            // Iterate through each emote and jumbo it, up to the max of 2.
            int limit = 0;
            foreach (var emote in emotes) {
                if (limit >= 2)
                    return;

               // try {
                    string emoteUrl = null;

                    if (Emote.TryParse(emote, out Emote found)) {
                        emoteUrl = found.Url;
                    }
                    else {
                        int codepoint = Char.ConvertToUtf32(emote, 0);
                        string codepointHex = codepoint.ToString("X").ToLower();

                        emoteUrl = $"https://raw.githubusercontent.com/twitter/twemoji/gh-pages/v/12.1.3/72x72/{codepointHex}.png";
                    }

                    var req = await _client.GetStreamAsync(emoteUrl);
                    await Context.Channel.SendFileAsync(req, Path.GetFileName(emoteUrl));
              //  }
                //catch (HttpRequestException) { } // Failed to download emote, skip it

                limit++;
            }
        }

        /// <summary>
        /// Gets the latest Insider PC build for the Dev Channel.
        /// </summary>
        [Command("latestdev")]
        [Alias("latest")]
        [Summary("shows the latest Dev Channel Insider build")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetLatestDevBuildAsync() {
            // Get build.
            var post = await Context.Client.GetLatestBuildPostAsync(BuildType.DevPc);
            if (post == null) {
                await ReplyAsync($"The latest Windows 10 Dev Channel build couldn't be found. :crying_cat_face: :tools:");
                return;
            }

            // Send.
            await ReplyAsync($"The latest Windows 10 Dev Channel build is **{post.BuildNumber}**. :cat: :tools:\n<{post.Link}>");
        }

        /// <summary>
        /// Gets the latest Insider PC build for the Beta Channel.
        /// </summary>
        [Command("latestbeta")]
        [Summary("shows the latest Beta Channel Insider build")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetLatestBetaBuildAsync() {
            // Get build.
            var post = await Context.Client.GetLatestBuildPostAsync(BuildType.BetaPc);
            if (post == null) {
                await ReplyAsync($"The latest Windows 10 Beta Channel build couldn't be found. :crying_cat_face: :paintbrush:");
                return;
            }

            // Send.
            await ReplyAsync($"The latest Windows 10 Beta Channel build is **{post.BuildNumber}**. :cat: :paintbrush:\n<{post.Link}>");
        }

        /// <summary>
        /// Gets the latest Insider PC build for the Release Preview Channel.
        /// </summary>
        [Command("latestrp")]
        [Summary("shows the latest Release Preview Channel Insider build")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetLatestReleasePreviewBuildAsync() {
            // Get build.
            var post = await Context.Client.GetLatestBuildPostAsync(BuildType.ReleasePreviewPc);
            if (post == null) {
                await ReplyAsync($"The latest Windows 10 Release Preview Channel build couldn't be found. :crying_cat_face: :package:");
                return;
            }

            // Send.
            await ReplyAsync($"The latest Windows 10 Release Preview Channel build is **{post.BuildNumber}**. :cat: :package:\n<{post.Link}>");
        }

        /// <summary>
        /// Gets the latest Insider server build.
        /// </summary>
        [Command("latestserver")]
        [Summary("shows the latest Server Insider build")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetLatestServerBuildAsync() {
            // Get build.
            var post = await Context.Client.GetLatestBuildPostAsync(BuildType.Server);
            if (post == null) {
                await ReplyAsync($"The latest Windows Insider Server build couldn't be found. :crying_cat_face: :desktop:");
                return;
            }

            // Send.
            await ReplyAsync($"The latest Windows Insider Server build is **{post.BuildNumber}**. :cat: :desktop:\n<{post.Link}>");
        }

        /// <summary>
        /// Replies with the bot's info.
        /// </summary>
        [Command("info")]
        [Alias("about")]
        [Summary("shows my info")]
        [Remarks(Constants.RemarkGeneral)]
        public async Task GetBotInfoAsync() {
            // Get passed time.
            var timeSpan = DateTime.Now.ToLocalTime() - Context.Client.StartTime.ToLocalTime();

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
            var embed = new EmbedBuilder {
                Author = new EmbedAuthorBuilder(),
                Footer = new EmbedFooterBuilder()
            };
            embed.Author.IconUrl = Context.Client.CurrentUser?.GetAvatarUrl();
            embed.Footer.Text = $"Version: {Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";

            embed.AddField(e => { e.Name = "About"; e.Value = Constants.AboutMessage; });

            // Add general overview fields.
            var shardId = Context.Guild != null ? (Context.Client.GetShardIdFor(Context.Guild) + 1) : 1;
            embed.AddField(e => { e.Name = "Servers"; e.Value = Context.Client.Guilds.Count.ToString(); e.IsInline = true; });
            embed.AddField(e => { e.Name = "Shard"; e.Value = $"{shardId} of {Context.Client.Shards.Count}"; e.IsInline = true; });
            if (Context.Guild != null)
                embed.AddField(e => { e.Name = "Join date"; e.Value = Context.Guild.CurrentUser.JoinedAt?.ToLocalTime().ToString("d"); e.IsInline = true; });
            embed.AddField(e => { e.Name = "Uptime"; e.Value = timeString; });

            // If in a guild, make color.
            if (Context.Guild != null) {
                // Get current guild user.
                var guildUser = Context.Guild.CurrentUser;

                // Get highest role with color.
                var highestrole = Context.Guild.EveryoneRole;
                foreach (var role in guildUser.Roles) {
                    if (role.Position > highestrole.Position && role.Color.RawValue != 0)
                        highestrole = role;
                }

                // Set color, username, and join date.
                embed.Color = highestrole.Color;
                embed.Author.Name = guildUser.Nickname ?? guildUser.Username;

                // Add channel and roles.
                var channel = Context.Client.GetSpeakingChannelForSocketGuild(Context.Guild);
                var roleDev = Context.Client.GetRoleForIGuild(Context.Guild, RoleType.InsiderDev);
                var roleBeta = Context.Client.GetRoleForIGuild(Context.Guild, RoleType.InsiderBeta);
                var roleReleasePreview = Context.Client.GetRoleForIGuild(Context.Guild, RoleType.InsiderReleasePreview);
                embed.AddField(e => { e.Name = "Insider channel"; e.Value = channel?.Mention ?? "None"; });
                embed.AddField(e => { e.Name = "Insider Dev role"; e.Value = roleDev?.Mention ?? "None"; e.IsInline = true; });
                embed.AddField(e => { e.Name = "Insider Beta role"; e.Value = roleBeta?.Mention ?? "None"; e.IsInline = true; });
                embed.AddField(e => { e.Name = "Insider Release Preview role"; e.Value = roleReleasePreview?.Mention ?? "None"; e.IsInline = true; });
            }
            else {
                // Set username.
                embed.Author.Name = Context.Client.CurrentUser.Username;
            }

            // Select and send message with embed.
            await ReplyRandomAsync(embed.Build(),
                "Here are my stats:",
                "Here you go:",
                "My information awaits:"
            );
        }

        #endregion
    }
}
