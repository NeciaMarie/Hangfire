﻿// This file is part of Hangfire.
// Copyright © 2013-2014 Sergey Odinokov.
// 
// Hangfire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// Hangfire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with Hangfire. If not, see <http://www.gnu.org/licenses/>.

using System;
using Hangfire.Logging;

namespace Hangfire.Server
{
    internal class ServerWatchdog : IBackgroundProcess
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ServerWatchdogOptions _options;

        public ServerWatchdog(ServerWatchdogOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            _options = options;
        }

        public void Execute(BackgroundProcessContext context)
        {
            using (var connection = context.Storage.GetConnection())
            {
                var serversRemoved = connection.RemoveTimedOutServers(_options.ServerTimeout);
                if (serversRemoved != 0)
                {
                    Logger.Info(String.Format(
                        "{0} servers were removed due to timeout", 
                        serversRemoved));
                }
            }

            context.CancellationToken.WaitHandle.WaitOne(_options.CheckInterval);
        }

        public override string ToString()
        {
            return "Server Watchdog";
        }
    }
}