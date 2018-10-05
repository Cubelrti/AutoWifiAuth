using AutoWifiAuth.Commands;
using AutoWifiAuth.Dtos;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoWifiAuth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ShowWindowCommand.getInstance().SetContext(this);
            InitializeComponent();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartService(null, null);
        }

        private void StartService(object sender, RoutedEventArgs e)
        {
            // stop the original thread.
            source.Cancel();
            // reissue a token source.
            source = new CancellationTokenSource();
            AddState("[AuthService] startService.");
            Hide();
            MyNotifyIcon.ShowBalloonTip("WifiAuth", "Auto Wifi Auth service is running,", BalloonIcon.Info);
            ServiceImpl(source.Token);
        }

        public async Task NewLogin()
        {
            var ih = Helpers.Internet.GetInstance();

            // access 10.1.61.1
            try
            {
                await WebLoginMethod(ih);
            }
            catch (Exception)
            {
                // access 10.10.43.3
                var data = await ih.GetAsync(Configs.Protocol.serverUri);
                if ((await data.ReadAsStringAsync()).Contains("注销页"))
                {
                    //logined.
                    return;
                }
                // construct url param
                DateTime baseDate = new DateTime(1970, 1, 1);
                TimeSpan diff = DateTime.Now - baseDate;
                var msec = Math.Floor(diff.TotalMilliseconds);
                var querystr = $"drcom/login?callback=dr{msec}&DDDDD={Username.Text}&upass={Password.Password}&0MKKey=123456&R1=0&R3=0&R6=0&para=00&v6ip=&_={msec}";
                var signin = await ih.GetAsync(Configs.Protocol.serverUri + querystr);
                var result = await signin.ReadAsStringAsync();
                if (result.Contains("NID") || result.Contains("etime"))
                {
                    AddState("[AuthClient] Validated.");
                    // auto remember
                    User user = new User
                    {
                        Username = Username.Text,
                        Password = Password.Password
                    };
                    File.WriteAllText(@"user.json", JsonConvert.SerializeObject(user));
                    // popup
                    MyNotifyIcon.ShowBalloonTip("Success", "You are authed into BJTU Wifi.", BalloonIcon.Info);
                }
                else
                {
                    AddState("[AuthClient] Error.");
                    await Task.Delay(5000);
                }
            }
        }

        public async Task Login()
        {
            var ih = Helpers.Internet.GetInstance();

            //access 10.10.42.3
            {
                var data = await ih.GetAsync(Configs.Protocol.serverUri);
                if ((await data.ReadAsStringAsync()).Contains("注　销"))
                {
                    //logined.
                    return;
                }
            }
            // get md5calg data.
            string text = "", code = "";
            try
            {
                var data = await ih.GetAsync(Configs.Protocol.codeUri);
                text = await data.ReadAsStringAsync();
                code = text.Substring(9, 8);
                if(code == " html PU")
                {
                    await WebLoginMethod(ih);
                }

                AddState("[AuthClient] getcode:" + code);
                // psw calg

                string fullcode = String.Concat(Configs.Protocol.pid, Password.Password, code);

                string md5 = Helpers.Crypto.CalculateMD5Hash(fullcode).ToLower();

                string upass = String.Concat(md5, code, Configs.Protocol.pid);

                var formData = new Dictionary<string, string> {
                    {"DDDDD",Username.Text },
                    {"upass",upass },
                    {"R1","0" },
                    {"R2","1" },
                    {"para","00" },
                    {"hid1","" },
                    {"hid2","" },
                    {"0MKKey","123456" }
                };
                AddState("[AuthClient] Validating...");
                var returnStatus = await (await ih.PostAsync(Configs.Protocol.serverUri, formData)).ReadAsStringAsync();
                if (returnStatus.Contains("您已经成功登录。"))
                {
                    AddState("[AuthClient] Validated.");
                    // auto remember
                    User user = new User
                    {
                        Username = Username.Text,
                        Password = Password.Password
                    };
                    File.WriteAllText(@"user.json", JsonConvert.SerializeObject(user));
                    // popup
                    MyNotifyIcon.ShowBalloonTip("Success", "You are authed into BJTU Wifi.", BalloonIcon.Info);
                }
                else
                {
                    AddState("[AuthClient] Error.");
                }
            }
            catch (Exception)
            {
                AddState("[AuthClient] GetCodeFailedException:retry");
            }
        }

        private async Task WebLoginMethod(Helpers.Internet ih)
        {
            var result = await (await ih.PostAsync(Configs.Protocol.anotherServerUri, new Dictionary<string, string> {
                        {"DDDDD",Username.Text },
                        {"upass",Password.Password },
                        {"C2","on" },
                        {"0MKKey","��¼(Login)" }
                    })).ReadAsStringAsync();
            if (result.Contains("您已经成功登录。"))
            {
                AddState("[AuthClient] Validated.");
                // auto remember
                User user = new User
                {
                    Username = Username.Text,
                    Password = Password.Password
                };
                File.WriteAllText(@"user.json", JsonConvert.SerializeObject(user));
                // popup
                MyNotifyIcon.ShowBalloonTip("Success", "You are authed into BJTU Wifi.", BalloonIcon.Info);
            }
            else
            {
                AddState("[AuthClient] Error.");
                throw new Exception("Web Login Failed");
            }
            AddState("[AuthClient] Validated.");
            return;
        }

        CancellationTokenSource source = new CancellationTokenSource();

        private void StopService(object sender, RoutedEventArgs e) {
            source.Cancel();
            AddState("[AuthService] Stopped.");
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void ServiceImpl(CancellationToken cancellationToken) {
            var ih = Helpers.Internet.GetInstance();
            while (true)
            {
                try
                {
                    //access 10.10.42.3
                    var data = await ih.GetAsync(Configs.Protocol.serverUri);
                    var text = await data.ReadAsStringAsync();
                    var webData = await ih.GetAsync("http://10.1.61.1");
                    var webText = await webData.ReadAsStringAsync();
                    if (text.Contains("注销页") || webText.Contains("注销页"))
                    {
                        //logined.
                        await Task.Delay(60000, cancellationToken);
                    }
                    else
                    {
                        //try to login.
                        await NewLogin();
                    }
                }
                catch (Exception)
                {
                    // network error
                    //MyNotifyIcon.ShowBalloonTip("WifiAuth", e.ToString() , BalloonIcon.Error);
                    await Task.Delay(60000);
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    continue;
                }
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        public void AddState(string state)
        {
            var item = new ListBoxItem()
            {
                Content = state
            };
            
            Status.Items.Add(item);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // add event listener
            Commands.NetworkChangeListener listener = new Commands.NetworkChangeListener(this);
            try
            {
                // get saved username and psws
                User user = JsonConvert.DeserializeObject<User>(File.ReadAllText(@"./user.json"));
                AddState("[AuthRemember] OK.");
                Username.Text = user.Username;
                Password.Password = user.Password;
                StartService(this, null);
            }
            catch (IOException)
            {
                AddState("[AuthRemember] Not Found. Enter manually.");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AutoStartup.SetAsStartup();
        }
    }
}
