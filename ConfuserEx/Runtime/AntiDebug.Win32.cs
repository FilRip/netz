﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Confuser.Runtime
{
    internal static class AntiDebugWin32
    {
#pragma warning disable IDE0051
        static void Initialize()
#pragma warning restore IDE0051
        {
            string x = "COR";
            if (Environment.GetEnvironmentVariable(x + "_PROFILER") != null ||
                Environment.GetEnvironmentVariable(x + "_ENABLE_PROFILING") != null)
                Environment.FailFast(null);

            Thread thread = new Thread(Worker)
            {
                IsBackground = true,
            };
            thread.Start(null);
        }

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern int OutputDebugString(string str);

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
                // Managed
                if (Debugger.IsAttached || Debugger.IsLogging())
                    Environment.FailFast("");

                // IsDebuggerPresent
                if (IsDebuggerPresent())
                    Environment.FailFast("");

                // OpenProcess
                Process ps = Process.GetCurrentProcess();
                if (ps.Handle == IntPtr.Zero)
                    Environment.FailFast("");
                ps.Close();

                // OutputDebugString
                if (OutputDebugString("") > IntPtr.Size)
                    Environment.FailFast("");

                // CloseHandle
                try
                {
                    CloseHandle(IntPtr.Zero);
                }
                catch
                {
                    Environment.FailFast("");
                }

                if (!th.IsAlive)
                    Environment.FailFast("");

                Thread.Sleep(1000);
            }
        }
    }
}
