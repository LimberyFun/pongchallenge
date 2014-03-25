using System.ComponentModel;

namespace PongGame
{
    public class Pad : INotifyPropertyChanged
    {
        private int _padLength = 80;
        public int PadLength
        {
            get { return _padLength; }
            set
            {
                _padLength = value;
                OnPropertyChanged("PadLength");
            }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged("Y");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}