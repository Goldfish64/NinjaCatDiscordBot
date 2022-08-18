/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: InteractionModuleBase.cs
* 
* Copyright (c) 2022 John Davis
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
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot {
    /// <summary>
    /// Base class for bot interactions.
    /// </summary>
    public abstract class CatInteractionModuleBase : InteractionModuleBase<ShardedInteractionContext> {
        public NinjaCatDiscordClient CatClient {
            get { return Context.Client as NinjaCatDiscordClient; }
        }

        protected Task RespondRandomWithButtonsAsync(Embed embed, MessageComponent components, params string[] messages) {
            if (messages == null)
                throw new ArgumentNullException();

            var index = CatClient.GetRandomNumber(messages.Length - 1);
            return RespondAsync(messages[index], embed : embed, components: components);
        }

        protected Task RespondRandomAsync(Embed embed, params string[] messages) {
            return RespondRandomWithButtonsAsync(embed, null, messages);
        }

        protected async Task<bool> CheckIfGuild() {
            // Get guild. If null show error.
            if (Context.Guild == null) {
                await RespondAsync($"This command can't be used here. Run this command again from a server channel.");
                return false;
            }

            // If the guild is the Bots server, never speak.
            if (Context.Guild.Id == Constants.BotsGuildId) {
                // Send message.
                await RespondAsync($"Because this is the bots server, this command can't be used here.");
                return false;
            }

            return true;
        }

        protected async Task<bool> CheckManagerPerms() {
            // Bot settings require the Manager Server permission, or owner ID override.
            var user = Context.User as IGuildUser;
            if (user?.Id != Constants.OwnerId && user?.GuildPermissions.ManageGuild != true) {
                await ReplyAsync($"Sorry, but only those who have the **Manage Server** permission can modify bot settings.");
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Base class for bot interactions - roles.
    /// </summary>
    public abstract class RoleInteractionModuleBase : CatInteractionModuleBase {
        private async Task<string> GetRoleTypeName(RoleType roleType) {
            if (!await CheckIfGuild())
                return string.Empty;

            //
            // Ensure a channel is setup.
            //
            var insiderChannel = CatClient.GetSpeakingChannelForSocketGuild(Context.Guild);
            if (insiderChannel == null) {
                await RespondAsync($"I'm not currently announcing Insider builds when they are released. Please set up a channel first.");
                return string.Empty;
            }

            var roleTypeName = string.Empty;
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
            return roleTypeName;
        }

        protected async Task ProcessGetRole(RoleType roleType) {
            var roleTypeName = await GetRoleTypeName(roleType);
            if (roleTypeName == string.Empty) {
                return;
            }

            //
            // Get current role setting. No stored setting means there are no role pings for this channel.
            //
            IRole insiderRole = CatClient.GetRoleForIGuild(Context.Guild, roleType);
            if (insiderRole == null)
                await RespondAsync($"When new **{roleTypeName}** Insider builds are released, I'm not mentioning a role.");
            else
                await RespondAsync($"When new **{roleTypeName}** Insider builds are released, I'll mention {insiderRole.Mention}.");
        }

        protected async Task ProcessSetRole(IRole insiderRole, RoleType roleType) {
            var roleTypeName = await GetRoleTypeName(roleType);
            if (roleTypeName == string.Empty) {
                return;
            }

            if (!await CheckManagerPerms())
                return;

            //
            // Validate and set role.
            //
            var insiderChannel = CatClient.GetSpeakingChannelForSocketGuild(Context.Guild);
            if (!insiderRole.IsMentionable && !Context.Guild.CurrentUser.GetPermissions(insiderChannel).MentionEveryone) {
                await RespondAsync($"I can't mention this role. Please ensure I have the **Mention Everyone** permission in the {insiderChannel.Mention} channel, or make the role mentionable.");
                return;
            }
            CatClient.SetRole(Context.Guild, insiderRole, roleType);
            await RespondAsync($"When new **{roleTypeName}** Insider builds are released, I'll now mention {insiderRole.Mention}.");
        }

        protected async Task ProcessOffRole(RoleType roleType) {
            var roleTypeName = await GetRoleTypeName(roleType);
            if (roleTypeName == string.Empty) {
                return;
            }

            if (!await CheckManagerPerms())
                return;

            CatClient.SetRole(Context.Guild, null, roleType);
            await RespondAsync($"When new **{roleTypeName}** Insider builds are released, I'll no longer mention a role.");
        }

    }
}
