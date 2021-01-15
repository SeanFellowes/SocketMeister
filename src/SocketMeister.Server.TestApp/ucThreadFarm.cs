using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SocketMeister1.Server.TestApp
{
    public partial class ucThreadFarm : UserControl
    {
        private const int spacer = 0;

        private List<Label> lCol1 = new List<Label>();
        private List<Label> lCol2 = new List<Label>();
        private List<Label> lCol3 = new List<Label>();
        private List<Label> lCol4 = new List<Label>();
        private List<Label> lCol6 = new List<Label>();
        private List<Label> lCol7 = new List<Label>();
        private List<IThreadHost> threads = null;


        public ucThreadFarm()
        {
            InitializeComponent();
        }

        private void ucThreadFarm_Resize(object sender, EventArgs e)
        {
            CH1.Left = 0;
            CH2.Left = CH1.Left + CH1.Width + spacer;
            CH3.Left = CH2.Left + CH2.Width + spacer;
            CH4.Left = CH3.Left + CH3.Width + spacer;
            CH6.Left = CH4.Left + CH4.Width + spacer;
            CH7.Left = CH6.Left + CH6.Width + spacer;
            int wi = this.Width - (CH7.Left + 2);
            if (wi > 0) CH7.Width = wi;
            else CH7.Width = 100;
            Repos();
        }

        public void Setup()
        {
            //  CLEAN UP
            foreach (Label lb in lCol1) { this.Controls.Remove(lb); }
            foreach (Label lb in lCol2) { this.Controls.Remove(lb); }
            foreach (Label lb in lCol3) { this.Controls.Remove(lb); }
            foreach (Label lb in lCol4) { this.Controls.Remove(lb); }
            foreach (Label lb in lCol6) { this.Controls.Remove(lb); }
            foreach (Label lb in lCol7) { this.Controls.Remove(lb); }
            lCol1.Clear();
            lCol2.Clear();
            lCol3.Clear();
            lCol4.Clear();
            lCol6.Clear();
            lCol7.Clear();

            threads = Global.Server.ThreadHosts;
            int ctr = 1;
            foreach (IThreadHost th in threads)
            {
                //  COL 1
                Label lb = new Label();
                lb.Text = ctr.ToString();
                lb.TextAlign = ContentAlignment.MiddleRight;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lCol1.Add(lb);
                this.Controls.Add(lb);

                //  COL 2
                lb = new Label();
                lb.Text = th.Description;
                lb.TextAlign = ContentAlignment.MiddleLeft;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lCol2.Add(lb);
                this.Controls.Add(lb);

                //  COL 3
                lb = new Label();
                lb.Text = "";
                lb.TextAlign = ContentAlignment.MiddleLeft;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lCol3.Add(lb);
                this.Controls.Add(lb);

                //  COL 4
                lb = new Label();
                lb.Text = "";
                lb.TextAlign = ContentAlignment.MiddleLeft;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lCol4.Add(lb);
                this.Controls.Add(lb);

                //  COL 6
                lb = new Label();
                lb.Text = "";
                lb.TextAlign = ContentAlignment.MiddleRight;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lCol6.Add(lb);
                this.Controls.Add(lb);

                //  COL 7
                lb = new Label();
                lb.Text = "";
                lb.TextAlign = ContentAlignment.MiddleLeft;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lCol7.Add(lb);
                this.Controls.Add(lb);

                //  SUBSCRIBE TO EVENTS
                th.ErrorCountChanged += thread_ErrorCountChanged;
                th.ProcessingStatusChanged += thread_ProcessingStatusChanged;
                th.IsRunningChanged += thread_IsRunningChanged;
                th.ProcessingDescriptionChanged += thread_ProcessingDescriptionChanged;

                ctr++;
            }


            Repos();
        }

        private void thread_ErrorCountChanged(object sender, ThreadHostErrorCountChangedEventArgs e)
        {
            IThreadHost thread = (IThreadHost)sender;
            SetErrorCount(thread, e.ErrorCount);
        }
        private void thread_IsRunningChanged(object sender, ThreadHostIsRunningChangedEventArgs e)
        {
            IThreadHost thread = (IThreadHost)sender;
            SetIsRunningChanged(thread, e.IsRunning);
        }
        private void thread_ProcessingDescriptionChanged(object sender, ThreadHostProcessingDescriptionChangedEventArgs e)
        {
            IThreadHost thread = (IThreadHost)sender;
            SetProcessingDescription(thread, e.ProcessingDescription);
        }
        private void thread_ProcessingStatusChanged(object sender, ThreadHostIsProcessingChangedEventArgs e)
        {
            IThreadHost thread = (IThreadHost)sender;
            SetProcessingStatusChanged(thread, e.ProcessingStatus);
        }

        private void SetErrorCount(IThreadHost Thread, int Value)
        {
            if (InvokeRequired) Invoke(new MethodInvoker( delegate { SetErrorCount(Thread, Value); } ));
            else
            {
                int index = threads.IndexOf(Thread);
                if (index >= 0) lCol6[index].Text = Value.ToString();
            }
        }
        private void SetProcessingStatusChanged(IThreadHost Thread, ThreadHostProcessingStatusTypes Value)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetProcessingStatusChanged(Thread, Value); }));
            else
            {
                int index = threads.IndexOf(Thread);
                if (index < 0) return;
                if (Value == ThreadHostProcessingStatusTypes.Processing)
                    (lCol4[index]).BackColor = Color.DarkGreen;
                else if (Value == ThreadHostProcessingStatusTypes.Finalizing)
                    (lCol4[index]).BackColor = Color.Orange;
                else
                    (lCol4[index]).BackColor = Color.White;
            }
        }
        private void SetIsRunningChanged(IThreadHost Thread, bool Value)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetIsRunningChanged(Thread, Value); }));
            else
            {
                int index = threads.IndexOf(Thread);
                if (index < 0) return;
                if (Value == true)
                    (lCol3[index]).BackColor = Color.DarkGreen;
                else
                    (lCol3[index]).BackColor = Color.White;
            }
        }
        private void SetProcessingDescription(IThreadHost Thread, string Value)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetProcessingDescription(Thread, Value); }));
            else
            {
                int index = threads.IndexOf(Thread);
                if (index >= 0) lCol7[index].Text = Value;
            }
        }




        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            IThreadHost th = (IThreadHost)cb.Tag;
            th.IsEnabled = cb.Checked; 
        }

        private void Repos()
        {
            try
            {
                int height = 18;
                int rows = lCol1.Count;
                for (int row = 0; row < rows; row++)
                {
                    //  COL 1
                    (lCol1[row]).Height = height;

                    if (row == 0)
                    {
                        (lCol1[row]).Left = CH1.Left;
                        (lCol1[row]).Width = CH1.Width;
                        (lCol1[row]).Top = CH1.Top + CH1.Height + spacer;
                    }
                    else
                    {
                        (lCol1[row]).Left = (lCol1[row - 1]).Left;
                        (lCol1[row]).Width = (lCol1[row - 1]).Width;
                        (lCol1[row]).Top = (lCol1[row - 1]).Top + (lCol1[row - 1]).Height + spacer;
                    }

                    //  COL 2
                    (lCol2[row]).Height = height;

                    if (row == 0)
                    {
                        (lCol2[row]).Left = CH2.Left;
                        (lCol2[row]).Width = CH2.Width;
                        (lCol2[row]).Top = CH2.Top + CH2.Height + spacer;
                    }
                    else
                    {
                        (lCol2[row]).Left = (lCol2[row - 1]).Left;
                        (lCol2[row]).Width = (lCol2[row - 1]).Width;
                        (lCol2[row]).Top = (lCol2[row - 1]).Top + (lCol2[row - 1]).Height + spacer;
                    }

                    //  COL 3
                    (lCol3[row]).Height = height;

                    if (row == 0)
                    {
                        (lCol3[row]).Left = CH3.Left;
                        (lCol3[row]).Width = CH3.Width;
                        (lCol3[row]).Top = CH3.Top + CH3.Height + spacer;
                    }
                    else
                    {
                        (lCol3[row]).Left = (lCol3[row - 1]).Left;
                        (lCol3[row]).Width = (lCol3[row - 1]).Width;
                        (lCol3[row]).Top = (lCol3[row - 1]).Top + (lCol3[row - 1]).Height + spacer;
                    }

                    //  COL 4
                    (lCol4[row]).Height = height;

                    if (row == 0)
                    {
                        (lCol4[row]).Left = CH4.Left;
                        (lCol4[row]).Width = CH4.Width;
                        (lCol4[row]).Top = CH4.Top + CH4.Height + spacer;
                    }
                    else
                    {
                        (lCol4[row]).Left = (lCol4[row - 1]).Left;
                        (lCol4[row]).Width = (lCol4[row - 1]).Width;
                        (lCol4[row]).Top = (lCol4[row - 1]).Top + (lCol4[row - 1]).Height + spacer;
                    }



                    //  COL 6
                    (lCol6[row]).Height = height;

                    if (row == 0)
                    {
                        (lCol6[row]).Left = CH6.Left;
                        (lCol6[row]).Width = CH6.Width;
                        (lCol6[row]).Top = CH6.Top + CH6.Height + spacer;
                    }
                    else
                    {
                        (lCol6[row]).Left = (lCol6[row - 1]).Left;
                        (lCol6[row]).Width = (lCol6[row - 1]).Width;
                        (lCol6[row]).Top = (lCol6[row - 1]).Top + (lCol6[row - 1]).Height + spacer;
                    }

                    //  COL 7
                    (lCol7[row]).Height = height;

                    if (row == 0)
                    {
                        (lCol7[row]).Left = CH7.Left;
                        (lCol7[row]).Width = CH7.Width;
                        (lCol7[row]).Top = CH7.Top + CH7.Height + spacer;
                    }
                    else
                    {
                        (lCol7[row]).Left = (lCol7[row - 1]).Left;
                        (lCol7[row]).Width = (lCol7[row - 1]).Width;
                        (lCol7[row]).Top = (lCol7[row - 1]).Top + (lCol7[row - 1]).Height + spacer;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}
