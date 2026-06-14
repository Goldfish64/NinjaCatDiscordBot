/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: NinjaCatBot.cs
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

      var firstRun = true;

      // Start checking for new builds.
      timerBuild = new Timer(async (s) => {
        // Check for latest builds.
        var builds = await client.GetAllLatestInsiderBuildsAsync();
        if (builds == null)
          return;

        // Stop timer.
        timerBuild.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

        // Check for any new builds.
        foreach (var build in builds.Keys) {
          if (builds[build] == null) {
            continue;
          }

          var hasBuildPreviously = client.CurrentInsiderBuilds.ContainsKey(build);
          if (hasBuildPreviously && client.CurrentInsiderBuilds[build] == builds[build].Link) {
            continue;
          }

          client.LogInfo($"New build type {builds[build].Type} received: {builds[build].Link}");
          client.CurrentInsiderBuilds[build] = builds[build].Link;
          client.SaveSettings();

          // Disregard if the bot just started and the build type was never seen before.
          if (firstRun && !hasBuildPreviously) {
            client.LogInfo("Bot just started, ignoring new build");
            continue;
          }

          foreach (var shard in client.Shards)
            client.SendNewInsiderBuildToShard(shard, builds[build]);

          if (builds[build].Type == Models.InsiderBuildType.Experimental) {
            await client.UpdateGameAsync();
          }
        }

        firstRun = false;

        // Restart timer.
        timerBuild.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
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
