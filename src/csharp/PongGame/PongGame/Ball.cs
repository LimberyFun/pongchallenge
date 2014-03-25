using System.ComponentModel;

namespace PongGame
{
    public class Ball : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private bool _movingRight;

        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                OnPropertyChanged("X");
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged("Y");
            }
        }

        public bool MovingRight
        {
            get { return _movingRight; }
            set
            {
                _movingRight = value;
                OnPropertyChanged("MovingRight");
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