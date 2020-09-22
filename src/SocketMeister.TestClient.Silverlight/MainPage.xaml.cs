using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SocketMeister;
using SocketMeister.Testing;

namespace Test.Client.Silverlight
{
    public partial class MainPage : UserControl
    {
        private readonly Guid guid = System.Guid.NewGuid();
        private  SocketMeister.Testing.ClientController testClient = null;

        public MainPage()
        {
            InitializeComponent();
            StatusColour.Background = new SolidColorBrush(Color.FromArgb(255, 234, 144, 15));
            StatusText.Text = "Connecting to test server...";
            //  SEAN SEAN SEAN - GET THIS WORKING
            //testClient = new TestClient(999);
            testClient.ControlBusConnectionStatusChanged += TestClient_ControlBusConnectionStatusChanged;

        }

        private void TestClient_ControlBusConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                StatusText.Text = e.IPAddress + ":" + e.Port;
                if (e.Status == SocketClient.ConnectionStatuses.Connected)
                {
                    StatusColour.Background = new SolidColorBrush(Color.FromArgb(255, 0, 150, 0));
                    StatusText.Text = "Connected to " + e.IPAddress + ":" + e.Port;
                }
                else if (e.Status == SocketClient.ConnectionStatuses.Connecting)
                {
                    StatusColour.Background = new SolidColorBrush(Color.FromArgb(255, 234, 144, 15));
                    StatusText.Text = "Connecting to " + e.IPAddress + ":" + e.Port;
                }
                else if (e.Status == SocketClient.ConnectionStatuses.Disconnecting)
                {
                    StatusColour.Background = new SolidColorBrush(Color.FromArgb(255, 250, 34, 34));
                    StatusText.Text = "Disconnecting from " + e.IPAddress + ":" + e.Port;
                }
                else
                {
                    StatusColour.Background = new SolidColorBrush(Color.FromArgb(255, 131, 131, 131));
                    StatusText.Text = "Disconnected from " + e.IPAddress + ":" + e.Port;
                }
            });
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            testClient.Stop();
        }
    }
}
