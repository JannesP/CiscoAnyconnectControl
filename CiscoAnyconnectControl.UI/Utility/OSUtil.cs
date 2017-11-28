using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.UI.Utility
{
    abstract class OSUtil : I_OSUtil
    {
        private Lazy<OSUtil> _instance = new Lazy<OSUtil>(() =>
        {
            Console.WriteLine(Environment.OSVersion);
            switch (Environment.OSVersion)
            {
                
            }
            return new WindowsOSUtil();
        });

        public OSUtil Instance => _instance.Value;

        public abstract bool AddUiToSystemStart();
        public abstract bool RemoveUiFromSystemStart();
    }
}
