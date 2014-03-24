module Pong
open System
open PongTypes
open PongRenderer
// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
[<Literal>]
let speedConstant = 0.3

let movePaddleUp previousPaddlePosition =
    match previousPaddlePosition with
    | p when p <= 0.0 -> previousPaddlePosition
    | _ -> previousPaddlePosition - 1.0

let movePaddleDown previousPaddlePosition =
    match previousPaddlePosition with
    | p when p >= 22.0 -> previousPaddlePosition
    | _ -> previousPaddlePosition + 1.0

let playerOnePaddleUp state = 
    {state with PlayerOnePaddlePosition = movePaddleUp state.PlayerOnePaddlePosition; PreviousPlayerOnePaddlePosition = state.PlayerOnePaddlePosition}

let playerOnePaddleDown state =
    {state with PlayerOnePaddlePosition = movePaddleDown state.PlayerOnePaddlePosition; PreviousPlayerOnePaddlePosition = state.PlayerOnePaddlePosition}

let playerTwoPaddleUp state = 
    {state with PlayerTwoPaddlePosition = movePaddleUp state.PlayerTwoPaddlePosition; PreviousPlayerTwoPaddlePosition = state.PlayerTwoPaddlePosition}

let playerTwoPaddleDown state =
    {state with PlayerTwoPaddlePosition = movePaddleDown state.PlayerTwoPaddlePosition; PreviousPlayerTwoPaddlePosition = state.PlayerTwoPaddlePosition}
  
let processInput state=
    if Console.KeyAvailable = false then 
        state 
    else 
        match Console.ReadKey(true).KeyChar with
        | '3' -> {state with Status = Exit}
        | '2' -> {state with Status = MoveTo(RunningTwoPlayer)}
        | '1' -> {state with Status = MoveTo(RunningSinglePlayer)}
        | 'a' -> playerOnePaddleUp state
        | 'z' -> playerOnePaddleDown state
        | 'k' -> playerTwoPaddleUp state
        | 'm' -> playerTwoPaddleDown state
        | _ -> state

let getNextBallPosition ballPosition direction = 
    {ballPosition with left = ballPosition.left + direction.horizontalSpeed; top = ballPosition.top + direction.verticalSpeed}

let moveBall state =
    {state with BallPosition = getNextBallPosition state.BallPosition state.BallDirection; PreviousBallPosition = state.BallPosition}

let (|BouncesFromTop|BouncesFromBottom|NoBounce|) position =
    match position with
    | p when p <= 0.0 -> BouncesFromTop
    | p when p >= 25.0 -> BouncesFromBottom
    | _ -> NoBounce

let getBouncedPositionAndDirectionFromTop position direction =
    ({position with top = speedConstant}, {direction with verticalSpeed = speedConstant})

let getBouncedPositionAndDirectionFromBottom position direction =
    ({position with top = 24.0}, {direction with verticalSpeed = -speedConstant})

let bounceBall state =
    match state.BallPosition.top with
    | BouncesFromTop -> let (bouncedPosition, bouncedDirection) = getBouncedPositionAndDirectionFromTop state.BallPosition state.BallDirection
                        {state with BallPosition = bouncedPosition; BallDirection = bouncedDirection; PreviousBallPosition = state.BallPosition}
    | BouncesFromBottom -> let (bouncedPosition, bouncedDirection) = getBouncedPositionAndDirectionFromBottom state.BallPosition state.BallDirection
                           {state with BallPosition = bouncedPosition; BallDirection = bouncedDirection; PreviousBallPosition = state.BallPosition} 
    | NoBounce -> state

let resetBall position direction state = 
    {state with BallPosition = position; BallDirection = direction; PreviousBallPosition = state.BallPosition}

let scorePlayerOne state =
    {state with PlayerOneScore = state.PlayerOneScore + 1}
    |> resetBall {BallPosition.left = 40.0; top = 12.0} {Direction.horizontalSpeed = -speedConstant; verticalSpeed = -speedConstant}

let scorePlayerTwo state =
    {state with PlayerTwoScore = state.PlayerTwoScore + 1}
    |> resetBall {BallPosition.left = 40.0; top = 12.0} {Direction.horizontalSpeed = speedConstant; verticalSpeed = speedConstant}

let detectScore state =
    match state.BallPosition.left with
    | p when p < 0.0 -> state |> scorePlayerTwo
    | p when p > 79.0 -> state |> scorePlayerOne
    | _  -> state

