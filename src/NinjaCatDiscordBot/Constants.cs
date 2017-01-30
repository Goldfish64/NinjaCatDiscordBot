/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: Constants.cs
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

namespace NinjaCatDiscordBot
{
    /// <summary>
    /// Contains constants.
    /// </summary>
    internal static class Constants
    {
        #region Constants

        public const string LogFileName = "nj-logfile.log";
        public const string ChannelsFileName = "nj-channels.json";

        public const string AppName = "Ninja Cat";
        public const string AppUrl = "https://github.com/Goldfish64/NinjaCatDiscordBot";
        public const string UserName = "Ninja Cat";
        public const ulong OwnerId = 191330317439598593;
        public const ulong BotsGuildId = 110373943822540800;

#if DEBUG
        public const string InviteUrl = "https://discordapp.com/oauth2/authorize?permissions=19456&client_id=238475402937499648&scope=bot";
        public const string CommandPrefix = "$njd-";
#else
        public const string InviteUrl = "https://discordapp.com/oauth2/authorize?permissions=19456&client_id=232369430456172545&scope=bot";
        public const string CommandPrefix = "$nj-";
#endif

        public const string AboutCommand = "about";
        public const string AboutCommandDesc = "get to know me";
        public const string HelpCommand = "help";
        public const string HelpCommandDesc = "get help";
        public const string HelpCommandKeyword = "help";
        public const string HomeCommand = "home";
        public const string HomeCommandDesc = "go to my homepage";
        public const string HomeCommandAlias = "source";
        public const string HomeCommandAlias2 = "sourcecode";
        public const string HomeCommandUrl = AppUrl;
        public const string InviteCommand = "invite";
        public const string InviteCommandDesc = "invite me to your server";
        public const string InviteCommandUrl = InviteUrl;
        public const string PingCommand = "ping";
        public const string PingCommandDesc = "pong";
        public static readonly string[] PingCommandKeywords = { "ping", "pong", "ping-pong" };
        public const string TrexCommand = "trex";
        public const string TrexCommandDesc = "shows the Windows 10 Skype emoticon";
        public const string TrexCommandUrl = "http://static.skaip.su/img/emoticons/180x180/f6fcff/win10.gif";
        public static readonly string[] TrexCommandKeywords = { "trex", "t-rex" };
        public const string LatestBuildCommand = "latestbuild";
        public const string LatestBuildCommandDesc = "gets the latest Insider build";
        public static readonly string[] LatestBuildKeywords = { "latest build", "latest insider build", "latest windows 10 build", "latest windows build", "latest insider preview build" };
        public const string EnrollCommand = "enrollinsider";
        public const string EnrollCommandDesc = "enroll in the Insider program";
        public const string EnrollCommandUrl = "https://insider.windows.com/";
        public const string TimeCommand = "time";
        public const string TimeCommandDesc = "shows the current time";
        public const string TimeCommandKeyword = "time";
        public const string PlatformCommand = "platform";
        public const string PlatformCommandDesc = "shows where I live";
        public const string UptimeCommand = "uptime";
        public const string UptimeCommandDesc = "shows my uptime";
        public const string ServersCommand = "servers";
        public const string ServersCommandDesc = "shows the number of servers I'm part of";
        public const string ServerNamesCommand = "servernames"; // Only for bot owner.
        public const string TestPermsCommand = "testperms";
        public const string AnnouncementCommand = "announce";

        public const string NicknameCommand = "nickname";
        public const string NicknameCommandDesc = "gets my nickname";
        public const string SetNicknameCommand = "setnickname";
        public const string SetNicknameCommandDesc = "sets my nickname";
        public const string ChannelCommand = "channel";
        public const string ChannelCommandDesc = "gets the channel I speak in";
        public const string SetChannelCommand = "setchannel";
        public const string SetChannelCommandDesc = "sets the channel I speak in";
        public const string DisableChannelCommand = "offchannel";
        public const string DisableChannelCommandDesc = "disables announcements";

        public static readonly string AboutMessage1 =
            $"Hi there! I am {UserName}, a Discord.Net bot!\n" +
            $"I was created by <@{OwnerId.ToString()}> with the purpose of letting you know about the latest in Windows Insider builds, but I can do other things too.\n\n" +
            $"For help on what I can do, type **{CommandPrefix}{HelpCommand}**.";
        public static readonly string AboutMessage2 =
            $"Greetings! I am the {UserName}, a bot built using the Discord.Net and Tweetinvi libraries!\n" +
            $"I was activated by <@{OwnerId.ToString()}> with the purpose of letting you know about the latest in Windows Insider builds, but I can do other things too.\n\n" +
            $"Your wish is my command, so type **{CommandPrefix}{HelpCommand}** for info on what I can do for you.";

        public static readonly string HelpBody =
            $"**{CommandPrefix}{AboutCommand}**: {AboutCommandDesc}.\n" +
            $"**{CommandPrefix}{HelpCommand}**: {HelpCommandDesc}.\n" +
            $"**{CommandPrefix}{HomeCommand}** or **{CommandPrefix}{HomeCommandAlias}**: {HomeCommandDesc}.\n" +
            $"**{CommandPrefix}{InviteCommand}**: {InviteCommandDesc}.\n" +
            $"**{CommandPrefix}{PingCommand}**: {PingCommandDesc}.\n" +
            $"**{CommandPrefix}{TrexCommand}**: {TrexCommandDesc}.\n" +
            $"**{CommandPrefix}{LatestBuildCommand}**: {LatestBuildCommandDesc}.\n" +
            $"**{CommandPrefix}{EnrollCommand}**: {EnrollCommandDesc}.\n" +
            $"**{CommandPrefix}{TimeCommand}**: {TimeCommandDesc}.\n" +
            $"**{CommandPrefix}{PlatformCommand}**: {PlatformCommandDesc}.\n" +
            $"**{CommandPrefix}{UptimeCommand}**: {UptimeCommandDesc}.\n" +
            $"**{CommandPrefix}{ServersCommand}**: {ServersCommandDesc}.\n\n" +
            $"Admin commands:\n" +
            $"**{CommandPrefix}{NicknameCommand}**: {NicknameCommandDesc}.\n" +
            $"**{CommandPrefix}{SetNicknameCommand}** *nickname*: {SetNicknameCommandDesc}.\n" +
            $"**{CommandPrefix}{ChannelCommand}**: {ChannelCommandDesc}.\n" +
            $"**{CommandPrefix}{SetChannelCommand}** *channel*: {SetChannelCommandDesc}.\n" +
            $"**{CommandPrefix}{DisableChannelCommand}**: {DisableChannelCommandDesc}.\n\n";

        #endregion
    }
}
