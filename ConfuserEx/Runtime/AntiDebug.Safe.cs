﻿using System;
using System.Diagnostics;
using System.Threading;

namespace Confuser.Runtime
{
    internal static class AntiDebugSafe
    {
#pragma warning disable IDE0051
        static void Initialize()
#pragma warning restore IDE0051
        {
            string x = "COR";
            var env = typeof(Environment);
            var method = env.GetMethod("GetEnvironmentVariable", new[] { typeof(string) });
            if (method != null &&
                "1".Equals(method.Invoke(null, new object[] { x + "_ENABLE_PROFILING" })))
                Environment.FailFast(null);

            Thread thread = new Thread(Worker)
            {
                IsBackground = true,
            };
            thread.Start(null);
        }

        static void Worker(object thread)
        {
            if (!(thread is Thread th))
            {
                th = new Thread(Worker)
                {
                    IsBackground = true
                };
                th.Start(Thread.CurrentThread);
                Thread.Sleep(500);
            }
            while (true)
            {
                if (Debugger.IsAttached || Debugger.IsLogging())
                    Environment.FailFast(null);

                if (!th.IsAlive)
                    Environment.FailFast(null);

                Thread.Sleep(1000);
            }
        }
    }
}
