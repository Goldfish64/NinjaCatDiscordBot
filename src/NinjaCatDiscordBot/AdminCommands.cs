﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: AdminCommands.cs
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
using Discord.Net;
using System;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Contains commands for the bot.
    /// </summary>
    public sealed partial class CommandModule : ModuleBase
    {
        #region Methods

        /// <summary>
        /// Tests the permissions.
        /// </summary>
        /// <returns></returns>
        [Command(Constants.TestPermsCommand)]
        public async Task TestPermsAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get channel.
            var channel = await client.GetSpeakingChannelForIGuildAsync(Context.Guild);

            // If the channel is null, return message saying that speaking is disabled.
            if (channel == null)
            {
                await ReplyAsync($"I'm not currently speaking in any channels.");
                return;
            }

            // Verify we have permission to speak.
            if (!(await Context.Guild.GetCurrentUserAsync()).GetPermissions(channel).SendMessages)
            {
                await ReplyAsync($"I don't have permission to send messages in {channel.Mention}. Please give me that permission.");
                return;
            }

            await ReplyAsync($"I'm all set to speak in {channel.Mention}!");
            return;
        }

        /// <summary>
        /// Gets with the bot's nickname.
        /// </summary>
        [Command(Constants.NicknameCommand)]
        public async Task GetNicknameAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null)
            {
                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"I can only have a nickname within a server. Run this command again from a server channel.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You need to run this command from a server channel to change my nickname.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.Message.Author.Mention}, I'm afraid I can't do that. To change my nickname, run this command in a server channel.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. My nickname can only be changed from a server channel.");
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
                    await ReplyAsync($"My name is {self.Mention}.");
                    break;

                case 1:
                    await ReplyAsync($"My nickname is currently {self.Mention}.");
                    break;

                case 2:
                    await ReplyAsync($"My name on this server is {self.Mention}.");
                    break;

                case 3:
                    await ReplyAsync($"I go by {self.Mention} on this server.");
                    break;
            }
        }

        /// <summary>
        /// Gets the channel the bot will speak in.
        /// </summary>
        [Command(Constants.ChannelCommand)]
        public async Task GetChannelAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null)
            {
                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"This is not a server channel. Run this command in a server channel.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You need to run this command from a server channel.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.Message.Author.Mention}, I'm afraid I can't do that. To see what channel I speak in, run this command in a server channel.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. My speaking channel can only be revealed from a server channel.");
                        break;
                }
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
            {
                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't speak in any channels.");
                return;
            }

            // Get channel.
            var channel = await client.GetSpeakingChannelForIGuildAsync(guild);

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // If the channel is still null, that means no announcements.
            if (channel == null)
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"I'm not speaking in any channel.");
                        break;

                    case 1:
                        await ReplyAsync($"I'm not making announcements.");
                        break;

                    case 2:
                        await ReplyAsync($"When new builds are released, I'm keeping quiet.");
                        break;

                    case 3:
                        await ReplyAsync($"I'm being quiet right now.");
                        break;

                    case 4:
                        await ReplyAsync($"I'm not announcing any builds.");
                        break;
                }
            }
            else
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"The channel I'll speak in is {channel.Mention}.");
                        break;

                    case 1:
                        await ReplyAsync($"My announcement channel is currently {channel.Mention}.");
                        break;

                    case 2:
                        await ReplyAsync($"When new builds are released, I'll announce them in {channel.Mention}.");
                        break;

                    case 3:
                        await ReplyAsync($"{channel.Mention} is the channel I'll speak in.");
                        break;

                    case 4:
                        await ReplyAsync($"I'll only announce builds in {channel.Mention}.");
                        break;
                }
            }
        }

        /// <summary>
        /// Sets the bot's nickname.
        /// </summary>
        /// <param name="nickname">The new nickname.</param>
        [Command(Constants.SetNicknameCommand)]
        public async Task SetNicknameAsync(params string[] nickname)
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // If guild is null show error.
            if (Context.Guild == null)
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"I can only have a nickname within a server. Run this command again from a server channel.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You need to run this command from a server channel to change my nickname.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.User.Mention}, I'm afraid I can't do that. To change my nickname, run this command in a server channel.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. My nickname can only be changed from a server channel.");
                        break;
                }
                return;
            }

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage nicknames permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageNicknames != true)
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"Sorry, but only those who have permission to manage nicknames on this server can change my nickname.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You need to be able to manage nicknames on this server to change my nickname.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.User.Mention}, I'm afraid I can't do that. You must have the manage nickname permission to change my nickname.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. To change my nickname you must be able to manage nicknames on this server.");
                        break;
                }
                return;
            }

            // Get self.
            var self = await Context.Guild.GetCurrentUserAsync();

            // Change nickname.
            await self.ModifyAsync(u => u.Nickname = string.Join(" ", nickname));

            // Select and send message.
            switch (client.GetRandomNumber(4))
            {
                default:
                    await ReplyAsync($"I am now known as {self.Mention}");
                    break;

                case 1:
                    await ReplyAsync($"My new nickname is {self.Mention}");
                    break;

                case 2:
                    await ReplyAsync($"My name in this server is now {self.Mention}. Thank you.");
                    break;

                case 3:
                    await ReplyAsync($"A new name? For me? Thanks! I'll use {self.Mention} as my name now.");
                    break;
            }
        }

        /// <summary>
        /// Sets the bot's speaking channel.
        /// </summary>
        /// <param name="channel">The new channel.</param>
        [Command(Constants.SetChannelCommand)]
        public async Task SetChannelAsync(ITextChannel channel = null)
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // If guild is null show error.
            if (Context.Guild == null)
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"I can only speak within a server. Run this command again from a server channel.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You need to run this command from a server channel to change my speaking channel.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.User.Mention}, I'm afraid I can't do that. To change the channel I speak in, run this command in a server channel.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. My channel can only be changed from a server channel.");
                        break;
                }
                return;
            }

            // If the guild is the Bots server, never speak.
            if (Context.Guild.Id == Constants.BotsGuildId)
            {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't speak in any channels.");
                return;
            }

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage server permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true)
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"Sorry, but only those who have permission to manage this server can change the channel I speak in.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You need to be able to manage this server to change my announcement channel.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.User.Mention}, I'm afraid I can't do that. You must have manage server permissions to change my channel.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. To change the channel I speak in you must be able to manage this server.");
                        break;
                }
                return;
            }

            // Get channel if null.
            channel = channel ?? Context.Channel as ITextChannel;

            // Save channel.
            client.SpeakingChannels[channel.Guild.Id] = channel.Id;
            client.SaveSettings();

            // Select and send message.
            switch (client.GetRandomNumber(4))
            {
                default:
                    await ReplyAsync($"The channel I'll speak in from now on is {channel.Mention}.");
                    break;

                case 1:
                    await ReplyAsync($"My announcement channel is now {channel.Mention}.");
                    break;

                case 2:
                    await ReplyAsync($"When new builds are released, I'll now announce them in {channel.Mention}.");
                    break;

                case 3:
                    await ReplyAsync($"{channel.Mention} is the channel I'll now speak in.");
                    break;

                case 4:
                    await ReplyAsync($"I'll now announce builds only in {channel.Mention}.");
                    break;
            }
        }

        /// <summary>
        /// Disables the bot's speaking channel.
        /// </summary>
        [Command(Constants.DisableChannelCommand)]
        public async Task DisableChannelAsync()
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // If guild is null show error.
            if (Context.Guild == null)
            {
                // Pause for realism.
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"This is not a server channel. Run this command in a server channel.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You need to run this command from a server channel.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.User.Mention}, I'm afraid I can't do that. To see what channel I speak in, run this command in a server channel.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. My speaking channel can only be revealed from a server channel.");
                        break;
                }
                return;
            }

            // If the guild is the Bots server, never speak.
            if (Context.Guild.Id == Constants.BotsGuildId)
            {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't speak in any channels.");
                return;
            }

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage server permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true)
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"Sorry, but only those who have permission to manage this server can disable the channel I speak in.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You need to be able to manage this server to disable my announcements.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.User.Mention}, I'm afraid I can't do that. You must have manage server permissions to disable my channel.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. To disable public speaking you must be able to manage this server.");
                        break;
                }
                return;
            }

            // Set channel to 0.
            client.SpeakingChannels[Context.Guild.Id] = 0;
            client.SaveSettings();

            // Select and send message.
            switch (client.GetRandomNumber(4))
            {
                default:
                    await ReplyAsync($"I'll no longer speak in any channel.");
                    break;

                case 1:
                    await ReplyAsync($"I won't make announcements anymore.");
                    break;

                case 2:
                    await ReplyAsync($"When new builds are released, I'll keep quiet.");
                    break;

                case 3:
                    await ReplyAsync($"I'll stay quiet from now on.");
                    break;

                case 4:
                    await ReplyAsync($"I'll no longer announce builds.");
                    break;
            }
        }


        /// <summary>
        /// Send an announcement.
        /// </summary>
        /// <returns></returns>
        [Command(Constants.AnnouncementCommand)]
        public async Task SendAnnouncementAsync(string message)
        {
            // Bot is typing.
            await Context.Channel.TriggerTypingAsync();

            // Pause for realism.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get the user.
            var user = Context.Message.Author as IUser;

            // If the user is not master, show error.
            if (user?.Id != Constants.OwnerId)
            {
                // Select and send message.
                switch (client.GetRandomNumber(4))
                {
                    default:
                        await ReplyAsync($"Sorry, but only my master can send announcements.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You aren't my owner.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.Message.Author.Mention}, I'm afraid I can't do that. You aren't my master.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. Only my owner can send announcements.");
                        break;
                }
                return;
            }

            // Send message.
            await ReplyAsync($"I'll announce the following message to all my servers:\n{message}");

            // Log message.
            client.LogOutput($"ANNOUNCING: {message}");

            // Get guilds.
            var guilds = client.Guilds;

            // Announce in the specified channel of each guild.
            foreach (var guild in client.Guilds)
            {
                // Get channel.
                var channel = client.GetSpeakingChannelForSocketGuild(guild);

                // If the channel is null, continue on to the next guild.
                if (channel == null)
                {
                    client.LogOutput($"ROLLING OVER SERVER (NO SPEAKING): {guild.Name}");
                    continue;
                }

                // Verify we have permission to speak.
                if (!guild.CurrentUser.GetPermissions(channel).SendMessages)
                {
                    client.LogOutput($"ROLLING OVER SERVER (NO PERMS): {guild.Name}");
                    continue;
                }

                try
                {
                    // Wait 2 seconds.
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    // Send typing message.
                    await channel.TriggerTypingAsync();

                    // Pause for realism.
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    // Send message.
                    await channel.SendMessageAsync($"Announcement from <@{Constants.OwnerId.ToString()}> (bot owner):\n{message}");
                }
                catch (HttpException ex)
                {
                    client.LogOutput($"FAILURE IN SPEAKING FOR {guild.Name}: {ex}");
                }

                // Log server.
                client.LogOutput($"SPOKEN IN SERVER: {guild.Name}");
            }
        }

        #endregion
    }
}
