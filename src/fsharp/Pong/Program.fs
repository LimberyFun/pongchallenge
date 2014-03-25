module Pong
open System
open PongTypes
open PongEngine
open PongAi
open PongRenderer
open PongNetworking
open NetMQ

let resetState status =
    {
      GameState.Status = status
      PreviousBallPosition = {left = 40.0; top = 12.0} 
      BallPosition = {left = 40.0; top = 12.0} 
      PlayerOnePaddlePosition = 6.0
      PlayerTwoPaddlePosition = 6.0
      PreviousPlayerOnePaddlePosition = 6.0
      PreviousPlayerTwoPaddlePosition = 6.0
      BallDirection = {horizontalSpeed = -horizontalSpeedConstant; verticalSpeed = (randomVerticalSpeed ());};
      PlayerOneScore = 0
      PlayerTwoScore = 0
    }

let rec gameloop (context:NetMQContext) (socket:Option<NetMQSocket>) state = 
    Threading.Thread.Sleep (16)
    match state.Status with
    | Exit -> ()
    | MoveTo(RunningTwoPlayer) -> gameloop context None (resetState RunningTwoPlayer) 
    | MoveTo(RunningSinglePlayer) -> gameloop context None (resetState RunningSinglePlayer) 
    | MoveTo(PlayerOneWon) -> gameloop context None {state with Status = PlayerOneWon} 
    | MoveTo(PlayerTwoWon) -> gameloop context None {state with Status = PlayerTwoWon} 
    | MoveTo(WaitingForPartner) -> gameloop context (Some(context.CreateDealerSocket ())) {state with Status = WaitingForPartner} 
    | RunningTwoPlayer -> state |> processPlayerOneInput |> processPlayerTwoInput |> moveBall |> bounceBall |> bounceOfPaddle |> detectScore|>winPlayer |> render |> gameloop context None
    | RunningSinglePlayer -> state |> processPlayerTwoInput |> computerMove |> moveBall |> bounceBall |> bounceOfPaddle |> detectScore|>winPlayer |> render |> gameloop context None
    | WaitingForPartner -> socket |> getSocket |> startPongNetworking
    | _ -> processBlockingInput state |> render |> gameloop context None

[<EntryPoint>]
[<STAThreadAttribute>]
let main argv =
    Console.CursorVisible = false |> ignore
    let initialState = resetState InitScreen
    use context = NetMQContext.Create ()
    gameloop context None initialState 
    0 // return an integer exit code
