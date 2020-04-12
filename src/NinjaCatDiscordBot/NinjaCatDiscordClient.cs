/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatDiscordClient.cs
* 
* Copyright (c) 2016-2019 John Davis
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
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NinjaCatDiscordBot {
    /// <summary>
    /// Represents the bot settings.
    /// </summary>
    public class NinjaCatSettings {
        /// <summary>
        /// Gets the list of Insider channels.
        /// </summary>
        /// <remarks>Guild is the key, channel is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> InsiderChannels { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the list of Insider roles.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> InsiderRolesPrimary { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the list of Insider roles for skip ahead.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> InsiderRolesSkip { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the list of Insider roles for slow.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> InsiderRolesSlow { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the list of jumbo roles.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> JumboRoles { get; } = new ConcurrentDictionary<ulong, ulong>();
    }

    /// <summary>
    /// Represents a <see cref="DiscordShardedClient"/> with additional properties.
    /// </summary>
    public sealed class NinjaCatDiscordClient : DiscordShardedClient {
        #region Private variables

        // private StreamWriter logStreamWriter;
        private Random random = new Random();
        private object lockObject = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjaCatDiscordClient"/> class.
        /// </summary>
        public NinjaCatDiscordClient() : base(new DiscordSocketConfig() { TotalShards = Constants.ShardCount }) {
            // Write startup messages.
            LogInfo($"{Constants.AppName} on {RuntimeInformation.FrameworkDescription} has started.");
            LogInfo($"===============================================================");

            // Listen for events.
            Log += (message) => {
                // Log the output.
                LogInfo(message.ToString());
                return Task.CompletedTask;
            };

            // Get latest post URL, if there is one.
            if (File.Exists(Constants.LatestPostFileName))
                CurrentUrl = File.ReadAllText(Constants.LatestPostFileName);

            if (File.Exists(Constants.SettingsFileName))
                Settings = JsonConvert.DeserializeObject<NinjaCatSettings>(File.ReadAllText(Constants.SettingsFileName));
        }

        #endregion

        #region Properties

        public NinjaCatSettings Settings;

        /// <summary>
        /// Gets the time the client started.
        /// </summary>
        public DateTime StartTime { get; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the current post URL. Used for keeping track of new posts.
        /// </summary>
        public string CurrentUrl { get; set; } = "";

        #endregion

        #region Methods

        /// <summary>
        /// Gets a random number.
        /// </summary>
        /// <param name="maxValue">The maximum value of the number generated.</param>
        /// <returns>The random number.</returns>
        public int GetRandomNumber(int maxValue) {
            // Return a random number.
            return random.Next(maxValue);
        }

        /// <summary>
        /// Gets the speaking channel for the specified guild.
        /// </summary>
        /// <param name="guild">The <see cref="SocketGuild"/> to get the channel for.</param>
        /// <returns>An <see cref="SocketTextChannel"/> that should be used.</returns>
        public SocketTextChannel GetSpeakingChannelForSocketGuild(SocketGuild guild) {
            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
                return null;

            // Create channel variable.
            SocketTextChannel channel = null;

            // Try to get the saved channel.
            if (Settings.InsiderChannels.ContainsKey(guild.Id)) {
                // If it is zero, return null to not speak.
                if (Settings.InsiderChannels[guild.Id] == 0)
                    return null;
                else
                    channel = guild.Channels.SingleOrDefault(g => g.Id == Settings.InsiderChannels[guild.Id]) as SocketTextChannel;
            }

            // If the channel is null, delete the entry from the dictionary and use the default one.
            if (channel == null) {
                Settings.InsiderChannels.TryRemove(guild.Id, out ulong outVar);
                channel = guild.DefaultChannel;
                SaveSettings();
            }

            // Return the channel.
            return channel;
        }

        /// <summary>
        /// Gets the speaking channel for the specified guild.
        /// </summary>
        /// <param name="guild">The <see cref="IGuild"/> to get the channel for.</param>
        /// <returns>An <see cref="SocketTextChannel"/> that should be used.</returns>
        public async Task<ITextChannel> GetSpeakingChannelForIGuildAsync(IGuild guild) {
            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
                return null;

            // Create channel variable.
            ITextChannel channel = null;

            // Try to get the saved channel.
            if (Settings.InsiderChannels.ContainsKey(guild.Id)) {
                // If it is zero, return null to not speak.
                if (Settings.InsiderChannels[guild.Id] == 0)
                    return null;
                else
                    channel = (await guild.GetChannelsAsync()).SingleOrDefault(g => g.Id == Settings.InsiderChannels[guild.Id]) as ITextChannel;
            }

            // If the channel is null, delete the entry from the dictionary and use the default one.
            if (channel == null) {
                Settings.InsiderChannels.TryRemove(guild.Id, out ulong outVar);
                channel = (await guild.GetChannelsAsync()).SingleOrDefault(g => g.Id == guild.DefaultChannelId) as ITextChannel;
                SaveSettings();
            }

            // Return the channel.
            return channel;
        }

        /// <summary>
        /// Gets the desired role for the specified guild.
        /// </summary>
        /// <param name="guild">The <see cref="IGuild"/> to get the role for.</param>
        /// <returns>An <see cref="IRole"/> that should be used.</returns>
        public IRole GetRoleForIGuild(IGuild guild, RoleType type) {
            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
                return null;

            var roles = Settings.InsiderRolesPrimary;
            switch (type) {
                case RoleType.InsiderPrimary:
                    break;

                case RoleType.InsiderSlow:
                    roles = Settings.InsiderRolesSlow;
                    break;

                case RoleType.InsiderSkip:
                    roles = Settings.InsiderRolesSkip;
                    break;

                case RoleType.Jumbo:
                    roles = Settings.JumboRoles;
                    break;

                default:
                    return null;
            }

            IRole role = null;
            if (roles.ContainsKey(guild.Id)) {
                // If it is zero, return null to not speak.
                if (roles[guild.Id] == 0)
                    return null;
                else
                    role = guild.Roles.SingleOrDefault(g => g.Id == roles[guild.Id]) as IRole;
            }

            // If the role is null, delete the entry from the dictionary and use the default one.
            if (role == null) {
                roles.TryRemove(guild.Id, out ulong outVar);
                SaveSettings();
            }
            return role;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void SaveSettings() {
            lock (lockObject) {
                File.WriteAllText(Constants.LatestPostFileName, CurrentUrl);
                File.WriteAllText(Constants.SettingsFileName, JsonConvert.SerializeObject(Settings));
            }
        }

        /// <summary>
        /// Logs the specified error to the console and logfile.
        /// </summary>
        /// <param name="info">The information to log.</param>
        public void LogError(string info) {
            Console.WriteLine($"ERROR: {DateTime.Now}: {info}");
        }

        /// <summary>
        /// Logs the specified information to the console and logfile.
        /// </summary>
        /// <param name="info">The information to log.</param>
        public void LogInfo(string info) {
            Console.WriteLine($"INFO: {DateTime.Now}: {info}");
        }

        /// <summary>
        /// Gets the latest build of the specified type.
        /// </summary>
        /// <param name="type">The type of build to get.</param>
        public async Task<Tuple<string, string, BuildType>> GetLatestBuildNumberAsync(BuildType type = BuildType.NormalPc) {
            // Create HTTP client.
            var client = new HttpClient();

            // Get most recent build post..
            BlogEntry post = null;
            for (int page = 1; page <= 10; page++) {
                // Get page.
                var doc = XDocument.Parse(await client.GetStringAsync($"https://blogs.windows.com/windowsexperience/tag/windows-insider-program/feed/?paged={page}"));
                var entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                              select new BlogEntry() {
                                  Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                                  Title = item.Elements().First(i => i.Name.LocalName == "title").Value,
                                  Desc = item.Elements().First(i => i.Name.LocalName == "description").Value
                              };
                var list = entries.ToList();

                // Get post.
                switch (type) {
                    case BuildType.NormalPc:
                        post = list.Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build") && !p.Title.ToLowerInvariant().Contains("server") && (!p.Desc.ToLowerInvariant().Contains("skip ahead") || p.Desc.ToLowerInvariant().Contains("fast ring"))).FirstOrDefault();
                        break;

                    case BuildType.Server:
                        post = list.Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build") && p.Title.ToLowerInvariant().Contains("server") && !p.Desc.ToLowerInvariant().Contains("skip ahead")).FirstOrDefault();
                        break;

                    case BuildType.SkipAheadPc:
                        post = list.Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build") && p.Desc.ToLowerInvariant().Contains("skip ahead")).FirstOrDefault();
                        if (post != null) {
                            // If post indicates a merge of rings, just return the latest fast.
                            if (post.Desc.ToLowerInvariant().Contains("fast ring"))
                                return await GetLatestBuildNumberAsync(BuildType.NormalPc);
                        }
                        break;

                    case BuildType.SlowPc:
                        post = list.Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build") && p.Desc.Contains("Slow") && !p.Title.ToLowerInvariant().Contains("server") && !p.Desc.ToLowerInvariant().Contains("skip ahead")).FirstOrDefault();
                        break;
                }
                if (post != null)
                    break;
            }

            // If post is still null, no build was found.
            if (post == null)
                return null;

            var build = Regex.Match(post.Title, @"\d{5,}").Value;
            return new Tuple<string, string, BuildType>(build, post.Link, type);
        }

        /// <summary>
        /// Updates the game.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateGameAsync() {
            try {
                var build = await GetLatestBuildNumberAsync();
                if (build == null)
                    return;

                var game = $"on {build.Item1} | {Constants.CommandPrefix}{Constants.HelpCommand}";
                foreach (var shard in Shards)
                    await shard?.SetGameAsync(game);
            }
            catch (Exception ex) {
                LogError($"Failed to update game: {ex}");
                foreach (var shard in Shards)
                    await shard?.SetGameAsync("on Windows 10");
            }
        }

        #endregion
    }

    public enum BuildType {
        NormalPc,
        Server,
        SkipAheadPc,
        SlowPc
    }

    public enum RoleType {
        InsiderPrimary,
        InsiderSkip,
        InsiderSlow,
        Jumbo
    }
}
