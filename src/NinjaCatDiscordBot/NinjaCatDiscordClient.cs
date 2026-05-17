/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatDiscordClient.cs
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
using Discord.Net;
using Discord.WebSocket;
using NinjaCatDiscordBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
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
    public ConcurrentDictionary<ulong, ulong> InsiderChannels { get; set; } = new ConcurrentDictionary<ulong, ulong>();

    /// <summary>
    /// Gets the list of Dev Channel Insider roles.
    /// </summary>
    /// <remarks>Guild is the key, role is the value.</remarks>
    public ConcurrentDictionary<ulong, ulong> InsiderRolesDev { get; set; } = new ConcurrentDictionary<ulong, ulong>();

    /// <summary>
    /// Gets the list of Beta Channel Insider roles.
    /// </summary>
    /// <remarks>Guild is the key, role is the value.</remarks>
    public ConcurrentDictionary<ulong, ulong> InsiderRolesBeta { get; set; } = new ConcurrentDictionary<ulong, ulong>();

    /// <summary>
    /// Gets the list of Release Preview Insider roles.
    /// </summary>
    /// <remarks>Guild is the key, role is the value.</remarks>
    public ConcurrentDictionary<ulong, ulong> InsiderRolesReleasePreview { get; set; } = new ConcurrentDictionary<ulong, ulong>();

    /// <summary>
    /// Gets the list of jumbo roles.
    /// </summary>
    /// <remarks>Guild is the key, role is the value.</remarks>
    public ConcurrentDictionary<ulong, ulong> JumboRoles { get; set; } = new ConcurrentDictionary<ulong, ulong>();
  }

  /// <summary>
  /// Represents a <see cref="DiscordShardedClient"/> with additional properties.
  /// </summary>
  public sealed class NinjaCatDiscordClient : DiscordShardedClient {
    #region Private variables

    private Random random = new Random();
    private object lockObject = new object();

    private HttpClient httpClient;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="NinjaCatDiscordClient"/> class.
    /// </summary>
    public NinjaCatDiscordClient() : base(new DiscordSocketConfig() { TotalShards = Constants.ShardCount }) {
      // Write startup messages.
      LogInfo($"{Constants.AppName} on {RuntimeInformation.FrameworkDescription} has started.");
      LogInfo($"===============================================================");

      httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 Edg/114.0.1823.37");

      Interactions = new InteractionService(this);

      // Listen for events.
      Log += (message) => {
        // Log the output.
        LogInfo(message.ToString());
        return Task.CompletedTask;
      };

      // Get latest build data and settings files.
      if (File.Exists(Constants.LatestInsiderBuildsFileName)) {
        CurrentInsiderBuilds = JsonSerializer.Deserialize<Dictionary<InsiderBuildType, string>>(File.ReadAllText(Constants.LatestInsiderBuildsFileName));
      }

      if (File.Exists(Constants.SettingsFileName)) {
        var options = new JsonSerializerOptions {
          PropertyNameCaseInsensitive = true,
        };
        Settings = JsonSerializer.Deserialize<NinjaCatSettings>(File.ReadAllText(Constants.SettingsFileName), options);
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
    /// Gets or sets the current Insider build URLs, used for keeping track of new releases.
    /// </summary>
    public Dictionary<InsiderBuildType, string> CurrentInsiderBuilds { get; set; } = new();

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
        } catch (HttpException ex) {
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
        case RoleType.InsiderExperimental:
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
        case RoleType.InsiderExperimental:
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
        File.WriteAllText(Constants.LatestInsiderBuildsFileName, JsonSerializer.Serialize(CurrentInsiderBuilds));
        File.WriteAllText(Constants.SettingsFileName, JsonSerializer.Serialize(Settings));
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

    private async Task<LearnToc> GetLearnTocAsync() {
      var flightHubTocJson = await httpClient.GetStringAsync("https://learn.microsoft.com/en-us/windows-insider/toc.json");
      return JsonSerializer.Deserialize<LearnToc>(flightHubTocJson);
    }

    private InsiderBuild GetInsiderBuildFromToc(LearnToc toc, InsiderBuildType buildType) {
      // Get the first build for the type
      var latestBuildItem = (
        // Top level ->
        from releaseNotesToc in toc.Items
        where releaseNotesToc.Title.ToLowerInvariant() == "release notes"

        // Release notes ->
        from buildTypeToc in releaseNotesToc.Children
        where buildTypeToc.Title.ToLowerInvariant() == InsiderBuild.Names[buildType].ToLowerInvariant()

        // Beta/Experimental/etc ->
        from buildToc in buildTypeToc.Children
        select buildToc).FirstOrDefault();

      if (latestBuildItem != null) {
        var buildTitle = latestBuildItem.Title.ToLowerInvariant();
        const string buildText = "build ";
        return new InsiderBuild() {
          BuildNumber = buildTitle.Substring(buildTitle.IndexOf(buildText) + buildText.Length),
          Link = "https://learn.microsoft.com/en-us/windows-insider/" + latestBuildItem.LinkHref,
          Type = buildType
        };
      }

      //LogError($"Unable to get build for type {buildType}");
      return null;
    }

    public async Task<InsiderBuild> GetLatestInsiderBuildAsync(InsiderBuildType buildType) {
      if (buildType == InsiderBuildType.Server) {
        try {
          // Get server feed.
          var doc = XDocument.Parse(await httpClient.GetStringAsync($"https://techcommunity.microsoft.com/t5/s/gxcuf89792/rss/board?board.id=WindowsServerInsiders"));
          var blogEntry = (
            from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
            where item.Elements().First(i => i.Name.LocalName == "link").Value.ToLowerInvariant().ContainsAny("announcing-windows-server-preview", "announcing-windows-server-vnext-preview")
            select item).FirstOrDefault();

          if (blogEntry != null) {
            var buildTitle = blogEntry.Elements().First(i => i.Name.LocalName == "title").Value.ToLowerInvariant();
            const string buildText = "build ";
            return new InsiderBuild() {
              BuildNumber = buildTitle.Substring(buildTitle.IndexOf(buildText) + buildText.Length),
              Link = blogEntry.Elements().First(i => i.Name.LocalName == "link").Value,
              Type = buildType
            };
          }
        } catch (HttpRequestException ex) {
          LogError($"Exception when getting post for server: {ex}");
        }

        return null;
      }

      try {
        return GetInsiderBuildFromToc(await GetLearnTocAsync(), buildType);
      } catch (Exception ex) {
        LogError($"Exception when getting build for type {buildType}: {ex}");
      }

      return null;
    }

    public async Task<Dictionary<InsiderBuildType, InsiderBuild>> GetAllLatestInsiderBuildsAsync() {
      var builds = new Dictionary<InsiderBuildType, InsiderBuild>();

      try {
        // Get flight hub TOC.
        var flightHubToc = await GetLearnTocAsync();

        // Get each build type.
        foreach (InsiderBuildType buildType in Enum.GetValues(typeof(InsiderBuildType))) {
          builds[buildType] = GetInsiderBuildFromToc(flightHubToc, buildType);
        }
        return builds;
      } catch (Exception ex) {
        LogError($"Exception when getting builds: {ex}");
        return null;
      }
    }

    private async Task SendInsiderBuildToGuild(DiscordSocketClient shard, SocketGuild guild, InsiderBuild build) {
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
      var roleExperimental = GetRoleForIGuild(guild, RoleType.InsiderExperimental);
      var roleBeta = GetRoleForIGuild(guild, RoleType.InsiderBeta);
      var roleReleasePreview = GetRoleForIGuild(guild, RoleType.InsiderReleasePreview);

      var roleText = string.Empty;
      var roleType = InsiderBuild.Roles[build.Type];
      switch (roleType) {
        case RoleType.InsiderExperimental:
          roleText = $"{roleExperimental?.Mention} ";
          break;

        case RoleType.InsiderBeta:
          roleText = $"{roleBeta?.Mention} ";
          break;

        case RoleType.InsiderReleasePreview:
          roleText = $"{roleReleasePreview?.Mention} ";
          break;
      }


      var typeText = InsiderBuild.Names[build.Type];
      var emotesText = $":smiley_cat: :{InsiderBuild.Emotes[build.Type]}:";

      try {
        await StartTyping(channel);
        switch (GetRandomNumber(3)) {
          default:
            await channel.SendMessageAsync($"{roleText} Windows Insider Preview {typeText} Build {build.BuildNumber} has just been released! {emotesText}\n{build.Link}");
            break;

          case 1:
            await channel.SendMessageAsync($"{roleText} Windows Insider Preview {typeText} Build {build.BuildNumber} has just been released! Yes! {emotesText}\n{build.Link}");
            break;

          case 2:
            await channel.SendMessageAsync($"{roleText}Better check for updates now! Windows Insider Preview {typeText} Build {build.BuildNumber} has just been released! {emotesText}\n{build.Link}");
            break;
        }
      } catch (Exception ex) {
        LogError($"Failed to speak in {guild.Name} ({shard.ShardId}/{Shards.Count - 1}): {ex}");
      }

      // Log server.
      LogInfo($"Spoke in {guild.Name} ({shard.ShardId}/{Shards.Count - 1})");
    }

    public async void SendNewInsiderBuildToShard(DiscordSocketClient shard, InsiderBuild build) {
      // If the MS server is in this shard, announce there first.
      var msGuild = shard.Guilds.SingleOrDefault(g => g.Id == Constants.MsGuildId);
      if (msGuild != null)
        await SendInsiderBuildToGuild(shard, msGuild, build);

      foreach (var guild in shard.Guilds) {
        // Skip MS guild.
        if (guild.Id == Constants.MsGuildId)
          continue;

        await SendInsiderBuildToGuild(shard, guild, build);
      }
    }

    /// <summary>
    /// Updates the game.
    /// </summary>
    /// <returns></returns>
    public async Task UpdateGameAsync() {
      try {
        var build = await GetLatestInsiderBuildAsync(InsiderBuildType.ExperimentalFuturePlatforms);
        if (build == null)
          return;

        var game = $"on build {build.BuildNumber}";
        foreach (var shard in Shards)
          await shard?.SetGameAsync(game);
      } catch (Exception ex) {
        LogError($"Failed to update game: {ex}");
        foreach (var shard in Shards)
          await shard?.SetGameAsync("on Windows 11");
      }
    }

    #endregion
  }
}
