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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

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

            // Get owner of bot.
            //var owner = client.GetUser(Constants.OwnerId);

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

        private async Task<Tuple<string, string>> GetLatestBuildNumberAsync(string type = "pc")
        {
            // Create HTTP client.
            var client = new HttpClient();

            // Get blog entries.
            var doc = XDocument.Parse(await client.GetStringAsync("https://blogs.windows.com/windowsexperience/tag/windows-insider-program/feed/"));
            var entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                          select new BlogEntry()
                          { Link = item.Elements().First(i => i.Name.LocalName == "link").Value, Title = item.Elements().First(i => i.Name.LocalName == "title").Value };
            var list = entries.ToList();

            // Get second page.
            doc = XDocument.Parse(await client.GetStringAsync("https://blogs.windows.com/windowsexperience/tag/windows-insider-program/feed/?paged=2"));
            entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                      select new BlogEntry()
                      { Link = item.Elements().First(i => i.Name.LocalName == "link").Value, Title = item.Elements().First(i => i.Name.LocalName == "title").Value };
            list.AddRange(entries.ToList());

            // Get most recent build post.
            var post = list.Where(p => p.Title.ToLowerInvariant().Contains("insider preview build") && p.Title.ToLowerInvariant().Contains(type)).FirstOrDefault();

            // Get build number.
            var build = Regex.Match(post.Title, @"\d{5,}", type == "mobile" ? RegexOptions.RightToLeft : RegexOptions.None).Value;

            // Return info.
            return new Tuple<string, string>(build, post.Link);
        }

        /// <summary>
        /// Gets the latest Insider PC build.
        /// </summary>
        [Command(Constants.LatestBuildCommand)]
        private async Task GetLatestBuildAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Get build.
            var data = await GetLatestBuildNumberAsync();

            // Send.
            await ReplyAsync($"The latest Windows 10 build for PCs is **{data.Item1}**. :cat: :computer:\n{data.Item2}");
        }

        /// <summary>
        /// Gets the latest Insider mobile build.
        /// </summary>
        [Command(Constants.LatestMobileBuildCommand)]
        private async Task GetLatestMobileBuildAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Get build.
            var data = await GetLatestBuildNumberAsync("mobile");

            // Send.
            await ReplyAsync($"The latest Windows 10 Mobile build is **{data.Item1}**. :cat: :telephone:\n{data.Item2}");
        }

        /// <summary>
        /// Gets the latest Insider server build.
        /// </summary>
        [Command(Constants.LatestServerBuildCommand)]
        private async Task GetLatestServerBuildAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Get build.
            var data = await GetLatestBuildNumberAsync("server");

            // Send.
            await ReplyAsync($"The latest Windows Server build is **{data.Item1}**. :cat: :desktop:\n{data.Item2}");
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

            // Get user.
            var user = Context.Client.CurrentUser;

            // Get biggest server.
            SocketGuild biggestServer = null;
            foreach (var shard in client.Shards)
            {
                foreach (var guild in shard.Guilds)
                {
                    // Omit Discord Bots from results.
                    if (guild.Id == Constants.BotsGuildId)
                        continue;

                    if (biggestServer == null || guild.MemberCount > biggestServer.MemberCount)
                        biggestServer = guild;
                }
            }

            // Get smallest server.
            SocketGuild smallestServer = null;
            foreach (var shard in client.Shards)
            {
                foreach (var guild in shard.Guilds)
                {
                    if (smallestServer == null || guild.MemberCount < smallestServer.MemberCount)
                        smallestServer = guild;
                }
            }

            // Build embed.
            var embed = new EmbedBuilder();
            embed.Author = new EmbedAuthorBuilder();
            embed.Author.IconUrl = user.GetAvatarUrl();

            // If in a guild, make color.
            if (Context.Guild != null)
            {
                // Get current user.
                var guildUser = await Context.Guild.GetCurrentUserAsync();

                // Get highest role.
                var highestrole = Context.Guild.EveryoneRole;
                foreach (var role in guildUser.RoleIds)
                {
                    var newRole = Context.Guild.GetRole(role);
                    if (newRole.Position > highestrole.Position)
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
                embed.Author.Name = user.Username;
            }

            // Add final fields.
            var shardId = Context.Guild != null ? (client.GetShardIdFor(Context.Guild) + 1) : 0;
            embed.AddField((e) => { e.Name = "Servers"; e.Value = client.Guilds.Count.ToString(); e.IsInline = true; });
            embed.AddField((e) => { e.Name = "Shard"; e.Value = $"{shardId.ToString()}/{client.Shards.Count.ToString()}"; e.IsInline = true; });

            embed.AddField((e) => { e.Name = "Largest server"; e.Value = $"{biggestServer.Name} ({biggestServer.MemberCount})"; e.IsInline = true; });
            embed.AddField((e) => { e.Name = "Smallest server"; e.Value = $"{smallestServer.Name} ({smallestServer.MemberCount})"; e.IsInline = true; });

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
