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

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcD.Modes
{
    public class ModeList<TMode> : SortedList<char, TMode> where TMode : Mode
    {
        protected IrcDaemon IrcDaemon;

        public ModeList(IrcDaemon ircDaemon)
        {
            IrcDaemon = ircDaemon;
        }

        public void Add<T>(T element) where T : TMode
        {
            bool exists = (bool)typeof(ModeList<TMode>).GetMethod("Exist").MakeGenericMethod(new[] { element.GetType() }).Invoke(this, null);
            if (exists == false)
            {
                Add(element.Char, element);
            }
        }

        public T GetMode<T>() where T : TMode
        {
            return Values.FirstOrDefault(mode => mode is T) as T;
        }

        public void RemoveMode<T>() where T : TMode
        {
            if (Exist<T>())
            {
                var mode = GetMode<T>();
                Remove(mode.Char);
            }
        }

        public override string ToString()
        {
            var modes = new StringBuilder();

            foreach (var mode in Values)
            {
                modes.Append(mode.Char);
            }

            return modes.ToString();
        }

        public bool Exist<TExist>() where TExist : TMode
        {
            return Values.Any(m => m is TExist);
        }
    }
}