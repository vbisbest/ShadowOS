using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using System.Threading;

namespace System
{
    public static class DispatcherHelper
    {
        public static Dispatcher UIDispatcher
        {
            get
            {
                return (Application.Current == null) ? Dispatcher.CurrentDispatcher : Application.Current.Dispatcher;
            }
        }
    }

    public static class ActionHelper
    {
        #region Action
        public static Thread BeginInvoke(this Action a)
        {
            return a.DelayInvoke(TimeSpan.Zero);
        }

        public static Thread DelayInvoke(this Action a, TimeSpan ts)
        {
            Thread t = new Thread(new ThreadStart(() =>
            {
                try
                {
                    if (ts != TimeSpan.Zero)
                    {
                        Thread.Sleep(ts);
                    }

                    a();
                }
                finally
                {
                }
            }));

            t.IsBackground = true;
            t.Start();

            return t;
        }
        #endregion

        #region Thread
        public static void Invoke(this Action a, Thread t)
        {
            if (t != null)
            {
                Dispatcher d = Dispatcher.FromThread(t);

                if (d != null)
                {
                    d.Invoke(a);
                }
            }
        }

        public static void BeginInvoke(this Action a, Thread t)
        {
            if (t != null)
            {
                Dispatcher d = Dispatcher.FromThread(t);

                a.BeginInvoke(d);
            }
        }

        public static Thread DelayInvoke(this Action a, Thread t, TimeSpan ts)
        {
            Thread rc = null;

            if (t != null)
            {
                Dispatcher d = Dispatcher.FromThread(t);

                rc = a.DelayInvoke(d, ts);
            }

            return rc;
        }
        #endregion

        #region Dispatcher
        public static Thread Delay(this Action a, Dispatcher d, TimeSpan ts)
        {
            Thread t = new Thread(new ThreadStart(() =>
            {
                try
                {
                    if (ts != TimeSpan.Zero)
                    {
                        Thread.Sleep(ts);
                    }

                    a.Invoke(d);
                }
                finally
                {
                }
            }));

            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.MTA); // Doesn't need to run COM
            t.Start();

            return t;
        }

        public static DispatcherOperation Invoke(this Action a, Dispatcher d)
        {
            DispatcherOperation rc = null;

            if (d != null)
            {
                if (!d.CheckAccess())
                {
                    d.Invoke(Wrap(a));
                }
                else
                {
                    Wrap(a)();
                }
            }

            return rc;
        }

        public static DispatcherOperation BeginInvoke(this Action a, Dispatcher d)
        {
            DispatcherOperation rc = null;

            if (d != null)
            {
                if (!d.CheckAccess())
                {
                    rc = d.BeginInvoke(DispatcherPriority.Send, new ThreadStart(Wrap(a)));
                }
                else
                {
                    Wrap(a)();
                }
            }

            return rc;
        }

        public static Thread CleanInvoke(this Action a, Dispatcher d)
        {
            Thread rc = null;

            if (d.CheckAccess())
            {
                Action background = () =>
                {
                    a.Invoke(d);
                };

                rc = background.BeginInvoke();
            }
            else
            {
                a.BeginInvoke(d);
            }

            return rc;
        }

        public static Thread DelayInvoke(this Action a, Dispatcher d, TimeSpan ts)
        {
            Thread t = new Thread(new ThreadStart(() =>
            {
                try
                {
                    if (ts != TimeSpan.Zero)
                    {
                        Thread.Sleep(ts);
                    }

                    a.BeginInvoke(d);
                }
                finally
                {
                }
            }));

            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.MTA);
            t.Start();

            return t;
        }
        #endregion

        static Action Wrap(Action a)
        {
            Action rc = () =>
            {
                try
                {
                    a();
                }
                finally
                {
                }
            };

            return rc;
        }
    }
}
