module PongEngine
open System
open System.Windows.Input
open PongTypes

[<Literal>]
let horizontalSpeedConstant = 0.5
[<Literal>]
let verticalSpeedConstantMax = 5
[<Literal>]
let verticalSpeedConstantMin = -5

let randomizer = new Random()

let movePaddleUp previousPaddlePosition =
    match previousPaddlePosition with
    | p when p <= 0.0 -> previousPaddlePosition
    | _ -> previousPaddlePosition - 1.0

let movePaddleDown previousPaddlePosition paddleHeight =
    match previousPaddlePosition with
    | p when p >=(24.0 - paddleHeight) -> previousPaddlePosition
    | _ -> previousPaddlePosition + 1.0

let playerOnePaddleUp state = 
    {state with PlayerOnePaddlePosition = movePaddleUp state.PlayerOnePaddlePosition; PreviousPlayerOnePaddlePosition = state.PlayerOnePaddlePosition}

let playerOnePaddleDown state =
    {state with PlayerOnePaddlePosition = movePaddleDown state.PlayerOnePaddlePosition state.PaddleHeight; PreviousPlayerOnePaddlePosition = state.PlayerOnePaddlePosition}

let playerTwoPaddleUp state = 
    {state with PlayerTwoPaddlePosition = movePaddleUp state.PlayerTwoPaddlePosition; PreviousPlayerTwoPaddlePosition = state.PlayerTwoPaddlePosition}

let playerTwoPaddleDown state =
    {state with PlayerTwoPaddlePosition = movePaddleDown state.PlayerTwoPaddlePosition state.PaddleHeight; PreviousPlayerTwoPaddlePosition = state.PlayerTwoPaddlePosition}
  
let userInputPlayerOneUp state =
    if Keyboard.IsKeyDown(Key.A) then playerOnePaddleUp state else state

let userInputPlayerOneDown state =
    if Keyboard.IsKeyDown(Key.Z) then playerOnePaddleDown state else state

let userInputPlayerTwoUp state =
    if Keyboard.IsKeyDown(Key.K) then playerTwoPaddleUp state else state

let userInputPlayerTwoDown state =
    if Keyboard.IsKeyDown(Key.M) then playerTwoPaddleDown state else state

let userInputRestartTwoPlayer state =
    if Keyboard.IsKeyDown(Key.D2) then {state with Status = MoveTo(RunningTwoPlayer)} else state

let userInputRestartSinglePlayer state =
    if Keyboard.IsKeyDown(Key.D1) then {state with Status = MoveTo(RunningSinglePlayer)} else state

let userInputExit state =
    if Keyboard.IsKeyDown(Key.D3) then {state with Status = Exit} else state

let processPlayerOneInput state =
    state 
    |> userInputPlayerOneUp |> userInputPlayerOneDown
 
let processPlayerTwoInput state =
    state
    |> userInputPlayerTwoUp |> userInputPlayerTwoDown
    |> userInputRestartSinglePlayer |> userInputRestartTwoPlayer |> userInputExit

let processBlockingInput state =
    if (Console.KeyAvailable = false) then
        state
    else
        match Console.ReadKey().KeyChar with
        | '1' -> {state with Status = MoveTo(RunningSinglePlayer)}
        | '2' -> {state with Status = MoveTo(RunningTwoPlayer)}
        | '3' -> {state with Status = Exit}
        | '4' -> {state with Status = MoveTo(GetServerAddressAndPort)}
        | '5' -> {state with Status = MoveTo(GetOwnPortAndIp(BePongServer))}
        | _   -> state


let getNextBallPosition ballPosition direction = 
    {ballPosition with left = ballPosition.left + direction.horizontalSpeed; top = ballPosition.top + direction.verticalSpeed}

let moveBall state =
    {state with BallPosition = getNextBallPosition state.BallPosition state.BallDirection; PreviousBallPosition = state.BallPosition}

