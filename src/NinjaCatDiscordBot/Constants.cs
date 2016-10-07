/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: Constants.cs
* 
* Copyright (c) 2016 John Davis
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

        public const string AppName = "Ninja Cat Bot";
        public const string AppUrl = "https://github.com/Goldfish64/NinjaCatDiscordBot";
        public const string UserName = "NinjaCat";
        public const string Nickname = "Ninja Cat";

        public const string CommandPrefix = "$nj-";

        public const string AboutCommand = "about";
        public const string AboutCommandDesc = "get to know me";
        public const string HelpCommand = "help";
        public const string HelpCommandDesc = "get help";
        public const string HelpCommandKeyword = "help";
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

        public const string SettingsModule = "settings";
        public const string GetGroup = "get";
        public const string SetGroup = "set";
        public const string DisableGroup = "disable";
        public const string NicknameCommand = "nickname";
        public const string GetNicknameCommandDesc = "gets my nickname";
        public const string SetNicknameCommandDesc = "sets my nickname";
        public const string ChannelCommand = "channel";
        public const string GetChannelCommandDesc = "gets the channel I speak in";
        public const string SetChannelCommandDesc = "sets the channel I speak in";
        public const string DisableChannelCommandDesc = "disables announcements";

        public static readonly string AboutMessage1 = 
            $"Hi there! I am {Nickname}, a Discord.Net bot!\n" +
            $"I was created by <@191330317439598593> with the purpose of letting you know about the latest in Windows Insider builds, but I can do other things too.\n\n" +
            $"For help on what I can do, type **{CommandPrefix}{HelpCommand}**.";
        public static readonly string AboutMessage2 =
            $"Greetings! I am the {Nickname}, a bot built using the Discord.Net and Tweetinvi libraries!\n" +
            $"I was activated by <@191330317439598593> with the purpose of letting you know about the latest in Windows Insider builds, but I can do other things too.\n\n" +
            $"Your wish is my command, so type **{CommandPrefix}{HelpCommand}** for info on what I can do for you.";

        #endregion
    }
}
