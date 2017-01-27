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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Represents a <see cref="DiscordSocketClient"/> with additional properties.
    /// </summary>
    public sealed class NinjaCatDiscordClient : DiscordSocketClient
    {
        #region Private variables

        private StreamWriter logStreamWriter;
        private Random random = new Random();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjaCatDiscordClient"/> class.
        /// </summary>
        public NinjaCatDiscordClient()
        {
            // Open log file.
            logStreamWriter = File.AppendText(Constants.LogFileName);

            // Write startup messages.
            LogOutput($"{Constants.AppName} has started.");
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
                SpeakingChannels.Add(entry.Key, entry.Value);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of speaking channels.
        /// </summary>
        /// <remarks>Guild is the key, channel is the value.</remarks>
        public Dictionary<ulong, ulong> SpeakingChannels { get; } = new Dictionary<ulong, ulong>();

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
                SpeakingChannels.Remove(guild.Id);
                channel = guild.Channels.SingleOrDefault(g => g.Id == guild.DefaultChannelId) as SocketTextChannel;
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
                SpeakingChannels.Remove(guild.Id);
                channel = (await guild.GetChannelsAsync()).SingleOrDefault(g => g.Id == guild.DefaultChannelId) as ITextChannel;
                SaveSettings();
            }

            // Return the channel.
            return channel;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void SaveSettings()
        {
            // Serialize settings to JSON.
            File.WriteAllText(Constants.ChannelsFileName, JsonConvert.SerializeObject(SpeakingChannels));
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
            logStreamWriter.WriteLine($"{timeDate}: {info}");
            logStreamWriter.Flush();
        }

        #endregion
    }
}
