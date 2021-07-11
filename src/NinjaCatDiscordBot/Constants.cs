/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: Constants.cs
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

namespace NinjaCatDiscordBot {
    /// <summary>
    /// Contains constants.
    /// </summary>
    internal static class Constants {
        #region Constants

        public const string LatestPostFileName = "nj-latestposturl.txt";
        public const string SettingsFileName = "nj-settings.json";

        public const string AppUrl = "https://github.com/Goldfish64/NinjaCatDiscordBot";
        public const string InviteUrl = "<https://discordapp.com/oauth2/authorize?permissions=444480&client_id={0}&scope=bot>";
        public const string UserName = "Ninja Cat";
        public const ulong OwnerId = 191330317439598593;
        public const ulong BotsGuildId = 110373943822540800;
        public const ulong MsGuildId = 150662382874525696;
        public const string OwnerName = "Goldfish64";

#if RELEASE
        public const string AppName = "Ninja Cat";
        public const string CommandPrefix = "$nj-";
        public const int ShardCount = 2;
#else
        public const string AppName = "Ninja Cat beta (DEBUG)";
        public const string CommandPrefix = "$njd-";
        public const int ShardCount = 2;
#endif

        public const string RemarkGeneral = "GeneralCommand";
        public const string RemarkAdmin = "AdminCommand";
        public const string RemarkInternal = "InternalCommand";

        public const string HelpCommand = "help";
        
        public const string AnnouncementCommand = "announce"; // Bot owner only.

        public const string ChannelCommand = "channel";
        public const string RoleJumboCommand = "jumborole";
        public const string SetRoleJumboCommand = "setjumborole";

        public static readonly string AboutMessage =
            $"Hi there! I am {UserName}, a Discord.Net bot!\n" +
            $"I was created by **{OwnerName}** with the purpose of letting you know about the latest in Windows Insider builds\n\n" +
            $"For help on what I can do, type **{CommandPrefix}{HelpCommand}**.";

        #endregion
    }
}
