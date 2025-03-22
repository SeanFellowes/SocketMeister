#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace SocketMeister.MiniTestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal class LogItem
        {
            private readonly DateTime timeStamp = DateTime.UtcNow;

            public SeverityType Severity { get; set; }
            public string Source { get; set; }
            public string Text { get; set; }
            public string TimeStamp => timeStamp.ToString("HH:mm:ss fff");
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

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly List<ClientControl> _clients = new List<ClientControl>();
        private object _lock = new object();
        private readonly ObservableCollection<LogItem> _log = new ObservableCollection<LogItem>();
        private int _processingSimulationDelayMs = 1000;
        private bool _windowClosingProcessed = false;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                Top = 0;
                Left = 850;
                Height = 1000;
                Visibility = Visibility.Visible;
                IPAddress.Text = "127.0.0.1";

                lvLog.ItemsSource = _log;

                //  Add Clients
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
        }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            foreach (ClientControl clientControl in _clients)
            {
                if (clientControl.IsRunning && clientControl.TestSubscriptions == true)
                {
                    clientControl.ProcessSubscriptions();
                }
            }
        }

        public int ProcessingSimulationDelayMs
        {
            get { lock (_lock) { return _processingSimulationDelayMs; } }
            set { lock (_lock) { _processingSimulationDelayMs = value; } }
        }


        private void Client_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;
            if (ct.TraceEvents == true) return; // Ignore exceptions if trace events are enabled as they will be logged
            Log(SeverityType.Error, "Client " + ct.ClientId, e.Exception.Message);
        }

        private void Client_TraceEventReceived(object sender, TraceEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;
            Log(e.Severity, e.Source, e.Message);
        }


        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var ct = (ClientControl)sender;
                var receivedBytes = (byte[])e.Parameters[0];

                string preview = Encoding.UTF8.GetString(receivedBytes, 0, Math.Min(receivedBytes.Length, 10));
                string msgRec = $"Message {e.MessageId} received \"{preview}{(receivedBytes.Length > 10 ? "..." : "")}\" ({receivedBytes.Length} bytes)";

                if (ProcessingSimulationDelayMs > 0)
                {
                    msgRec += $" Simulating {(int)processingDelaySlider.Value} ms processing time";
                }

                Log(SeverityType.Information, $"Client {ct.ClientId}", msgRec);
            }));

            if (ProcessingSimulationDelayMs > 0)
            {
                DateTime loopUntil = DateTime.Now.AddMilliseconds(ProcessingSimulationDelayMs);
                while (DateTime.Now < loopUntil)
                {
                    Thread.Sleep(50);
                }
            }
        }


        private void Client_ServerStopping(object sender, EventArgs e)
        {
            ClientControl ct = (ClientControl)sender;
            if (ct.TraceEvents == true) return; // Ignore if trace events are enabled as they will be logged
            Log(SeverityType.Warning, "Client " + ct.ClientId, "Server has notified that it is stopping.");
        }

        private void Client_SendRequestButtonPressed(object sender, EventArgs e)
        {
            ClientControl client = (ClientControl)sender;
            client.SendMessage(tbTextToSend.Text);
        }

        private void Client_BroadcastReceived(object sender, SocketClient.BroadcastReceivedEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;

            byte[] receivedBytes = (byte[])e.Parameters[0];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);
            Log(SeverityType.Information, "Client " + ct.ClientId, "BroadcastReceived: " + e.Name + ", " + msgRec);
        }

        private void Client_ResponseReceived(object sender, ResponseReceived e)
        {
            ClientControl ct = (ClientControl)sender;
            Log(e.Severity, "Client " + ct.ClientId, e.DisplayText);
        }


#if !NET35
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_windowClosingProcessed)
                    return;

                _windowClosingProcessed = true;
                _dispatcherTimer.Stop();

                // Stop all clients asynchronously
                List<ClientControl> runningClients = new List<ClientControl>();
                foreach (ClientControl ct in _clients)
                {
                    if (ct.IsRunning) runningClients.Add(ct);
                }
                var stopTasks = runningClients.Select(client => Task.Run(() => client.Stop())).ToList();

                // Wait for all clients to stop
                await Task.WhenAll(stopTasks);

                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex);
                throw;
            }
        }
#else
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_windowClosingProcessed)
                    return;

                _windowClosingProcessed = true;
                _dispatcherTimer.Stop();

                // Stop all clients synchronously
                foreach (ClientControl ct in _clients)
                {
                    if (ct.IsRunning)
                    {
                        try
                        {
                            ct.Stop(); // Assuming Stop() is synchronous in .NET 3.5
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error stopping client: " + ex);
                        }
                    }
                }

                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex);
                throw;
            }
        }
#endif


        private void Log(SeverityType Severity, string Source, string Text)
        {
            if (Text.Length > 150) Text = Text.Substring(0, 147) + "...";
            LogItem i = new LogItem() { Severity = Severity, Source = Source, Text = Text };
            if (_log.Count > 99) _log.RemoveAt(_log.Count - 1);

            _log.Insert(0, i);
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClientControl Client;
                for (int x = 0; x < ClientsSlider.Value; x++ )
                {
                    Client = _clients[x];
                    Client.ExceptionRaised += Client_ExceptionRaised;
                    Client.MessageReceived += Client_MessageReceived;
                    Client.SendMessageButtonPressed += Client_SendRequestButtonPressed;
                    Client.ServerStopping += Client_ServerStopping;
                    Client.BroadcastReceived += Client_BroadcastReceived;
                    Client.ResponseReceived += Client_ResponseReceived;
                    Client.TraceEventReceived += Client_TraceEventReceived;

                    if (EndpointRB2.IsChecked == false)
                    {
                        Client.Start(IPAddress.Text, 4505);
                    }
                    else
                    {
                        List<SocketEndPoint> eps = new List<SocketEndPoint>();

                        SocketEndPoint ep1 = new SocketEndPoint(IPAddress.Text, 4505);
                        eps.Add(ep1);

                        SocketEndPoint ep2 = new SocketEndPoint(IPAddress.Text, 4506);
                        eps.Add(ep2);

                        Client.Start(eps);
                    }
                }

                _dispatcherTimer.Tick += DispatcherTimer_Tick;
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                _dispatcherTimer.Start();

                EndpointRB1.IsEnabled = false;
                EndpointRB2.IsEnabled = false;
                StartBtn.IsEnabled = false;
                ClientsSlider.IsEnabled = false;
                IPAddress.IsEnabled = false;
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

            ClientControl Client;
            for (int x = 0; x < _clients.Count; x++)
            {
                Client = _clients[x];
                if (x < ClientsSlider.Value)
                {
                    Client.IsEnabled = true;
                }
                else
                {
                    Client.IsEnabled = false;
                }               
            }
        }

        private void processingDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (processingDelayText == null) return;    //  Occurs during form initialization
            processingDelayText.Text = processingDelaySlider.Value.ToString() + " ms";
            ProcessingSimulationDelayMs = (int)processingDelaySlider.Value;
        }

        private void sendTimeoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sendTimeoutText == null) return;    // Occurs during form initialization
            sendTimeoutText.Text = sendTimeoutSlider.Value.ToString() + " ms";
            for (int x = 0; x < _clients.Count; x++)
            {
                _clients[x].MessageSendingTimeoutMs = (int)sendTimeoutSlider.Value;
            }
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'

