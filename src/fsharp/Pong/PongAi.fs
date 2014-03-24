module PongAi
open PongTypes
open PongEngine

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

