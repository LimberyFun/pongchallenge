module PongTypes


type GameStatus =
    | InitScreen
    | RunningSinglePlayer
    | RunningTwoPlayer
    | RunningNetworkPlayerAsHost
    | RunningNetworkPlayerAsClient
    | WaitingForPartner
    | PlayerOneWon
    | PlayerTwoWon
    | MoveTo of GameStatus
    | Exit

type PaddlePosition = double
type Score = int

type BallPosition ={left : double;top : double}
  
type Direction = {horizontalSpeed : double; verticalSpeed : double}
type GameState = 
    {
      Status : GameStatus
      BallPosition : BallPosition     
      PreviousBallPosition : BallPosition
      BallDirection : Direction
      PlayerOnePaddlePosition : PaddlePosition
      PlayerTwoPaddlePosition : PaddlePosition
      PreviousPlayerOnePaddlePosition : PaddlePosition
      PreviousPlayerTwoPaddlePosition : PaddlePosition
      PlayerOneScore : Score
      PlayerTwoScore : Score
    }

