module PongMessage

type GameStateUpdate =
    {
        BallHorizontalPosition : int
        BallVerticalPosition : int
        PlayerOnePaddlePosition : int
        PlayerTwoPaddlePosition : int
        PaddleHeight : int
        PlayerOneScore :int
        PlayerTwoScore : int
    }

type Message =
    | RequestNetworkGame of string
    | StartHostingNeworkGame
    | ConnectToNetworkGame of string
    | StartGame
    | ServerGameUpdate of GameStateUpdate
    | ControlInput of int
    | GameOver of string

let getMessageType message =
    match message with
    | RequestNetworkGame _ -> "rqnetworkgame"
    | StartHostingNeworkGame -> "startHostingNetworkgame"
    | ConnectToNetworkGame _ -> "connectToGame"
    | StartGame -> "startGame"
    | ServerGameUpdate _ -> "gameupdate"
    | ControlInput _ -> "control"
    | GameOver _ -> "game over"