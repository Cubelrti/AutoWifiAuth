using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoWifiAuth.Commands
{
    class NetworkChangeListener
    {
        public MainWindow window;

        public NetworkChangeListener(MainWindow window) {
            NetworkChange.NetworkAddressChanged += new
                NetworkAddressChangedEventHandler(AddressChangedCallback);
            this.window = window;
        }
        void AddressChangedCallback(object sender, EventArgs e)
        {

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface n in adapters)
            {
                //Console.WriteLine("   {0} is {1}", n.Name, n.OperationalStatus);
                if (n.Name.Contains("WLAN"))
                {
                    if(n.OperationalStatus == OperationalStatus.Up)
                        Application.Current.Dispatcher.Invoke(() => window.NewLogin());
                    return;
                }
            }
            
        }
    }
}
