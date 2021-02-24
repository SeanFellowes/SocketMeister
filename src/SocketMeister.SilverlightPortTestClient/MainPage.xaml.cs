using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SocketMeister
{
    public partial class MainPage : UserControl
    {
        internal class LogItem
        {
            public enum SeverityType { Information = 0, Warning = 1, Error = 2 }

            readonly DateTime timeStamp = DateTime.Now;

            public SeverityType Severity { get; set; }
            public string Source { get; set; }
            public string Text { get; set; }
            public string TimeStamp { get { return timeStamp.ToString("HH:mm:ss fff"); } }
            public SolidColorBrush Background
            {
                get
                {
                    if (Severity == SeverityType.Error) return new SolidColorBrush(Color.FromArgb(255, 255, 204, 204));
                    if (Severity == SeverityType.Warning) return new SolidColorBrush(Color.FromArgb(255, 253, 210, 159));
                    else return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }
            public string SeverityDescription
            {
                get
                {
                    if (Severity == SeverityType.Error) return "Error";
                    if (Severity == SeverityType.Warning) return "Warning";
                    else return "Info";
                }
            }
        }

        DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        List<ClientControl> _clients = new List<ClientControl>();
        ObservableCollection<LogItem> _log = new ObservableCollection<LogItem>();
        bool _windowClosingProcessed = false;


        public MainPage()
        {
            InitializeComponent();

            StopBtn.IsEnabled = false;
            try
            {
                IPAddress.Text = "127.0.0.1";

                lvLog.ItemsSource = _log;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }


        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            foreach (ClientControl clientControl in _clients)
            {
                if (clientControl.TestSubscriptions == true)
                {
                    clientControl.ProcessSubscriptions();
                }
            }
        }

        private void Client_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;
            Log(LogItem.SeverityType.Error, "Client " + ct.Port, e.Exception.Message);
        }

        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;

            byte[] receivedBytes = (byte[])e.Parameters[0];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

            Log(LogItem.SeverityType.Information, "Client " + ct.Port, "MessageReceived: " + msgRec);
        }

        private void Client_ServerStopping(object sender, EventArgs e)
        {
            ClientControl ct = (ClientControl)sender;
            Log(LogItem.SeverityType.Warning, "Client " + ct.Port, "Server is stopping");
        }

        private void Client_SendRequestButtonPressed(object sender, EventArgs e)
        {
            ClientControl client = (ClientControl)sender;
            client.SendRequest(tbTextToSend.Text);
        }

        private void Client_SubscriptionMessageReceived(object sender, SocketClient.SubscriptionMessageReceivedEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;

            byte[] receivedBytes = (byte[])e.Parameters[0];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

            Log(LogItem.SeverityType.Information, "Client " + ct.Port, "SubscriptionMessageReceived: " + e.SubscriptionName + ", " + msgRec);
        }



        private void Log(LogItem.SeverityType Severity, string Source, string Text)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (Text.Length > 150) Text = Text.Substring(0, 147) + "...";
                LogItem i = new LogItem() { Severity = Severity, Source = Source, Text = Text };
                if (_log.Count > 99) _log.RemoveAt(_log.Count - 1);

                _log.Insert(0, i);
            });

        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StopBtn.IsEnabled = true;
                StartBtn.IsEnabled = true;

                _clients.Add(Client1);
                _clients.Add(Client2);
                _clients.Add(Client3);
                _clients.Add(Client4);
                _clients.Add(Client5);
                _clients.Add(Client6);
                _clients.Add(Client7);
                _clients.Add(Client8);
                _clients.Add(Client9);
                _clients.Add(Client10);
                _clients.Add(Client11);
                _clients.Add(Client12);
                _clients.Add(Client13);
                _clients.Add(Client14);
                _clients.Add(Client15);
                _clients.Add(Client16);
                _clients.Add(Client17);
                _clients.Add(Client18);
                _clients.Add(Client19);
                _clients.Add(Client20);
                _clients.Add(Client21);
                _clients.Add(Client22);
                _clients.Add(Client23);
                _clients.Add(Client24);
                _clients.Add(Client25);
                _clients.Add(Client26);
                _clients.Add(Client27);
                _clients.Add(Client28);
                _clients.Add(Client29);
                _clients.Add(Client30);
                _clients.Add(Client31);
                _clients.Add(Client32);
                _clients.Add(Client33);

                foreach (ClientControl Client in _clients)
                {
                    Client.ExceptionRaised += Client_ExceptionRaised;
                    Client.MessageReceived += Client_MessageReceived;
                    Client.SendRequestButtonPressed += Client_SendRequestButtonPressed;
                    Client.ServerStopping += Client_ServerStopping;
                    Client.SubscriptionMessageReceived += Client_SubscriptionMessageReceived;

                    Client.Start(IPAddress.Text);
                }

                _dispatcherTimer.Tick += _dispatcherTimer_Tick;
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                _dispatcherTimer.Start();

                StartBtn.IsEnabled = false;
                IPAddress.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }

        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Stop()
        {
            if (_windowClosingProcessed == false)
            {
                StopBtn.IsEnabled = false;

                _windowClosingProcessed = true;
                _dispatcherTimer.Stop();

                foreach (ClientControl c in _clients)
                {
                    c.IsEnabled = false;
                }

                foreach (ClientControl c in _clients)
                {
                    Thread bgClose = new Thread(
                    new ThreadStart(delegate
                    {
                        c.Stop();
                    }));
                    bgClose.IsBackground = true;
                    bgClose.Start();
                }

                DateTime maxWait = DateTime.Now.AddSeconds(15);
                while (DateTime.Now < maxWait && true == true)
                {
                    bool allClosed = true;

                    foreach (ClientControl c in _clients)
                    {
                        if (c.Status != SocketClient.ConnectionStatuses.Disconnected)
                        {
                            allClosed = false;
                            break;
                        }
                    }

                    if (allClosed == true) break;

                    Thread.Sleep(250);
                }
            }
        }
    }
}
