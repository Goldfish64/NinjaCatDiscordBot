/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatDiscordClient.cs
* 
* Copyright (c) 2016-2018 John Davis
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

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Represents a <see cref="DiscordSocketClient"/> with additional properties.
    /// </summary>
    public sealed class NinjaCatDiscordClient : DiscordShardedClient
    {
        #region Private variables

        // private StreamWriter logStreamWriter;
        private Random random = new Random();
        private object lockObject = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjaCatDiscordClient"/> class.
        /// </summary>
        public NinjaCatDiscordClient() : base(new DiscordSocketConfig() { TotalShards = Constants.ShardCount })
        {
            // Write startup messages.
            LogOutput($"{Constants.AppName} on {RuntimeInformation.FrameworkDescription} has started.");
            LogOutput($"===============================================================");

            // Listen for events.
            Log += (message) =>
            {
                // Log the output.
                LogOutput(message.ToString());
                return Task.CompletedTask;
            };

            // Get latest post URL, if there is one.
            if (File.Exists(Constants.LatestPostFileName))
                CurrentUrl = File.ReadAllText(Constants.LatestPostFileName);

            // Create temporary dictionary.
            var channels = new Dictionary<ulong, ulong>();

            // Does the channels file exist? If so, deserialize JSON.
            if (File.Exists(Constants.ChannelsFileName))
                channels = JsonConvert.DeserializeObject<Dictionary<ulong, ulong>>(File.ReadAllText(Constants.ChannelsFileName));

            // Add each entry to the client.
            foreach (var entry in channels)
                SpeakingChannels[entry.Key] = entry.Value;

            // Create temporary dictionary.
            var roles = new Dictionary<ulong, ulong>();

            // Does the roles file exist? If so, deserialize JSON.
            if (File.Exists(Constants.RolesFileName))
                roles = JsonConvert.DeserializeObject<Dictionary<ulong, ulong>>(File.ReadAllText(Constants.RolesFileName));

            // Add each entry to the client.
            foreach (var entry in roles)
                SpeakingRoles[entry.Key] = entry.Value;

            // Create temporary dictionary.
            var rolesSkip = new Dictionary<ulong, ulong>();

            // Does the roles file exist? If so, deserialize JSON.
            if (File.Exists(Constants.RolesSkipFileName))
                rolesSkip = JsonConvert.DeserializeObject<Dictionary<ulong, ulong>>(File.ReadAllText(Constants.RolesSkipFileName));

            // Add each entry to the client.
            foreach (var entry in rolesSkip)
                SpeakingRolesSkip[entry.Key] = entry.Value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of speaking channels.
        /// </summary>
        /// <remarks>Guild is the key, channel is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> SpeakingChannels { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the list of speaking roles.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> SpeakingRoles { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the list of speaking roles for skip ahead.
        /// </summary>
        /// <remarks>Guild is the key, role is the value.</remarks>
        public ConcurrentDictionary<ulong, ulong> SpeakingRolesSkip { get; } = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// Gets the time the client started.
        /// </summary>
        public DateTime StartTime { get; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the current post URL. Used for keeping track of new posts.
        /// </summary>
        public string CurrentUrl { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a random number.
        /// </summary>
        /// <param name="maxValue">The maximum value of the number generated.</param>
        /// <returns>The random number.</returns>
        public int GetRandomNumber(int maxValue)
        {
            // Return a random number.
            return random.Next(maxValue);
        }

        /// <summary>
        /// Gets the speaking channel for the specified guild.
        /// </summary>
        /// <param name="guild">The <see cref="SocketGuild"/> to get the channel for.</param>
        /// <returns>An <see cref="SocketTextChannel"/> that should be used.</returns>
        public SocketTextChannel GetSpeakingChannelForSocketGuild(SocketGuild guild)
        {
            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
                return null;

            // Create channel variable.
            SocketTextChannel channel = null;

            // Try to get the saved channel.
            if (SpeakingChannels.ContainsKey(guild.Id))
            {
                // If it is zero, return null to not speak.
                if (SpeakingChannels[guild.Id] == 0)
                    return null;
                else
                    channel = guild.Channels.SingleOrDefault(g => g.Id == SpeakingChannels[guild.Id]) as SocketTextChannel;
            }

            // If the channel is null, delete the entry from the dictionary and use the default one.
            if (channel == null)
            {
                SpeakingChannels.TryRemove(guild.Id, out ulong outVar);
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
        public async Task<ITextChannel> GetSpeakingChannelForIGuildAsync(IGuild guild)
        {
            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
                return null;

            // Create channel variable.
            ITextChannel channel = null;

            // Try to get the saved channel.
            if (SpeakingChannels.ContainsKey(guild.Id))
            {
                // If it is zero, return null to not speak.
                if (SpeakingChannels[guild.Id] == 0)
                    return null;
                else
                    channel = (await guild.GetChannelsAsync()).SingleOrDefault(g => g.Id == SpeakingChannels[guild.Id]) as ITextChannel;
            }

            // If the channel is null, delete the entry from the dictionary and use the default one.
            if (channel == null)
            {
                SpeakingChannels.TryRemove(guild.Id, out ulong outVar);
                channel = (await guild.GetChannelsAsync()).SingleOrDefault(g => g.Id == guild.DefaultChannelId) as ITextChannel;
                SaveSettings();
            }

            // Return the channel.
            return channel;
        }

        /// <summary>
        /// Gets the speaking role for the specified guild.
        /// </summary>
        /// <param name="guild">The <see cref="IGuild"/> to get the role for.</param>
        /// <returns>An <see cref="SocketTextRole"/> that should be used.</returns>
        public IRole GetSpeakingRoleForIGuild(IGuild guild)
        {
            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
                return null;

            // Create role variable.
            IRole role = null;

            // Try to get the saved role.
            if (SpeakingRoles.ContainsKey(guild.Id))
            {
                // If it is zero, return null to not speak.
                if (SpeakingRoles[guild.Id] == 0)
                    return null;
                else
                    role = guild.Roles.SingleOrDefault(g => g.Id == SpeakingRoles[guild.Id]) as IRole;
            }

            // If the role is null, delete the entry from the dictionary and use the default one.
            if (role == null)
            {
                SpeakingRoles.TryRemove(guild.Id, out ulong outVar);
                SaveSettings();
            }

            // Return the role.
            return role;
        }

        /// <summary>
        /// Gets the speaking skip ahead role for the specified guild.
        /// </summary>
        /// <param name="guild">The <see cref="IGuild"/> to get the role for.</param>
        /// <returns>An <see cref="SocketTextRole"/> that should be used.</returns>
        public IRole GetSpeakingRoleSkipForIGuild(IGuild guild)
        {
            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
                return null;

            // Create role variable.
            IRole role = null;

            // Try to get the saved role.
            if (SpeakingRolesSkip.ContainsKey(guild.Id))
            {
                // If it is zero, return null to not speak.
                if (SpeakingRolesSkip[guild.Id] == 0)
                    return null;
                else
                    role = guild.Roles.SingleOrDefault(g => g.Id == SpeakingRolesSkip[guild.Id]) as IRole;
            }

            // If the role is null, delete the entry from the dictionary and use the default one.
            if (role == null)
            {
                SpeakingRolesSkip.TryRemove(guild.Id, out ulong outVar);
                SaveSettings();
            }

            // Return the role.
            return role;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void SaveSettings()
        {
            lock (lockObject)
            {
                // Save latest post URL.
                File.WriteAllText(Constants.LatestPostFileName, CurrentUrl);

                // Serialize settings to JSON.
                File.WriteAllText(Constants.ChannelsFileName, JsonConvert.SerializeObject(SpeakingChannels));
                File.WriteAllText(Constants.RolesFileName, JsonConvert.SerializeObject(SpeakingRoles));
                File.WriteAllText(Constants.RolesSkipFileName, JsonConvert.SerializeObject(SpeakingRolesSkip));
            }
        }

        /// <summary>
        /// Logs the specified information to the console and logfile.
        /// </summary>
        /// <param name="info">The information to log.</param>
        public void LogOutput(string info)
        {
            // Write to console.
            Console.WriteLine($"{DateTime.Now}: {info}");
        }

        /// <summary>
        /// Gets the latest build of the specified type.
        /// </summary>
        /// <param name="type">The type of build to get.</param>
        public async Task<Tuple<string, string>> GetLatestBuildNumberAsync(BuildType type = BuildType.NormalPc)
        {
            // Create HTTP client.
            var client = new HttpClient();

            // Get most recent build post..
            BlogEntry post = null;
            for (int page = 1; page <= 10; page++)
            {
                // Get page.
                var doc = XDocument.Parse(await client.GetStringAsync($"https://blogs.windows.com/windowsexperience/tag/windows-insider-program/feed/?paged={page}"));
                var entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                            select new BlogEntry()
                            { Link = item.Elements().First(i => i.Name.LocalName == "link").Value, Title = item.Elements().First(i => i.Name.LocalName == "title").Value };
                var list = entries.ToList();

                // Get post.
                switch (type)
                {
                    case BuildType.NormalPc:
                        post = list.Where(p => p.Title.ToLowerInvariant().Contains("insider preview build") && !p.Title.ToLowerInvariant().Contains("server") && !p.Title.ToLowerInvariant().Contains("skip")).FirstOrDefault();
                        break;

                    case BuildType.Mobile:
                        post = list.Where(p => p.Title.ToLowerInvariant().Contains("insider preview build") && p.Title.ToLowerInvariant().Contains("mobile") && !p.Title.ToLowerInvariant().Contains("skip")).FirstOrDefault();
                        break;

                    case BuildType.Server:
                        post = list.Where(p => p.Title.ToLowerInvariant().Contains("insider preview build") && p.Title.ToLowerInvariant().Contains("server") && !p.Title.ToLowerInvariant().Contains("skip")).FirstOrDefault();
                        break;

                    case BuildType.SkipAheadPc:
                        post = list.Where(p => p.Title.ToLowerInvariant().Contains("insider preview build") && p.Title.ToLowerInvariant().Contains("skip")).FirstOrDefault();
                        break;
                }
                if (post != null)
                    break;
            }

            // If post is still null, no build was found.
            if (post == null)
                return null;

            // Get build number.
            var build = Regex.Match(post.Title, @"\d{5,}", type == BuildType.Mobile ? RegexOptions.RightToLeft : RegexOptions.None).Value;

            // Return info.
            return new Tuple<string, string>(build, post.Link);
        }

        /// <summary>
        /// Updates the game.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateGameAsync()
        {
            try
            {
                // Get build.
                var build = await GetLatestBuildNumberAsync();
                if (build == null)
                    return;

                // Create string.
                var game = $"on {build.Item1} | {Constants.CommandPrefix}{Constants.HelpCommand}";

                // Update game.
                foreach (var shard in Shards)
                    await shard?.SetGameAsync(game);
            }
            catch (Exception ex)
            {
                // Log failure.
                LogOutput($"FAILURE IN GAME: {ex}");

                // Reset game.
                foreach (var shard in Shards)
                    await shard?.SetGameAsync("on Windows 10");
            }
        }

        #endregion
    }

    public enum BuildType
    {
        NormalPc,
        Mobile,
        Server,
        SkipAheadPc
    }
}
