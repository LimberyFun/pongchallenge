using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PongGame
{
    public enum Proficiency
    {
        Beginner = 3,
        Intermediate = 25,
        Expert = 50
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private double _angle = 90;
        private double _speed;
        private int _padSpeed;
        private Ball _ball;
        private Pad _player1;
        private Pad _player2;
        private readonly DispatcherTimer _timer;
        private bool _levelBreak;
        private bool _isSinglePlayer = false;
        private int _proficency = (int)Proficiency.Beginner;

        private int _lpoints;
        public int LeftPoints
        {
            get { return _lpoints; }
            set
            {
                _lpoints = value;
                OnPropertyChanged("LeftPoints");
            }
        }

        private int _rpoints;
        public int RightPoints
        {
            get { return _rpoints; }
            set
            {
                _rpoints = value;
                OnPropertyChanged("RightPoints");
            }
        }

        private int _level = 0;
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged("Level");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            InitGame();
        }

        private void MainWindow_OnKeyDown(object sender, KeyboardEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.A) && _player1.Y - _padSpeed >= 0) _player1.Y -= _padSpeed;
            if (Keyboard.IsKeyDown(Key.Z) && _player1.Y + _padSpeed <= MainCanvas.ActualHeight - _player1.PadLength)
                _player1.Y += _padSpeed;
            if (Keyboard.IsKeyDown(Key.Up) && _player2.Y - _padSpeed >= 0) _player2.Y -= _padSpeed;
            if (Keyboard.IsKeyDown(Key.Down) && _player2.Y + _padSpeed <= (MainCanvas.ActualHeight - _player2.PadLength))
                _player2.Y += _padSpeed;
        }

        private void InitGame()
        {
            _speed = 3;
            _padSpeed = 70;
            _ball =new Ball { X = 180, Y = 110, MovingRight = true };
            _player1 = new Pad { Y = 90};
            _player2 = new Pad { Y = 90 };

            LeftPoints = 0;
            RightPoints = 0;
            Level = 0;
            _isSinglePlayer = false;
            _levelBreak = false;
            DataContext = this;
            LeftPad.DataContext = _player1;
            RightPad.DataContext = _player2;
            Ball.DataContext = _ball;
            
            if(_timer.IsEnabled)
                _timer.Stop();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Validate the board area
            if (_ball.Y <= 0) 
                _angle = _angle + (180 - 2 * _angle);
            if (_ball.Y >= MainCanvas.ActualHeight - 20) 
                _angle = _angle + (180 - 2 * _angle);

            if (IsCollisioned() == true)
            {
                ChangeAngle();
                AlternateDirection();
            }

            //http://www.mathsisfun.com/geometry/radians.html
            var radians = (Math.PI / 180) * _angle;
            _ball.X += Math.Sin(radians) * _speed;
            _ball.Y += (-Math.Cos(radians)) * _speed;

            if (_ball.X >= 790)
            {
                LeftPoints += 1;
                Reset();
            }
            if (_ball.X <= 5)
            {
                RightPoints += 1;
                Reset();
            }
            //var v = (new Random().Next(1, 100));
            //if (v % 10 == 0)
            {
                if (_ball.X <= 10 && _isSinglePlayer)
                {
                    MovePad((int) _ball.Y);
                }
            }

            if ((LeftPoints != 0 && LeftPoints%10 == 0) || (RightPoints!=0 && RightPoints%10 == 0))
            {
                if(_levelBreak) return;
                if(_lpoints > _rpoints)
                    Level = _lpoints/10;
                if(_rpoints > _lpoints)
                    Level = _rpoints / 10;

                _speed = _level + 3;
                _levelBreak = true;
            }
        }

        private void Reset()
        {
            _ball.Y = 210;
            _ball.X = 380;
            _levelBreak = false;
        }

        private void MovePad(int yPosition)
        {
            _player1.Y = yPosition;
        }

        private void ChangeAngle()
        {
            var r = new Random();
            if (_ball.MovingRight) _angle = 270 - r.Next(1, 40);
            if (_ball.MovingRight == false) _angle = 90 + r.Next(1, 40);
        }

        private void AlternateDirection()
        {
            _ball.MovingRight = !_ball.MovingRight;
        }

        private bool IsCollisioned()
        {
            bool collisionResult = false;
            if (_ball.MovingRight)
                collisionResult = _ball.X >= 760 && (_ball.Y > _player2.Y - 20 && _ball.Y < _player2.Y + 80);

            if (_ball.MovingRight == false)
            {
                collisionResult = _ball.X <= 20 && (_ball.Y > _player1.Y - 20 && _ball.Y < _player1.Y + 80);
            }

            return collisionResult;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            InitGame();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Quit Y/N?", "Pong game Quit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            _isSinglePlayer = true;
            MenuSingle.IsChecked = true;
            MenuDouble.IsChecked = false;
            InitGame();
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            _isSinglePlayer = false;
            MenuSingle.IsChecked = false;
            MenuDouble.IsChecked = true;
            InitGame();
        }
    }
}