let (|BouncesFromTop|BouncesFromBottom|NoBounce|) position =
    match position with
    | p when p <= 0.0 -> BouncesFromTop
    | p when p > 24.0 -> BouncesFromBottom
    | _ -> NoBounce

let getBouncedPositionAndDirectionFromTop position direction =
    ({position with top = 1.0}, {direction with verticalSpeed = -direction.verticalSpeed})

let getBouncedPositionAndDirectionFromBottom position direction =
    ({position with top = 24.0}, {direction with verticalSpeed = -direction.verticalSpeed})

let bounceBall state =
    match state.BallPosition.top with
    | BouncesFromTop -> let (bouncedPosition, bouncedDirection) = getBouncedPositionAndDirectionFromTop state.BallPosition state.BallDirection
                        {state with BallPosition = bouncedPosition; BallDirection = bouncedDirection;}
    | BouncesFromBottom -> let (bouncedPosition, bouncedDirection) = getBouncedPositionAndDirectionFromBottom state.BallPosition state.BallDirection
                           {state with BallPosition = bouncedPosition; BallDirection = bouncedDirection;} 
    | NoBounce -> state

let resetBall position direction state = 
    {state with BallPosition = position; BallDirection = direction;}

let randomVerticalSpeed () = 
    Convert.ToDouble(randomizer.Next(verticalSpeedConstantMin, verticalSpeedConstantMax)) / 10.0 

let scorePlayerOne state =
    {state with PlayerOneScore = state.PlayerOneScore + 1}
    |> resetBall {BallPosition.left = 40.0; top = 12.0} {Direction.horizontalSpeed = -horizontalSpeedConstant; verticalSpeed = (randomVerticalSpeed ())}

let scorePlayerTwo state =
    {state with PlayerTwoScore = state.PlayerTwoScore + 1}
    |> resetBall {BallPosition.left = 40.0; top = 12.0} {Direction.horizontalSpeed = horizontalSpeedConstant; verticalSpeed = (randomVerticalSpeed ())}

let detectScore state =
    match state.BallPosition.left with
    | p when p < 0.0 -> state |> scorePlayerTwo
    | p when p > 79.0 -> state |> scorePlayerOne
    | _  -> state

let paddleIntersect paddlePosition ballPosition paddleHeight =
    match ballPosition.top with
    | p when p >= paddlePosition && p <= paddlePosition + paddleHeight  -> true
    | _ -> false

let (|HitPlayerOnePaddle|HitPlayerTwoPaddle|NoHit|) state = 
    match state with
    | s when s.BallPosition.left <= 2.0 && paddleIntersect s.PlayerOnePaddlePosition s.BallPosition s.PaddleHeight -> HitPlayerOnePaddle
    | s when s.BallPosition.left >= 77.0 && paddleIntersect s.PlayerTwoPaddlePosition s.BallPosition s.PaddleHeight -> HitPlayerTwoPaddle
    | _ -> NoHit

let reverseBallDirection direction =
    {direction with horizontalSpeed = -direction.horizontalSpeed}
  
let bounceOfPlayerOnePaddle state =
    {state with BallPosition = {state.BallPosition with left = 2.0}; BallDirection = (reverseBallDirection state.BallDirection);}

let bounceOfPlayerTwoPaddle state =
    {state with BallPosition = {state.BallPosition with left = 77.0}; BallDirection = (reverseBallDirection state.BallDirection);}

let bounceOfPaddle state =
    match state with
    | HitPlayerOnePaddle -> bounceOfPlayerOnePaddle state
    | HitPlayerTwoPaddle -> bounceOfPlayerTwoPaddle state
    | NoHit              -> state
    
let winPlayer state =
    match state with
    | s when s.PlayerOneScore = 10 -> {state with Status= MoveTo(PlayerOneWon)}
    | s when s.PlayerTwoScore = 10 -> {state with Status= MoveTo(PlayerTwoWon)}
    |_ -> state
