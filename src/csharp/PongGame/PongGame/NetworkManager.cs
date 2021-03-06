﻿using System;
using System.Text;
using NetMQ;
using NetMQ.zmq;

namespace PongGame
{
    public enum SocketType
    {
        Dealer,
        Reply,
        Request
    }

    public interface INetworkManager : IDisposable
    {
        Action<string> OnDataReceived { get; set; }
        bool IsConnected { get; }
        void Send(Message message, bool needResponse = false);
        void Send(GameUpdate message, bool needResponse = false);
        string Receieve();
        void Connect(string address);
        void Connect();
    }

    public class NetworkManager : INetworkManager
    {
        private NetMQSocket _clientSocket;
        private bool _isConnected;
        private readonly SocketType _socketType;
        private NetMQContext context;

        public Action<string> OnDataReceived { get; set; }

        public bool IsConnected
        {
            get { return _isConnected; }
        }

        public NetworkManager(SocketType socketType)
        {
            _socketType = socketType;
            context = NetMQContext.Create();
        }

        public void Send(Message message, bool needResponse = false)
        {
            if (!_isConnected)
                Connect();

            Console.WriteLine("Sending request {0}...", message);
            if (_socketType == SocketType.Dealer)
                _clientSocket.SendMore(Guid.NewGuid().ToString());

            _clientSocket.SendMore(message.MessageType)
                .Send(message.MessageText);

            if (needResponse) Receieve();
        }

        public void Send(GameUpdate message, bool needResponse = false)
        {
            if (!_isConnected)
                Connect();
                
            _clientSocket.SendMore(message.MessageType)
               .SendMore(message.HorizontalPosition.ToString())
               .SendMore(message.VerticalPosition.ToString())
               .SendMore(message.Player1PadPosition.ToString())
               .SendMore(message.Player2PadPosition.ToString())
               .SendMore(message.PadHeight.ToString())
               .SendMore(message.Player1Score.ToString())
               .Send(message.Player2Score.ToString());

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
                builder.Append(string.Format("{0}:", Encoding.UTF8.GetString(rec)));
            }

            var replyString = builder.ToString();
            Console.WriteLine("Received reply : {0}", replyString);

            if (OnDataReceived != null)
                OnDataReceived(replyString);

            return replyString;
        }

        public void Connect(string address)
        {
            if (_socketType == SocketType.Dealer)
            {
                _clientSocket = context.CreateDealerSocket();
                _clientSocket.Connect(address);
            }
            if (_socketType == SocketType.Reply)
            {
                _clientSocket = context.CreateResponseSocket();
                _clientSocket.Bind(address);
            }
            if (_socketType == SocketType.Request)
            {
                _clientSocket = context.CreateRequestSocket();
                _clientSocket.Connect(address);
            }

            _isConnected = true;
        }

        public void Connect()
        {
           Connect("tcp://127.0.0.1:5555");
        }

        public void Dispose()
        {
            if (_clientSocket != null)
                _clientSocket.Dispose();
            if (context != null)
                context.Dispose();
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
