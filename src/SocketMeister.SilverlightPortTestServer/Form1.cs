using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SocketMeister
{
    internal delegate void SetLabelTextDelegate(Label label, string text);

    internal delegate void ListBoxAddItemDelegate(LogEventArgs Item);

    internal delegate void SetButtonEnabledDelegate(Button button, bool enabled);

    internal delegate void SetCheckBoxEnabledDelegate(CheckBox checkbox, bool enabled);


    public partial class Form1 : Form
    {
        private readonly List<UcSocketServer> _servers = new List<UcSocketServer>();

        public Form1()
        {
            InitializeComponent();

            _servers.Add(ucSocketServer1);
            _servers.Add(ucSocketServer2);
            _servers.Add(ucSocketServer3);
            _servers.Add(ucSocketServer4);
            _servers.Add(ucSocketServer5);
            _servers.Add(ucSocketServer6);
            _servers.Add(ucSocketServer7);
            _servers.Add(ucSocketServer8);
            _servers.Add(ucSocketServer9);
            _servers.Add(ucSocketServer10);
            _servers.Add(ucSocketServer11);
            _servers.Add(ucSocketServer12);
            _servers.Add(ucSocketServer13);
            _servers.Add(ucSocketServer14);
            _servers.Add(ucSocketServer15);
            _servers.Add(ucSocketServer16);
            _servers.Add(ucSocketServer17);
            _servers.Add(ucSocketServer18);
            _servers.Add(ucSocketServer19);
            _servers.Add(ucSocketServer20);
            _servers.Add(ucSocketServer21);
            _servers.Add(ucSocketServer22);
            _servers.Add(ucSocketServer23);
            _servers.Add(ucSocketServer24);
            _servers.Add(ucSocketServer25);
            _servers.Add(ucSocketServer26);
            _servers.Add(ucSocketServer27);
            _servers.Add(ucSocketServer28);
            _servers.Add(ucSocketServer29);
            _servers.Add(ucSocketServer30);
            _servers.Add(ucSocketServer31);
            _servers.Add(ucSocketServer32);
            _servers.Add(ucSocketServer33);

            try
            {
                Top = 0;
                Left = 0;
                Width = 800;

                foreach (UcSocketServer uc in _servers)
                {
                    uc.MessageText = "I had planned on using drones early on—not too much because I think drones can be so overused. But I wanted to also shoot from the hornet's POV. Hornets articulate themselves in a totally different way than just the normal drone beauty shot. That's when I got tipped off about racing drones, which I had not used before. They're smaller, and the way they can articulate through the forest on a dime is very different from the regular drone.";
                    uc.Start();
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }

        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (UcSocketServer uc in _servers)
            {
                uc.Stop(true);
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }






        private void ShowException(Exception ex)
        {
            string e = ex.Message;
            if (ex.StackTrace != null)
            {
                e = e + Environment.NewLine + Environment.NewLine + ex.StackTrace;
            }
            MessageBox.Show(e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //private void tbMessage_TextChanged(object sender, EventArgs e)
        //{
        //    foreach (ucSocketServer uc in _servers)
        //    {
        //        uc.MessageText = tbMessageText.Text;
        //    }
        //}


    }
}
