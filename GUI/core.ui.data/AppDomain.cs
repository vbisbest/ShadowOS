using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace core.ui.data
{
    public class AppDomain
    {
        public static AppDomain CurrentDomain { get; private set; }

        static AppDomain()
        {
            CurrentDomain = new AppDomain();
        }

        public List<Assembly> GetAssemblies()
        {
            List<Assembly> rc = new List<Assembly>();

            rc.AddRange(System.AppDomain.CurrentDomain.GetAssemblies());

            return rc;
        }
    }
}
