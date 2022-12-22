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
using IrcD.Modes.UserModes;
using System.Collections.Generic;

namespace IrcD.Commands
{
    public class Away : CommandBase
    {
        public Away(IrcDaemon ircDaemon)
            : base(ircDaemon, "AWAY", "A")
        {
            if (!ircDaemon.Capabilities.Contains("away-notify"))
            {
                ircDaemon.Capabilities.Add("away-notify");
            }
        }

        [CheckRegistered]
        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            if (args.Count == 0)
            {
                info.AwayMessage = null;
                info.Modes.RemoveMode<ModeAway>();
                IrcDaemon.Replies.SendUnAway(info);
            }
            else
            {
                info.AwayMessage = args[0];
                info.Modes.Add(new ModeAway());
                IrcDaemon.Replies.SendNowAway(info);
            }

            foreach (var channel in info.Channels)
            {
                foreach (var user in channel.Users)
                {
                    if (user.Capabilities.Contains("away-notify"))
                    {
                        Send(new AwayArgument(info, user, (args.Count == 0) ? null : args[0]));
                    }
                }
            }
        }

        protected override int PrivateSend(CommandArgument commandArgument)
        {
            var arg = GetSaveArgument<AwayArgument>(commandArgument);

            BuildMessageHeader(arg);

            if (arg.AwayMessage != null)
            {
                Command.Append(arg.AwayMessage);
            }

            return arg.Receiver.WriteLine(Command);
        }
    }
}