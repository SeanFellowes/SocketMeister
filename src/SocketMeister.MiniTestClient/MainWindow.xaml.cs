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
using SocketMeister;

namespace SocketMeister.MiniTestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ClientControl> _clients = new List<ClientControl>();

        public MainWindow()
        {
            InitializeComponent();

            _clients.Add(Client1);
            _clients.Add(Client2);
            _clients.Add(Client3);
            _clients.Add(Client4);
            _clients.Add(Client5);
            _clients.Add(Client6);

            foreach (ClientControl Client in _clients)
            {
                Client.SendRequestButtonPressed += Client_SendRequestButtonPressed;
            }

        }

        private void Client_SendRequestButtonPressed(object sender, EventArgs e)
        {
            ClientControl client = (ClientControl)sender;
            client.SendRequest("This is a test");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ClientControl c in _clients)
            {
                c.Stop();
            }
        }
    }
}
