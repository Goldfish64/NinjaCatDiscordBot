/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: AdminCommands.cs
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
using Discord.Commands;
using Discord.WebSocket;
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
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get channel.
            var channel = await client.GetSpeakingChannelForIGuildAsync(Context.Guild);
            var currentUser = client.GetGuild(Context.Guild.Id).CurrentUser;

            // If the channel is null, return message saying that speaking is disabled.
            if (channel == null)
            {
                await ReplyAsync($"I'm not currently speaking in any channels.");
                return;
            }

            // Verify we have permission to speak.
            if (!currentUser.GetPermissions(channel).SendMessages)
            {
                await ReplyAsync($"I don't have permission to send messages in {channel.Mention}. Please give me that permission.");
                return;
            }

            // Check role permissions to toggle mentionable flag on/off.
            var role = client.GetSpeakingRoleForIGuild(Context.Guild);
            var roleText = "";
            if (role?.IsMentionable == false && (!currentUser.GuildPermissions.ManageRoles || currentUser.Hierarchy <= role.Position))
                roleText = $"\n\nHowever, I cannot manage the **{role.Name}** role. Please ensure I'm above that role and have permission to manage roles.";

            await ReplyAsync($"I'm all set to speak in {channel.Mention}!{roleText}");
            return;
        }

        /// <summary>
        /// Gets the channel the bot will speak in.
        /// </summary>
        [Command(Constants.ChannelCommand)]
        public async Task GetChannelAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null)
            {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
            {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't announce builds.");
                return;
            }

            // Get channel.
            var channel = await client.GetSpeakingChannelForIGuildAsync(guild);

            // If the channel is still null, that means no announcements.
            if (channel == null)
                await ReplyAsync($"When new builds are released, I'm keeping quiet.");
            else
                await ReplyAsync($"When new builds are released, I'll announce them in {channel.Mention}.");
        }

        /// <summary>
        /// Gets the role the bot will ping when builds come out.
        /// </summary>
        [Command(Constants.RoleCommand)]
        public async Task GetRoleAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null)
            {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
            {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't ping anyone.");
                return;
            }

            // Get role.
            var role = client.GetSpeakingRoleForIGuild(guild);
            if (role == null)
                await ReplyAsync($"I'm not pinging a role when new builds come out.");
            else
                await ReplyAsync($"The role I ping is **{role.Name}** when new builds are released.");
        }

        /// <summary>
        /// Gets the role the bot will ping when skip ahead builds come out.
        /// </summary>
        [Command(Constants.RoleSkipCommand)]
        public async Task GetRoleSkipAsync()
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null)
            {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
            {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't ping anyone.");
                return;
            }

            // Get role.
            var role = client.GetSpeakingRoleSkipForIGuild(guild);
            if (role == null)
                await ReplyAsync($"I'm not pinging a special role when new skip ahead builds come out.");
            else
                await ReplyAsync($"The role I ping is **{role.Name}** when new skip ahead builds are released.");
        }

        /// <summary>
        /// Gets the role required for jumbo.
        /// </summary>
        [Command(Constants.RoleJumboCommand)]
        public async Task GetRoleJumbo() {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null) {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId) {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I am off.");
                return;
            }

            // Get role.
            var role = client.GetJumboRoleForIGuild(guild);
            if (role == null)
                await ReplyAsync($"I don't require a special role for the jumbo command.");
            else
                await ReplyAsync($"To use the jumbo command, you must have **{role.Name}**.");
        }

        /// <summary>
        /// Sets the bot's speaking channel.
        /// </summary>
        /// <param name="channel">The new channel.</param>
        [Command(Constants.SetChannelCommand)]
        public async Task SetChannelAsync(ITextChannel channel = null)
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null)
            {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
            {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't make announcements.");
                return;
            }

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage server permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true)
            {
                await ReplyAsync($"Sorry, but only those who have permission to manage this server can change the channel I speak in.");
                return;
            }

            // If channel is valid, enable announcements.
            if (channel != null)
            {
                // Save channel.
                client.SpeakingChannels[guild.Id] = channel.Id;
                client.SaveSettings();
                await ReplyAsync($"When new builds are released, I'll now announce them in {channel.Mention}.");
            }
            else
            {
                // Set channel to 0.
                client.SpeakingChannels[guild.Id] = 0;
                client.SaveSettings();
                await ReplyAsync($"When new builds are released, I'll now keep quiet.");
            }
        }

        /// <summary>
        /// Sets the role that is pinged when new builds are released.
        /// </summary>
        /// <param name="role">The new role. A null value will disable role pinging.</param>
        [Command(Constants.SetRoleCommand)]
        public async Task SetRoleAsync(IRole role = null)
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null)
            {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
            {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't ping anyone.");
                return;
            }

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage server permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true)
            {
                await ReplyAsync($"Sorry, but only those who have permission to manage this server can change the role I ping.");
                return;
            }

            // If role is valid, save it. Otherwise remove the role.
            if (role != null)
            {
                // Save role.
                client.SpeakingRoles[guild.Id] = role.Id;
                client.SaveSettings();
                await ReplyAsync($"The role I'll ping from now on is **{role.Name}** when new builds are released.");
            }
            else
            {
                // Remove role.
                client.SpeakingRoles.TryRemove(guild.Id, out ulong outVar);
                client.SaveSettings();
                await ReplyAsync($"I'll no longer ping a role when new builds come out.");
            }
        }

        /// <summary>
        /// Sets the role that is pinged when new skip ahead builds are released.
        /// </summary>
        /// <param name="role">The new role. A null value will disable role pinging.</param>
        [Command(Constants.SetRoleSkipCommand)]
        public async Task SetRoleSkipAsync(IRole role = null)
        {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null)
            {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId)
            {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I can't ping anyone.");
                return;
            }

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage server permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true)
            {
                await ReplyAsync($"Sorry, but only those who have permission to manage this server can change the role I ping.");
                return;
            }

            // If role is valid, save it. Otherwise remove the role.
            if (role != null)
            {
                // Save role.
                client.SpeakingRolesSkip[guild.Id] = role.Id;
                client.SaveSettings();
                await ReplyAsync($"The role I'll ping from now on is **{role.Name}** when new skip ahead builds are released.");
            }
            else
            {
                // Remove role.
                client.SpeakingRolesSkip.TryRemove(guild.Id, out ulong outVar);
                client.SaveSettings();
                await ReplyAsync($"I'll no longer ping a role when new skip ahead builds come out.");
            }
        }

        /// <summary>
        /// Sets the role that is pinged when new skip ahead builds are released.
        /// </summary>
        /// <param name="role">The new role. A null value will disable role pinging.</param>
        [Command(Constants.SetRoleJumboCommand)]
        public async Task SetRoleJumboAsync(IRole role = null) {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null) {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId) {
                // Send message.
                await ReplyAsync($"Because this is the bots server, I'm off.");
                return;
            }

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage server permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true) {
                await ReplyAsync($"Sorry, but only those who have permission to manage this server can change the jumbo role.");
                return;
            }

            // If role is valid, save it. Otherwise remove the role.
            if (role != null) {
                // Save role.
                client.JumboRoles[guild.Id] = role.Id;
                client.SaveSettings();
                await ReplyAsync($"I'll now require users to have **{role.Name}** in order to use the jumbo command.");
            }
            else {
                // Remove role.
                client.JumboRoles.TryRemove(guild.Id, out ulong outVar);
                client.SaveSettings();
                await ReplyAsync($"I'll no longer require a role for the jumbo command.");
            }
        }

        /// <summary>
        /// Send an announcement.
        /// </summary>
        [Command(Constants.AnnouncementCommand)]
        public async Task SendAnnouncementAsync(string message)
        {
            // Bot is typing, with added pause for realism.
            await Context.Channel.TriggerTypingAsync();
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

            // Send message to shards.
            foreach (var shard in client.Shards)
                SendMessageShardAsync(client, shard, message);
        }

        /// <summary>
        /// Force-update the bot's game.
        /// </summary>
        [Command(Constants.UpdateGameCommand)]
        public async Task UpdateGameAsync()
        {
            // Bot is typing, with added pause for realism.
            await Context.Channel.TriggerTypingAsync();
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
                        await ReplyAsync($"Sorry, but only my master can force-update my game.");
                        break;

                    case 1:
                        await ReplyAsync($"No can do. You aren't my owner.");
                        break;

                    case 2:
                        await ReplyAsync($"I'm sorry {Context.Message.Author.Mention}, I'm afraid I can't do that. You aren't my master.");
                        break;

                    case 3:
                        await ReplyAsync($"Not happening. Only my owner can force-update my game.");
                        break;
                }
                return;
            }

            // Send message.
            await ReplyAsync($"Forcing game update now...");

            // Update game.
            await client.UpdateGameAsync();
        }

        /// <summary>
        /// Test ping..
        /// </summary>
        [Command(Constants.TestPingCommand)]
        public async Task TestPingAsync()
        {
            // Bot is typing, with added pause for realism.
            await Context.Channel.TriggerTypingAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage server permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true)
            {
                await ReplyAsync($"Sorry, but only those who have permission to manage this server can send a test ping.");
                return;
            }

            // Get current guild user.
            var currentUser = client.GetGuild(Context.Guild.Id).CurrentUser;

            // Check if the role is mentionable.
            // If not, attempt to make it mentionable, and revert the setting after the message is sent.
            var role = client.GetSpeakingRoleForIGuild(Context.Guild);
            if (role != null)
            {
                var mentionable = role?.IsMentionable;
                if (mentionable == false && currentUser.GuildPermissions.ManageRoles && currentUser.Hierarchy > role.Position)
                    await role.ModifyAsync((e) => e.Mentionable = true);

                // Send message.
                await ReplyAsync($"Insiders role: {role.Mention}");

                // Revert mentionable setting.
                if (mentionable == false && currentUser.GuildPermissions.ManageRoles && currentUser.Hierarchy > role.Position)
                    await role.ModifyAsync((e) => e.Mentionable = false);
            }
            else
            {
                await ReplyAsync($"Insiders role: No role configured.");
            }

            // Check if the skip role is mentionable.
            // If not, attempt to make it mentionable, and revert the setting after the message is sent.
            var roleSkip = client.GetSpeakingRoleSkipForIGuild(Context.Guild);
            if (roleSkip != null)
            {
                var mentionableSkip = role?.IsMentionable;
                if (mentionableSkip == false && currentUser.GuildPermissions.ManageRoles && currentUser.Hierarchy > roleSkip.Position)
                    await roleSkip.ModifyAsync((e) => e.Mentionable = true);

                // Send message.
                await ReplyAsync($"Skip ahead role: {roleSkip.Mention}");

                // Revert mentionable setting.
                if (mentionableSkip == false && currentUser.GuildPermissions.ManageRoles && currentUser.Hierarchy > roleSkip.Position)
                    await roleSkip.ModifyAsync((e) => e.Mentionable = false);
            }
            else
            {
                await ReplyAsync($"Skip ahead role: No role configured.");
            }
        }

        private async void SendMessageShardAsync(NinjaCatDiscordClient client, DiscordSocketClient shard, string message)
        {
            // Announce in the specified channel of each guild.
            foreach (var guild in shard.Guilds)
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

                    // Bot is typing, with added pause for realism.
                    await Context.Channel.TriggerTypingAsync();
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    // Send message.
                    await channel.SendMessageAsync($"Announcement from **{Constants.OwnerName}** (bot owner):\n{message}");
                }
                catch (Exception ex)
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
