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
using System.Windows.Navigation;
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
            private readonly string _source;
            private readonly LogEntry _logEntry;

            public LogItem(LogEntry logEntry, string Source)
            {
                _logEntry = logEntry;
                _source = Source;
            }

            public LogEventType EventType { get { return _logEntry.EventType; } }
            public Severity Severity { get {  return _logEntry.Severity; } }
            public string Source => _source;
            public string Text 
            { 
                get 
                {
                    if (_logEntry.Message.Length > 150) return _logEntry.Message.Substring(0, 147) + "...";
                    return _logEntry.Message; 
                } 
            }
            public string TimeStamp { get { return _logEntry.Timestamp.ToString("HH:mm:ss fff"); } }

            public SolidColorBrush Background
            {
                get
                {
                    if (EventType == LogEventType.Internal) return new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                    else if (Severity == Severity.Debug) return new SolidColorBrush(Color.FromArgb(255, 180, 180, 250));
                    else if (Severity == Severity.Error) return new SolidColorBrush(Color.FromArgb(255, 255, 204, 204));
                    else if (Severity == Severity.Warning) return new SolidColorBrush(Color.FromArgb(255, 253, 210, 159));
                    else return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }
            public SolidColorBrush Foreground
            {
                get
                {
                    if (EventType == LogEventType.Exception) return new SolidColorBrush(Color.FromArgb(255, 220, 0, 0));
                    else if (EventType == LogEventType.ConnectionEvent) return new SolidColorBrush(Color.FromArgb(255, 20, 20, 200));
                    else if (EventType == LogEventType.PollingEvent) return new SolidColorBrush(Color.FromArgb(255, 140, 140, 140));
                    else if (EventType == LogEventType.SubscriptionSyncEvent) return new SolidColorBrush(Color.FromArgb(255, 140, 140, 140));
                    else return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                }
            }

            public string SeverityDescription
            {
                get
                {
                    if (Severity == Severity.Error) return "Error";
                    if (Severity == Severity.Warning) return "Warning";
                    if (Severity == Severity.Debug) return "Debug";
                    else return "Info";
                }
            }
        }


        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly List<ClientControl> _clients = new List<ClientControl>();
        private object _lock = new object();
        private readonly ObservableCollection<LogItem> _log = new ObservableCollection<LogItem>();
        private int _messageSendingTimeoutMs = 5000;
        private int _processingSimulationDelayMs = 1000;
        private bool _windowClosingProcessed = false;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                Top = 0;
                Left = 850;
                Height = SystemParameters.FullPrimaryScreenHeight; 
                Visibility = Visibility.Visible;
                IPAddress.Text = "127.0.0.1";

#if VERSION4
                this.Title = "SocketMeister Mini Test Client (Legacy Version 4)";
#elif NET35
                this.Title = "SocketMeister Mini Test Client (.NET 3.5)";
#else
                this.Title = "SocketMeister Mini Test Client";
#endif

                sendTimeoutSlider.Value = _messageSendingTimeoutMs;
                processingDelaySlider.Value = _processingSimulationDelayMs;

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

        public int MessageSendingTimeoutMs
        {
            get { lock (_lock) { return _messageSendingTimeoutMs; } }
            set { lock (_lock) { _messageSendingTimeoutMs = value; } }
        }


        public int ProcessingSimulationDelayMs
        {
            get { lock (_lock) { return _processingSimulationDelayMs; } }
            set { lock (_lock) { _processingSimulationDelayMs = value; } }
        }


        private void Client_LogRaised(object sender, LogEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;
            Log(e.LogEntry, ct.Name);
        }

 



        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var ct = (ClientControl)sender;
                var receivedBytes = (byte[])e.Parameters[0];

                string preview = Encoding.UTF8.GetString(receivedBytes, 0, Math.Min(receivedBytes.Length, 10));
#if !VERSION4
                string msgRec = $"Message {e.MessageId} received \"{preview}{(receivedBytes.Length > 10 ? "..." : "")}\" ({receivedBytes.Length} bytes)";
#else
                string msgRec = $"Message received \"{preview}{(receivedBytes.Length > 10 ? "..." : "")}\" ({receivedBytes.Length} bytes)";
#endif

                if (ProcessingSimulationDelayMs > 0)
                {
                    msgRec += $" Simulating {(int)processingDelaySlider.Value} ms processing time";
                }

                Log(new LogEntry($"MessageReceived event raised: {msgRec}", Severity.Debug, LogEventType.UserMessage), "Client " + ct.ClientId);
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
            Log(new LogEntry($"ServerStopping event raised.", Severity.Debug, LogEventType.UserMessage), "Client " + ct.ClientId);
        }

        private void Client_SendRequestButtonPressed(object sender, EventArgs e)
        {
            ClientControl client = (ClientControl)sender;
            client.SendMessage(tbTextToSend.Text, MessageSendingTimeoutMs);
        }

        private void Client_BroadcastReceived(object sender, SocketClient.BroadcastReceivedEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;

            byte[] receivedBytes = (byte[])e.Parameters[0];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);
            Log(new LogEntry("BroadcastReceived: " + e.Name + ", " + msgRec, Severity.Debug, LogEventType.UserMessage), "Client " + ct.ClientId);
        }

        private void Client_ResponseReceived(object sender, ResponseReceived e)
        {
            ClientControl ct = (ClientControl)sender;
            Log(new LogEntry($"ResponseReceived event raised.", Severity.Debug, LogEventType.UserMessage), "Client " + ct.ClientId);
        }

        private void Client_StatusChanged(object sender, EventArgs e)
        {
            //ClientControl ct = (ClientControl)sender;
            //Log(new LogEntry("ConnectionStatusChanged event raised: " + ct.Status, Severity.Debug, LogEventType.ConnectionEvent), "Client " + ct.ClientId);
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

                // Cancel closing until tasks complete
                e.Cancel = true;

                // Stop all clients asynchronously
                List<ClientControl> runningClients = _clients.Where(ct => ct.IsRunning).ToList();
                var stopTasks = runningClients.Select(client => Task.Run(() => client.Stop())).ToList();

                await Task.WhenAll(stopTasks); // Ensure all clients stop before proceeding

                // Now allow window to close
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
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


        private void Log(LogEntry logEntry, string ClientId)
        {
            LogItem i = new LogItem(logEntry, ClientId);
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
                    Client.MessageReceived += Client_MessageReceived;
                    Client.SendMessageButtonPressed += Client_SendRequestButtonPressed;
                    Client.ServerStopping += Client_ServerStopping;
                    Client.BroadcastReceived += Client_BroadcastReceived;
                    Client.ResponseReceived += Client_ResponseReceived;
                    Client.LogRaised += Client_LogRaised;
                    Client.StatusChanged += Client_StatusChanged; 

                    List<SocketEndPoint> eps = new List<SocketEndPoint>();
                    if (EndpointRB2.IsChecked == false)
                    {
                        SocketEndPoint ep1 = new SocketEndPoint(IPAddress.Text, 4505);
                        eps.Add(ep1);
                    }
                    else
                    {
                        SocketEndPoint ep1 = new SocketEndPoint(IPAddress.Text, 4505);
                        eps.Add(ep1);

                        SocketEndPoint ep2 = new SocketEndPoint(IPAddress.Text, 4506);
                        eps.Add(ep2);
                    }
                    Client.Start(eps);
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
            MessageSendingTimeoutMs = (int)sendTimeoutSlider.Value;
            sendTimeoutText.Text = sendTimeoutSlider.Value.ToString() + " ms";
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'

