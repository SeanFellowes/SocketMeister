﻿#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly ObservableCollection<LogItem> _log = new ObservableCollection<LogItem>();
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
                if (clientControl.TestSubscriptions == true)
                {
                    clientControl.ProcessSubscriptions();
                }
            }
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
            ClientControl ct = (ClientControl)sender;

            byte[] receivedBytes = (byte[])e.Parameters[0];
            string msgRec;
            if (receivedBytes.Length > 30)
            {
                msgRec = Encoding.UTF8.GetString(receivedBytes, 0, 30) + "...";
            }
            else
            {
                msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);
            }

            Log(SeverityType.Information, "Client " + ct.ClientId, "MessageReceived (" + receivedBytes.Length + " bytes): " + msgRec);
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


        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_windowClosingProcessed)
                    return;

                _windowClosingProcessed = true;
                _dispatcherTimer.Stop();

                // Stop all clients asynchronously
                var stopTasks = _clients.Select(client => Task.Run(() => client.Stop())).ToList();

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

            if (ClientsSlider.Value > 6) Client7.IsEnabled = true;
            else Client7.IsEnabled = false;

            if (ClientsSlider.Value > 7) Client8.IsEnabled = true;
            else Client8.IsEnabled = false;

            if (ClientsSlider.Value > 8) Client9.IsEnabled = true;
            else Client9.IsEnabled = false;

            if (ClientsSlider.Value > 9) Client10.IsEnabled = true;
            else Client10.IsEnabled = false;

            if (ClientsSlider.Value > 10) Client11.IsEnabled = true;
            else Client11.IsEnabled = false;

            if (ClientsSlider.Value > 11) Client12.IsEnabled = true;
            else Client12.IsEnabled = false;

            if (ClientsSlider.Value > 12) Client13.IsEnabled = true;
            else Client13.IsEnabled = false;

            if (ClientsSlider.Value > 13) Client14.IsEnabled = true;
            else Client14.IsEnabled = false;

            if (ClientsSlider.Value > 14) Client15.IsEnabled = true;
            else Client15.IsEnabled = false;

            if (ClientsSlider.Value > 15) Client16.IsEnabled = true;
            else Client16.IsEnabled = false;

            if (ClientsSlider.Value > 16) Client17.IsEnabled = true;
            else Client17.IsEnabled = false;

            if (ClientsSlider.Value > 17) Client18.IsEnabled = true;
            else Client18.IsEnabled = false;

            if (ClientsSlider.Value > 18) Client19.IsEnabled = true;
            else Client19.IsEnabled = false;

            if (ClientsSlider.Value > 19) Client20.IsEnabled = true;
            else Client20.IsEnabled = false;
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'

