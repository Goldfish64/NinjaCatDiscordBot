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
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
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

        private Random random = new Random();
        private object lockObject = new object();

        private HttpClient httpClient = new HttpClient();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjaCatDiscordClient"/> class.
        /// </summary>
        public NinjaCatDiscordClient() : base(new DiscordSocketConfig() { TotalShards = Constants.ShardCount }) {
            // Write startup messages.
            LogInfo($"{Constants.AppName} on {RuntimeInformation.FrameworkDescription} has started.");
            LogInfo($"===============================================================");
            Interactions = new InteractionService(this);

            // Listen for events.
            Log += (message) => {
                // Log the output.
                LogInfo(message.ToString());
                return Task.CompletedTask;
            };

            // Get latest post URL, if there is one.
            if (File.Exists(Constants.LatestPostFileName))
                CurrentUrl = File.ReadAllText(Constants.LatestPostFileName);

            if (File.Exists(Constants.SettingsFileName)) {
                Settings = JsonConvert.DeserializeObject<NinjaCatSettings>(File.ReadAllText(Constants.SettingsFileName));
            } else {
                Settings = new NinjaCatSettings();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public NinjaCatSettings Settings;

        /// <summary>
        /// Gets the interaction service.
        /// </summary>
        public InteractionService Interactions { get; }

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
        /// Starts the bot.
        /// </summary>
        /// <returns></returns>
        public async Task StartBotAsync() {
            for (int i = 0; i < 5; i++) {
                try {
                    await LoginAsync(TokenType.Bot, Credentials.DiscordToken);
                    await StartAsync();

                    return;
                }
                catch (HttpException ex) {
                    LogError($"Exception when logging in, waiting 5: {ex}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5));
            }

            throw new InvalidOperationException();
        }

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
                    channel = (await guild.GetTextChannelsAsync()).SingleOrDefault(g => g.Id == Settings.InsiderChannels[guild.Id]) as ITextChannel;
            }

            // If the channel is null, delete the entry from the dictionary and use the default one.
            if (channel == null) {
                Settings.InsiderChannels.TryRemove(guild.Id, out ulong outVar);
                channel = await guild.GetDefaultChannelAsync();
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
                    role = guild.Roles.SingleOrDefault(g => g.Id == roles[guild.Id]);
            } else {
                return null;
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

        public void SetRole(IGuild guild, IRole role, RoleType roleType) {
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
        /// Sends typing feedback.
        /// </summary>
        public async Task StartTyping(IMessageChannel channel) {
            await channel.TriggerTypingAsync();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
        }

        /// <summary>
        /// Gets the latest blog post of the specified type.
        /// </summary>
        /// <param name="type">The type of build to get. Specify <see cref="BuildType.Unknown"/> to get the latest build regardless of type.</param>
        /// <returns>A <see cref="BlogEntry"/> representing the build blog post or null if no build was found.</returns>
        public async Task<BlogEntry> GetLatestBuildPostAsync(BuildType type = BuildType.Unknown) {
            BlogEntry post = null;
            try {
                for (int page = 1; page <= 10; page++) {
                    // Get page.
                    var doc = XDocument.Parse(await httpClient.GetStringAsync($"https://blogs.windows.com/windows-insider/feed/?paged={page}"));
                    var entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                                  where item.Elements().First(i => i.Name.LocalName == "link").Value.ToLowerInvariant().ContainsAny("insider-preview", "windows-10-build", "windows-11-build")
                                  select item;
                    var posts = entries.Select(async item => await BlogEntry.Create(
                            httpClient,
                            item.Elements().First(i => i.Name.LocalName == "title").Value,
                            item.Elements().First(i => i.Name.LocalName == "link").Value,
                            item.Elements().First(i => i.Name.LocalName == "description").Value))
                        .Select(t => t.Result)
                        .Where(i => i != null)
                        .ToList();

                    // Get first post of desired type if a type was specified.
                    if (type == BuildType.Unknown)
                        post = posts.FirstOrDefault();
                    else if (type == BuildType.BetaPc)
                        post = posts.Where(p => p.BuildType == BuildType.BetaPc || p.BuildType == BuildType.DevBetaPc || p.BuildType == BuildType.BetaReleasePreviewPc).FirstOrDefault();
                    else if (type == BuildType.ReleasePreviewPc)
                        post = posts.Where(p => p.BuildType == BuildType.ReleasePreviewPc || p.BuildType == BuildType.BetaReleasePreviewPc).FirstOrDefault();
                    else if (type == BuildType.DevPc)
                        post = posts.Where(p => p.BuildType == BuildType.DevPc || p.BuildType == BuildType.DevBetaPc).FirstOrDefault();
                    else
                        post = posts.Where(p => p.BuildType == type).FirstOrDefault();
                    if (post != null)
                        return post;
                }
            }
            catch (HttpRequestException ex) {
                LogError($"Exception when getting post for type {type}: {ex}");
                return null;
            }
            
            LogError($"Unable to get new post for type {type}");
            return null;
        }

        private async Task SendBuildToGuild(DiscordSocketClient shard, SocketGuild guild, BlogEntry blogEntry) {
            var channel = GetSpeakingChannelForSocketGuild(guild);
            if (channel == null) {
                LogInfo($"Rolling over {guild.Name} (disabled) ({shard.ShardId}/{Shards.Count - 1})");
                return;
            }

            // Verify we have permission to speak.
            if (guild.CurrentUser?.GetPermissions(channel).SendMessages != true) {
                LogInfo($"Rolling over {guild.Name} (no perms) ({shard.ShardId}/{Shards.Count - 1})");
                return;
            }

            // Get all roles.
            var roleDev = GetRoleForIGuild(guild, RoleType.InsiderDev);
            var roleBeta = GetRoleForIGuild(guild, RoleType.InsiderBeta);
            var roleReleasePreview = GetRoleForIGuild(guild, RoleType.InsiderReleasePreview);

            var roleText = string.Empty;
            var typeText = string.Empty;
            var emotesText = ":smiley_cat:";
            switch (blogEntry.BuildType) {
                case BuildType.DevPc:
                    roleText = $"{roleDev?.Mention} ";
                    typeText = " to the Dev Channel";
                    emotesText += " :tools:";
                    break;

                case BuildType.BetaPc:
                    roleText = $"{roleBeta?.Mention} ";
                    typeText = " to the Beta Channel";
                    emotesText += " :paintbrush:";
                    break;

                case BuildType.ReleasePreviewPc:
                    roleText = $"{roleReleasePreview?.Mention} ";
                    typeText = " to the Release Preview Channel";
                    emotesText += " :package:";
                    break;

                case BuildType.BetaReleasePreviewPc:
                    roleText = $"{roleBeta?.Mention} {roleReleasePreview?.Mention} ";
                    typeText = " to the Beta and Release Preview Channels";
                    emotesText += " :paintbrush: :package:";
                    break;

                case BuildType.DevBetaPc:
                    roleText = $"{roleDev?.Mention} {roleBeta?.Mention} ";
                    typeText = " to the Dev and Beta Channels";
                    emotesText += " :tools: :paintbrush:";
                    break;

                case BuildType.Server:
                    typeText = " for Server";
                    emotesText += " :desktop:";
                    break;
            }

            try {
                await StartTyping(channel);
                switch (GetRandomNumber(3)) {
                    default:
                        await channel.SendMessageAsync($"{roleText}{blogEntry.OSName} Insider Preview Build {blogEntry.BuildNumber} has just been released{typeText}! {emotesText}\n{blogEntry.Link}");
                        break;

                    case 1:
                        await channel.SendMessageAsync($"{roleText}{blogEntry.OSName} Insider Preview Build {blogEntry.BuildNumber} has just been released{typeText}! Yes! {emotesText}\n{blogEntry.Link}");
                        break;

                    case 2:
                        await channel.SendMessageAsync($"{roleText}Better check for updates now! {blogEntry.OSName} Insider Preview Build {blogEntry.BuildNumber} has just been released{typeText}! {emotesText}\n{blogEntry.Link}");
                        break;
                }
            }
            catch (Exception ex) {
                LogError($"Failed to speak in {guild.Name} ({shard.ShardId}/{Shards.Count - 1}): {ex}");
            }

            // Log server.
            LogInfo($"Spoke in {guild.Name} ({shard.ShardId}/{Shards.Count - 1})");
        }

        public async void SendNewBuildToShard(DiscordSocketClient shard, BlogEntry blogEntry) {
            // If the MS server is in this shard, announce there first.
            var msGuild = shard.Guilds.SingleOrDefault(g => g.Id == Constants.MsGuildId);
            if (msGuild != null)
                await SendBuildToGuild(shard, msGuild, blogEntry);

            foreach (var guild in shard.Guilds) {
                // Skip MS guild.
                if (guild.Id == Constants.MsGuildId)
                    continue;

                await SendBuildToGuild(shard, guild, blogEntry);
            }
        }

        /// <summary>
        /// Updates the game.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateGameAsync() {
            try {
                var build = await GetLatestBuildPostAsync(BuildType.DevPc);
                if (build == null)
                    return;

                var game = $"on build {build.BuildNumber}";
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

    /// <summary>
    /// Represents a blog entry.
    /// </summary>
    public class BlogEntry {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogEntry"/> class.
        /// </summary>
        /// <param name="title">The blog post title.</param>
        /// <param name="link">The blog post link.</param>
        /// <param name="description">The blog post description.</param>
        public BlogEntry(string title, string link, string description) {
            Title = title;
            Link = link;
            Description = description;

            // Parse build number.
            try {
                BuildNumber = Regex.Match(Title, @"\d{5,}\.?\d*").Value;
            }
            catch (ArgumentException) { }

            // Parse build type.
            if (Link.ToLowerInvariant().Contains("server"))
                BuildType = BuildType.Server;
            else {
                // Parse only first sentence.
                /* var endIndex = Description.ToLowerInvariant().IndexOf(". ");
                 if (endIndex == -1) {
                     endIndex = Description.ToLowerInvariant().IndexOf(".");
                 }
                 var desc = Description.ToLowerInvariant().Substring(0, endIndex);*/
                var desc = Description.ToLowerInvariant();
                if (desc.Contains("dev and beta cha"))
                    BuildType = BuildType.DevBetaPc;
                else if (desc.Contains("dev cha"))
                    BuildType = BuildType.DevPc;
                else if (desc.Contains("beta and release preview cha") || desc.Contains("beta and the release preview cha"))
                    BuildType = BuildType.BetaReleasePreviewPc;
                else if (desc.Contains("beta cha"))
                    BuildType = BuildType.BetaPc;
                else if (desc.Contains("release preview cha"))
                    BuildType = BuildType.ReleasePreviewPc;
                else
                    BuildType = BuildType.Unknown;
            }

            // Parse OS name.
            if (Link.ToLowerInvariant().Contains("windows-10"))
                OSName = "Windows 10";
            else if (Link.ToLowerInvariant().Contains("windows-11"))
                OSName = "Windows 11";
            else
                OSName = "Windows";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the blog post title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the blog post link.
        /// </summary>
        public string Link { get; }

        /// <summary>
        /// Gets the blog post description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the build number.
        /// </summary>
        public string BuildNumber { get; }

        /// <summary>
        /// Gets the build type.
        /// </summary>
        public BuildType BuildType { get; }

        public string OSName { get; }

        #endregion

        #region Methods

        public static async Task<BlogEntry> Create(HttpClient httpClient, string title, string link, string description) {
            // Get actual post content if the description in the feed is too short.
            if (!description.ToLowerInvariant().Contains("hello windows insiders")) {
                var doc = (await httpClient.GetStringAsync(link)).ToLowerInvariant();
                var index = doc.IndexOf("hello windows insiders");

                if (index != -1) {
                    description = doc.Substring(index);
                    var indexEnd = description.IndexOf("channel");
                    if (indexEnd != -1 && indexEnd + "channel".Length < description.Length)
                        description = description.Substring(0, indexEnd + "channel".Length);
                }
                
            }

            return new BlogEntry(title, link, description);
        }

        #endregion
    }

    public class BuildResult {
        public BlogEntry BlogPost { get; }

        public BuildType Type { get; }

        public string Number { get; }
    }

    public enum BuildType {
        Unknown,
        DevPc,
        BetaPc,
        DevBetaPc,
        ReleasePreviewPc,
        BetaReleasePreviewPc,
        Server
    }

    public enum RoleType {
        InsiderDev,
        InsiderBeta,
        InsiderReleasePreview,
        Jumbo
    }
}
