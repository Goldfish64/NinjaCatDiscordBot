/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: CommandModuleBase.cs
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
using System.Threading.Tasks;

namespace NinjaCatDiscordBot {
    public class NinjaCatCommandContext : ICommandContext {
        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="NinjaCatCommandContext" /> class with the provided client and message.
        /// </summary>
        /// <param name="client">The underlying client.</param>
        /// <param name="msg">The underlying message.</param>
        public NinjaCatCommandContext(NinjaCatDiscordClient client, SocketUserMessage msg) {
            if (client == null || msg == null)
                throw new ArgumentNullException();

            Client = client;
            Message = msg;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="NinjaCatDiscordClient" /> that the command is executed with.
        /// </summary>
        public NinjaCatDiscordClient Client { get; }

        /// <summary>
        /// Gets the <see cref="SocketGuild" /> that the command is executed in.
        /// </summary>
        public SocketGuild Guild => (Message.Channel as SocketGuildChannel)?.Guild;

        /// <summary>
        /// Gets the <see cref="ISocketMessageChannel" /> that the command is executed in.
        /// </summary>
        public ISocketMessageChannel Channel => Message.Channel;

        /// <summary>
        /// Gets the <see cref="SocketUser" /> who executed the command.
        /// </summary>
        public SocketUser User => Message.Author;

        /// <summary>
        /// Gets the <see cref="SocketUserMessage" /> that the command is interpreted from.
        /// </summary>
        public SocketUserMessage Message { get; }

        #endregion

        #region Interface properties

        IDiscordClient ICommandContext.Client => Client;

        IGuild ICommandContext.Guild => Guild;

        IMessageChannel ICommandContext.Channel => Channel;

        IUser ICommandContext.User => User;

        IUserMessage ICommandContext.Message => Message;

        #endregion

        #region Methods

        /// <summary>
        /// Sends typing feedback.
        /// </summary>
        public async Task StartTyping() {
            await Channel.TriggerTypingAsync();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
        }

        #endregion
    }

    public abstract class CommandModuleBase : ModuleBase<NinjaCatCommandContext> {
        protected override void BeforeExecute(CommandInfo command) {
            base.BeforeExecute(command);

            // Start typing.
            Context.StartTyping().Wait();
        }

        protected Task<IUserMessage> ReplyRandomAsync(Embed embed, params string[] messages) {
            if (messages == null)
                throw new ArgumentNullException();

            var index = Context.Client.GetRandomNumber(messages.Length - 1);
            return ReplyAsync(messages[index], embed: embed);
        }
    }
}
