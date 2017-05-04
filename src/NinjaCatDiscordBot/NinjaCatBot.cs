/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatBot.cs
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
using Tweetinvi;

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

            // Certain things are to be done when the bot joins a guild.
            client.JoinedGuild += async (guild) =>
            {
                // Update server count.
                await UpdateSiteServerCountAsync();

                // Pause for 5 seconds.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Create variable for speaking channel mention.
                var speakingChannel = string.Empty;

                // Get speaking channel.
                var channel = client.GetSpeakingChannelForSocketGuild(guild);

                // Get current user.
                var user = channel.Guild.CurrentUser;

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
                                $"By default, I'll speak in {speakingChannel}, but you can change it with the **{Constants.CommandPrefix}{Constants.SetChannelCommand}** command.");
                            break;

                        case 1:
                            await channel.SendMessageAsync($"{Constants.AboutMessage2}\n\n" +
                                $"I'll speak in {speakingChannel} by default, but it can be changed with the **{Constants.CommandPrefix}{Constants.SetChannelCommand}** command.");
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

            // Update count on guild leave.
            client.LeftGuild += async (guild) => await UpdateSiteServerCountAsync();

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
                        await msg.Channel.SendMessageAsync($"I'm sorry, but something happened. Error: {result.ErrorReason}");
                    }
                    return;
                }
            };

            // Log in to Discord. Token is stored in the Credentials class.
            await client.LoginAsync(TokenType.Bot, Credentials.DiscordToken);
            await client.StartAsync();

            // Log in to Twitter.
            Auth.SetUserCredentials(Credentials.TwitterConsumerKey, Credentials.TwitterConsumerSecret,
                Credentials.TwitterAccessToken, Credentials.TwitterAccessSecret);

            // Create Twitter stream to follow @donasarkar.
            var donaUser = User.GetUserFromScreenName("donasarkar");
            var stream = Tweetinvi.Stream.CreateFilteredStream();
            stream.AddFollow(donaUser);

#if DEBUG
            // Used for testing tweets.
            var goldfishUser = User.GetUserFromScreenName("goldfishx64");
            stream.AddFollow(goldfishUser);
#endif

            // Listen for incoming tweets from Dona.
            stream.MatchingTweetReceived += async (s, e) =>
            {
                // Get tweet.
                var tweet = e.Tweet.RetweetedTweet ?? e.Tweet;

                // If the tweet is a reply or if it doesn't belong to a known user, ignore it.
                if (tweet.CreatedBy.Id != donaUser.Id || !string.IsNullOrEmpty(tweet.InReplyToScreenName))
                    return;

                // Log tweet.
                client.LogOutput($"TWEET: {tweet.FullText}");

                // Is it a no-build tweet from Dona?
                if (tweet.FullText.ToLowerInvariant().Contains("no build") || tweet.FullText.ToLowerInvariant().Contains("no new build") ||
                    tweet.FullText.ToLowerInvariant().Contains("not releasing") || tweet.FullText.ToLowerInvariant().Contains("not flighting"))
                {
                    // Log tweet.
                    client.LogOutput($"TWEET CONFIRMED: NO BUILDS TODAY");

                    // Send message to guilds.
                    foreach (var shard in client.Shards)
                        SendNoBuildsToShard(shard);
                }
                else
                {
                    // Get build number. If empty, ignore the tweet.
                    var build = Regex.Match(tweet.FullText, @"\d{5,}").Value;
                    if (string.IsNullOrWhiteSpace(build))
                        return;

                    // Try to get a blogs URL.
                    var fullUrl = string.Empty;
                    foreach (var url in tweet.Urls)
                    {
                        // Create the HttpClient.
                        using (var httpClient = new HttpClient())
                        {
                            // Retry up to three times.
                            for (int i = 0; i < 3; i++)
                            {
                                // Get full URL.
                                var tempUrl = url.ExpandedURL;
                                var response = await httpClient.GetAsync(tempUrl);

                                // If the response was a redirect, try again up to 10 times.
                                var count = 10;
                                while ((response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Moved) ||
                                    count < 10)
                                {
                                    tempUrl = response.Headers.Location.ToString();
                                    response = await httpClient.GetAsync(tempUrl);
                                    count++;
                                }

                                // Did the request succeed? If it did, get the URL. Otherwise, log the error and retry.
                                if (response.IsSuccessStatusCode)
                                    fullUrl = tempUrl;
                                else
                                    client.LogOutput($"URLFETCH ERROR: {response.StatusCode}");
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

                    // Log tweet.
                    client.LogOutput($"TWEET CONFIRMED: NEW BUILD");

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
                    if (fullUrl.ToLowerInvariant().Contains("pc") && fullUrl.ToLowerInvariant().Contains("mobile"))
                        platform = " for both PC and Mobile";
                    else if (fullUrl.ToLowerInvariant().Contains("pc"))
                        platform = " for PC";
                    else if (fullUrl.ToLowerInvariant().Contains("mobile"))
                        platform = " for Mobile";

                    // Send build to guilds.
                    foreach (var shard in client.Shards)
                        SendNewBuildToShard(shard, build, ring + platform, fullUrl);
                }
            };

            // Listen for stop.
            stream.StreamStopped += (s, e) =>
            {
                // Log error.
                client.LogOutput($"TWEET STREAM STOPPED: {e.Exception}");
            };

            // Create timer for POSTing server count.
            var serverCountTimer = new Timer(async (e) => await UpdateSiteServerCountAsync(), null, TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));

            // Create timer for game play status of builds.
            var buildPlayTimer = new Timer(async (e) => await client.UpdateGameAsync(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(30));

            // Start the stream.
            stream.StartStreamMatchingAllConditions();
        }

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

        private async void SendNoBuildsToShard(DiscordSocketClient shard)
        {
            // Announce in the specified channel of each guild.
            foreach (var guild in shard.Guilds)
            {
                try
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
                    if (!guild.CurrentUser.GetPermissions(channel).SendMessages)
                    {
                        client.LogOutput($"ROLLING OVER SERVER (NO PERMS) ({shard.ShardId}/{client.Shards.Count - 1}): {guild.Name}");
                        continue;
                    }

                    // Wait a second.
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    // Send typing message.
                    await channel.TriggerTypingAsync();

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
                            await channel.SendMessageAsync($"There won't be any builds today. Maybe tomorrow.:crying_cat_face:");
                            break;
                    }

                    // Log server.
                    client.LogOutput($"SPOKEN IN SERVER ({shard.ShardId}/{client.Shards.Count - 1}): {guild.Name}");
                }
                catch (Exception ex)
                {
                    client.LogOutput($"FAILURE IN SPEAKING FOR {guild.Name} ({shard.ShardId}/{client.Shards.Count - 1}): {ex}");
                }
            }
        }

        private async void SendNewBuildToShard(DiscordSocketClient shard, string build, string type, string url)
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
                var roleText = string.Empty;

                // Does the role exist?
                if (role != null)
                    roleText = $"{role.Mention} ";

                try
                {
                    // Wait a second.
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    // Send typing message.
                    await channel.TriggerTypingAsync();

                    // Pause for realism.
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    // Select and send message.
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
                catch (Exception ex)
                {
                    client.LogOutput($"FAILURE IN SPEAKING FOR {guild.Name} ({shard.ShardId}/{client.Shards.Count - 1}): {ex}");
                }

                // Log server.
                client.LogOutput($"SPOKEN IN SERVER: {guild.Name} ({shard.ShardId}/{client.Shards.Count - 1})");
            }
        }

        #endregion
    }
}
