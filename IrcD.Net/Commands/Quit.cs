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
using System.Linq;

namespace IrcD.Commands
{
    public class Quit : CommandBase
    {
        public Quit(IrcDaemon ircDaemon)
            : base(ircDaemon, "QUIT", "Q")
        { }

        [CheckRegistered]
        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            var message = args.Count > 0 ? args.First() : IrcDaemon.Options.StandardQuitMessage;
            info.Remove(message);
        }

        protected override int PrivateSend(CommandArgument commandArgument)
        {
            var arg = GetSaveArgument<QuitArgument>(commandArgument);
            BuildMessageHeader(arg);

            Command.Append(arg.Message);

            return arg.Receiver.WriteLine(Command);
        }
    }
}