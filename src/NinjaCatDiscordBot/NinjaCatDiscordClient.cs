/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatDiscordClient.cs
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
        public NinjaCatDiscordClient() : base(new DiscordSocketConfig() { TotalShards = 6 })
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
        /// Gets the time the client started.
        /// </summary>
        public DateTime StartTime { get; } = DateTime.Now;

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
                ulong outVar;
                SpeakingChannels.TryRemove(guild.Id, out outVar);
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
                ulong outVar;
                SpeakingChannels.TryRemove(guild.Id, out outVar);
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
                ulong outVar;
                SpeakingRoles.TryRemove(guild.Id, out outVar);
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
                // Serialize settings to JSON.
                File.WriteAllText(Constants.ChannelsFileName, JsonConvert.SerializeObject(SpeakingChannels));
                File.WriteAllText(Constants.RolesFileName, JsonConvert.SerializeObject(SpeakingRoles));
            }
        }

        /// <summary>
        /// Logs the specified information to the console and logfile.
        /// </summary>
        /// <param name="info">The information to log.</param>
        public void LogOutput(string info)
        {
            // Get current time and date.
            var timeDate = DateTime.Now;

            // Write to console and logfile.
            Console.WriteLine($"{timeDate}: {info}");
            //  logStreamWriter.WriteLine($"{timeDate}: {info}");
            //  logStreamWriter.Flush();
        }

        /// <summary>
        /// Updates the game.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateGameAsync()
        {
            try
            {
                // Create HTTP client.
                var client = new HttpClient();

                // Get blog entries.
                var doc = XDocument.Parse(await client.GetStringAsync("https://blogs.windows.com/windowsexperience/tag/windows-insider-program/feed/"));
                var entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                              select new BlogEntry()
                              { Link = item.Elements().First(i => i.Name.LocalName == "link").Value, Title = item.Elements().First(i => i.Name.LocalName == "title").Value };
                var list = entries.ToList();

                // Get second page.
                doc = XDocument.Parse(await client.GetStringAsync("https://blogs.windows.com/windowsexperience/tag/windows-insider-program/feed/?paged=2"));
                entries = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                          select new BlogEntry()
                          { Link = item.Elements().First(i => i.Name.LocalName == "link").Value, Title = item.Elements().First(i => i.Name.LocalName == "title").Value };
                list.AddRange(entries.ToList());

                // Get most recent build post.
                var post = list.Where(p => p.Title.ToLowerInvariant().Contains("insider preview build") && p.Title.ToLowerInvariant().Contains("pc")).FirstOrDefault();

                // Get build number.
                var build = Regex.Match(post.Title, @"\d{5,}", RegexOptions.None).Value;

                // Create string.
                var game = $"on {build} | {Constants.CommandPrefix}{Constants.HelpCommand}";

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
}
