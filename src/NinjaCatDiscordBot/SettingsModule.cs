/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: SettingsModule.cs
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
using System;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Contains settings commands.
    /// </summary>
    [Module(Constants.SettingsModule)]
    public sealed class SettingsModule
    {
        #region Classes

        /// <summary>
        /// Contains settings "get" commands.
        /// </summary>
        [Group(Constants.GetGroup)]
        public sealed class GetGroup
        {
            #region Private variables

            private NinjaCatDiscordClient client;

            #endregion

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="GetGroup"/> class.
            /// </summary>
            /// <param name="client">The <see cref="IDiscordClient"/> to use.</param>
            public GetGroup(IDiscordClient client)
            {
                // Check to see if client is valid.
                if (!(client is NinjaCatDiscordClient))
                    throw new ArgumentException($"This module requires a {nameof(NinjaCatDiscordClient)}.", nameof(client));

                // Get client.
                this.client = client as NinjaCatDiscordClient;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Replies to the specified message with the bot's nickname.
            /// </summary>
            /// <param name="message">The message to reply to.</param>
            [Command(Constants.NicknameCommand)]
            public async Task GetNicknameAsync(IUserMessage message)
            {
                // Bot is typing.
                await message.Channel.TriggerTypingAsync();

                // Get guild. If null show error.
                var guild = (message.Channel as IGuildChannel)?.Guild;
                if (guild == null)
                {
                    // Pause for realism.
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    // Select and send message.
                    switch (client.GetRandomNumber(4))
                    {
                        default:
                            await message.Channel.SendMessageAsync($"I can only have a nickname within a server. Run this command again from a server channel.");
                            break;

                        case 1:
                            await message.Channel.SendMessageAsync($"No can do. You need to run this command from a server channel to change my nickname.");
                            break;

                        case 2:
                            await message.Channel.SendMessageAsync($"I'm sorry {message.Author.Mention}, I'm afraid I can't do that. To change my nickname, run this command in a server channel.");
                            break;

                        case 3:
                            await message.Channel.SendMessageAsync($"Not happening. My nickname can only be changed from a server channel.");
                            break;
                    }
                    return;
                }

                // Get self.
                var self = await guild?.GetCurrentUserAsync();

                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await message.Channel.SendMessageAsync($"My name is {self.Mention}.");
                        break;

                    case 1:
                        await message.Channel.SendMessageAsync($"My nickname is currently {self.Mention}.");
                        break;

                    case 2:
                        await message.Channel.SendMessageAsync($"My name on this server is {self.Mention}.");
                        break;

                    case 3:
                        await message.Channel.SendMessageAsync($"I go by {self.Mention} on this server.");
                        break;
                }
            }

            /// <summary>
            /// Replies to the specified message with the channel the bot will speak in.
            /// </summary>
            /// <param name="message">The message to reply to.</param>
            [Command(Constants.ChannelCommand)]
            public async Task GetChannelAsync(IUserMessage message)
            {
                // Bot is typing.
                await message.Channel.TriggerTypingAsync();

                // Get guild. If null show error.
                var guild = (message.Channel as IGuildChannel)?.Guild;
                if (guild == null)
                {
                    // Pause for realism.
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    // Select and send message.
                    switch (client.GetRandomNumber(4))
                    {
                        default:
                            await message.Channel.SendMessageAsync($"This is not a server channel. Run this command in a server channel.");
                            break;

                        case 1:
                            await message.Channel.SendMessageAsync($"No can do. You need to run this command from a server channel.");
                            break;

                        case 2:
                            await message.Channel.SendMessageAsync($"I'm sorry {message.Author.Mention}, I'm afraid I can't do that. To see what channel I speak in, run this command in a server channel.");
                            break;

                        case 3:
                            await message.Channel.SendMessageAsync($"Not happening. My speaking channel can only be revealed from a server channel.");
                            break;
                    }
                    return;
                }

                // Create channel variable.
                ITextChannel channel = null;

                // Try to get the saved channel.
                if (client.SpeakingChannels.ContainsKey(guild.Id))
                    channel = await guild.GetTextChannelAsync(client.SpeakingChannels[guild.Id]);

                // If the channel is null, delete the entry from the dictionary and use the default one.
                if (channel == null)
                {
                    client.SpeakingChannels.Remove(guild.Id);
                    channel = await guild.GetDefaultChannelAsync();
                }

                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await message.Channel.SendMessageAsync($"The channel I'll speak in is {channel.Mention}.");
                        break;

                    case 1:
                        await message.Channel.SendMessageAsync($"My announcement channel is currently {channel.Mention}.");
                        break;

                    case 2:
                        await message.Channel.SendMessageAsync($"When new builds are released, I'll announce them in {channel.Mention}.");
                        break;

                    case 3:
                        await message.Channel.SendMessageAsync($"{channel.Mention} is the channel I'll speak in.");
                        break;

                    case 4:
                        await message.Channel.SendMessageAsync($"I'll only announce builds in {channel.Mention}.");
                        break;
                }
            }

            #endregion
        }

        /// <summary>
        /// Contains settings "set" commands.
        /// </summary>
        [Group(Constants.SetGroup)]
        public sealed class SetGroup
        {
            #region Private variables

            private NinjaCatDiscordClient client;

            #endregion

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="SetGroup"/> class.
            /// </summary>
            /// <param name="client">The <see cref="IDiscordClient"/> to use.</param>
            public SetGroup(IDiscordClient client)
            {
                // Check to see if client is valid.
                if (!(client is NinjaCatDiscordClient))
                    throw new ArgumentException($"This module requires a {nameof(NinjaCatDiscordClient)}.", nameof(client));

                // Get client.
                this.client = client as NinjaCatDiscordClient;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Replies to the specified message with the bot's nickname after setting it.
            /// </summary>
            /// <param name="message">The message to reply to.</param>
            /// <param name="nickname">The new nickname.</param>
            [Command(Constants.NicknameCommand)]
            public async Task SetNicknameAsync(IUserMessage message, string nickname)
            {
                // Bot is typing.
                await message.Channel.TriggerTypingAsync();

                // Get the user.
                var user = message.Author as IGuildUser;

                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // If the user is null or lacks the manage server permission, show error.
                if (user?.GuildPermissions.ManageGuild != true)
                {
                    // Select and send message.
                    switch (client.GetRandomNumber(4))
                    {
                        default:
                            await message.Channel.SendMessageAsync($"Sorry, but only those who have permission to manage this server can change my nickname.");
                            break;

                        case 1:
                            await message.Channel.SendMessageAsync($"No can do. You need to be able to manage this server to change my nickname.");
                            break;

                        case 2:
                            await message.Channel.SendMessageAsync($"I'm sorry {message.Author.Mention}, I'm afraid I can't do that. You must have manage server permissions to change my nickname.");
                            break;

                        case 3:
                            await message.Channel.SendMessageAsync($"Not happening. To change my nickname you must be able to manage this server.");
                            break;
                    }
                    return;
                }

                // Get guild. If null show error.
                var guild = (message.Channel as IGuildChannel)?.Guild;
                if (guild == null)
                {
                    // Select and send message.
                    switch (client.GetRandomNumber(4))
                    {
                        default:
                            await message.Channel.SendMessageAsync($"I can only have a nickname within a server. Run this command again from a server channel.");
                            break;

                        case 1:
                            await message.Channel.SendMessageAsync($"No can do. You need to run this command from a server channel to change my nickname.");
                            break;

                        case 2:
                            await message.Channel.SendMessageAsync($"I'm sorry {message.Author.Mention}, I'm afraid I can't do that. To change my nickname, run this command in a server channel.");
                            break;

                        case 3:
                            await message.Channel.SendMessageAsync($"Not happening. My nickname can only be changed from a server channel.");
                            break;
                    }
                    return;
                }

                // Get self.
                var self = await guild?.GetCurrentUserAsync();

                // Change nickname.
                await self.ModifyAsync(u => u.Nickname = nickname);

                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await message.Channel.SendMessageAsync($"I am now known as {self.Mention}");
                        break;

                    case 1:
                        await message.Channel.SendMessageAsync($"My new nickname is {self.Mention}");
                        break;

                    case 2:
                        await message.Channel.SendMessageAsync($"My name in this server is now {self.Mention}. Thank you.");
                        break;

                    case 3:
                        await message.Channel.SendMessageAsync($"A new name? For me? Thanks! I'll use {self.Mention} as my name now.");
                        break;
                }
            }

            /// <summary>
            /// Replies to the specified message with the bot's speaking channel after setting it.
            /// </summary>
            /// <param name="message">The message to reply to.</param>
            /// <param name="channel">The new channel.</param>
            [Command(Constants.ChannelCommand)]
            public async Task SetChannelAsync(IUserMessage message, ITextChannel channel)
            {
                // Bot is typing.
                await message.Channel.TriggerTypingAsync();

                // Get the user.
                var user = message.Author as IGuildUser;

                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // If the user is null or lacks the manage server permission, show error.
                if (user?.GuildPermissions.ManageGuild != true)
                {
                    // Select and send message.
                    switch (client.GetRandomNumber(4))
                    {
                        default:
                            await message.Channel.SendMessageAsync($"Sorry, but only those who have permission to manage this server can change the channel I speak in.");
                            break;

                        case 1:
                            await message.Channel.SendMessageAsync($"No can do. You need to be able to manage this server to change my announcement channel.");
                            break;

                        case 2:
                            await message.Channel.SendMessageAsync($"I'm sorry {message.Author.Mention}, I'm afraid I can't do that. You must have manage server permissions to change my channel.");
                            break;

                        case 3:
                            await message.Channel.SendMessageAsync($"Not happening. To change the channel I speak in you must be able to manage this server.");
                            break;
                    }
                    return;
                }

                // Save channel.
                client.SpeakingChannels[channel.Guild.Id] = channel.Id;

                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await message.Channel.SendMessageAsync($"The channel I'll speak in from now on is {channel.Mention}.");
                        break;

                    case 1:
                        await message.Channel.SendMessageAsync($"My announcement channel is now {channel.Mention}.");
                        break;

                    case 2:
                        await message.Channel.SendMessageAsync($"When new builds are released, I'll now announce them in {channel.Mention}.");
                        break;

                    case 3:
                        await message.Channel.SendMessageAsync($"{channel.Mention} is the channel I'll now speak in.");
                        break;

                    case 4:
                        await message.Channel.SendMessageAsync($"I'll now announce builds only in {channel.Mention}.");
                        break;
                }
            }

            #endregion
        }

        /// <summary>
        /// Contains settings "disable" commands.
        /// </summary>
        [Group(Constants.DisableGroup)]
        public sealed class DisableGroup
        {
            #region Private variables

            private NinjaCatDiscordClient client;

            #endregion

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="DisableGroup"/> class.
            /// </summary>
            /// <param name="client">The <see cref="IDiscordClient"/> to use.</param>
            public DisableGroup(IDiscordClient client)
            {
                // Check to see if client is valid.
                if (!(client is NinjaCatDiscordClient))
                    throw new ArgumentException($"This module requires a {nameof(NinjaCatDiscordClient)}.", nameof(client));

                // Get client.
                this.client = client as NinjaCatDiscordClient;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Replies to the specified message after disabling the channel.
            /// </summary>
            /// <param name="message">The message to reply to.</param>
            /// <param name="channel">The new channel.</param>
            [Command(Constants.ChannelCommand)]
            public async Task DisableChannelAsync(IUserMessage message)
            {
                // Bot is typing.
                await message.Channel.TriggerTypingAsync();

                // Get guild. If null show error.
                var guild = (message.Channel as IGuildChannel)?.Guild;
                if (guild == null)
                {
                    // Pause for realism.
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    // Select and send message.
                    switch (client.GetRandomNumber(4))
                    {
                        default:
                            await message.Channel.SendMessageAsync($"This is not a server channel. Run this command in a server channel.");
                            break;

                        case 1:
                            await message.Channel.SendMessageAsync($"No can do. You need to run this command from a server channel.");
                            break;

                        case 2:
                            await message.Channel.SendMessageAsync($"I'm sorry {message.Author.Mention}, I'm afraid I can't do that. To see what channel I speak in, run this command in a server channel.");
                            break;

                        case 3:
                            await message.Channel.SendMessageAsync($"Not happening. My speaking channel can only be revealed from a server channel.");
                            break;
                    }
                    return;
                }

                // Get the user.
                var user = message.Author as IGuildUser;

                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // If the user is null or lacks the manage server permission, show error.
                if (user?.GuildPermissions.ManageGuild != true)
                {
                    // Select and send message.
                    switch (client.GetRandomNumber(4))
                    {
                        default:
                            await message.Channel.SendMessageAsync($"Sorry, but only those who have permission to manage this server can disable the channel I speak in.");
                            break;

                        case 1:
                            await message.Channel.SendMessageAsync($"No can do. You need to be able to manage this server to disable my announcements.");
                            break;

                        case 2:
                            await message.Channel.SendMessageAsync($"I'm sorry {message.Author.Mention}, I'm afraid I can't do that. You must have manage server permissions to disable my channel.");
                            break;

                        case 3:
                            await message.Channel.SendMessageAsync($"Not happening. To disable public speaking you must be able to manage this server.");
                            break;
                    }
                    return;
                }

                // Set channel to 0.
                client.SpeakingChannels[guild.Id] = 0;

                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await message.Channel.SendMessageAsync($"I'll no longer speak in any channel.");
                        break;

                    case 1:
                        await message.Channel.SendMessageAsync($"I won't make announcements anymore.");
                        break;

                    case 2:
                        await message.Channel.SendMessageAsync($"When new builds are released, I'll keep quiet.");
                        break;

                    case 3:
                        await message.Channel.SendMessageAsync($"I'll stay quiet from now on.");
                        break;

                    case 4:
                        await message.Channel.SendMessageAsync($"I'll no longer announce builds.");
                        break;
                }
            }

            #endregion
        }

        #endregion
    }
}
