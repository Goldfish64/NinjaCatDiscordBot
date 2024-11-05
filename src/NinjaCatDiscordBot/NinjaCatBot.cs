/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatBot.cs
* 
* Copyright (c) 2016 - 2022 John Davis
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

using Discord.Interactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot {
  /// <summary>
  /// Represents the Ninja Cat bot.
  /// </summary>
  public partial class NinjaCatBot {
    #region Private variables

    private NinjaCatDiscordClient client;
    private Timer timerBuild;
    private Timer timerServerBuild;

    #endregion

    #region Entry method

    /// <summary>
    /// Main method.
    /// </summary>
    public static void Main(string[] args) => new NinjaCatBot().Start().GetAwaiter().GetResult();

    #endregion

    #region Methods

    /// <summary>
    /// Starts the bot.
    /// </summary>
    private async Task Start() {
      // Initialize client command modules.
      bool commandsRegistered = false;
      client = new NinjaCatDiscordClient();
      await client.Interactions.AddModuleAsync<BotCommandsModuleNew>(null);

      // Register commands on ready.
      client.ShardReady += async (s) => {
        if (!commandsRegistered) {
          try {
            await client.Interactions.RegisterCommandsGloballyAsync();
            commandsRegistered = true;
            client.LogInfo($"Commands registered");
          } catch (Exception ex) {
            client.LogInfo($"Commands registration failed: {ex}");
          }
        }
      };
      client.InteractionCreated += async (s) => {
        var ctx = new ShardedInteractionContext(client, s);
        await client.Interactions.ExecuteCommandAsync(ctx, null);
      };

      // Log in to Discord. Token is stored in the Credentials class.
      await client.StartBotAsync();

      // Start checking for new builds.
      timerBuild = new Timer(async (s) => {
        // Builds generally release between 10AM and 5PM PST. Do not check outside these times.
        //     if (DateTime.UtcNow.Hour < 17 && !string.IsNullOrWhiteSpace(client.CurrentUrl))
        //     return;

        // If we cannot get the new post, try again later.
        var post = await client.GetLatestBuildPostAsync();
        if (post == null)
          return;

        // Have we ever seen a post yet? This prevents false announcements if the bot has never seen a post before.
        if (string.IsNullOrWhiteSpace(client.CurrentUrl)) {
          client.CurrentUrl = post.Link;
          client.SaveSettings();
          client.LogInfo($"Saved post as new latest build: {post.Link}");
          return;
        }

        // Is the latest post the same? If so, no need to announce it.
        if (client.CurrentUrl == post.Link)
          return;

        // Stop timer.
        timerBuild.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
        client.LogInfo($"New build received");

        // Save post.
        client.CurrentUrl = post.Link;
        client.SaveSettings();

        // Send build to guilds.
        foreach (var shard in client.Shards)
          client.SendNewBuildToShard(shard, post);
        await client.UpdateGameAsync();

        // Restart timer.
        timerBuild.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
      }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

      // Start checking for new server builds.
      timerServerBuild = new Timer(async (s) => {
        // If we cannot get the new server post, try again later.
        var post = await client.GetLatestBuildPostAsync(BuildType.Server);
        if (post == null)
          return;

        // Have we ever seen a post yet? This prevents false announcements if the bot has never seen a post before.
        if (string.IsNullOrWhiteSpace(client.CurrentServerUrl)) {
          client.CurrentServerUrl = post.Link;
          client.SaveSettings();
          client.LogInfo($"Saved post as new latest server build: {post.Link}");
          return;
        }

        // Is the latest post the same? If so, no need to announce it.
        if (client.CurrentServerUrl == post.Link)
          return;

        // Stop timer.
        timerServerBuild.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
        client.LogInfo($"New server build received");

        // Save post.
        client.CurrentServerUrl = post.Link;
        client.SaveSettings();

        // Send build to guilds.
        foreach (var shard in client.Shards)
          client.SendNewBuildToShard(shard, post);

        // Restart timer.
        timerServerBuild.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
      }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

      // Wait a minute for bot to start up.
      await Task.Delay(TimeSpan.FromMinutes(1));

      // Create thread for updating game.
      var serverCountThread = new Thread(new ThreadStart(async () => {
        while (true) {
          await client.UpdateGameAsync();
          await Task.Delay(TimeSpan.FromHours(24));
        }
      }));
      serverCountThread.Start();

      // Wait forever.
      await Task.Delay(-1);
    }

    #endregion
  }
}
