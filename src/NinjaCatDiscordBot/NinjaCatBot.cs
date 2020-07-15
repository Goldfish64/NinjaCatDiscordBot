/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatBot.cs
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
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot {
    /// <summary>
    /// Represents the Ninja Cat bot.
    /// </summary>
    public partial class NinjaCatBot {
        #region Private variables

        private NinjaCatDiscordClient client;
        private Timer timerBuild;

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
        private async Task Start() {
            client = new NinjaCatDiscordClient();

            // Certain things are to be done when the bot joins a guild.
            client.JoinedGuild += async (guild) => {
                // Pause for 5 seconds.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Check to see if this is a bot farm.
                if (await CheckBotGuild(guild))
                    return;

                // Dev began Oct 2. 2016.
            };

            // Log in to Discord. Token is stored in the Credentials class.
            await client.LoginAsync(TokenType.Bot, Credentials.DiscordToken);
            await client.StartAsync();

            // Start checking for new builds.
            timerBuild = new Timer(async (s) => {
                var post = await client.GetLatestBuildPostAsync();

                // Have we ever seen a post yet? This prevents false announcements if the bot has never seen a post before.
                if (string.IsNullOrWhiteSpace(client.CurrentUrl)) {
                    client.CurrentUrl = post.Link;
                    client.SaveSettings();
                    client.LogInfo($"Saved post as new latest build: {post.Link}");
                    return;
                }

                // Is the latest post the same? If so, no need to announce it.
                if (client.CurrentUrl == post.Link)
                    return;

                // Stop timer.
                timerBuild.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
                client.LogInfo($"New build received");

                // Save post.
                client.CurrentUrl = post.Link;
                client.SaveSettings();

                // Get first sentence of post. We'll parse the ring out of this.
                var description = post.Description.ToLowerInvariant().Substring(0, post.Description.ToLowerInvariant().IndexOf(". "));
                if (description == null) {
                    client.LogError($"Post description is blank");
                    return;
                }

                // Send build to guilds.
                foreach (var shard in client.Shards)
                    client.SendNewBuildToShard(shard, post);
                await client.UpdateGameAsync();

                // Restart timer.
                timerBuild.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            // Wait a minute for bot to start up.
            await Task.Delay(TimeSpan.FromMinutes(1));

            // Create thread for updating game.
            var serverCountThread = new Thread(new ThreadStart(async () => {
                while (true) {
                    await client.UpdateGameAsync();
                    await Task.Delay(TimeSpan.FromHours(1));
                }
            }));
            serverCountThread.Start();

            // Wait forever.
            await Task.Delay(-1);
        }

        private async Task<bool> CheckBotGuild(SocketGuild guild) {
            // If the server is the Discord bots server, ignore.
            if (guild.Id == Constants.BotsGuildId)
                return false;

            // Ensure guild is updated.
            if (guild.Users.Count != guild.MemberCount)
                await guild.DownloadUsersAsync();

            // Is this a bot guild?
            if (guild.MemberCount >= 50 && (guild.Users.Count(u => u.IsBot) / (double)guild.MemberCount) >= 0.9) {
                client.LogInfo($"Leaving bot server {guild.Name}");
                try {
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
