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
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
        /// Replies with the about message.
        /// </summary>
        [Command(Constants.AboutCommand)]
        private async Task ReplyAboutAsync()
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
        /// Replies with help.
        /// </summary>
        [Command(Constants.HelpCommand)]
        private async Task ReplyHelpAsync()
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
        /// Replies with the homepage URL.
        /// </summary>
        [Command(Constants.HomeCommand)]
        [Alias(Constants.HomeCommandAlias, Constants.HomeCommandAlias2)]
        private async Task ReplyHomeAsync()
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
        /// Replies with the invite URL.
        /// </summary>
        [Command(Constants.InviteCommand)]
        private async Task ReplyInviteAsync()
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
        /// Replies with a pong.
        /// </summary>
        [Command(Constants.PingCommand)]
        private async Task ReplyPingAsync()
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
        /// Replies with the T-Rex.
        /// </summary>
        [Command(Constants.TrexCommand)]
        private async Task ReplyTrexAsync()
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
        /// Replies with the latest Insider build.
        /// </summary>
        [Command(Constants.LatestBuildCommand)]
        private async Task ReplyLatestBuildAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Create the HttpClient.
            using (var httpClient = new HttpClient())
            {
                // Get the latest build list containing the newest 50 builds from BuildFeed.
                var response = await httpClient.GetStringAsync("https://buildfeed.net/api/GetBuilds?limit=50");

                // Parse JSON and get the latest public build.
                var builds = JArray.Parse(response).ToList();
                var newestBuild = builds.First(b => (int)b["SourceType"] == 0);

                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Select and send message.
                switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"I've got the latest public build for you. It is **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}**. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 1:
                        await ReplyAsync($"Ask and you shall receive. The latest public build is **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}**. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 2:
                        await ReplyAsync($"Yes master. Right away master. **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}** is the latest and greatest. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 3:
                        await ReplyAsync($"**{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}** is the newest public build according to my sources. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;
                }
            }
        }

        /// <summary>
        /// Replies with the Insider program URL.
        /// </summary>
        [Command(Constants.EnrollCommand)]
        private async Task ReplyEnrollAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Send URL.
            await ReplyAsync(Constants.EnrollCommandUrl);
        }

        /// <summary>
        /// Replies with the local time.
        /// </summary>
        [Command(Constants.TimeCommand)]
        private async Task ReplyTimeAsync()
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
        /// Replies with the bot's platform.
        /// </summary>
        [Command(Constants.PlatformCommand)]
        private async Task ReplyPlatformAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();
            
            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(5))
            {
                default:
                    await ReplyAsync($"I'm currently living on {RuntimeInformation.FrameworkDescription.Trim()} on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}. Now tell me where *you* live so that I may visit you.");
                    break;

                case 1:
                    await ReplyAsync($"I call {RuntimeInformation.FrameworkDescription.Trim()} on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} home. What's yours?");
                    break;

                case 2:
                    await ReplyAsync($"For me, home is on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} running {RuntimeInformation.FrameworkDescription.Trim()}. Where is yours?");
                    break;

                case 3:
                    await ReplyAsync($"Questions, questions. My home is {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} running {RuntimeInformation.FrameworkDescription.Trim()}.");
                    break;

                case 4:
                    await ReplyAsync($"I live in a box running {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} and {RuntimeInformation.FrameworkDescription.Trim()}. What's that? You live in one too?");
                    break;
            }
        }

        /// <summary>
        /// Replies with the bot's uptime.
        /// </summary>
        [Command(Constants.UptimeCommand)]
        private async Task ReplyUptimeAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get passed time.
            var time = DateTime.Now.ToLocalTime() - (Context.Client as NinjaCatDiscordClient).StartTime.ToLocalTime();

            // Create string.
            var values = (time.Days > 0 ? time.Days.ToString() + $" day{(time.Days > 1 ? "s" : "")}{(time.Hours > 0 ? ", " : "")}" : string.Empty) +
                (time.Hours > 0 ? time.Hours.ToString() + $" hour{(time.Hours > 1 ? "s" : "")}{(time.Minutes > 0 ? ", " : "")}" : string.Empty) +
                (time.Minutes > 0 ? time.Minutes.ToString() + $" minute{(time.Minutes > 1 ? "s" : "")}{(time.Seconds > 0 ? ", " : "")}" : string.Empty) +
                (time.Seconds > 0 ? time.Seconds.ToString() + $" second{(time.Seconds > 1 ? "s" : "")}" : string.Empty);

            // Select and send message.
            switch ((Context.Client as NinjaCatDiscordClient).GetRandomNumber(3))
            {
                default:
                    await ReplyAsync($"I've been up for {values}.");
                    break;

                case 0:
                    await ReplyAsync($"It's been {values} since I started.");
                    break;

                case 1:
                    await ReplyAsync($"I started {values} ago.");
                    break;
            }
        }

        /// <summary>
        /// Replies with the bot's servers.
        /// </summary>
        [Command(Constants.ServersCommand)]
        private async Task ReplyServersAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get guild count.
            var count = client.Guilds.Count;

            // Select and send message.
            switch (client.GetRandomNumber(3))
            {
                default:
                    await ReplyAsync($"I'm currently a member of {count} server{(count > 1 ? "s" : "")}.");
                    break;

                case 1:
                    await ReplyAsync($"I'm part of {count} server{(count > 1 ? "s" : "")}.");
                    break;

                case 2:
                    await ReplyAsync($"I reside in {count} server{(count > 1 ? "s" : "")} currently.");
                    break;
            }
        }

        /// <summary>
        /// Replies with the bot's servers.
        /// </summary>
        /// <returns></returns>
        [Command(Constants.ServerNamesCommand)]
        public async Task ReplyServerNamesAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get the user.
            var user = Context.Message.Author as IUser;

            // If the user is not master, show error.
            if (user?.Id != Constants.OwnerId)
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"Sorry, but only my master can list the servers I'm in.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You aren't my owner.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.Message.Author.Mention}, I'm afraid I can't do that. You aren't my master.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. Only my owner can list the servers I'm in.");
                        break;
                }
                return;
            }

            // Get guilds.
            var guilds = client.Guilds;

            // Send message.
            await ReplyAsync($"I'm currently a member of {guilds.Count} server{(guilds.Count > 1 ? "s" : "")}:\n{string.Join(", ", guilds)}");
        }

        #endregion
    }
}
