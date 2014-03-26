using System;
using System.Text;
using NetMQ;
using NetMQ.zmq;

namespace PongGame
{
    public interface INetworkManager : IDisposable
    {
        Action<string> OnDataReceived { get; set; }
        bool IsConnected { get; }
        void Send(Message message, bool needResponse = false);
        string Receieve();
        void Connect();
    }

    public class NetworkManager : INetworkManager, IDisposable
    {
        private NetMQSocket _clientSocket;
        private bool _isConnected;

        public Action<string> OnDataReceived { get; set; }

        public bool IsConnected
        {
            get { return _isConnected; }
        }

        public void Send(Message message, bool needResponse = false)
        {
            if (!_isConnected)
                Connect();

            Console.WriteLine("Sending request {0}...", message);
            _clientSocket.Send(message.MessageType, sendMore: true);
            _clientSocket.Send(message.MessageText);

            if (needResponse) Receieve();
        }

        public string Receieve()
        {
            if (!_isConnected)
                Connect();

            var builder = new StringBuilder();
            var end = true;
            while (end)
            {
                var rec = _clientSocket.Receive(out end);
                builder.Append(string.Format("{0}:", Encoding.Unicode.GetString(rec)));
            }

            var replyString = builder.ToString();
            Console.WriteLine("Received reply : {0}", replyString);

            if (OnDataReceived != null)
                OnDataReceived(replyString);

            return replyString;
        }

        public void Connect()
        {
            using (var context = NetMQContext.Create())
                _clientSocket = context.CreateSocket(ZmqSocketType.Rep);
           
            _clientSocket.Connect("tcp://localhost:5555");
            _isConnected = true;
        }


        public void Dispose()
        {
            if(_clientSocket!= null)
                _clientSocket.Dispose();
        }
    }

    /*
     * fr1: string indicating messagetype (all lower case)

RequestNetworkGame
rqnetworkgame
iport

StartHostingNetworkGame
string startHostingNetworkgame

ConnectToNetworkGame
connectToGame
iport to connect to
StartGame
startGame

ServerToClientUpdates
gameupdate
ball horizontalposition x/1000
ball veticalposition x/1000
playerone paddle position x/1000
playertwo paddle positoin x/1000
paddleheight
playeronescore
playertwoscore

ControlIinput
control
0 = noinput
1= up
2=down

GameOver
game over
String Who won
     */
}