let paddleIntersect paddlePosition ballPosition =
    match ballPosition.top with
    | p when p >= paddlePosition && p <= paddlePosition + 2.0  -> true
    | _ -> false

let (|HitPlayerOnePaddle|HitPlayerTwoPaddle|NoHit|) state = 
    match state with
    | s when s.BallPosition.left <= 2.0 && paddleIntersect s.PlayerOnePaddlePosition s.BallPosition -> HitPlayerOnePaddle
    | s when s.BallPosition.left >= 77.0 && paddleIntersect s.PlayerTwoPaddlePosition s.BallPosition -> HitPlayerTwoPaddle
    | _ -> NoHit

let reverseBallDirection direction =
    {direction with horizontalSpeed = -direction.horizontalSpeed; verticalSpeed = - direction.verticalSpeed}
  
let bounceOfPlayerOnePaddle state =
    state |> resetBall {state.BallPosition with left = 2.0;} (reverseBallDirection state.BallDirection)

let bounceOfPlayerTwoPaddle state =
    state |> resetBall {state.BallPosition with left = 77.0;} (reverseBallDirection state.BallDirection)

let bounceOfPaddle state =
    match state with
    | HitPlayerOnePaddle -> bounceOfPlayerOnePaddle state
    | HitPlayerTwoPaddle -> bounceOfPlayerTwoPaddle state
    | NoHit              -> state
    
let winPlayer state =
    match state with
    | s when s.PlayerOneScore = 10 -> {state with Status= PlayerOneWon}
    | s when s.PlayerTwoScore = 10 -> {state with Status= PlayerTwoWon}
    |_ -> state



let computeBounce ballPosition balldirection =
    match ballPosition.top with
    | BouncesFromTop -> getBouncedPositionAndDirectionFromTop ballPosition balldirection
    | BouncesFromBottom -> getBouncedPositionAndDirectionFromBottom ballPosition balldirection
    | NoBounce -> (ballPosition, balldirection)

let rec calculateIdealPaddlePositon ballPosition ballDirection =
    let (nextBallPosition, nextBallDirection) = computeBounce (getNextBallPosition ballPosition ballDirection) ballDirection
    
    match nextBallPosition.left with
    | p when p >= 0.0 -> calculateIdealPaddlePositon nextBallPosition nextBallDirection
    | _ -> nextBallPosition.top

let moveComputerPaddle state idealPosition =
    match state.PlayerOnePaddlePosition with
    | p when idealPosition < (p) -> playerOnePaddleUp state
    | p when idealPosition > (p + 2.0) -> playerOnePaddleDown state
    | _ -> state

let computerMove state =
    if state.BallDirection.horizontalSpeed >= 0.0 then 
        state
    else
        moveComputerPaddle state (calculateIdealPaddlePositon state.BallPosition state.BallDirection)

let resetState status =
    {
      GameState.Status = status
      PreviousBallPosition = {left = 40.0; top = 12.0} 
      BallPosition = {left = 40.0; top = 12.0} 
      PlayerOnePaddlePosition = 6.0
      PlayerTwoPaddlePosition = 6.0
      PreviousPlayerOnePaddlePosition = 6.0
      PreviousPlayerTwoPaddlePosition = 6.0
      BallDirection = {horizontalSpeed = -speedConstant; verticalSpeed = -speedConstant;};
      PlayerOneScore = 0
      PlayerTwoScore = 0
    }

let rec gameloop state = 
    Threading.Thread.Sleep (16)
    match state.Status with
    | Exit -> ()
    | MoveTo(RunningTwoPlayer) -> gameloop (resetState RunningTwoPlayer)
    | MoveTo(RunningSinglePlayer) -> gameloop (resetState RunningSinglePlayer)
    | RunningTwoPlayer -> state |> processInput |> moveBall |> bounceBall |> bounceOfPaddle |> detectScore|>winPlayer |> render |> gameloop
    | RunningSinglePlayer -> state |> processInput |> computerMove |> moveBall |> bounceBall |> bounceOfPaddle |> detectScore|>winPlayer |> render |> gameloop
    | _ -> processInput state |> render |> gameloop

[<EntryPoint>]
let main argv =
    Console.CursorVisible = false |> ignore
    let initialState = resetState InitScreen
    gameloop initialState
    0 // return an integer exit code
