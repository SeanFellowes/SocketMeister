using System;
using System.Collections.Generic;
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

namespace SocketMeister.MiniTestClient
{
    /// <summary>
    /// Interaction logic for ClientControl.xaml
    /// </summary>
    public partial class ClientControl : UserControl
    {
        private int _clientId = 1;
        private int _requestsReceived = 0;
        private int _requestsSent = 0;
        private int _messagesReceived = 0;
        private SocketClient _client = null;

        /// <summary>
        /// Event raised when an exception occurs
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;

        public event EventHandler<EventArgs> SendRequestButtonPressed;

        public event EventHandler<EventArgs> StatusChanged;

        /// <summary>
        /// Event raised whenever a message is received from the server.
        /// </summary>
        public event EventHandler<SocketClient.MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raised when a request message is received from the server. A response can be provided which will be returned to the server.
        /// </summary>
        public event EventHandler<SocketClient.RequestReceivedEventArgs> RequestReceived;



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

        public SocketClient.ConnectionStatuses Status
        {
            get
            {
                if (_client == null) return SocketClient.ConnectionStatuses.Disconnected;
                else return _client.ConnectionStatus;
            }
        }

        public void SendRequest(string Message)
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

                Dispatcher.Invoke(() =>
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


        public void Start()
        {
            List<SocketEndPoint> eps = new List<SocketEndPoint>();

            SocketEndPoint ep1 = new SocketEndPoint("127.0.0.1", 4505);
            SocketEndPoint ep2 = new SocketEndPoint("127.0.0.1", 4506);

            eps.Add(ep1);
            eps.Add(ep2);

            _client = new SocketClient(eps, true);
            _client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            _client.ExceptionRaised += Client_ExceptionRaised;
            _client.MessageReceived += Client_MessageReceived;
            _client.RequestReceived += Client_RequestReceived;
        }

        public void Stop()
        {
            if (_client == null) throw new Exception("Client has not been started");

            _client.Stop();
            _client.ConnectionStatusChanged -= Client_ConnectionStatusChanged;
            _client.ExceptionRaised -= Client_ExceptionRaised;
            _client.MessageReceived -= Client_MessageReceived;
            _client.RequestReceived -= Client_RequestReceived;
            _client.Dispose();
        }

        private void Client_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Status == SocketClient.ConnectionStatuses.Connected) bdStatus.Background = new SolidColorBrush(Colors.Green);
                else if (e.Status == SocketClient.ConnectionStatuses.Disconnected) bdStatus.Background = new SolidColorBrush(Colors.Red);
                else bdStatus.Background = new SolidColorBrush(Colors.Orange);

                if (e.Status == SocketClient.ConnectionStatuses.Connected) tbPort.Text = e.Port.ToString();
                else tbPort.Text = "-";
            });
            StatusChanged?.Invoke(this, new EventArgs());
        }

        private void Client_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ExceptionRaised?.Invoke(this, e);
        }

        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _messagesReceived++;
                tbMessagesReceived.Text = _messagesReceived.ToString();
            });
            MessageReceived?.Invoke(this, e);
        }

        private void Client_RequestReceived(object sender, SocketClient.RequestReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _requestsReceived++;
                tbRequestsReceived.Text = _requestsReceived.ToString();
            });
            RequestReceived?.Invoke(this, e);
        }


        private void btnSendRequest_Click(object sender, RoutedEventArgs e)
        {
            SendRequestButtonPressed?.Invoke(this, new EventArgs());
        }
    }

}
