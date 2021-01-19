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
                    else return new SolidColorBrush(Color.FromArgb(255, 255, 204, 204));
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


        List<ClientControl> _clients = new List<ClientControl>();
        ObservableCollection<LogItem> _log = new ObservableCollection<LogItem>();

        public MainWindow()
        {
            InitializeComponent();

            lvLog.ItemsSource = _log;

            _clients.Add(Client1);
            _clients.Add(Client2);
            _clients.Add(Client3);
            _clients.Add(Client4);
            _clients.Add(Client5);
            _clients.Add(Client6);

            foreach (ClientControl Client in _clients)
            {
                Client.SendRequestButtonPressed += Client_SendRequestButtonPressed;
                Client.ExceptionRaised += Client_ExceptionRaised;
            }
        }

        private void Client_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ClientControl ct = (ClientControl)sender;
            Log("Error", "Client " + ct.ClientId, e.Exception.Message);
        }

        private void Client_SendRequestButtonPressed(object sender, EventArgs e)
        {
            ClientControl client = (ClientControl)sender;
            client.SendRequest(tbTextToSend.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ClientControl c in _clients)
            {
                c.Stop();
            }
        }

        private void Log(string LogType, string Source, string Text)
        {
            Dispatcher.Invoke(() =>
            {
                if (Text.Length > 200) Text = Text.Substring(0, 197) + "...";
                LogItem i = new LogItem() { Severity =  LogItem.SeverityType.Error, Source = Source, Text = Text };
                if (_log.Count > 99) _log.RemoveAt(_log.Count - 1);

                _log.Insert(0, i);
            });

        }
    }
}
