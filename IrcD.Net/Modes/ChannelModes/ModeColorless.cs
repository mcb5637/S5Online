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
using IrcD.Commands;
using IrcD.Utils;
using System.Collections.Generic;
using System.Linq;

namespace IrcD.Modes.ChannelModes
{
    public class ModeColorless : ChannelMode
    {
        public ModeColorless()
            : base('c')
        {
        }

        public override bool HandleEvent(CommandBase command, ChannelInfo channel, UserInfo user, List<string> args)
        {
            if (command is PrivateMessage || command is Notice)
            {
                if (args[1].Any(c => c == IrcConstants.IrcColor))
                {
                    channel.IrcDaemon.Replies.SendCannotSendToChannel(user, channel.Name, "Color is not permitted in this channel");
                    return false;
                }
                if (args[1].Any(c => c == IrcConstants.IrcBold || c == IrcConstants.IrcNormal || c == IrcConstants.IrcUnderline || c == IrcConstants.IrcReverse))
                {
                    channel.IrcDaemon.Replies.SendCannotSendToChannel(user, channel.Name, "Control codes (bold/underline/reverse) are not permitted in this channel");
                    return false;
                }
            }
            return true;
        }
    }
}