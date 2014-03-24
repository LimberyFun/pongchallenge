module Pong
open System
open PongTypes
open PongEngine
open PongAi
open PongRenderer

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

let rec gameloop state = 
    Threading.Thread.Sleep (16)
    match state.Status with
    | Exit -> ()
    | MoveTo(RunningTwoPlayer) -> gameloop (resetState RunningTwoPlayer)
    | MoveTo(RunningSinglePlayer) -> gameloop (resetState RunningSinglePlayer)
    | RunningTwoPlayer -> state |> processPlayerOneInput |> processPlayerTwoInput |> moveBall |> bounceBall |> bounceOfPaddle |> detectScore|>winPlayer |> render |> gameloop
    | RunningSinglePlayer -> state |> processPlayerTwoInput |> computerMove |> moveBall |> bounceBall |> bounceOfPaddle |> detectScore|>winPlayer |> render |> gameloop
    | _ -> processBlockingInput state |> render |> gameloop

[<EntryPoint>]
[<STAThreadAttribute>]
let main argv =
    Console.CursorVisible = false |> ignore
    let initialState = resetState InitScreen
    gameloop initialState
    0 // return an integer exit code
