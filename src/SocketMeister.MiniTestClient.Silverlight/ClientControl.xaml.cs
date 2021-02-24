using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SocketMeister
{
    public partial class ClientControl : UserControl
    {
        private int _clientId = 1;
        private int _requestsReceived = 0;
        private int _requestsSent = 0;
        private Random _rnd = new Random();
        private int _messagesReceived = 0;
        private int _subscriptionCount = 0;
        private SocketClient _client = null;

        /// <summary>
        /// Event raised when an exception occurs
        /// </summary>
        internal event EventHandler<ExceptionEventArgs> ExceptionRaised;

        public event EventHandler<EventArgs> SendRequestButtonPressed;

        public event EventHandler<EventArgs> StatusChanged;

        /// <summary>
        /// Event raised whenever a message is received from the server.
        /// </summary>
        internal event EventHandler<SocketClient.MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Event raised whenever a subscription message is received from the server.
        /// </summary>
        internal event EventHandler<SocketClient.SubscriptionMessageReceivedEventArgs> SubscriptionMessageReceived;

        /// <summary>
        /// Raised when a request message is received from the server. A response can be provided which will be returned to the server.
        /// </summary>
        internal event EventHandler<SocketClient.RequestReceivedEventArgs> RequestReceived;

        /// <summary>
        /// Raised when a request message is received from the server. A response can be provided which will be returned to the server.
        /// </summary>
        public event EventHandler<EventArgs> ServerStopping;

        public ClientControl()
        {
            InitializeComponent();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbPort.Text = "-";
        }



        public int ClientId
        {
            get { return _clientId; }
            set
            {
                _clientId = value;
                tbClientId.Text = value.ToString();
            }

        }

        public bool TestSubscriptions
        {
            get { return cbSubscriptions.IsChecked.Value; }
            set
            {
                cbSubscriptions.IsChecked = value;
            }

        }

        internal SocketClient.ConnectionStatuses Status
        {
            get
            {
                if (_client == null) return SocketClient.ConnectionStatuses.Disconnected;
                else return _client.ConnectionStatus;
            }
        }

        public void ProcessSubscriptions()
        {
            if (cbSubscriptions.IsChecked == false) return;

            int cnt = _rnd.Next(1, 21);
            int direction = _rnd.Next(1, 3);


            if (direction == 1)
            {
                for (int ctr = 1; ctr <= cnt; ctr++)
                {
                    _client.AddSubscription(Guid.NewGuid().ToString());
                }
            }
            else
            {
                for (int ctr = 1; ctr <= cnt; ctr++)
                {
                    List<string> subs = _client.GetSubscriptions();
                    if (subs.Count > 0) _client.RemoveSubscription(subs[0]);
                    else break;
                }
            }

            if (_client.DoesSubscriptionExist("My Test Subscription 1") == false)
            {
                _client.AddSubscription("My Test Subscription 1");
            }

            int count = _client.GetSubscriptions().Count;
            tbSubscriptions.Text = count.ToString();
        }

        internal void SendRequest(string Message)
        {
            try
            {
                if (string.IsNullOrEmpty(Message)) throw new ArgumentNullException("Message cannot be null or empty");
                if (_client == null) throw new Exception("Client is null");

                byte[] toSend = new byte[Message.Length];
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(Message), 0, toSend, 0, toSend.Length);
                object[] p = new object[2];
                p[0] = _clientId;
                p[1] = toSend;
                byte[] result = _client.SendRequest(p);

                Dispatcher.BeginInvoke(() =>
                {
                    _requestsSent++;
                    tbRequestsSent.Text = _requestsSent.ToString();

                });
            }
            catch (Exception ex)
            {
                ExceptionRaised?.Invoke(this, new ExceptionEventArgs(ex, 1200));
            }
        }


        internal void Start(List<SocketEndPoint> eps)
        {
            _client = new SocketClient(eps, true);
            _client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            _client.CurrentEndPointChanged += Client_CurrentEndPointChanged;
            _client.ExceptionRaised += Client_ExceptionRaised;
            _client.MessageReceived += Client_MessageReceived;
            _client.RequestReceived += Client_RequestReceived;
            _client.ServerStopping += Client_ServerStopping;
            _client.SubscriptionMessageReceived += Client_SubscriptionMessageReceived;

            tbPort.Text = _client.CurrentEndPoint.Port.ToString();
        }

        public void Stop()
        {
            if (_client == null) throw new Exception("Client has not been started");

            //bdStatus.Background = new SolidColorBrush(Colors.Orange);

            _client.Stop();
            ////_client.ConnectionStatusChanged -= Client_ConnectionStatusChanged;
            //_client.CurrentEndPointChanged -= Client_CurrentEndPointChanged;
            //_client.ExceptionRaised -= Client_ExceptionRaised;
            //_client.MessageReceived -= Client_MessageReceived;
            //_client.RequestReceived -= Client_RequestReceived;
            //_client.ServerStopping -= Client_ServerStopping;
            //_client.SubscriptionMessageReceived -= Client_SubscriptionMessageReceived;
            _client.Dispose();

            //bdStatus.Background = new SolidColorBrush(Colors.Red);
        }

        private void Client_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (e.Status == SocketClient.ConnectionStatuses.Connected) bdStatus.Background = new SolidColorBrush(Colors.Green);
                else if (e.Status == SocketClient.ConnectionStatuses.Disconnected) bdStatus.Background = new SolidColorBrush(Colors.Red);
                else bdStatus.Background = new SolidColorBrush(Colors.Orange);
            });
            StatusChanged?.Invoke(this, new EventArgs());
        }

        private void Client_CurrentEndPointChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                tbPort.Text = _client.CurrentEndPoint.Port.ToString();
            });
        }



        private void Client_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ExceptionRaised?.Invoke(this, e);
        }

        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _messagesReceived++;
                tbMessagesReceived.Text = _messagesReceived.ToString();
            });
            MessageReceived?.Invoke(this, e);
        }

        private void Client_SubscriptionMessageReceived(object sender, SocketClient.SubscriptionMessageReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _messagesReceived++;
                tbMessagesReceived.Text = _messagesReceived.ToString();
            });
            SubscriptionMessageReceived?.Invoke(this, e);
        }



        private void Client_RequestReceived(object sender, SocketClient.RequestReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _requestsReceived++;
                tbRequestsReceived.Text = _requestsReceived.ToString();
            });
            RequestReceived?.Invoke(this, e);
        }

        private void Client_ServerStopping(object sender, EventArgs e)
        {
            ServerStopping?.Invoke(this, e);
        }

        private void btnSendRequest_Click(object sender, RoutedEventArgs e)
        {
            SendRequestButtonPressed?.Invoke(this, new EventArgs());
        }

    }
}
