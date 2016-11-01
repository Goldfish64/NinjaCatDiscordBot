/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatBot.cs
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
using Discord.API.Rest;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tweetinvi;

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Represents the Ninja Cat bot.
    /// </summary>
    public partial class NinjaCatBot
    {
        #region Private variables

        private StreamWriter logStreamWriter;
        private NinjaCatDiscordClient client;

        #endregion

        #region Entry method

        /// <summary>
        /// Main method.
        /// </summary>
        public static void Main(string[] args) => new NinjaCatBot().Start().GetAwaiter().GetResult();

        #endregion

        #region Private methods

        /// <summary>
        /// Starts the bot.
        /// </summary>
        private async Task Start()
        {
            // Open log file.
            logStreamWriter = File.AppendText(Constants.LogFileName);

            // Write startup messages.
            LogOutput($"{Constants.AppName} has started.");
            LogOutput($"===============================================================");

            // Create Discord client.
            client = new NinjaCatDiscordClient();
            client.Log += (message) =>
            {
                // Log the output.
                LogOutput(message.ToString());
                return Task.CompletedTask;
            };

            // Create command service and map.
            var commands = new CommandService();
            var commandMap = new DependencyMap();

            // Add client to map and load commands from assembly.
            commandMap.Add<IDiscordClient>(client);
            await commands.AddModules(Assembly.GetEntryAssembly());

            // Certain things are to be done when the bot joins a guild.
            client.JoinedGuild += async (guild) =>
            {
                // Pause for 5 seconds.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Create variable for speaking channel mention.
                var speakingChannel = string.Empty;

                // Get speaking channel.
                var channel = await client.GetSpeakingChannelForGuildAsync(guild);

                // Get current user.
                var user = await channel.Guild.GetCurrentUserAsync();

                // Does the bot have permission to message? If not return.
                if (!user.GetPermissions(channel).SendMessages)
                    return;

                // Get the mention if speaking is enabled.
                if (channel != null)
                    speakingChannel = channel.Mention;

                // Bot is typing in default channel.
                await channel.TriggerTypingAsync();

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
                            await channel.SendMessageAsync($"{Constants.AboutMessage1}\n\n" +
                                $"By default, I'll speak in {speakingChannel}, but you can change it with the **{Constants.CommandPrefix}{Constants.SettingsSetModule} {Constants.ChannelCommand}** command.");
                            break;

                        case 1:
                            await channel.SendMessageAsync($"{Constants.AboutMessage2}\n\n" +
                                $"I'll speak in {speakingChannel} by default, but it can be changed with the **{Constants.CommandPrefix}{Constants.SettingsSetModule} {Constants.ChannelCommand}** command.");
                            break;
                    }
                }
                else
                {
                    // Select and send message.
                    switch (client.GetRandomNumber(2))
                    {
                        default:
                            await channel.SendMessageAsync(Constants.AboutMessage1);
                            break;

                        case 1:
                            await channel.SendMessageAsync(Constants.AboutMessage2);
                            break;
                    }
                }
            };

            // Listen for messages.
            client.MessageReceived += async (message) =>
            {
                // Get the message and check to see if it is a user message.
                var msg = message as IUserMessage;
                if (msg == null)
                    return;

                // Keeps track of where the command begins.
                var pos = 0;

                // Try to parse a command if only the bot is mentioned.
                if (msg.MentionedUserIds.SingleOrDefault(u => u == client.CurrentUser.Id) == client.CurrentUser.Id)
                {
                    var successResult = ParseResult.FromSuccess(new ReadOnlyCollection<TypeReaderResult>(new List<TypeReaderResult>()), new ReadOnlyCollection<TypeReaderResult>(new List<TypeReaderResult>()));

                    // LINQ stuff from http://stackoverflow.com/a/2912483.
                    if (msg.Content.ToLowerInvariant().Contains(Constants.HelpCommandKeyword))
                    {
                        // Execute the help command and return.
                        await commands.Commands.Single(c => c.Text == Constants.HelpCommand).Execute(new CommandContext(client, msg), successResult, commandMap);
                        return;
                    }
                    //else if (PingCommandKeywords.Any(s => command.Contains(s)))
                    //    await SendPing(user, channel);
                    else if (Constants.TrexCommandKeywords.Any(s => msg.Content.ToLowerInvariant().Contains(s)))
                    {
                        // Execute the trex command and return.
                        await commands.Commands.Single(c => c.Text == Constants.TrexCommand).Execute(new CommandContext(client, msg), successResult, commandMap);
                        return;
                    }
                    else if (Constants.LatestBuildKeywords.Any(s => msg.Content.ToLowerInvariant().Contains(s)))
                    {
                        // Execute the latestbuild command and return.
                        await commands.Commands.Single(c => c.Text == Constants.LatestBuildCommand).Execute(new CommandContext(client, msg), successResult, commandMap);
                        return;
                    }
                    else if (msg.Content.ToLowerInvariant().Contains(Constants.TimeCommandKeyword))
                    {
                        // Execute the time command and return.
                        await commands.Commands.Single(c => c.Text == Constants.TimeCommand).Execute(new CommandContext(client, msg), successResult, commandMap);
                        return;
                    }
                }

                // Attempt to parse a command.
                if (msg.HasStringPrefixLower(Constants.CommandPrefix, ref pos))
                {
                    var result = await commands.Execute(new CommandContext(client, msg), msg.Content.ToLowerInvariant().Substring(pos));
                    if (!result.IsSuccess)
                    {
                        // Bot is typing.
                        await msg.Channel.TriggerTypingAsync();

                        // Pause for realism.
                        await Task.Delay(TimeSpan.FromSeconds(0.75));

                        // Is the command just unknown?
                        if (result.Error == CommandError.UnknownCommand)
                            await msg.Channel.SendMessageAsync($"I'm sorry, but I don't know what that means. Type **{Constants.CommandPrefix}{Constants.HelpCommand}** for help.");
                        else
                            await msg.Channel.SendMessageAsync($"I'm sorry, but something happened. Error: {result.ErrorReason}");
                    }
                    return;
                }
            };

            // Log in to Discord. Token is stored in the Credentials class.
            await client.LoginAsync(TokenType.Bot, Credentials.DiscordToken);
            await client.ConnectAsync();

            // Set game.
            await client.SetGame("on Windows 10");

            // Log in to Twitter.
            Auth.SetUserCredentials(Credentials.TwitterConsumerKey, Credentials.TwitterConsumerSecret,
                Credentials.TwitterAccessToken, Credentials.TwitterAccessSecret);

            // Create Twitter stream to follow @donasarkar.
            var donaUser = User.GetUserFromScreenName("donasarkar");
            var insiderUser = User.GetUserFromScreenName("windowsinsider");
            var stream = Tweetinvi.Stream.CreateFilteredStream();
            stream.AddFollow(donaUser);
            stream.AddFollow(insiderUser);

