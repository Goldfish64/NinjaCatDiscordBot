/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: CommandModule.cs
* 
* Copyright (c) 2016 John Davis
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
    [Module]
    public sealed class CommandModule
    {
        #region Private variables

        private NinjaCatDiscordClient client;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandModule"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IDiscordClient"/> to use.</param>
        public CommandModule(IDiscordClient client)
        {
            // Check to see if client is valid.
            if (!(client is NinjaCatDiscordClient))
                throw new ArgumentException($"This module requires a {nameof(NinjaCatDiscordClient)}.", nameof(client));

            // Get client.
            this.client = client as NinjaCatDiscordClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Replies to the specified message with the about message.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.AboutCommand)]
        private async Task ReplyAboutAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Create variable for speaking channel mention.
            var speakingChannel = string.Empty;

            // Get guild. If null, ignore it.
            var guild = (message.Channel as IGuildChannel)?.Guild;
            if (guild != null)
            {
                // Get speaking channel.
                var channel = await client.GetSpeakingChannelForGuildAsync(guild);

                // Get the mention if speaking is enabled.
                if (channel != null)
                    speakingChannel = channel.Mention;
            }

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Dev began Oct 2. 2016.
            // Is a speaking channel set?
            if (!string.IsNullOrEmpty(speakingChannel))
            {
                // Select and send message.
                switch (client.GetRandomNumber(2))
                {
                    default:
                        await message.Channel.SendMessageAsync($"{Constants.AboutMessage1}\n\n" +
                            $"I'm currently speaking in {speakingChannel}, but you can change it with the **{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.SetGroup} {Constants.ChannelCommand}** command.");
                        break;

                    case 1:
                        await message.Channel.SendMessageAsync($"{Constants.AboutMessage2}\n\n" +
                            $"I'm currently speaking in {speakingChannel}, but it can be changed with the **{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.SetGroup} {Constants.ChannelCommand}** command.");
                        break;
                }
            }
            else
            {
                // Select and send message.
                switch (client.GetRandomNumber(2))
                {
                    default:
                        await message.Channel.SendMessageAsync(Constants.AboutMessage1);
                        break;

                    case 1:
                        await message.Channel.SendMessageAsync(Constants.AboutMessage2);
                        break;
                }
            }
        }

        /// <summary>
        /// Replies to the specified message with help.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.HelpCommand)]
        private async Task ReplyHelpAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (client.GetRandomNumber(2))
            {
                default:
                    await message.Channel.SendMessageAsync($"So you need help huh? You've come to the right place. :cat::question:\n\n" +
                        $"My set of commands include:\n" +
                        $"**{Constants.CommandPrefix}{Constants.AboutCommand}**: {Constants.AboutCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.HelpCommand}**: {Constants.HelpCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.HomeCommand}** or **{Constants.CommandPrefix}{Constants.HomeCommandAlias}**: {Constants.HomeCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.InviteCommand}**: {Constants.InviteCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.PingCommand}**: {Constants.PingCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.TrexCommand}**: {Constants.TrexCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.LatestBuildCommand}**: {Constants.LatestBuildCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.EnrollCommand}**: {Constants.EnrollCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.TimeCommand}**: {Constants.TimeCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.PlatformCommand}**: {Constants.PlatformCommandDesc}.\n\n" +
                        $"Settings commands:\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.GetGroup} {Constants.NicknameCommand}**: {Constants.GetNicknameCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.SetGroup} {Constants.NicknameCommand}** *nickname*: {Constants.SetNicknameCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.GetGroup} {Constants.ChannelCommand}**: {Constants.GetChannelCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.SetGroup} {Constants.ChannelCommand}** *channel*: {Constants.SetChannelCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.DisableGroup} {Constants.ChannelCommand}**: {Constants.DisableChannelCommandDesc}.\n\n" +
                        $"If you mention me and include a command, I'll usually respond in some fashion.");
                    break;

                case 1:
                    await message.Channel.SendMessageAsync($"You need help? Why didn't you just say so? :cat::question:\n\n" +
                        $"My set of commands include:\n" +
                        $"**{Constants.CommandPrefix}{Constants.AboutCommand}**: {Constants.AboutCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.HelpCommand}**: {Constants.HelpCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.HomeCommand}** or **{Constants.CommandPrefix}{Constants.HomeCommandAlias}**: {Constants.HomeCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.InviteCommand}**: {Constants.InviteCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.PingCommand}**: {Constants.PingCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.TrexCommand}**: {Constants.TrexCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.LatestBuildCommand}**: {Constants.LatestBuildCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.EnrollCommand}**: {Constants.EnrollCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.TimeCommand}**: {Constants.TimeCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.PlatformCommand}**: {Constants.PlatformCommandDesc}.\n\n" +
                        $"Settings commands:\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.GetGroup} {Constants.NicknameCommand}**: {Constants.GetNicknameCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.SetGroup} {Constants.NicknameCommand}** *nickname*: {Constants.SetNicknameCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.GetGroup} {Constants.ChannelCommand}**: {Constants.GetChannelCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.SetGroup} {Constants.ChannelCommand}** *channel*: {Constants.SetChannelCommandDesc}.\n" +
                        $"**{Constants.CommandPrefix}{Constants.SettingsModule} {Constants.DisableGroup} {Constants.ChannelCommand}**: {Constants.DisableChannelCommandDesc}.\n\n" +
                        $"If you mention me with a command, I might get back to you.");
                    break;
            }
        }

        /// <summary>
        /// Replies to the specified message with the homepage URL.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.HomeCommand)]
        [Alias(Constants.HomeCommandAlias)]
        private async Task ReplyHomeAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Send URL.
            await message.Channel.SendMessageAsync(Constants.HomeCommandUrl);
        }

        /// <summary>
        /// Replies to the specified message with the invite URL.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.InviteCommand)]
        private async Task ReplyInviteAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Send invite URL.
            await message.Channel.SendMessageAsync(Constants.InviteCommandUrl);
        }

        /// <summary>
        /// Replies to the specified message with a pong.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.PingCommand)]
        private async Task ReplyPingAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (client.GetRandomNumber(5))
            {
                default:
                    await message.Channel.SendMessageAsync($"Pong {message.Author.Mention}! :ping_pong:");
                    break;

                case 0:
                    await message.Channel.SendMessageAsync($"I'm very good at ping pong {message.Author.Mention}. :ping_pong:");
                    break;

                case 1:
                    await message.Channel.SendMessageAsync($"I know where you live {message.Author.Mention}. :smirk_cat: :house:");
                    break;

                case 2:
                    await message.Channel.SendMessageAsync($"You know, I can do other things besides play ping-pong {message.Author.Mention}. :ping_pong:");
                    break;

                case 3:
                    await message.Channel.SendMessageAsync($"Why, {message.Author.Mention}. Are you lonely?");
                    break;

                case 4:
                    await message.Channel.SendMessageAsync($"Remember, {message.Author.Mention}, I am always here. :slight_smile:");
                    break;
            }
        }

        /// <summary>
        /// Replies to the specified message with the T-Rex.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.TrexCommand)]
        private async Task ReplyTrexAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message with link.
            switch (client.GetRandomNumber(3))
            {
                default:
                    await message.Channel.SendMessageAsync($"Here you go.\n{Constants.TrexCommandUrl}");
                    break;

                case 1:
                    await message.Channel.SendMessageAsync($"ROAAAAR!\n{Constants.TrexCommandUrl}");
                    break;

                case 2:
                    await message.Channel.SendMessageAsync($"Here I am riding the T-Rex!\n{Constants.TrexCommandUrl}");
                    break;
            }
        }

        /// <summary>
        /// Replies to the specified message with the latest Insider build.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.LatestBuildCommand)]
        private async Task ReplyLatestBuildAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

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
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await message.Channel.SendMessageAsync($"I've got the latest public build for you. It is **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}**. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 1:
                        await message.Channel.SendMessageAsync($"Ask and you shall receive. The latest public build is **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}**. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 2:
                        await message.Channel.SendMessageAsync($"Yes master. Right away master. **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}** is the latest and greatest. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 3:
                        await message.Channel.SendMessageAsync($"**{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}** is the newest public build according to my sources. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;
                }
            }
        }

        /// <summary>
        /// Replies to the specified message with the Insider program URL.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.EnrollCommand)]
        private async Task ReplyEnrollAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Send URL.
            await message.Channel.SendMessageAsync(Constants.EnrollCommandUrl);
        }

        /// <summary>
        /// Replies to the specified message with the local time.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.TimeCommand)]
        private async Task ReplyTimeAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Get current time and time zone.
            var time = DateTime.Now.ToLocalTime();
            var timeZone = TimeZoneInfo.Local;

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (client.GetRandomNumber(6))
            {
                default:
                    await message.Channel.SendMessageAsync($"My watch says {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 1:
                    await message.Channel.SendMessageAsync($"I have no idea where you live, but my watch says {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 2:
                    await message.Channel.SendMessageAsync($"My current time is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 3:
                    await message.Channel.SendMessageAsync($"My internal clock is telling me it is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 4:
                    await message.Channel.SendMessageAsync($"Just glanced at the Peanuts clock on the wall. It is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 5:
                    await message.Channel.SendMessageAsync($"Beep. Boop. The current local time is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;
            }
        }

        /// <summary>
        /// Replies to the specified message with the bot's platform.
        /// </summary>
        /// <param name="message">The message to reply to.</param>
        [Command(Constants.PlatformCommand)]
        private async Task ReplyPlatformAsync(IUserMessage message)
        {
            // Bot is typing.
            await message.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (client.GetRandomNumber(5))
            {
                default:
                    await message.Channel.SendMessageAsync($"I'm currently living on {RuntimeInformation.FrameworkDescription.Trim()} on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}. Now tell me where *you* live so that I may visit you.");
                    break;

                case 1:
                    await message.Channel.SendMessageAsync($"I call {RuntimeInformation.FrameworkDescription.Trim()} on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} home. What's yours?");
                    break;

                case 2:
                    await message.Channel.SendMessageAsync($"For me, home is on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} running {RuntimeInformation.FrameworkDescription.Trim()}. Where is yours?");
                    break;

                case 3:
                    await message.Channel.SendMessageAsync($"Questions, questions. My home is {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} running {RuntimeInformation.FrameworkDescription.Trim()}.");
                    break;

                case 4:
                    await message.Channel.SendMessageAsync($"I live in a box running {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} and {RuntimeInformation.FrameworkDescription.Trim()}. What's that? You live in one too?");
                    break;
            }
        }

        #endregion
    }
}
