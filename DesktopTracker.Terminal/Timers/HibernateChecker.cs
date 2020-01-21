using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DesktopTracker.Terminal.Timers
{
    public class HibernateChecker
    {
        private readonly DateTime maxHibernateTime;

        public HibernateChecker(DateTime max)
        {
            maxHibernateTime = max;
        }

        public void CheckHibernate(object state)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)state;

            if (DateTime.UtcNow >= maxHibernateTime)
            {
                //Bring the PC to sleep
                SetSuspendState(false, true, true);

                //Signal thread to stop, since we call true
                autoEvent.Set();
            }
        }

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);


    }
}
