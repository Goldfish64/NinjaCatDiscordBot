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

        //public const string LogFileName = "nj-logfile.log";
        public const string ChannelsFileName = "nj-channels.json";
        public const string RolesFileName = "nj-roles.json";

        public const string AppUrl = "https://github.com/Goldfish64/NinjaCatDiscordBot";
        public const string UserName = "Ninja Cat";
        public const ulong OwnerId = 191330317439598593;
        public const ulong BotsGuildId = 110373943822540800;
        public const string OwnerName = "Goldfish64#9003";

#if DEBUG
        public const string AppName = "Ninja Cat beta (DEBUG)";
        public const string InviteUrl = "https://discordapp.com/oauth2/authorize?permissions=19456&client_id=238475402937499648&scope=bot";
        public const string CommandPrefix = "$njd-";
#else
        public const string AppName = "Ninja Cat";
        public const string InviteUrl = "https://discordapp.com/oauth2/authorize?permissions=19456&client_id=232369430456172545&scope=bot";
        public const string CommandPrefix = "$nj-";
#endif

        public const string AboutCommand = "about";
        public const string AboutCommandDesc = "get to know me";
        public const string HelpCommand = "help";
        public const string HelpCommandDesc = "get help";
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
        public const string TrexCommand = "trex";
        public const string TrexCommandDesc = "shows the Windows 10 Skype emoticon";
        public const string TrexCommandUrl = "http://static.skaip.su/img/emoticons/180x180/f6fcff/win10.gif";
        public const string LatestBuildCommand = "latestbuild";
        public const string LatestBuildCommandDesc = "gets the latest Insider PC build";
        public const string LatestMobileBuildCommand = "latestmobilebuild";
        public const string LatestMobileBuildCommandDesc = "gets the latest Insider Mobile build";
        public const string LatestServerBuildCommand = "latestserverbuild";
        public const string LatestServerBuildCommandDesc = "gets the latest Insider Server build";
        public const string TimeCommand = "time";
        public const string TimeCommandDesc = "shows the current time";
        public const string BotInfoCommand = "info";
        public const string BotInfoCommandDesc = "shows my info";
        public const string ServersCommand = "servers";
        public const string ServersCommandDesc = "lists the servers I'm in";
        public const string ServerCountBotsCommand = "botservers";
       // public const string LargestServerCommand = "largestserver";
        public const string TestPermsCommand = "testperms";
        public const string TestPermsCommandDesc = "tests my speaking channel permissions";
        public const string AnnouncementCommand = "announce"; // Bot owner only.
        public const string UpdateGameCommand = "updategame"; // Bot owner only.
        public const string PingJaskaCommand = "jaska"; // Bot owner only.

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
        public const string RoleCommand = "role";
        public const string RoleCommandDesc = "gets the role I ping when new builds are released";
        public const string SetRoleCommand = "setrole";
        public const string SetRoleCommandDesc = "sets the role I ping when new builds are released";
        public const string DisableRoleCommand = "offrole";
        public const string DisableRoleCommandDesc = "disables the announcement role";

        public static readonly string AboutMessage1 =
            $"Hi there! I am {UserName}, a Discord.Net bot!\n" +
            $"I was created by **{OwnerName}** with the purpose of letting you know about the latest in Windows Insider builds, but I can do other things too.\n\n" +
            $"For help on what I can do, type **{CommandPrefix}{HelpCommand}**.";
        public static readonly string AboutMessage2 =
            $"Greetings! I am the {UserName}, a bot built using the Discord.Net and Tweetinvi libraries!\n" +
            $"I was activated by **{OwnerName}** with the purpose of letting you know about the latest in Windows Insider builds, but I can do other things too.\n\n" +
            $"Your wish is my command, so type **{CommandPrefix}{HelpCommand}** for info on what I can do for you.";

        public static readonly string HelpBody =
            $"**{CommandPrefix}{AboutCommand}**: {AboutCommandDesc}.\n" +
            $"**{CommandPrefix}{HelpCommand}**: {HelpCommandDesc}.\n" +
            $"**{CommandPrefix}{BotInfoCommand}**: {BotInfoCommandDesc}.\n" +
         //   $"**{CommandPrefix}{ServersCommand}**: {ServersCommandDesc}.\n" +
            $"**{CommandPrefix}{HomeCommand}** or **{CommandPrefix}{HomeCommandAlias}**: {HomeCommandDesc}.\n" +
            $"**{CommandPrefix}{InviteCommand}**: {InviteCommandDesc}.\n" +
            $"**{CommandPrefix}{PingCommand}**: {PingCommandDesc}.\n" +
            $"**{CommandPrefix}{TimeCommand}**: {TimeCommandDesc}.\n" +
            $"**{CommandPrefix}{TrexCommand}**: {TrexCommandDesc}.\n" +
            $"**{CommandPrefix}{LatestBuildCommand}**: {LatestBuildCommandDesc}.\n" +
            $"**{CommandPrefix}{LatestMobileBuildCommand}**: {LatestMobileBuildCommandDesc}.\n" +
            $"**{CommandPrefix}{LatestServerBuildCommand}**: {LatestServerBuildCommandDesc}.\n\n" +
            $"Admin commands:\n" +
            $"**{CommandPrefix}{NicknameCommand}**: {NicknameCommandDesc}.\n" +
            $"**{CommandPrefix}{SetNicknameCommand}** *nickname*: {SetNicknameCommandDesc}.\n" +
            $"**{CommandPrefix}{ChannelCommand}**: {ChannelCommandDesc}.\n" +
            $"**{CommandPrefix}{SetChannelCommand}** *channel*: {SetChannelCommandDesc}.\n" +
            $"**{CommandPrefix}{DisableChannelCommand}**: {DisableChannelCommandDesc}.\n" +
            $"**{CommandPrefix}{RoleCommand}**: {RoleCommandDesc}.\n" +
            $"**{CommandPrefix}{SetRoleCommand}** *role*: {SetRoleCommandDesc}.\n" +
            $"**{CommandPrefix}{DisableRoleCommand}**: {DisableRoleCommandDesc}.\n" +
            $"**{CommandPrefix}{TestPermsCommand}**: {TestPermsCommandDesc}.";

        #endregion
    }
}
