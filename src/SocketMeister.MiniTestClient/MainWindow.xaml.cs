using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using SocketMeister;

namespace SocketMeister.MiniTestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                this.Top = 0;
                this.Left = 850;
                this.Height = 900;
                this.Visibility = Visibility.Visible;

                lvLog.ItemsSource = _log;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            foreach(ClientControl clientControl in _clients)
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
            Log(LogItem.SeverityType.Error, "Client " + ct.ClientId, e.Exception.Message);
        }

        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;

            byte[] receivedBytes = (byte[])e.Parameters[0];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

            Log(LogItem.SeverityType.Information, "Client " + ct.ClientId, "MessageReceived: " + msgRec);
        }

        private void Client_ServerStopping(object sender, EventArgs e)
        {
            ClientControl ct = (ClientControl)sender;
            Log(LogItem.SeverityType.Warning, "Client " + ct.ClientId, "Server is stopping");
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

            Log(LogItem.SeverityType.Information, "Client " + ct.ClientId, "SubscriptionMessageReceived: " + e.SubscriptionName + ", " + msgRec);
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _dispatcherTimer.Stop();
            foreach (ClientControl c in _clients)
            {
                c.Stop();
            }
        }

        private void Log(LogItem.SeverityType Severity, string Source, string Text)
        {
            Dispatcher.Invoke(() =>
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
                _clients.Add(Client1);
                if (ClientsSlider.Value > 1) _clients.Add(Client2);
                if (ClientsSlider.Value > 2) _clients.Add(Client3);
                if (ClientsSlider.Value > 3) _clients.Add(Client4);
                if (ClientsSlider.Value > 4) _clients.Add(Client5);
                if (ClientsSlider.Value > 5) _clients.Add(Client6);
                if (ClientsSlider.Value > 6) _clients.Add(Client7);
                if (ClientsSlider.Value > 7) _clients.Add(Client8);
                if (ClientsSlider.Value > 8) _clients.Add(Client9);
                if (ClientsSlider.Value > 9) _clients.Add(Client10);
                if (ClientsSlider.Value > 10) _clients.Add(Client11);
                if (ClientsSlider.Value > 11) _clients.Add(Client12);
                if (ClientsSlider.Value > 12) _clients.Add(Client13);
                if (ClientsSlider.Value > 13) _clients.Add(Client14);
                if (ClientsSlider.Value > 14) _clients.Add(Client15);
                if (ClientsSlider.Value > 15) _clients.Add(Client16);
                if (ClientsSlider.Value > 16) _clients.Add(Client17);
                if (ClientsSlider.Value > 17) _clients.Add(Client18);
                if (ClientsSlider.Value > 18) _clients.Add(Client19);
                if (ClientsSlider.Value > 19) _clients.Add(Client20);

                foreach (ClientControl Client in _clients)
                {
                    Client.ExceptionRaised += Client_ExceptionRaised;
                    Client.MessageReceived += Client_MessageReceived;
                    Client.SendRequestButtonPressed += Client_SendRequestButtonPressed;
                    Client.ServerStopping += Client_ServerStopping;
                    Client.SubscriptionMessageReceived += Client_SubscriptionMessageReceived;

                    if (EndpointRB1.IsChecked == true) Client.Start(1);
                    else Client.Start(2);
                }

                _dispatcherTimer.Tick += _dispatcherTimer_Tick;
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                _dispatcherTimer.Start();

                EndpointRB1.IsEnabled = false;
                EndpointRB2.IsEnabled = false;
                StartBtn.IsEnabled = false;
                ClientsSlider.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ClientsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ClientsText == null) return;

            ClientsText.Text = ClientsSlider.Value.ToString();

            if (ClientsSlider.Value > 1) Client2.IsEnabled = true;
            else Client2.IsEnabled = false;

            if (ClientsSlider.Value > 2) Client3.IsEnabled = true;
            else Client3.IsEnabled = false;

            if (ClientsSlider.Value > 3) Client4.IsEnabled = true;
            else Client4.IsEnabled = false;

            if (ClientsSlider.Value > 4) Client5.IsEnabled = true;
            else Client5.IsEnabled = false;

            if (ClientsSlider.Value > 5) Client6.IsEnabled = true;
            else Client6.IsEnabled = false;

        }
    }
}
