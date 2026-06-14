/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: BotCommands.cs
* 
* Copyright (c) 2016 - 2026 John Davis
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
using Discord.WebSocket;
using NinjaCatDiscordBot.Models;
using NinjaCatDiscordBot.Properties;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot {
  /// <summary>
  /// Contains commands for the bot.
  /// </summary>
  public partial class BotCommandsModuleNew : CatInteractionModuleBase {
    #region Variables

    private readonly HttpClient httpClient = new HttpClient();

    #endregion

    #region Private methods

    private async Task GetLatestBuildsAsync(InsiderBuildType buildType) {
      await RespondAsync($"Getting the latest {InsiderBuild.Names[buildType]} builds...");
      var builds = await CatClient.GetLatestInsiderBuildsAsync(buildType);
      if (builds?.Count == 0) {
        await ModifyOriginalResponseAsync((m) => { m.Content = $"The latest {InsiderBuild.Names[buildType]} builds couldn't be found. :crying_cat_face: :{InsiderBuild.Emotes[buildType]}:"; });
        return;
      }

      var message = $"Here's the latest {InsiderBuild.Names[buildType]} builds: :cat: :{InsiderBuild.Emotes[buildType]}:\n\n";
      foreach (var build in builds) {
        message += $"**{build.GetDisplayName()}** ({build.BuildNumber}): <{build.Link}>\n";
      }

      await ModifyOriginalResponseAsync((m) => { m.Content = message; });
    }

    #endregion

    #region Command methods

    #region Channel command group

    /// <summary>
    /// Channel command group (get/set/off).
    /// </summary>
    [Group("channel", "Channel commands")]
    public class ChannelCommandsModule : CatInteractionModuleBase {
      /// <summary>
      /// Get announcement channel.
      /// </summary>
      [SlashCommand("get", "Gets the channel used for Insider build announcements")]
      public async Task GetChannelAsync() {
        if (!await CheckIfGuild())
          return;

        //
        // Get current channel setting. No stored setting means announcements are disabled.
        //
        ITextChannel insiderChannel = CatClient.GetSpeakingChannelForSocketGuild(Context.Guild);
        if (insiderChannel == null)
          await RespondAsync($"When new Insider builds are released, I'm keeping quiet.");
        else
          await RespondAsync($"When new Insider builds are released, I'll announce them in {insiderChannel.Mention}.");
      }

      /// <summary>
      /// Set announcement channel.
      /// </summary>
      [SlashCommand("set", "Sets the channel used for Insider build announcements")]
      public async Task SetChannelAsync(ITextChannel channel) {
        if (!await CheckIfGuild() || !await CheckManagerPerms())
          return;

        //
        // Validate and set channel.
        //
        if (!Context.Guild.CurrentUser.GetPermissions(channel).SendMessages) {
          await RespondAsync($"I can't send messages there. Please ensure I have the **Send Messages** permission in that channel.");
          return;
        }
        CatClient.SetInsiderChannel(Context.Guild, channel);
        await RespondAsync($"When new Insider builds are released, I'll now announce them in {channel.Mention}.");
      }

      /// <summary>
      /// Turn off announcement channel.
      /// </summary>
      [SlashCommand("off", "Disables Insider build announcements")]
      public async Task OffChannelAsync() {
        if (!await CheckIfGuild() || !await CheckManagerPerms())
          return;

        //
        // Disable announcements.
        //
        CatClient.SetInsiderChannel(Context.Guild, null);
        await RespondAsync($"When new Insider builds are released, I'll now keep quiet.");
      }
    }

    #endregion

    #region Role command group

    /// <summary>
    /// Role command group (experimental/beta/rp).
    /// </summary>
    [Group("role", "Role commands")]
    public class RoleCommandsModule : CatInteractionModuleBase {
      /// <summary>
      /// Experimental role command group (get/set/off).
      /// </summary>
      [Group("experimental", "Experimental role commands")]
      public class ExperimentalRoleCommandsModule : RoleInteractionModuleBase {
        /// <summary>
        /// Get dev role.
        /// </summary>
        [SlashCommand("get", "Gets the role that will be mentioned for Experimental Channel Insider build announcements")]
        public async Task GetExperimentalRoleAsync() {
          await ProcessGetRole(RoleType.InsiderExperimental);
        }

        /// <summary>
        /// Set dev role.
        /// </summary>
        [SlashCommand("set", "Sets the role that will be mentioned for Experimental Channel Insider build announcements")]
        public async Task SetExperimentalRoleAsync(IRole role) {
          await ProcessSetRole(role, RoleType.InsiderExperimental);
        }

        /// <summary>
        /// Turn off dev role.
        /// </summary>
        [SlashCommand("off", "Disables role mentions for Experimental Channel Insider build announcements")]
        public async Task OffExperimentalRoleAsync() {
          await ProcessOffRole(RoleType.InsiderExperimental);
        }
      }

      /// <summary>
      /// Beta role command group (get/set/off).
      /// </summary>
      [Group("beta", "Beta role commands")]
      public class BetaRoleCommandsModule : RoleInteractionModuleBase {
        /// <summary>
        /// Get dev role.
        /// </summary>
        [SlashCommand("get", "Gets the role that will be mentioned for Beta Channel Insider build announcements")]
        public async Task GetBetaRoleAsync() {
          await ProcessGetRole(RoleType.InsiderBeta);
        }

        /// <summary>
        /// Set dev role.
        /// </summary>
        [SlashCommand("set", "Sets the role that will be mentioned for Beta Channel Insider build announcements")]
        public async Task SetBetaRoleAsync(IRole role) {
          await ProcessSetRole(role, RoleType.InsiderBeta);
        }

        /// <summary>
        /// Turn off dev role.
        /// </summary>
        [SlashCommand("off", "Disables role mentions for Beta Channel Insider build announcements")]
        public async Task OffBetaRoleAsync() {
          await ProcessOffRole(RoleType.InsiderBeta);
        }
      }

      /// <summary>
      /// Release Preview role command group (get/set/off).
      /// </summary>
      [Group("releasepreview", "Release Preview role commands")]
      public class ReleasePreviewRoleCommandsModule : RoleInteractionModuleBase {
        /// <summary>
        /// Get release preview role.
        /// </summary>
        [SlashCommand("get", "Gets the role that will be mentioned for Release Preview Channel Insider build announcements")]
        public async Task GetReleasePreviewRoleAsync() {
          await ProcessGetRole(RoleType.InsiderReleasePreview);
        }

        /// <summary>
        /// Set release preview role.
        /// </summary>
        [SlashCommand("set", "Sets the role that will be mentioned for Release Preview Channel Insider build announcements")]
        public async Task SetReleasePreviewRoleAsync(IRole role) {
          await ProcessSetRole(role, RoleType.InsiderReleasePreview);
        }

        /// <summary>
        /// Turn off release preview role.
        /// </summary>
        [SlashCommand("off", "Disables role mentions for Release Preview Channel Insider build announcements")]
        public async Task OffReleasePreviewRoleAsync() {
          await ProcessOffRole(RoleType.InsiderReleasePreview);
        }
      }

      /// <summary>
      /// Jumbo role command group (get/set/off).
      /// </summary>
      [Group("jumbo", "Jumbo role commands")]
      public class JumboRoleCommandsModule : RoleInteractionModuleBase {
        /// <summary>
        /// Get jumbo role.
        /// </summary>
        [SlashCommand("get", "Gets the role, if any, that is required to use the jumbo command")]
        public async Task GetJumboRoleAsync() {
          if (!await CheckIfGuild()) {
            return;
          }

          var role = CatClient.GetRoleForIGuild(Context.Guild, RoleType.Jumbo);
          if (role == null)
            await RespondAsync($"I don't require a special role for the jumbo command.");
          else
            await RespondAsync($"To use the jumbo command, you must have **{role.Name}**.");
        }

        /// <summary>
        /// Set jumbo role.
        /// </summary>
        [SlashCommand("set", "Sets the role that will be required to use the jumbo command")]
        public async Task SetJumboRoleAsync(IRole role) {
          if (!await CheckIfGuild() || !await CheckManagerPerms()) {
            return;
          }

          CatClient.SetRole(Context.Guild, role, RoleType.Jumbo);
          await RespondAsync($"I'll now require users to have **{role.Name}** in order to use the jumbo command.");
        }

        /// <summary>
        /// Turn off jumbo role.
        /// </summary>
        [SlashCommand("off", "Disables the role requirement to use the jumbo command")]
        public async Task OffJumboRoleAsync() {
          if (!await CheckIfGuild() || !await CheckManagerPerms()) {
            return;
          }

          CatClient.SetRole(Context.Guild, null, RoleType.Jumbo);
          await RespondAsync($"I'll no longer require a role for the jumbo command.");
        }
      }
    }

    #endregion

    #region Test command group

    /// <summary>
    /// Role command group (dev/beta/rp).
    /// </summary>
    [Group("test", "Test functionality commands")]
    public class TestCommandsModule : CatInteractionModuleBase {
      /// <summary>
      /// Tests announcement permissions.
      /// </summary>
      /// <returns></returns>
      [SlashCommand("perms", "Test announcement permissions")]
      public async Task TestPermsAsync() {
        if (!await CheckIfGuild()) {
          return;
        }

        var channel = CatClient.GetSpeakingChannelForSocketGuild(Context.Guild);
        var currentUser = Context.Client.GetGuild(Context.Guild.Id).CurrentUser;

        if (channel == null) {
          await RespondAsync($"When new Insider builds are released, I'm keeping quiet.");
          return;
        }
        var sb = new StringBuilder();
        sb.AppendLine($"Channel: {channel.Mention}; can speak: {currentUser.GetPermissions(channel).SendMessages}");

        // Get all roles.
        var roleExperimental = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderExperimental);
        var roleBeta = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderBeta);
        var roleReleasePreview = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderReleasePreview);

        var mentionability = true;
        if (roleExperimental != null) {
          var experimentalMentionability = roleExperimental.IsMentionable || Context.Guild.CurrentUser.GetPermissions(channel).MentionEveryone;
          mentionability &= experimentalMentionability;
          sb.AppendLine($"Experimental role: {roleExperimental.Mention}; can mention: {experimentalMentionability}");
        }
        if (roleBeta != null) {
          var betaMentionability = roleBeta.IsMentionable || Context.Guild.CurrentUser.GetPermissions(channel).MentionEveryone;
          mentionability &= betaMentionability;
          sb.AppendLine($"Beta role: {roleBeta.Mention}; can mention: {betaMentionability}");
        }
        if (roleReleasePreview != null) {
          var rpMentionability = roleReleasePreview.IsMentionable || Context.Guild.CurrentUser.GetPermissions(channel).MentionEveryone;
          mentionability &= rpMentionability;
          sb.AppendLine($"Release Preview role: {roleReleasePreview.Mention}; can mention: {rpMentionability}");
        }

        if (!currentUser.GetPermissions(channel).SendMessages) {
          sb.AppendLine($"I can't send messages in {channel.Mention}. Please ensure I have the **Send Messages** permission in that channel.");
        }
        if (!mentionability)
          sb.AppendLine($"One or more roles cannot be mentioned. Please ensure I have the **Mention Everyone** permission in the {channel.Mention} channel, or make the role(s) mentionable.");
        await RespondAsync(sb.ToString());
      }

      /// <summary>
      /// Test ping.
      /// </summary>
      [SlashCommand("ping", "Test ping all announcement roles (manager only)")]
      public async Task TestPingAsync() {
        if (!await CheckIfGuild() || !await CheckManagerPerms()) {
          return;
        }
        await RespondAsync($"Pinging roles...");

        // Get all roles.
        var roleExperimental = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderExperimental);
        var roleBeta = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderBeta);
        var roleReleasePreview = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderReleasePreview);

        if (roleExperimental != null) {
          await ReplyAsync($"Experimental role: {roleExperimental.Mention}", allowedMentions: new AllowedMentions() { RoleIds = { roleExperimental.Id } });
        } else {
          await ReplyAsync($"Experimental role: none");
        }

        if (roleBeta != null) {
          await ReplyAsync($"Beta role: {roleBeta.Mention}", allowedMentions: new AllowedMentions() { RoleIds = { roleBeta.Id } });
        } else {
          await ReplyAsync($"Beta role: none");
        }

        if (roleReleasePreview != null) {
          await ReplyAsync($"Release Preview role: {roleReleasePreview.Mention}", allowedMentions: new AllowedMentions() { RoleIds = { roleReleasePreview.Id } });
        } else {
          await ReplyAsync($"Release Preview role: none");
        }
      }
    }

    #endregion

    /// <summary>
    /// Gets the homepage URL.
    /// </summary>
    [SlashCommand("source", "Go to my source code")]
    public async Task GetHomeAsync() {
      await RespondRandomAsync(null,
          $"My source code is here:\n{Constants.AppUrl}",
          $"Here is where my source code is stored:\n{Constants.AppUrl}",
          $"My source:\n{Constants.AppUrl}"
      );
    }

    /// <summary>
    /// Gets the invite URL.
    /// </summary>
    [SlashCommand("invite", "Invite me to your server")]
    public async Task GetInviteAsync() {
      var inviteUrl = string.Format(Constants.InviteUrl, Context.Client.CurrentUser.Id);
      await RespondRandomAsync(null,
          $"This link will let me be on *your* server:\n{inviteUrl}",
          $"So you want me in *your* server huh? Use this link:\n{inviteUrl}",
          $"Use this link to add me to *your* server:\n{inviteUrl}"
      );
    }

    /// <summary>
    /// Gets the T-Rex.
    /// </summary>
    [SlashCommand("trex", "Shows the Windows Skype emoticon")]
    public async Task GetTrexAsync() {
      using (var stream = new MemoryStream(Resources.trex))
        await RespondWithFileAsync(new FileAttachment(stream, "trex.gif"));
    }

    /// <summary>
    /// Jumbo.
    /// </summary>
    [SlashCommand("jumbo", "Jumboify emote")]
    public async Task SendJumboAsync(string emote) {
      // Check perms if on server.
      if (Context.Guild != null) {
        var role = CatClient.GetRoleForIGuild(Context.Guild, RoleType.Jumbo);
        var user = Context.User as SocketGuildUser;

        if (role != null && !user.Roles.Contains(role))
          return;
      }

      string emoteUrl = null;
      if (Emote.TryParse(emote, out Emote found)) {
        emoteUrl = found.Url;
      } else {
        int codepoint = Char.ConvertToUtf32(emote, 0);
        string codepointHex = codepoint.ToString("X").ToLower();

        emoteUrl = $"https://raw.githubusercontent.com/twitter/twemoji/gh-pages/v/13.0.1/72x72/{codepointHex}.png";
      }

      var req = await httpClient.GetStreamAsync(emoteUrl);
      await RespondWithFileAsync(new FileAttachment(req, Path.GetFileName(emoteUrl)));
    }

    /// <summary>
    /// Gets the latest Insider builds for the Experimental Channel.
    /// </summary>
    [SlashCommand("latestexperimental", "Shows the latest Experimental Insider builds")]
    public async Task GetLatestExperimentalBuildsAsync() {
      await GetLatestBuildsAsync(InsiderBuildType.Experimental);
    }

    /// <summary>
    /// Gets the latest Insider builds for the Beta Channel.
    /// </summary>
    [SlashCommand("latestbeta", "Shows the latest Beta Insider builds")]
    public async Task GetLatestBetaBuildsAsync() {
      await GetLatestBuildsAsync(InsiderBuildType.Beta);
    }

    /// <summary>
    /// Gets the latest Insider builds for the Release Preview Channel.
    /// </summary>
    [SlashCommand("latestreleasepreview", "Shows the latest Release Preview Insider builds")]
    public async Task GetLatestReleasePreviewBuildsAsync() {
      await GetLatestBuildsAsync(InsiderBuildType.ReleasePreview);
    }

    /// <summary>
    /// Gets the latest Insider server build.
    /// </summary>
    [SlashCommand("latestserver", "Shows the latest Server Insider build")]
    public async Task GetLatestServerBuildAsync() {
      await GetLatestBuildsAsync(InsiderBuildType.Server);
    }

    /// <summary>
    /// Replies with the bot's info.
    /// </summary>
    [SlashCommand("info", "Shows my info")]
    public async Task GetBotInfoAsync() {
      // Get passed time.
      var timeSpan = DateTime.Now.ToUniversalTime() - CatClient.StartTime.ToUniversalTime();

      // Create string. From http://stackoverflow.com/questions/1138723/timespan-to-friendly-string-library-c.
      var parts = new[]
          {
                    Tuple.Create("day", timeSpan.Days),
                    Tuple.Create("hour", timeSpan.Hours),
                    Tuple.Create("minute", timeSpan.Minutes),
                    Tuple.Create("second", timeSpan.Seconds)
                }.SkipWhile(i => i.Item2 <= 0);
      var timeString = string.Join(", ", parts.Select(p => string.Format("{0} {1}{2}", p.Item2, p.Item1, p.Item2 > 1 ? "s" : string.Empty)));

      // Build embed.
      var embed = new EmbedBuilder {
        Author = new EmbedAuthorBuilder(),
        Footer = new EmbedFooterBuilder()
      };
      embed.Author.IconUrl = Context.Client.CurrentUser?.GetAvatarUrl();
      embed.Footer.Text = $"Version: {Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";

      embed.AddField(e => { e.Name = "About"; e.Value = Constants.AboutMessage; });

      // Add general overview fields.
      var shardId = Context.Guild != null ? (Context.Client.GetShardIdFor(Context.Guild) + 1) : 1;
      embed.AddField(e => { e.Name = "Servers"; e.Value = Context.Client.Guilds.Count.ToString(); e.IsInline = true; });
      embed.AddField(e => { e.Name = "Shard"; e.Value = $"{shardId} of {Context.Client.Shards.Count}"; e.IsInline = true; });
      if (Context.Guild != null)
        embed.AddField(e => { e.Name = "Join date"; e.Value = Context.Guild.CurrentUser.JoinedAt?.ToUniversalTime().ToString("yyyy-MM-dd"); e.IsInline = true; });
      embed.AddField(e => { e.Name = "Uptime"; e.Value = timeString; });

      // If in a guild, make color.
      if (Context.Guild != null) {
        // Get current guild user.
        var guildUser = Context.Guild.CurrentUser;

        // Get highest role with color.
        var highestrole = Context.Guild.EveryoneRole;
        foreach (var role in guildUser.Roles) {
          if (role.Position > highestrole.Position && role.Color.RawValue != 0)
            highestrole = role;
        }

        // Set color, username, and join date.
        embed.Color = highestrole.Color;
        embed.Author.Name = guildUser.Nickname ?? guildUser.Username;

        // Add channel and roles.
        var channel = CatClient.GetSpeakingChannelForSocketGuild(Context.Guild);
        var roleExperimental = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderExperimental);
        var roleBeta = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderBeta);
        var roleReleasePreview = CatClient.GetRoleForIGuild(Context.Guild, RoleType.InsiderReleasePreview);
        embed.AddField(e => { e.Name = "Insider channel"; e.Value = channel?.Mention ?? "None"; });
        embed.AddField(e => { e.Name = "Insider Experimental role"; e.Value = roleExperimental?.Mention ?? "None"; e.IsInline = true; });
        embed.AddField(e => { e.Name = "Insider Beta role"; e.Value = roleBeta?.Mention ?? "None"; e.IsInline = true; });
        embed.AddField(e => { e.Name = "Insider Release Preview role"; e.Value = roleReleasePreview?.Mention ?? "None"; e.IsInline = true; });
      } else {
        // Set username.
        embed.Author.Name = Context.Client.CurrentUser.Username;
      }

      // Select and send message with embed.
      MessageComponent components = null;
      if (Context.User?.Id == Constants.OwnerId) {
        components = new ComponentBuilder().WithButton("Update Game", "button_updategame").WithButton("Restart", "button_restart").WithButton("Register commands", "button_registercommands").Build();
      }

      await RespondRandomWithButtonsAsync(embed.Build(), components,
      "Here are my stats:",
      "Here you go:",
      "My information awaits:"
      );
    }

    /// <summary>
    /// Forces a game update.
    /// </summary>
    [ComponentInteraction("button_updategame", true)]
    public async Task ButtonUpdateGame() {
      if (Context.User?.Id != Constants.OwnerId) {
        await RespondAsync($"This command is owner-only.");
        return;
      }

      await RespondAsync($"Forcing game update now...");
      await CatClient.UpdateGameAsync();
    }

    /// <summary>
    /// Restarts the bot.
    /// </summary>
    [ComponentInteraction("button_restart", true)]
    public async Task ButtonRestartAsync() {
      if (Context.User?.Id != Constants.OwnerId) {
        await RespondAsync($"This command is owner-only.");
        return;
      }

      await RespondAsync($"Restarting...");
      Environment.Exit(-1);
    }

    /// <summary>
    /// Reregisters commands.
    /// </summary>
    [ComponentInteraction("button_registercommands", true)]
    public async Task ButtonRegisterCommands() {
      if (Context.User?.Id != Constants.OwnerId) {
        await RespondAsync($"This command is owner-only.");
        return;
      }

      // Remove all and register commands.
      await RespondAsync($"Registering commands...");
      await CatClient.Interactions.RemoveModuleAsync<BotCommandsModuleNew>();
      await CatClient.Interactions.RegisterCommandsGloballyAsync();

      await CatClient.Interactions.AddModuleAsync<BotCommandsModuleNew>(null);
      await CatClient.Interactions.RegisterCommandsGloballyAsync();
    }

    #endregion
  }
}
