/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatDiscordClient.cs
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
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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
        /// Gets the list of Dev Channel Insider roles.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> InsiderRolesDev { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the list of Beta Channel Insider roles.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> InsiderRolesBeta { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the list of Release Preview Insider roles.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> InsiderRolesReleasePreview { get; } = new ConcurrentDictionary<ulong, ulong>();

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

            // Initialize commands.
            Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null).Wait();

            // Listen for messages.
            MessageReceived += async (message) => {
                var msg = message as SocketUserMessage;
                if (msg == null)
                    return;

                // Keeps track of where the command begins.
                var pos = 0;

                // Attempt to parse a command. Silently ignore unknown commanads.
                if (msg.HasStringPrefixLower(Constants.CommandPrefix, ref pos)) {
                    var result = await Commands.ExecuteAsync(new NinjaCatCommandContext(this, msg), msg.Content.Substring(pos), null);
                    if (!result.IsSuccess) {
                        if (result.Error == CommandError.UnknownCommand)
                            return;

                        await msg.Channel.TriggerTypingAsync();
                        await Task.Delay(TimeSpan.FromSeconds(0.5));

                        await msg.Channel.SendMessageAsync($"I'm sorry, but something happened. Error: {result.ErrorReason}\n\nIf there are spaces in a parameter, make sure to surround it with quotes.");
                    }
                    return;
                }
            };
        }

        #endregion

        #region Properties

        public NinjaCatSettings Settings;

        public CommandService Commands { get; } = new CommandService();

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

            ConcurrentDictionary<ulong, ulong> roles;
            switch (type) {
                case RoleType.InsiderDev:
                    roles = Settings.InsiderRolesDev;
                    break;

                case RoleType.InsiderBeta:
                    roles = Settings.InsiderRolesBeta;
                    break;

                case RoleType.InsiderReleasePreview:
                    roles = Settings.InsiderRolesReleasePreview;
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

        public void SetInsiderChannel(IGuild guild, ITextChannel channel) {
            Settings.InsiderChannels[guild.Id] = channel?.Id ?? 0;
            SaveSettings();
        }

        public void SetInsiderRole(IGuild guild, IRole role, RoleType roleType) {
            ConcurrentDictionary<ulong, ulong> roles;
            switch (roleType) {
                case RoleType.InsiderDev:
                    roles = Settings.InsiderRolesDev;
                    break;

                case RoleType.InsiderBeta:
                    roles = Settings.InsiderRolesBeta;
                    break;

                case RoleType.InsiderReleasePreview:
                    roles = Settings.InsiderRolesReleasePreview;
                    break;

                case RoleType.Jumbo:
                    roles = Settings.JumboRoles;
                    break;

                default:
                    return;
            }

            if (role != null)
                roles[guild.Id] = role.Id;
            else
                roles.TryRemove(guild.Id, out _);
            SaveSettings();
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
        public async Task<Tuple<string, string, BuildType>> GetLatestBuildNumberAsync(BuildType type) {
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
                //
                // TODO: Validate wording when beta/release preview channels see first post.
                //
                switch (type) {
                    case BuildType.DevPc:
                        post = list.Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build") && !p.Title.ToLowerInvariant().Contains("server") && p.Desc.ToLowerInvariant().Contains("dev channel")).FirstOrDefault();
                        break;

                    case BuildType.BetaPc:
                        post = list.Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build") && !p.Title.ToLowerInvariant().Contains("server") && p.Desc.ToLowerInvariant().Contains("beta channel")).FirstOrDefault();
                        break;

                    case BuildType.ReleasePreviewPc:
                        post = list.Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build") && !p.Title.ToLowerInvariant().Contains("server") && p.Desc.ToLowerInvariant().Contains("release preview channel")).FirstOrDefault();
                        break;

                    case BuildType.Server:
                        post = list.Where(p => p.Link.ToLowerInvariant().Contains("insider-preview-build") && p.Title.ToLowerInvariant().Contains("server")).FirstOrDefault();
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
                var build = await GetLatestBuildNumberAsync(BuildType.DevPc);
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
        DevPc,
        BetaPc,
        ReleasePreviewPc,
        Server
    }

    public enum RoleType {
        InsiderDev,
        InsiderBeta,
        InsiderReleasePreview,
        Jumbo
    }
}
