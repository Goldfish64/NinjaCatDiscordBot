/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: Program.cs
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tweetinvi;

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Represents the program.
    /// </summary>
    public partial class Program
    {
        #region Constants

        private const string AppName = "Ninja Cat Bot";
        private const string AppUrl = "https://github.com/Goldfish64/NinjaCatDiscordBot";
        private const string UserName = "NinjaCat";
        private const string Nickname = "Ninja Cat";

        private const char CommandPrefixChar = '$';
        private const string CommandPrefix = "ninjacat-";
        private const string CommandPrefixShort = "nj-";
        private readonly string CommandHelpPrefix = CommandPrefixChar + CommandPrefix;
        private readonly string CommandHelpPrefixShort = CommandPrefixChar + CommandPrefixShort;

        private const string AboutCommand = "about";
        private const string AboutCommandDesc = "get to know me";
        private const string HelpCommand = "help";
        private const string HelpCommandDesc = "get help";
        private const string HelpCommandKeyword = "help";
        private const string PingCommand = "ping";
        private const string PingCommandDesc = "pong";
        private readonly string[] PingCommandKeywords = { "ping", "pong", "ping-pong" };
        private const string TrexCommand = "trex";
        private const string TrexCommandDesc = "shows the Windows 10 Skype emoticon";
        private const string TrexCommandUrl = "http://static.skaip.su/img/emoticons/180x180/f6fcff/win10.gif";
        private readonly string[] TrexCommandKeywords = { "trex", "t-rex" };
        private const string LatestBuildCommand = "latestbuild";
        private const string LatestBuildCommandDesc = "gets the latest Insider build";
        private readonly string[] LatestBuildKeywords = { "latest build", "latest insider build", "latest windows 10 build", "latest windows build", "latest insider preview build" };
        private const string TimeCommand = "time";
        private const string TimeCommandDesc = "shows the current time";
        private const string TimeCommandKeyword = "time";
        private const string PlatformCommand = "platform";
        private const string PlatformCommandDesc = "shows where I live";

        #endregion

        #region Private variables

        private DiscordClient client;
        private Random random = new Random();

        #endregion

        #region Entry method

        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        #endregion

        #region Private methods

        /// <summary>
        /// Starts the bot.
        /// </summary>
        private async Task Start()
        {
            // Create Discord client.
            client = new DiscordClient(x =>
            {
                x.AppName = AppName;
                x.AppUrl = AppUrl;
                x.MessageCacheSize = 0;
                x.LogLevel = LogSeverity.Error;
                x.LogHandler = Log;
            })
            .UsingCommands(x =>
            {
                x.AllowMentionPrefix = false;
                x.PrefixChar = CommandPrefixChar;
                x.ErrorHandler = OnCommandError;
            });

            // Listen for the messages.
            client.MessageReceived += async (s, e) =>
            {
                // Is the bot the only mentioned user?
                if (e.Message.IsMentioningMe() && e.Message.MentionedUsers.Count() == 1)
                    await ParseFriendlyCommands(e.Message.Text.ToLowerInvariant(), e.User, e.Channel);
            };

            // Set nickname when bot joins a server.
            client.ServerAvailable += async (s, e) => await e.Server.CurrentUser.Edit(nickname: Nickname);

            // Register the about command.
            client.GetService<CommandService>().CreateCommand(CommandPrefix + AboutCommand).Alias(new string[] { CommandPrefixShort + AboutCommand })
                .Description(AboutCommandDesc).Do(async e => await SendAbout(e.Channel));

            // Register the help command.
            client.GetService<CommandService>().CreateCommand(CommandPrefix + HelpCommand).Alias(new string[] { CommandPrefixShort + HelpCommand })
                .Description(HelpCommandDesc).Do(async e => await SendHelp(e.Channel));

            // Register the ping command.
            client.GetService<CommandService>().CreateCommand(CommandPrefix + PingCommand).Alias(new string[] { CommandPrefixShort + PingCommand })
                .Description(PingCommandDesc).Do(async e => await SendPing(e.User, e.Channel));

            // Register the trex command.
            client.GetService<CommandService>().CreateCommand(CommandPrefix + TrexCommand).Alias(new string[] { CommandPrefixShort + TrexCommand })
                .Description(TrexCommandDesc).Do(async e => await SendTrex(e.Channel));

            // Register the latest command.
            client.GetService<CommandService>().CreateCommand(CommandPrefix + LatestBuildCommand).Alias(new string[] { CommandPrefixShort + LatestBuildCommand })
                .Description(LatestBuildCommandDesc).Do(async e => await SendLatestBuild(e.Channel));

            // Register the time command.
            client.GetService<CommandService>().CreateCommand(CommandPrefix + TimeCommand).Alias(new string[] { CommandPrefixShort + TimeCommand })
                .Description(TimeCommandDesc).Do(async e => await SendTime(e.Channel));

            // Register the platform command.
            client.GetService<CommandService>().CreateCommand(CommandPrefix + PlatformCommand).Alias(new string[] { CommandPrefixShort + PlatformCommand })
                .Description(PlatformCommandDesc).Do(async e => await SendPlatform(e.Channel));

            // Try to connect to Discord until connected.
            while (true)
            {
                try
                {
                    // Connect to Discord.
                    await client.Connect(DiscordToken, TokenType.Bot);
                    client.SetGame("on Windows 10");
                    break;
                }
                catch (Exception ex)
                {
                    // Disconnect.
                    await client.Disconnect();

                    // Log error and wait to retry.
                    client.Log.Error("Login could not be completed.", ex);
                    await Task.Delay(client.Config.FailedReconnectDelay);
                }
            }

            // Log in to Twitter.
            Auth.SetUserCredentials(TwitterConsumerKey, TwitterConsumerSecret, TwitterAccessToken, TwitterAccessSecret);

            // Create Twitter stream to follow @donasarkar.
            var donaUser = Tweetinvi.User.GetUserFromScreenName("donasarkar");
            var stream = Stream.CreateFilteredStream();
            stream.AddFollow(donaUser);

            // Listen for incoming tweets.
            stream.MatchingTweetReceived += async (s, e) =>
            {
                // If the tweet is a reply or if it doesn't belong to Dona, ignore it.
                if (e.Tweet.CreatedBy.Id != donaUser.Id || !string.IsNullOrEmpty(e.Tweet.InReplyToScreenName))
                    return;

                // Is it a no-build tweet?
                if (e.Tweet.FullText.ToLowerInvariant().Contains("no build") || e.Tweet.FullText.ToLowerInvariant().Contains("no new build"))
                {
                    // Bot is typing.
                    foreach (var server in client.Servers)
                        await server.DefaultChannel.SendIsTyping();

                    // Write tweet to console.
                    Console.WriteLine("TWEET: " + e.Tweet.FullText + "\n");

                    // Send a message to the default channel in each server.
                    foreach (var server in client.Servers)
                        await server.DefaultChannel.SendMessage($"I've just received word that there won't be any builds today. Bummer. :crying_cat_face:");
                }
                // Is it a new build tweet (yay!)? It must contain a 5 or more digit number and the blogs URL.
                else if (e.Tweet.FullText.Count(c => char.IsDigit(c)) >= 5 && e.Tweet.Urls.Count(i => i.ExpandedURL.Contains("blogs.windows.com/windowsexperience") && i.ExpandedURL.Contains("insider-preview-build")) > 0)
                {
                    // Bot is typing.
                    foreach (var server in client.Servers)
                        await server.DefaultChannel.SendIsTyping();

                    // Write tweet to console.
                    Console.WriteLine("TWEET: " + e.Tweet.FullText + "\n");

                    // Get the build number.
                    var build = Regex.Match(e.Tweet.FullText, @"\d{4,}").Value;

                    // Send a message to the default channel in each server.
                    foreach (var server in client.Servers)
                        await server.DefaultChannel.SendMessage(!string.IsNullOrWhiteSpace(build) ?
                            $"Yay! A new Windows 10 Insider build has just been released! If my sources are correct, it is {build}. :mailbox_with_mail: :smiley_cat:\n{e.Tweet.Url}" :
                            $"Yay! A new Windows 10 Insider build has just been released! I'm not entirely sure which one though. :mailbox_with_mail: :smiley_cat:\n{e.Tweet.Url}");
                }
            };

            // Start the stream.
            await stream.StartStreamMatchingAllConditionsAsync();
        }

        /// <summary>
        /// Logs errors.
        /// </summary>
        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine("DISCORD: " + e.Exception ?? e.Message);
        }

        /// <summary>
        /// Handles command errors.
        /// </summary>
        private async void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            // If the command doesn't begin with the prefix and "nj" or "ninjacat", ignore it.

            // Get error message. If it is valid, send it.
            var message = e.Exception?.Message;
            if (!string.IsNullOrEmpty(message))
            {
                await e.Channel.SendMessage($"Error: {message}");
                client.Log.Error("Command", message);
                return;
            }

            // Show a generic error message.
            switch (e.ErrorType)
            {
                case CommandErrorType.Exception:
                    await e.Channel.SendMessage("I'm sorry, but an unknown error happened.");
                    break;
                case CommandErrorType.UnknownCommand:
                    await e.Channel.SendMessage($"I'm sorry, but I don't know what that means. Type **{CommandHelpPrefix}{HelpCommand}** or **{CommandHelpPrefixShort}{HelpCommand}** for help.");
                    break;
            }
        }

        /// <summary>
        /// Sends the about message to the specified channel.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to send the message to.</param>
        private async Task SendAbout(Channel channel)
        {
            // Bot is typing.
            await channel.SendIsTyping();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (random.Next(3))
            {
                default:
                    await channel.SendMessage(
                        $"Hi there! I am {Nickname}, a Discord.Net bot!\n" +
                        $"I was created on October 2, 2016 by <@191330317439598593> with the purpose of letting you know about the latest in Windows Insider builds, but I can do other things too.\n\n" +
                        $"For help on what I can do, type **{CommandHelpPrefix}{HelpCommand}** or **{CommandHelpPrefixShort}{HelpCommand}**.");
                    break;

                case 1:
                    await channel.SendMessage(
                        $"Greetings! I am the {Nickname}, a bot built using the Discord.Net library!\n" +
                        $"I was activated on October 2, 2016 by <@191330317439598593> with the purpose of letting you know about the latest in Windows Insider builds, but I can do other things too.\n\n" +
                        $"Your wish is my command, so type **{CommandHelpPrefix}{HelpCommand}** or **{CommandHelpPrefixShort}{HelpCommand}** for info on what I can do for you.");
                    break;

                case 2:
                    await channel.SendMessage(
                        $"__**EXPERIMENT #{client.CurrentUser.Discriminator.ToString("D4")} ACTIVATED**__\n\n" +
                        $"**EXPERIMENT NAME**: {Nickname.ToUpperInvariant()}\n" +
                        $"**PRIMARY FUNCTION**: WAITS FOR BUILDS FROM THE WINDOWS INSIDER MASTER, DONA SARKAR\n" +
                        $"**CREATOR**: <@191330317439598593>\n" +
                        $"**ACTIVATION DATE**: 10/2/2016\n" +
                        $"**ACTIVATION LOCATION**: UNKNOWN\n" +
                        $"**ACTIVATION REASON**: UNKNOWN\n" +
                        $"**PROGRAMMED IN**: C#, ALONG WITH THE DISCORD.NET AND TWEETINVI LIBRARIES\n\n" +
                        $"FOR HELP, TYPE **{CommandHelpPrefix}{HelpCommand.ToUpperInvariant()}** or **{CommandHelpPrefixShort}{HelpCommand.ToUpperInvariant()}**");
                    break;
            }
        }

        /// <summary>
        /// Sends help to the specified channel.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to send help to.</param>
        private async Task SendHelp(Channel channel)
        {
            // Bot is typing.
            await channel.SendIsTyping();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (random.Next(2))
            {
                default:
                    await channel.SendMessage($"So you need help huh? You've come to the right place.\n\n" +
                        $"My commands are:\n" +
                        $"**{CommandHelpPrefix}{AboutCommand}** or **{CommandHelpPrefixShort}{AboutCommand}**: {AboutCommandDesc}. :cat:\n" +
                        $"**{CommandHelpPrefix}{HelpCommand}** or **{CommandHelpPrefixShort}{HelpCommand}**: {HelpCommandDesc}. :question:\n" +
                        $"**{CommandHelpPrefix}{PingCommand}** or **{CommandHelpPrefixShort}{PingCommand}**: {PingCommandDesc}. :ping_pong:\n" +
                        $"**{CommandHelpPrefix}{TrexCommand}** or **{CommandHelpPrefixShort}{TrexCommand}**: {TrexCommandDesc}. :gift:\n" +
                        $"**{CommandHelpPrefix}{LatestBuildCommand}** or **{CommandHelpPrefixShort}{LatestBuildCommand}**: {LatestBuildCommandDesc}. :mailbox_with_mail:\n" +
                        $"**{CommandHelpPrefix}{TimeCommand}** or **{CommandHelpPrefixShort}{TimeCommand}**: {TimeCommandDesc}. :alarm_clock:\n" +
                        $"**{CommandHelpPrefix}{PlatformCommand}** or **{CommandHelpPrefixShort}{PlatformCommand}**: {PlatformCommandDesc}. :house:\n\n" +
                        $"If you mention me and include a command, I'll usually respond in some fashion.");
                    break;

                case 1:
                    await channel.SendMessage($"You need help? Why didn't you just say so?\n\n" +
                        $"My set of commands include:\n" +
                        $"**{CommandHelpPrefix}{AboutCommand}** or **{CommandHelpPrefixShort}{AboutCommand}**: {AboutCommandDesc}. :cat:\n" +
                        $"**{CommandHelpPrefix}{HelpCommand}** or **{CommandHelpPrefixShort}{HelpCommand}**: {HelpCommandDesc}. :question:\n" +
                        $"**{CommandHelpPrefix}{PingCommand}** or **{CommandHelpPrefixShort}{PingCommand}**: {PingCommandDesc}. :ping_pong:\n" +
                        $"**{CommandHelpPrefix}{TrexCommand}** or **{CommandHelpPrefixShort}{TrexCommand}**: {TrexCommandDesc}. :gift:\n" +
                        $"**{CommandHelpPrefix}{LatestBuildCommand}** or **{CommandHelpPrefixShort}{LatestBuildCommand}**: {LatestBuildCommandDesc}. :mailbox_with_mail:\n" +
                        $"**{CommandHelpPrefix}{TimeCommand}** or **{CommandHelpPrefixShort}{TimeCommand}**: {TimeCommandDesc}. :alarm_clock:\n" +
                        $"**{CommandHelpPrefix}{PlatformCommand}** or **{CommandHelpPrefixShort}{PlatformCommand}**: {PlatformCommandDesc}. :house:\n\n" +
                        $"If you mention me with a command, I might get back to you.");
                    break;
            }
        }

        /// <summary>
        /// Sends a ping/pong to the specified channel.
        /// </summary>
        /// <param name="user">The <see cref="Discord.User"/> that requested the ping.</param>
        /// <param name="channel">The <see cref="Channel"/> to return the ping to.</param>
        /// <returns></returns>
        private async Task SendPing(Discord.User user, Channel channel)
        {
            // Bot is typing.
            await channel.SendIsTyping();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (random.Next(5))
            {
                default:
                    await channel.SendMessage($"Pong {user.Mention}! :ping_pong:");
                    break;

                case 0:
                    await channel.SendMessage($"I'm very good at ping pong {user.Mention}. :ping_pong:");
                    break;

                case 1:
                    await channel.SendMessage($"I know where you live {user.Mention}. :smirk_cat: :house:");
                    break;

                case 2:
                    await channel.SendMessage($"You know, I can do other things besides play ping-pong {user.Mention}. :ping_pong:");
                    break;

                case 3:
                    await channel.SendMessage($"Why, {user.Mention}. Are you lonely?");
                    break;

                case 4:
                    await channel.SendMessage($"Remember, {user.Mention}, I am always here. :slight_smile:");
                    break;
            }
        }

        /// <summary>
        /// Sends the T-Rex to the specified channel.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to send the T-Rex to.</param>
        private async Task SendTrex(Channel channel)
        {
            // Bot is typing.
            await channel.SendIsTyping();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message with link.
            switch (random.Next(3))
            {
                default:
                    await channel.SendMessage($"Here you go.\n{TrexCommandUrl}");
                    break;

                case 1:
                    await channel.SendMessage($"ROAAAAR!\n{TrexCommandUrl}");
                    break;

                case 2:
                    await channel.SendMessage($"Here I am riding the T-Rex!\n{TrexCommandUrl}");
                    break;
            }
        }

        /// <summary>
        /// Sends the latest Insider build to the specified channel.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to send the build to.</param>
        private async Task SendLatestBuild(Channel channel)
        {
            // Bot is typing.
            await channel.SendIsTyping();

            // Create the HttpClient.
            using (var httpClient = new HttpClient())
            {
                // Get the latest build list containing the newest 50 builds from BuildFeed.
                var response = await httpClient.GetStringAsync("https://buildfeed.net/api/GetBuilds?limit=50");

                // Parse JSON and get the latest public build.
                var builds = JArray.Parse(response).ToList();
                var newestBuild = builds.First(b => (int)b["SourceType"] == 0 && b["SourceDetails"].ToString().Contains("insider"));

                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Select and send message.
                switch (random.Next(4))
                {
                    default:
                        await channel.SendMessage($"I've got the latest Insider build for you. It is **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}**. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 1:
                        await channel.SendMessage($"Ask and you shall receive. The latest Insider build is **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}**. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 2:
                        await channel.SendMessage($"Yes master. Right away master. **{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}** is the latest and greatest. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;

                    case 3:
                        await channel.SendMessage($"**{newestBuild["MajorVersion"]}.{newestBuild["MinorVersion"]}.{newestBuild["Number"]}.{newestBuild["Revision"]}** is the newest Insider build according to my sources. :cat:\nhttps://buildfeed.net/build/{newestBuild["Id"]}");
                        break;
                }
            }
        }

        /// <summary>
        /// Sends the local time to the specified channel.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to send the time to.</param>
        private async Task SendTime(Channel channel)
        {
            // Bot is typing.
            await channel.SendIsTyping();

            // Get current time and time zone.
            var time = DateTime.Now.ToLocalTime();
            var timeZone = TimeZoneInfo.Local;

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (random.Next(6))
            {
                default:
                    await channel.SendMessage($"My watch says {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 1:
                    await channel.SendMessage($"I have no idea where you live, but my watch says {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 2:
                    await channel.SendMessage($"My current time is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 3:
                    await channel.SendMessage($"My internal clock is telling me it is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 4:
                    await channel.SendMessage($"Just glanced at the Peanuts clock on the wall. It is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;

                case 5:
                    await channel.SendMessage($"Beep. Boop. The current local time is {time.ToString("T")} {(timeZone.IsDaylightSavingTime(time) ? timeZone.DaylightName : timeZone.StandardName)}.");
                    break;
            }
        }

        /// <summary>
        /// Sends information about the platform the bot is running on to the specified channel.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to send the information to.</param>
        private async Task SendPlatform(Channel channel)
        {
            // Bot is typing.
            await channel.SendIsTyping();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Select and send message.
            switch (random.Next(5))
            {
                default:
                    await channel.SendMessage($"I'm currently living on {RuntimeInformation.FrameworkDescription.Trim()} on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}. Now tell me where *you* live so that I may visit you.");
                    break;

                case 1:
                    await channel.SendMessage($"I call {RuntimeInformation.FrameworkDescription.Trim()} on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} home. What's yours?");
                    break;

                case 2:
                    await channel.SendMessage($"For me, home is on {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} running {RuntimeInformation.FrameworkDescription.Trim()}. Where is yours?");
                    break;

                case 3:
                    await channel.SendMessage($"Questions, questions. My home is {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} running {RuntimeInformation.FrameworkDescription.Trim()}.");
                    break;

                case 4:
                    await channel.SendMessage($"I live in a box running {RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()} and {RuntimeInformation.FrameworkDescription.Trim()}. What's that? You live in one too?");
                    break;
            }
        }

        /// <summary>
        /// Parses a friendly command.
        /// </summary>
        /// <param name="command">The command to parse.</param>
        /// <param name="user">The <see cref="Discord.User"/> that sent the command.</param>
        /// <param name="channel">The <see cref="Channel"/> that the command was sent on.</param>
        private async Task ParseFriendlyCommands(string command, Discord.User user, Channel channel)
        {
            // Try to parse a command.
            // LINQ stuff from http://stackoverflow.com/a/2912483.
            if (command.Contains(HelpCommandKeyword))
                await SendHelp(channel);
            else if (PingCommandKeywords.Any(s => command.Contains(s)))
                await SendPing(user, channel);
            else if (TrexCommandKeywords.Any(s => command.Contains(s)))
                await SendTrex(channel);
            else if (LatestBuildKeywords.Any(s => command.Contains(s)))
                await SendLatestBuild(channel);
            else if (command.Contains(TimeCommandKeyword))
                await SendTime(channel);
        }

        #endregion
    }
}
