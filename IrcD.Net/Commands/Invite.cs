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

using IrcD.Channel;
using IrcD.Commands.Arguments;
using System.Collections.Generic;

namespace IrcD.Commands
{
    public class Invite : CommandBase
    {
        public Invite(IrcDaemon ircDaemon)
            : base(ircDaemon, "INVITE", "I")
        { }

        [CheckRegistered]
        [CheckParamCount(2)]
        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            UserInfo invited;
            if (!IrcDaemon.Nicks.TryGetValue(args[0], out invited))
            {
                IrcDaemon.Replies.SendNoSuchNick(info, args[0]);
            }

            var channel = args[1];
            ChannelInfo chan;
            if (IrcDaemon.Channels.TryGetValue(channel, out chan))
            {
                if (chan.UserPerChannelInfos.ContainsKey(invited.Nick))
                {
                    IrcDaemon.Replies.SendUserOnChannel(info, invited, chan);
                    return;
                }

                if (!chan.Modes.HandleEvent(this, chan, info, args))
                {
                    return;
                }

                if (!invited.Invited.Contains(chan))
                {
                    invited.Invited.Add(chan);
                }
            }

            //TODO channel does not exist? ... clean up below

            IrcDaemon.Replies.SendInviting(info, invited, channel);
            Send(new InviteArgument(info, invited, invited, chan));
        }

        protected override int PrivateSend(CommandArgument commandArgument)
        {
            var arg = GetSaveArgument<InviteArgument>(commandArgument);

            BuildMessageHeader(arg);

            Command.Append(arg.Invited.Nick);
            Command.Append(" ");
            Command.Append(arg.Channel.Name);

            return arg.Receiver.WriteLine(Command);
        }
    }
}