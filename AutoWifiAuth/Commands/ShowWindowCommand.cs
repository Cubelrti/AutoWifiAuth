using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AutoWifiAuth
{
    public class ShowWindowCommand : ICommand
    {
        private static ShowWindowCommand _instance;
        public MainWindow window;

        public static ShowWindowCommand getInstance() {
            if (_instance == null) {
                _instance = new ShowWindowCommand();
            }
            return _instance;
        }

        public void SetContext(MainWindow window) {
            this.window = window;
        }

        public void Execute(object parameter)
        {
            _instance.window.Show();
            _instance.window.Activate();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
