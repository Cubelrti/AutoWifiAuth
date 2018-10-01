using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWifiAuth.Commands
{
    class AutoStartup
    {
        public static void SetAsStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("aldwin", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
        }
    }
}
