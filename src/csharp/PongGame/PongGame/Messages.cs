
namespace PongGame
{
    public enum ControlInput
    {
        NoInput = 0,
        Up = 1,
        Down = 2
    }

    public class Message
    {
        public string MessageType { get; set; }
        public string MessageText { get; set; }

        public Message() { }
        public Message(string messageType, string messageText)
        {
            MessageType = messageType;
            MessageText = messageText;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", MessageType.ToLower(), MessageText);
        }
    }

    public class GameUpdate : Message
    {
        public int HorizontalPosition { get; set; }
        public int VerticalPosition { get; set; }
        public int Player1PadPosition { get; set; }
        public int Player2PadPosition { get; set; }
        public int PadHeight { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
    }
}
