﻿/*
 *  The ircd.net project is an IRC deamon implementation for the .NET Plattform
 *  It should run on both .NET and Mono
 *
 *  Copyright (c) 2009-2010 Thomas Bruderer <apophis@apophis.ch>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using IrcD.Commands.Arguments;
using System.Collections.Generic;

namespace IrcD.Commands
{
    public class Notice : CommandBase
    {
        public Notice(IrcDaemon ircDaemon)
            : base(ircDaemon, "NOTICE", "O")
        { }

        [CheckRegistered]
        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            if (args.Count < 1)
            {
                IrcDaemon.Replies.SendNoRecipient(info, Name);
                return;
            }
            if (args.Count < 2)
            {
                IrcDaemon.Replies.SendNoTextToSend(info);
                return;
            }

            if (IrcDaemon.ValidChannel(args[0]))
            {
                if (IrcDaemon.Channels.ContainsKey(args[0]))
                {
                    var chan = IrcDaemon.Channels[args[0]];

                    if (!chan.Modes.HandleEvent(this, chan, info, args))
                    {
                        return;
                    }

                    // Send Channel Message
                    Send(new NoticeArgument(info, chan, chan.Name, args[1]));
                }
                else
                {
                    IrcDaemon.Replies.SendCannotSendToChannel(info, args[0]);
                }
            }
            else if (IrcDaemon.ValidNick(args[0]))
            {
                if (IrcDaemon.Nicks.ContainsKey(args[0]))
                {
                    var user = IrcDaemon.Nicks[args[0]];

                    if (user.AwayMessage != null)
                    {
                        IrcDaemon.Replies.SendAwayMessage(info, user);
                    }

                    // Send PM
                    Send(new NoticeArgument(info, user, user.Nick, args[1]));
                }
                else
                {
                    IrcDaemon.Replies.SendNoSuchNick(info, args[0]);
                }
            }
            else
            {
                IrcDaemon.Replies.SendNoSuchNick(info, args[0]);
            }
        }

        protected override int PrivateSend(CommandArgument commandArgument)
        {
            var arg = GetSaveArgument<NoticeArgument>(commandArgument);

            BuildMessageHeader(arg);

            Command.Append(arg.Target);
            Command.Append(" :");
            Command.Append(arg.Message);

            if (arg.Sender == null)
            {
                return arg.Receiver.WriteLine(Command);
            }
            else
            {
                return arg.Receiver.WriteLine(Command, arg.Sender);
            }
        }
    }
}