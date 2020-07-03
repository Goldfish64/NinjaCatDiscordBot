/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: AdminCommands.cs
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
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot {
    /// <summary>
    /// Contains commands for the bot.
    /// </summary>
    public class AdminCommandModule : CommandModuleBase {
        #region Private methods

        private bool IsStringNone(string text) {
            return (text.ToLowerInvariant() == "none" || text.ToLowerInvariant() == "off");
        }

        private async Task<bool> CheckIfGuild() {
            // Get guild. If null show error.
            var guild = (Context.Channel as IGuildChannel)?.Guild;
            if (guild == null) {
                await ReplyAsync($"This command can't be used here. Run this command again from a server channel.");
                return false;
            }

            // If the guild is the Bots server, never speak.
            if (guild.Id == Constants.BotsGuildId) {
                // Send message.
                await ReplyAsync($"Because this is the bots server, this command can't be used here.");
                return false;
            }

            return true;
        }

        private async Task<bool> CheckManagerPerms() {
            // Bot settings require the Manager Server permission, or owner ID override.
            var user = Context.User as IGuildUser;
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true) {
                await ReplyAsync($"Sorry, but only those who have the **Manage Server** permission can modify bot settings.");
                return false;
            }
            return true;
        }

        private async Task ProcessRoleCommand(string role, RoleType roleType) {
            if (!await CheckIfGuild())
                return;

            IRole insiderRole;
            string roleTypeName = "";
            switch (roleType) {
                case RoleType.InsiderDev:
                    roleTypeName = "Dev Channel";
                    break;

                case RoleType.InsiderBeta:
                    roleTypeName = "Beta Channel";
                    break;

                case RoleType.InsiderReleasePreview:
                    roleTypeName = "Release Preview Channel";
                    break;
            }

            //
            // Ensure a channel is setup.
            //
            var insiderChannel = Context.Client.GetSpeakingChannelForSocketGuild(Context.Guild);
            if (insiderChannel == null) {
                await ReplyAsync($"I'm not currently announcing Insider builds when they are released. Please set up a channel using **{Constants.CommandPrefix}{Constants.ChannelCommand}* *.");
                return;
            }

            //
            // Get current role setting. No stored setting means there are no role pings for this channel.
            //
            if (string.IsNullOrWhiteSpace(role)) {
                insiderRole = Context.Client.GetRoleForIGuild(Context.Guild, roleType);
                if (insiderRole == null)
                    await ReplyAsync($"When new **{roleTypeName}** Insider builds are released, I'm not mentioning a role.");
                else
                    await ReplyAsync($"When new **{roleTypeName}** Insider builds are released, I'll mention {insiderRole.Mention}.");
                return;
            }

            if (!await CheckManagerPerms())
                return;

            //
            // Disable role if none or off is passed.
            //
            if (IsStringNone(role)) {
                Context.Client.SetInsiderRole(Context.Guild, null, roleType);
                await ReplyAsync($"When new **{roleTypeName}** Insider builds are released, I'll no longer mention a role.");
                return;
            }

            //
            // Role was passed, attempt to parse.
            //
            var roleReaderResult = await new RoleTypeReader<IRole>().ReadAsync(Context, role, null);
            if (!roleReaderResult.IsSuccess) {
                await ReplyAsync($"That role couldn't be found.");
                return;
            }
            insiderRole = roleReaderResult.BestMatch as IRole;

            //
            // Validate and set channel.
            //
            if (!insiderRole.IsMentionable && !Context.Guild.CurrentUser.GetPermissions(insiderChannel).MentionEveryone) {
                await ReplyAsync($"I can't mention this role. Please ensure I have the **Mention Everyone** permission in the {insiderChannel.Mention} channel, or make the role mentionable.");
                return;
            }
            Context.Client.SetInsiderRole(Context.Guild, insiderRole, roleType);
            await ReplyAsync($"When new **{roleTypeName}** Insider builds are released, I'll now mention {insiderRole.Mention}.");
        }

        #endregion

        #region Command methods

        [Command(Constants.ChannelCommand)]
        [Summary("When no channel is passed, gets the current setting. Otherwise, sets the channel used for Insider builds; specify *none* to disable build announcements.")]
        [Remarks(Constants.RemarkAdmin)]
        public async Task ChannelAsync([Remainder()] string channel = null) {
            ITextChannel insiderChannel;

            if (!await CheckIfGuild())
                return;

            //
            // Get current channel setting. No stored setting means announcements are disabled.
            //
            if (string.IsNullOrWhiteSpace(channel)) {
                insiderChannel = Context.Client.GetSpeakingChannelForSocketGuild(Context.Guild);
                if (insiderChannel == null)
                    await ReplyAsync($"When new Insider builds are released, I'm keeping quiet.");
                else
                    await ReplyAsync($"When new Insider builds are released, I'll announce them in {insiderChannel.Mention}.");
                return;
            }

            if (!await CheckManagerPerms())
                return;

            //
            // Disable announcements if none or off is passed.
            //
            if (IsStringNone(channel)) {
                Context.Client.SetInsiderChannel(Context.Guild, null);
                await ReplyAsync($"When new Insider builds are released, I'll now keep quiet.");
                return;
            }

            //
            // Channel was passed, attempt to parse.
            //
            var channelReaderResult = await new ChannelTypeReader<ITextChannel>().ReadAsync(Context, channel, null);
            if (!channelReaderResult.IsSuccess) {
                await ReplyAsync($"That channel couldn't be found.");
                return;
            }
            insiderChannel = channelReaderResult.BestMatch as ITextChannel;

            //
            // Validate and set channel.
            //
            if (!Context.Guild.CurrentUser.GetPermissions(insiderChannel).SendMessages) {
                await ReplyAsync($"I can't send messages there. Please ensure I have the **Send Messages** permission in that channel.");
                return;
            }
            Context.Client.SetInsiderChannel(Context.Guild, insiderChannel);
            await ReplyAsync($"When new Insider builds are released, I'll now announce them in {insiderChannel.Mention}.");
        }

        [Command("roledev")]
        [Alias("devrole")]
        [Summary("When no role is passed, gets the current setting. Otherwise, sets the role used for Dev Channel Insider builds; specify *none* to disable role mentions.")]
        [Remarks(Constants.RemarkAdmin)]
        public async Task RoleDevAsync([Remainder()] string role = null) {
            await ProcessRoleCommand(role, RoleType.InsiderDev);
        }

        [Command("rolebeta")]
        [Alias("betarole")]
        [Summary("When no role is passed, gets the current setting. Otherwise, sets the role used for Beta Channel Insider builds; specify *none* to disable role mentions.")]
        [Remarks(Constants.RemarkAdmin)]
        public async Task RoleBetaAsync([Remainder()] string role = null) {
            await ProcessRoleCommand(role, RoleType.InsiderBeta);
        }

        [Command("rolerp")]
        [Alias("rprole", "releasepreviewrole", "rolereleasepreview")]
        [Summary("When no role is passed, gets the current setting. Otherwise, sets the role used for Release Preview Channel Insider builds; specify *none* to disable role mentions.")]
        [Remarks(Constants.RemarkAdmin)]
        public async Task RoleReleasePreviewAsync([Remainder()] string role = null) {
            await ProcessRoleCommand(role, RoleType.InsiderReleasePreview);
        }

        private async Task<NinjaCatDiscordClient> StartTypingAndGetClient() {
            // Bot is typing, with added pause for realism.
            await Context.Channel.TriggerTypingAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Get client.
            return Context.Client as NinjaCatDiscordClient;
        }

        /// <summary>
        /// Tests the permissions.
        /// </summary>
        /// <returns></returns>
        [Command(Constants.TestPermsCommand)]
        public async Task TestPermsAsync() {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get channel.
            var channel = await client.GetSpeakingChannelForIGuildAsync(Context.Guild);
            var currentUser = client.GetGuild(Context.Guild.Id).CurrentUser;

            // If the channel is null, return message saying that speaking is disabled.
            if (channel == null) {
                await ReplyAsync($"I'm not currently speaking in any channels.");
                return;
            }

            // Verify we have permission to speak.
            if (!currentUser.GetPermissions(channel).SendMessages) {
                await ReplyAsync($"I don't have permission to send messages in {channel.Mention}. Please give me that permission.");
                return;
            }

            // Check role permissions to toggle mentionable flag on/off.
            var role = client.GetRoleForIGuild(Context.Guild, RoleType.InsiderDev);
            var roleText = "";
            if (role?.IsMentionable == false && (!currentUser.GuildPermissions.ManageRoles || currentUser.Hierarchy <= role.Position))
                roleText = $"\n\nHowever, I cannot manage the **{role.Name}** role. Please ensure I'm above that role and have permission to manage roles.";

            await ReplyAsync($"I'm all set to speak in {channel.Mention}!{roleText}");
            return;
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
            var role = client.GetRoleForIGuild(guild, RoleType.Jumbo);
            if (role == null)
                await ReplyAsync($"I don't require a special role for the jumbo command.");
            else
                await ReplyAsync($"To use the jumbo command, you must have **{role.Name}**.");
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
                client.Settings.JumboRoles[guild.Id] = role.Id;
                client.SaveSettings();
                await ReplyAsync($"I'll now require users to have **{role.Name}** in order to use the jumbo command.");
            }
            else {
                // Remove role.
                client.Settings.JumboRoles.TryRemove(guild.Id, out ulong outVar);
                client.SaveSettings();
                await ReplyAsync($"I'll no longer require a role for the jumbo command.");
            }
        }

        /// <summary>
        /// Send an announcement.
        /// </summary>
        [Command(Constants.AnnouncementCommand)]
        [Remarks(Constants.RemarkInternal)]
        public async Task SendAnnouncementAsync(string message) {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get the user.
            var user = Context.Message.Author as IUser;

            // If the user is not master, show error.
            if (user?.Id != Constants.OwnerId) {
                // Select and send message.
                switch (client.GetRandomNumber(4)) {
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
            client.LogInfo($"Announcing {message}");

            // Send message to shards.
            foreach (var shard in client.Shards)
                SendMessageShardAsync(client, shard, message);
        }

        /// <summary>
        /// Force-update the bot's game.
        /// </summary>
        [Command(Constants.UpdateGameCommand)]
        [Remarks(Constants.RemarkInternal)]
        public async Task UpdateGameAsync() {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get the user.
            var user = Context.Message.Author as IUser;

            // If the user is not master, show error.
            if (user?.Id != Constants.OwnerId) {
                // Select and send message.
                switch (client.GetRandomNumber(4)) {
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
        public async Task TestPingAsync() {
            // Get client.
            var client = await StartTypingAndGetClient();

            // Get the user.
            var user = Context.User as IGuildUser;

            // If the user is null, lacks the manage server permission, or is not master, show error.
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true) {
                await ReplyAsync($"Sorry, but only those who have permission to manage this server can send a test ping.");
                return;
            }

            // Get current guild user.
            var currentUser = client.GetGuild(Context.Guild.Id).CurrentUser;

            // Check if the role is mentionable.
            // If not, attempt to make it mentionable, and revert the setting after the message is sent.
            var role = client.GetRoleForIGuild(Context.Guild, RoleType.InsiderDev);
            if (role != null) {
               // var mentionable = role?.IsMentionable;
             //   if (mentionable == false && currentUser.GuildPermissions.ManageRoles && currentUser.Hierarchy > role.Position)
                 //   await role.ModifyAsync((e) => e.Mentionable = true);

                // Send message.
                await ReplyAsync($"Insiders role: {role.Mention}", allowedMentions : new AllowedMentions() { RoleIds = new System.Collections.Generic.List<ulong> { role.Id } });

                // Revert mentionable setting.
               // if (mentionable == false && currentUser.GuildPermissions.ManageRoles && currentUser.Hierarchy > role.Position)
                //    await role.ModifyAsync((e) => e.Mentionable = false);
            }
            else {
                await ReplyAsync($"Insiders role: No role configured.");
            }

            // Check if the skip role is mentionable.
            // If not, attempt to make it mentionable, and revert the setting after the message is sent.
            var roleSkip = client.GetRoleForIGuild(Context.Guild, RoleType.InsiderDev);
            if (roleSkip != null) {
                var mentionableSkip = roleSkip?.IsMentionable;
              //  if (mentionableSkip == false && currentUser.GuildPermissions.ManageRoles && currentUser.Hierarchy > roleSkip.Position)
               //     await roleSkip.ModifyAsync((e) => e.Mentionable = true);

                // Send message.
                await ReplyAsync($"Skip ahead role: {roleSkip.Mention}");

                // Revert mentionable setting.
              //  if (mentionableSkip == false && currentUser.GuildPermissions.ManageRoles && currentUser.Hierarchy > roleSkip.Position)
              //      await roleSkip.ModifyAsync((e) => e.Mentionable = false);
            }
            else {
                await ReplyAsync($"Skip ahead role: No role configured.");
            }
        }

        private async void SendMessageShardAsync(NinjaCatDiscordClient client, DiscordSocketClient shard, string message) {
            // Announce in the specified channel of each guild.
            foreach (var guild in shard.Guilds) {
                // Get channel.
                var channel = client.GetSpeakingChannelForSocketGuild(guild);

                // If the channel is null, continue on to the next guild.
                if (channel == null) {
                    client.LogInfo($"Rolling over server (disabled) {guild.Name}");
                    continue;
                }

                // Verify we have permission to speak.
                if (!guild.CurrentUser.GetPermissions(channel).SendMessages) {
                    client.LogInfo($"Rolling over server (no perms) {guild.Name}");
                    continue;
                }

                try {
                    // Wait 2 seconds.
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    // Bot is typing, with added pause for realism.
                    await Context.Channel.TriggerTypingAsync();
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    // Send message.
                    await channel.SendMessageAsync($"Announcement from **{Constants.OwnerName}** (bot owner):\n{message}");
                }
                catch (Exception ex) {
                    client.LogError($"Failed to speak in {guild.Name}: {ex}");
                }
                client.LogInfo($"Spoke in server {guild.Name}");
            }
        }


        /// <summary>
        /// Test ping..
        /// </summary>
        [Command("restart")]
        [Remarks(Constants.RemarkInternal)]
        public async Task RestartAsync() {
            // Get client.
            var client = Context.Client as NinjaCatDiscordClient;

            // Get the user.
            var user = Context.Message.Author as IUser;

            // If the user is not master, show error.
            if (user?.Id != Constants.OwnerId) {
                // Select and send message.
                switch (client.GetRandomNumber(4)) {
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

            // Shutdown bot.
            await ReplyAsync($"Exiting...");
            Environment.Exit(-1);
        }

        #endregion
    }
}
