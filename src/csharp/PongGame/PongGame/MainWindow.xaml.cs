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
    public partial class MainWindow : INotifyPropertyChanged
    {
        private double ballX;
        private double ballY;
        private double padHeight;
        private int player1Possition;
        private int player2Possition;
        private double Player1Score;
        private double Player2Score;
       // private bool _beSlave;
       

        private double _angle = 90;
        private double _speed;
        private int _padSpeed;
        private Ball _ball;
        private Pad _player1;
        private Pad _player2;
        private readonly DispatcherTimer _timer;
        private bool _levelBreak;
        private bool _isSinglePlayer = false;
        private readonly INetworkManager _networkManager;
        private INetworkManager _networkManagerPeer;
        private bool _beSlave;
        private bool _peered = false;
        private const string MyAddress = "127.0.0.1:1118";
        private bool _networkMode = false;

        public Action<Message> OnMessageReceived;
        public Action<GameUpdate> OnGameUpdateReceived;
        public Action<ControlInput> OnControlUpdateReceived;

        #region Properties
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
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            OnGameUpdateReceived += OnGameUpdate;
            OnControlUpdateReceived += OnControlUpdate;
            OnMessageReceived += OnMessage;
            _networkManager = new NetworkManager(SocketType.Dealer);
            _networkManager.OnDataReceived += OnDataReceived;
            InitGame();
        }
       
        private void MainWindow_OnKeyDown(object sender, KeyboardEventArgs e)
        {
            if (!_networkMode)
            {
                if (Keyboard.IsKeyDown(Key.A) && _player1.Y - _padSpeed >= 0) _player1.Y -= _padSpeed;
                if (Keyboard.IsKeyDown(Key.Z) && _player1.Y + _padSpeed <= MainCanvas.ActualHeight - _player1.PadLength)
                    _player1.Y += _padSpeed;
            }
            if (Keyboard.IsKeyDown(Key.Up) && _player2.Y - _padSpeed >= 0) _player2.Y -= _padSpeed;
            if (Keyboard.IsKeyDown(Key.Down) && _player2.Y + _padSpeed <= (MainCanvas.ActualHeight - _player2.PadLength))
                _player2.Y += _padSpeed;
        }

        private void UpdatePlayer1Pad(ControlInput controlInput)
        {
            if (controlInput == ControlInput.Up && _player1.Y - _padSpeed >= 0) _player1.Y -= 10;
            if (controlInput == ControlInput.Down && _player1.Y + _padSpeed <= MainCanvas.ActualHeight - _player1.PadLength)
                _player1.Y += 10;
        }

        private void InitGame()
        {
            _speed = 3;
            _padSpeed = 70;
            _ball =new Ball { X = 180, Y = 110, MovingRight = true };
            _player1 = new Pad { Y = 90};
            _player2 = new Pad { Y = 90 };
            _peered = false;
            _beSlave = false;

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
           
            if(_networkMode)
                BeginConnectingToNetwork();
        }

        private void OnGameUpdate(GameUpdate gameUpdate)
        {
            if (!_networkMode)
            {
                BeSlave();
            }
        }

        private void OnControlUpdate(ControlInput controlInput)
        {
            UpdatePlayer1Pad(controlInput);
        }

        private void OnMessage(Message message)
        {
            if (message.MessageType.ToLower() == "game over")
            {
                MessageBox.Show("Game over!");
            }
            else if (message.MessageType.ToLower() == "connecttogame")
            {
                _networkManagerPeer = new NetworkManager(SocketType.Request);
                _networkManagerPeer.OnDataReceived -= OnDataReceived;
                _networkManagerPeer.OnDataReceived += OnDataReceived;
                while (!_networkManagerPeer.IsConnected)
                    _networkManagerPeer.Connect("tcp://" + message.MessageText);
                
                _networkManager.Dispose();
                _peered = true;

                // Send start game command
                _networkManagerPeer.Send(new Message("startgame", string.Empty),true);
                _beSlave = true;
            }
            else if (message.MessageType.ToLower() == "starthostingnetworkgame")
            {
                _networkManagerPeer = new NetworkManager(SocketType.Reply);
                _networkManagerPeer.OnDataReceived -= OnDataReceived;
                _networkManagerPeer.OnDataReceived += OnDataReceived;
                while (!_networkManagerPeer.IsConnected)
                    _networkManagerPeer.Connect("tcp://" + MyAddress);
                _peered = true;
                _beSlave = false;
                
                _networkManager.Dispose();
                _networkManagerPeer.Receieve();
            }
        }

        private void BeginConnectingToNetwork()
        {
            while (!_networkManager.IsConnected)
            {
                _networkManager.Connect();
            }

            if (!_beSlave)
                _networkManager.Send(new Message("rqnetworkgame", MyAddress), true);
        }

        private void OnDataReceived(string data)
        {
            if(string.IsNullOrEmpty(data)) return;

            if (data.ToLower().StartsWith("gameupdate"))
            {
                var s = data.Split(':');
                if (s.Length == 9)
                {
                    if (OnGameUpdateReceived != null)
                    {
                        OnGameUpdateReceived(new GameUpdate()
                        {
                            HorizontalPosition = Get<int>(s[1]),
                            VerticalPosition = Get<int>(s[2]),
                            Player1PadPosition = Get<int>(s[3]),
                            Player2PadPosition = Get<int>(s[4]),
                            PadHeight = Get<int>(s[5]),
                            Player1Score = Get<int>(s[6]),
                            Player2Score = Get<int>(s[7])
                        });
                    }

                }
            }
            if (data.ToLower().StartsWith("control"))
            {
                var s = data.Split(':');
                if (s.Length == 3)
                {
                    if (OnControlUpdateReceived != null)
                    {
                        OnControlUpdateReceived((ControlInput) Get<int>(s[1]));
                    }
                }
            }
            if (data.ToLower().StartsWith("game over"))
            {
                if (OnMessageReceived != null)
                    OnMessageReceived(new Message() { MessageType = "game over"});
                
            }
            if (data.ToLower().StartsWith("connecttogame"))
            {
                var s = data.Split(':');
                if (s.Length == 4)
                {
                    if (OnMessageReceived != null)
                        OnMessageReceived(new Message() {MessageType = "connecttogame", MessageText = s[1] + ":" + s[2]});
                }
            }
            if (data.ToLower().StartsWith("starthostingnetworkgame"))
            {
                if (OnMessageReceived != null)
                    OnMessageReceived(new Message() { MessageType = "starthostingnetworkgame" });
            }
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

            if ((!_beSlave) && _peered)
                SendGameUpdate((int) _ball.X, (int) _ball.Y, _player1.Y, _player2.Y, _player1.PadLength, _lpoints,
                    _rpoints);
        }

        private void Reset()
        {
            _ball.Y = 210;
            _ball.X = 380;
            _levelBreak = false;

            if ((!_beSlave) && _peered)
                SendGameUpdate((int)_ball.X, (int)_ball.Y, _player1.Y, _player2.Y, _player1.PadLength, _lpoints,
                    _rpoints);
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private T Get<T>(string value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            if(_networkManager != null)
                _networkManager.Dispose();
            if (_networkManagerPeer != null)
                _networkManagerPeer.Dispose();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _networkMode = true;
            InitGame();
        }

        private void SendGameUpdate(int ballX, int ballY, int player1Paddle, int player2Paddle,
            int paddleHeight, int player1Score, int player2Score)
        {
            _networkManagerPeer.Send(new GameUpdate("gameupdate",string.Empty)
            {
                HorizontalPosition = (ballX * 1000) /800,
                VerticalPosition = (ballY * 1000)/475,
                Player1PadPosition = (player1Paddle * 1000) / 475,
                Player2PadPosition = (player2Paddle * 1000) / 475,
                PadHeight = (paddleHeight * 1000) / 475,
                Player1Score = player1Score,
                Player2Score = player2Score
            },true);
        }
        private  void BeSlave()
        {
            _ball.X = Convert.ToDouble((ballX * 800) / 1000);
            _ball.Y = Convert.ToDouble((ballY * 475) / 1000);
            _player1.Y = player1Possition;
            _player2.Y = player2Possition;
            Player1Score = _lpoints;
            Player2Score = _rpoints;
        }
    }
}
