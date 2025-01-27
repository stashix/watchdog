﻿using MT5Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingWatchdog.Logic.Services
{
    public interface IWatchdog : IDisposable
    {
        void Start(IEnumerable<ConnectionParams> connectionParams);

        void Stop();
    }
}
