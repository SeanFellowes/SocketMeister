//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Text;
//using System.Windows.Forms;

//namespace SocketMeister
//{
//    public class GridItem : INotifyPropertyChanged
//    {

//        public SeverityType Severity { get; }

//        readonly DateTime _timeStamp = DateTime.Now;

//        public string TimeStamp { get { return _timeStamp.ToString("HH:mm:ss fff"); } }
//        public string Source { get; }
//        public string Text { get; }

//        public GridItem() { }

//        public GridItem(SeverityType Severity, string Source, string Text)
//        {
//            this.Severity = Severity;
//            this.Source = Source;

//            if (Text.Length > 150) this.Text = Text.Substring(0, 147) + "...";
//            else this.Text = Text;

//            this.NotifyPropertyChanged("Severity");
//            this.NotifyPropertyChanged("Source");
//            this.NotifyPropertyChanged("Text");
//        }
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void NotifyPropertyChanged(string name)
//        {
//            if (PropertyChanged != null)
//                PropertyChanged(this, new PropertyChangedEventArgs(name));
//        }
//    }

//}
