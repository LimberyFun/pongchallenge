module PongAi
open System
open PongTypes
open PongEngine

let decideErrorFactor state =
    match (state.PlayerOneScore - state.PlayerTwoScore) with
    | s when s >= 4 -> 30000
    | s when s >= 0 -> 20000
    | s when s <= 4 -> 10000
    | _ -> 5000

let computeBounce ballPosition balldirection =
    match ballPosition.top with
    | BouncesFromTop -> getBouncedPositionAndDirectionFromTop ballPosition balldirection
    | BouncesFromBottom -> getBouncedPositionAndDirectionFromBottom ballPosition balldirection
    | NoBounce -> (ballPosition, balldirection)

let randomizeSign value =
    match (randomizer.Next(-50,50)) with
    | v when v < 0 -> -value
    | _ -> value

let addSomeUncertainty errorFactor position  =
    position + (Convert.ToDouble(randomizer.Next(0, errorFactor)) / 1000.0 |> randomizeSign)

let rec calculateIdealPaddlePositon ballPosition ballDirection errorFactor =
    let (nextBallPosition, nextBallDirection) = computeBounce (getNextBallPosition ballPosition ballDirection) ballDirection

    match nextBallPosition.left with
    | p when p >= 0.0 -> calculateIdealPaddlePositon nextBallPosition nextBallDirection errorFactor
    | _ -> nextBallPosition.top |> addSomeUncertainty errorFactor

let moveComputerPaddle state idealPosition =
    let doSomething = randomizer.Next(-50,50)
    match (doSomething, state.PlayerOnePaddlePosition) with
    | (d,_) when d < 0 -> state
    | (_,p) when idealPosition < (p) -> playerOnePaddleUp state
    | (_,p) when idealPosition > (p + 2.0) -> playerOnePaddleDown state
    | (_, _) -> state

let computerMove state =
    if state.BallDirection.horizontalSpeed >= 0.0 then 
        state
    else
        moveComputerPaddle state (calculateIdealPaddlePositon state.BallPosition state.BallDirection (decideErrorFactor state))

