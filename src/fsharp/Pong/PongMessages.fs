module PongMessage
open System

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
    | StartHostingNetworkGame
    | ConnectToNetworkGame of string
    | StartGame
    | ServerGameUpdate of GameStateUpdate
    | ControlInput of int
    | GameOver of string

let getMessageType message =
    match message with
    | RequestNetworkGame _ -> "rqnetworkgame"
    | StartHostingNetworkGame -> "startHostingNetworkgame"
    | ConnectToNetworkGame _ -> "connectToGame"
    | StartGame -> "startGame"
    | ServerGameUpdate _ -> "gameupdate"
    | ControlInput _ -> "control"
    | GameOver _ -> "game over"

let converToRemoteValue (scale:int32) value =
    let reallyBigIntergerRepresentingLocalDoubleValue = Convert.ToInt64(value * 1000000.0)
    let equallyBigScale = Convert.ToInt64(scale) * 1000000L
    let product = (reallyBigIntergerRepresentingLocalDoubleValue * 1000L)
    let result = (product / equallyBigScale)
    result.ToString()

let convertToLocalValue scale (value:string) =
    let valueAsDouble = Convert.ToDouble(value)
    (valueAsDouble * scale) / 1000.0

let convertToRemoteVerticalValue = converToRemoteValue 25
let convertToRemoteHorizontalValue = converToRemoteValue 79
let convertToLocalVerticalValue = convertToLocalValue 25.0
let convertToLocalHorizontalValue = convertToLocalValue 79.0