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
using System;
using System.Collections.Generic;
using System.Linq;

namespace IrcD.Commands
{
    public class List : CommandBase
    {
        public List(IrcDaemon ircDaemon)
            : base(ircDaemon, "LIST", "LIST")
        { }

        [CheckRegistered]
        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            if (info.IrcDaemon.Options.IrcMode == IrcMode.Rfc1459)
                IrcDaemon.Replies.SendListStart(info);

            // TODO: special List commands (if RfcModern)
            foreach (var ci in IrcDaemon.Channels.Values.Where(ci => !ci.Modes.IsPrivate() && !ci.Modes.IsSecret()))
            {
                IrcDaemon.Replies.SendListItem(info, ci);
            }
            IrcDaemon.Replies.SendListEnd(info);
        }

        protected override int PrivateSend(CommandArgument commandArgument)
        {
            throw new NotImplementedException();
        }
    }
}