#if DEBUG
            // Used for testing tweets.
            var goldfishUser = User.GetUserFromScreenName("goldfishx64");
            stream.AddFollow(goldfishUser);
#endif

            // Listen for incoming tweets from Dona.
            stream.MatchingTweetReceived += async (s, e) =>
            {
                // Get tweet.
#if DEBUG
                var tweet = e.Tweet.RetweetedTweet ?? e.Tweet;
#else
                var tweet = e.Tweet;
#endif

                // If the tweet is a reply or if it doesn't belong to a known user, ignore it.
                if (!(tweet.CreatedBy.Id == donaUser.Id || tweet.CreatedBy.Id == insiderUser.Id || string.IsNullOrEmpty(tweet.InReplyToScreenName)))
                    return;

                // Is it a no-build tweet from Dona?
                if (tweet.FullText.ToLowerInvariant().Contains("no build") || tweet.FullText.ToLowerInvariant().Contains("no new build") ||
                    tweet.FullText.ToLowerInvariant().Contains("not releasing") || tweet.FullText.ToLowerInvariant().Contains("not flighting"))
                {
                    // Bot is typing.
                    await TriggerTypingInAllGuilds();

                    // Log tweet.
                    LogOutput($"TWEET: {tweet.FullText}");

                    // Announce in the specified channel of each guild.
                    foreach (var guild in client.Guilds)
                    {
                        // Get channel.
                        var channel = await client.GetSpeakingChannelForGuildAsync(guild);

                        // If the channel is null, continue on to the next guild.
                        if (channel == null)
                            continue;

                        // Pause for realism.
                        await Task.Delay(TimeSpan.FromSeconds(1));

                        // Select and send message.
                        switch (client.GetRandomNumber(3))
                        {
                            default:
                                await channel.SendMessageAsync($"I've just received word that there won't be any builds today. Bummer. :crying_cat_face:");
                                break;

                            case 1:
                                await channel.SendMessageAsync($"Aww. No builds today. :crying_cat_face:");
                                break;

                            case 2:
                                await channel.SendMessageAsync($"There won't be any builds today. Maybe tommorrow.:crying_cat_face:");
                                break;
                        }
                    }
                }
                // Is the tweet from windowsinsider?
                else if (tweet.CreatedBy.Id == insiderUser.Id)
                {
                    // Get build number. If empty, ignore the tweet.
                    var build = Regex.Match(tweet.FullText, @"\d{5,}").Value;
                    if (string.IsNullOrWhiteSpace(build))
                        return;

                    // Try to get a blogs URL.
                    var fullUrl = string.Empty;
                    foreach (var url in tweet.Urls)
                    {
                        // Encode URL for transport.
                        var tempUrl = WebUtility.UrlEncode(url.ExpandedURL);

                        // Create the HttpClient.
                        using (var httpClient = new HttpClient())
                        {
                            // Configure the HttpClient to use https://lengthenurl.info/.
                            httpClient.BaseAddress = new Uri("https://lengthenurl.info/");
                            httpClient.DefaultRequestHeaders.Accept.Clear();
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            // Retry up to three times.
                            for (int i = 0; i < 3; i++)
                            {
                                // Send the request with the short URL and get the response back containing the long URL.
                                var response = await httpClient.GetAsync($"api/longurl/shorturl/?inputURL={tempUrl}");

                                // Did the request succeed? If it did, get the URL. Otherwise, log the error and retry.
                                if (response.IsSuccessStatusCode)
                                    fullUrl = (await response.Content.ReadAsAsync<ServicedUrl>()).LongURL.ToLowerInvariant();
                                else
                                    LogOutput($"URLFETCH ERROR: {response.StatusCode}");
                            }
                        }

                        // Check to see if URL has what it takes.
                        if (!string.IsNullOrEmpty(fullUrl) && fullUrl.Contains("blogs.windows.com/windowsexperience") && fullUrl.Contains("insider-preview-build"))
                            break;

                        // Clear URL.
                        fullUrl = string.Empty;
                    }

                    // If URL is invalid, return.
                    if (string.IsNullOrWhiteSpace(fullUrl))
                        return;

                    // Bot is typing.
                    await TriggerTypingInAllGuilds();

                    // Log tweet.
                    LogOutput($"TWEET: {tweet.FullText}");

                    // Create variables.
                    var ring = string.Empty;
                    var platform = string.Empty;

                    // Check for fast or slow, or both.
                    if (tweet.FullText.ToLowerInvariant().Contains("fast") && tweet.FullText.ToLowerInvariant().Contains("slow"))
                        ring = " to both the Fast and Slow rings";
                    else if (tweet.FullText.ToLowerInvariant().Contains("fast"))
                        ring = " to the Fast ring";
                    else if (tweet.FullText.ToLowerInvariant().Contains("slow"))
                        ring = " to the Slow ring";

                    // Check for PC or mobile, or both.
                    if (tweet.FullText.ToLowerInvariant().Contains("pc") && tweet.FullText.ToLowerInvariant().Contains("mobile"))
                        platform = " for both PC and Mobile";
                    else if (tweet.FullText.ToLowerInvariant().Contains("pc"))
                        platform = " for PC";
                    else if (tweet.FullText.ToLowerInvariant().Contains("mobile"))
                        platform = " for Mobile";

                    // Announce in the specified channel of each guild.
                    foreach (var guild in client.Guilds)
                    {
                        // Get channel.
                        var channel = await client.GetSpeakingChannelForGuildAsync(guild);

                        // If the channel is null, continue on to the next guild.
                        if (channel == null)
                            continue;

                        // Pause for realism.
                        await Task.Delay(TimeSpan.FromSeconds(1));

                        // Select and send message.
                        switch (client.GetRandomNumber(3))
                        {
                            default:
                                await channel.SendMessageAsync($"Yay! Windows 10 Insider Preview Build {build} has just been released{ring}{platform}! :mailbox_with_mail: :smiley_cat:\n{fullUrl}");
                                break;

                            case 1:
                                await channel.SendMessageAsync($"Windows 10 Insider Preview Build {build} has just been released{ring}{platform}! Yes! :mailbox_with_mail: :smiley_cat:\n{fullUrl}");
                                break;

                            case 2:
                                await channel.SendMessageAsync($"Better check for updates now! Windows 10 Insider Preview Build {build} has just been released{ring}{platform}! :mailbox_with_mail: :smiley_cat:\n{fullUrl}");
                                break;
                        }
                    }
                }
            };

            // Listen for stop.
            stream.StreamStopped += (s, e) => LogOutput($"STREAM STOPPED: {e.Exception}");

            // Start the stream.
            stream.StartStreamMatchingAllConditions();

            // Log stop.
            LogOutput("Bot is stopping...");
        }

        /// <summary>
        /// Logs the specified information to the console and logfile.
        /// </summary>
        /// <param name="info">The information to log.</param>
        private void LogOutput(string info)
        {
            // Get current time and date.
            var timeDate = DateTime.Now;

            // Write to console and logfile.
            Console.WriteLine($"{timeDate}: {info}");
            logStreamWriter.WriteLine($"{timeDate}: {info}");
            logStreamWriter.Flush();
        }

        /// <summary>
        /// Triggers the typing message in all guilds.
        /// </summary>
        private async Task TriggerTypingInAllGuilds()
        {
            // Go through each guild the bot is a part of.
            foreach (var guild in client.Guilds)
            {
                // Get channel.
                var channel = await client.GetSpeakingChannelForGuildAsync(guild);

                // If the channel is null, continue on to the next guild.
                if (channel == null)
                    continue;

                // Send typing message.
                await channel.TriggerTypingAsync();
            }
        }

        #endregion
    }
}
