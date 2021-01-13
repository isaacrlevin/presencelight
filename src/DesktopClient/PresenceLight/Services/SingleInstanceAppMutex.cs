using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Services
{
    class SingleInstanceAppMutex
    {
        private static Mutex s_mutex;

        public static bool TakeExclusivity()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var mutexName = $"Local\\{assembly.GetName().Name}-0e510f7b-aed2-40b0-ad72-d2d3fdc89a02";

            s_mutex = new Mutex(true, mutexName, out bool mutexCreated);
            if (!mutexCreated)
            {
                Trace.WriteLine("SingleInstanceAppMutex TakeExclusivity: false");
                s_mutex = null;
                return false;
            }
            return true;
        }

        public static void ReleaseExclusivity()
        {
            s_mutex?.ReleaseMutex();
            s_mutex?.Close();
            s_mutex = null;
        }
    }
}
