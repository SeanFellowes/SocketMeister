using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SocketMeister
{
    delegate void SetLabelTextDelegate(Label label, string text);
    delegate void ListBoxAddItemDelegate(LogEventArgs Item);
    delegate void SetButtonEnabledDelegate(Button button, bool enabled);
    delegate void SetCheckBoxEnabledDelegate(CheckBox checkbox, bool enabled);

    public partial class Main : Form
    {
        private readonly List<ucSocketServer> _servers = new List<ucSocketServer>();
        private readonly BindingList<LogEventArgs> _gridItems = new BindingList<LogEventArgs>();
        private readonly object _lock = new object();
        private bool _stopAutomaticMessageGenerator;


        public Main()
        {
            InitializeComponent();

            _servers.Add(ucSocketServer1);
            _servers.Add(ucSocketServer2);

            try
            {
                this.Top = 0;
                this.Left = 0;
                lblStatus.Text = "";

                dGrid.AutoGenerateColumns = false;
                _gridItems.AllowNew = true;
                _gridItems.AllowRemove = true;
                _gridItems.RaiseListChangedEvents = true;
                _gridItems.AllowEdit = false;
                dGrid.DataSource = _gridItems;

                tbMessage_TextChanged(null, null);

                foreach (ucSocketServer uc in _servers)
                {
                    uc.LogEventRaised += ServerUserControl_LogEventRaised;
                    uc.Start();
                }

                //  START A BACKGROUND PROCESS WHICH AUTOMATICALLY GENERATES BROADCASTS
                new Thread(new ThreadStart(delegate { AutomaticMessageGenerator(); })).Start();
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (_lock) { _stopAutomaticMessageGenerator = true; }
            foreach (ucSocketServer uc in _servers)
            {
                uc.Stop(true);
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }


        private void ServerUserControl_LogEventRaised(object sender, LogEventArgs e)
        {
            InsertListboxItem(e);
        }

        private void InsertListboxItem(LogEventArgs Item)
        {
            try
            {
                if (this.dGrid.InvokeRequired)
                {
                    ListBoxAddItemDelegate d = new ListBoxAddItemDelegate(InsertListboxItem);
                    this.Invoke(d, new object[] { Item });
                }
                else
                {
                    if (_gridItems.Count == 0) _gridItems.Add(Item);
                    else _gridItems.Insert(0, Item);
                }
            }
            catch { }
        }



        private void AutomaticMessageGenerator()
        {
            while (true == true)
            {
                lock(_lock) { if (_stopAutomaticMessageGenerator == true) return; }

                foreach (ucSocketServer uc in _servers)
                {
                    if (uc.NextAutomatedSend > DateTime.Now) continue;
                    try
                    {
                        uc.NextAutomatedSend = DateTime.Now.AddMinutes(10);
                        if (uc.SendAutomatedMessage() > 0) uc.NextAutomatedSend = DateTime.Now.AddMilliseconds(500);
                        else uc.NextAutomatedSend = DateTime.Now.AddMilliseconds(1000);
                    }
                    catch (Exception ex)
                    {
                        InsertListboxItem(new LogEventArgs(ex, "Server #" + uc.ServerId.ToString(), "Server #" + uc.ServerId.ToString()));
                        uc.NextAutomatedSend = DateTime.Now.AddMilliseconds(10000);
                    }
                }

                Thread.Sleep(200);
            }
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

        private void tbMessage_TextChanged(object sender, EventArgs e)
        {
            foreach (ucSocketServer uc in _servers)
            {
                uc.MessageText = tbMessageText.Text;
            }
        }


    }

}
