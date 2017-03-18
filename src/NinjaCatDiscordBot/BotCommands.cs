/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: BotCommands.cs
* 
* Copyright (c) 2016-2017 John Davis
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
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Gets the about message.
        /// </summary>
        [Command(Constants.AboutCommand)]
        private async Task GetAboutAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Create variable for speaking channel mention.
            var speakingChannel = string.Empty;

            // Get guild. If null, ignore it.
            var guild = (Context.Channel as IGuildChannel)?.Guild as SocketGuild;
            if (guild != null)
            {
                // Get speaking channel.
                var channel = client.GetSpeakingChannelForSocketGuild(guild);

                // Get the mention if speaking is enabled.
                if (channel != null)
                    speakingChannel = channel.Mention;
            }

            // Dev began Oct 2. 2016.
            // Is a speaking channel set?
            if (!string.IsNullOrEmpty(speakingChannel))
            {
                // Select and send message.
                switch (client.GetRandomNumber(2))
                {
                    default:
                        await ReplyAsync($"{Constants.AboutMessage1}\n\n" +
                            $"I'm currently speaking in {speakingChannel}, but you can change it with the **{Constants.CommandPrefix}{Constants.SetChannelCommand}** command.");
                        break;

                    case 1:
                        await ReplyAsync($"{Constants.AboutMessage2}\n\n" +
                            $"I'm currently speaking in {speakingChannel}, but it can be changed with the **{Constants.CommandPrefix}{Constants.SetChannelCommand}** command.");
                        break;
                }
            }
            else
            {
                // Select and send message.
                switch (client.GetRandomNumber(2))
                {
                    default:
                        await ReplyAsync(Constants.AboutMessage1);
                        break;

                    case 1:
                        await ReplyAsync(Constants.AboutMessage2);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets help.
        /// </summary>
        [Command(Constants.HelpCommand)]
        private async Task GetHelpAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(2))
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
        [Alias(Constants.HomeCommandAlias, Constants.HomeCommandAlias2)]
        private async Task GetHomeAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message with URL.
            switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(3))
            {
                default:
                    await ReplyAsync($"My source code is here:\n{Constants.HomeCommandUrl}");
                    break;

                case 1:
                    await ReplyAsync($"Here are where my insides are stored:\n{Constants.HomeCommandUrl}");
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
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message with invite URL.
            switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(3))
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
        /// Gets a pong.
        /// </summary>
        [Command(Constants.PingCommand)]
        private async Task GetPingAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(5))
            {
                default:
                    await ReplyAsync($"Pong {Context.Message.Author.Mention}! :ping_pong:");
                    break;

                case 0:
                    await ReplyAsync($"I'm very good at ping pong {Context.Message.Author.Mention}. :ping_pong:");
                    break;

                case 1:
                    await ReplyAsync($"I know where you live {Context.Message.Author.Mention}. :smirk_cat: :house:");
                    break;

                case 2:
                    await ReplyAsync($"You know, I can do other things besides play ping-pong {Context.Message.Author.Mention}. :ping_pong:");
                    break;

                case 3:
                    await ReplyAsync($"Why, {Context.Message.Author.Mention}. Are you lonely?");
                    break;

                case 4:
                    await ReplyAsync($"Remember, {Context.Message.Author.Mention}, I am always here. :slight_smile:");
                    break;
            }
        }

        /// <summary>
        /// Gets the T-Rex.
        /// </summary>
        [Command(Constants.TrexCommand)]
        private async Task GetTrexAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message with link.
            switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(3))
            {
                default:
                    await ReplyAsync($"Here you go.\n{Constants.TrexCommandUrl}");
                    break;

                case 1:
                    await ReplyAsync($"ROAAAAR!\n{Constants.TrexCommandUrl}");
                    break;

                case 2:
                    await ReplyAsync($"Here I am riding the T-Rex!\n{Constants.TrexCommandUrl}");
                    break;
            }
        }

        /// <summary>
        /// Gets the latest Insider PC build.
        /// </summary>
        [Command(Constants.LatestBuildCommand)]
        private async Task GetLatestBuildAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Create process for JSON fetching.
            var process = new Process();
            process.StartInfo.FileName = "WindowsBlogsJsonGetterApp.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            // Run process and get result.
            process.Start();
            var result = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Parse JSON and get the latest PC post.
            var posts = JArray.Parse(result).ToList();
            var newestBuild = posts.First(b => b["title"].ToString().ToLowerInvariant().Contains("pc"));

            // Get build number and link.
            var build = Regex.Match(newestBuild["title"].ToString(), @"\d{5,}").Value;
            var link = newestBuild["link"].ToString();

            // Send.
            await ReplyAsync($"The latest Windows 10 build for PCs is **{build}**. :cat: :computer:\n{link}");
        }

        /// <summary>
        /// Gets the latest Insider mobile build.
        /// </summary>
        [Command(Constants.LatestMobileBuildCommand)]
        private async Task GetLatestMobileBuildAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Create process for JSON fetching.
            var process = new Process();
            process.StartInfo.FileName = "WindowsBlogsJsonGetterApp.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            // Run process and get result.
            process.Start();
            var result = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Parse JSON and get the latest mobile post.
            var posts = JArray.Parse(result).ToList();
            var newestBuild = posts.First(b => b["title"].ToString().ToLowerInvariant().Contains("mobile"));

            // Get build number and link.
            var build = Regex.Match(newestBuild["title"].ToString(), @"\d{5,}").Value;
            var link = newestBuild["link"].ToString();

            // Send.
            await ReplyAsync($"The latest Windows 10 Mobile build is **{build}**. :cat: :telephone:\n{link}");
        }

        /// <summary>
        /// Gets the local time.
        /// </summary>
        [Command(Constants.TimeCommand)]
        private async Task GetTimeAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Get current time and time zone.
            var time = DateTime.Now.ToLocalTime();
            var timeZone = TimeZoneInfo.Local;

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(6))
            {
                default:
                    await ReplyAsync($"My watch says {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 1:
                    await ReplyAsync($"I have no idea where you live, but my watch says {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 2:
                    await ReplyAsync($"My current time is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 3:
                    await ReplyAsync($"My internal clock is telling me it is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 4:
                    await ReplyAsync($"Just glanced at the Peanuts clock on the wall. It is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 5:
                    await ReplyAsync($"Beep. Boop. The current local time is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;
            }
        }

        /// <summary>
        /// Replies with the bot's info.
        /// </summary>
        [Command(Constants.BotInfoCommand)]
        private async Task GetBotInfoAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

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

            // Get current user.
            var user = await Context.Guild.GetCurrentUserAsync();

            // Get highest role.
            var highestrole = Context.Guild.EveryoneRole;
            foreach (var role in user.RoleIds)
            {
                var newRole = Context.Guild.GetRole(role);
                if (newRole.Position > highestrole.Position)
                    highestrole = newRole;
            }

            // Build embed.
            var embed = new EmbedBuilder();
            embed.Author = new EmbedAuthorBuilder();
            embed.Author.IconUrl = user.GetAvatarUrl();
            embed.Author.Name = user.Nickname ?? user.Username;
            embed.Color = highestrole.Color;
            embed.AddField((e) => { e.Name = "Join date"; e.Value = user.JoinedAt?.ToLocalTime().ToString("d"); e.IsInline = true; });
            embed.AddField((e) => { e.Name = "Servers"; e.Value = client.Guilds.Count.ToString(); e.IsInline = true; });
            embed.AddField((e) => { e.Name = "Shard"; e.Value = $"{(client.GetShardIdFor(Context.Guild) + 1).ToString()}/{client.Shards.Count.ToString()}"; e.IsInline = true; });
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

        /// <summary>
        /// Gets the bot's servers.
        /// </summary>
        /// <returns></returns>
        [Command(Constants.ServersCommand)]
        public async Task GetServersAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get guilds.
            var guilds = client.Guilds;

            // Send message.
            await ReplyAsync($"I'm currently a member of {guilds.Count} server{(guilds.Count > 1 ? "s" : "")}:\n{string.Join(", ", guilds)}");
        }

        #endregion
    }
}
