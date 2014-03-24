module PongRenderer
open System
open PongTypes

let renderChar left top (char:string) =
    Console.SetCursorPosition(left,top)
    Console.Write(char)

let renderPaddle left top char =
    [0..2] |> List.map (fun e -> e + top) |> List.iter (fun e -> renderChar left e char)

let writePaddle left top =
    renderPaddle left top "█"

let deletePaddle left top =
    renderPaddle left top " "

let renderPlayerOnePaddle state =
    deletePaddle 1 (System.Convert.ToInt32(Math.Round(state.PreviousPlayerOnePaddlePosition)))
    writePaddle 1 (System.Convert.ToInt32(Math.Round(state.PlayerOnePaddlePosition)))

let renderPlayerTwoPaddle state =
    deletePaddle 78 (System.Convert.ToInt32(Math.Round(state.PreviousPlayerTwoPaddlePosition)))
    writePaddle 78 (System.Convert.ToInt32(Math.Round(state.PlayerTwoPaddlePosition)))

let decideScoreColor myScore otherPlayerScore defaultColour = 
    match (myScore, otherPlayerScore) with
    | (m,o) when m < o -> ConsoleColor.Red
    | (m,o) when m > 0 -> ConsoleColor.Green
    | (_,_) -> defaultColour

let renderPlayerOneScore state =
    let colour = Console.ForegroundColor
    Console.ForegroundColor <- decideScoreColor state.PlayerOneScore state.PlayerTwoScore colour
    renderChar 10 2 (state.PlayerOneScore.ToString ())
    Console.ForegroundColor <- colour

let renderPlayerTwoScore state  =
    let colour = Console.ForegroundColor
    Console.ForegroundColor <- decideScoreColor state.PlayerTwoScore state.PlayerOneScore colour
    renderChar 60 2 (state.PlayerTwoScore.ToString ())
    Console.ForegroundColor <- colour
   
let renderDivider () =
    let colour = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Blue
    [0..25] |> List.filter (fun e -> (e % 2  = 0)) |> List.iter (fun e -> renderChar 39 e "|")
    Console.ForegroundColor <- colour

let renderBall state =
    let colour = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Yellow
    renderChar (System.Convert.ToInt32(Math.Round(state.PreviousBallPosition.left))) (System.Convert.ToInt32(Math.Round(state.PreviousBallPosition.top))) " "
    renderChar (System.Convert.ToInt32(Math.Round(state.BallPosition.left))) (System.Convert.ToInt32(Math.Round(state.BallPosition.top))) "@"
    Console.ForegroundColor <- colour

let render state =
    match state.Status with 
    | InitScreen -> Console.SetCursorPosition(0,0)
                    Console.Write("Pong game")
                    Console.SetCursorPosition(0,2)
                    Console.Write("1: Single Player")
                    Console.SetCursorPosition(0,3)
                    Console.Write("2: Two Players")
                    Console.SetCursorPosition(0,4)
                    Console.Write("3: Exit")
                    state
    | RunningTwoPlayer -> renderPlayerOneScore state
                          renderPlayerTwoScore state
                          renderDivider ()    
                          renderBall state        
                          renderPlayerOnePaddle state
                          renderPlayerTwoPaddle state                                    
                          state
    | RunningSinglePlayer -> renderPlayerOneScore state
                             renderPlayerTwoScore state
                             renderDivider ()    
                             renderBall state        
                             renderPlayerOnePaddle state
                             renderPlayerTwoPaddle state                                    
                             state
    | PlayerOneWon -> Console.SetCursorPosition(0,0)
                      Console.Write("Player One won")
                      Console.SetCursorPosition(0,2)
                      Console.Write("1: Single Player")
                      Console.SetCursorPosition(0,3)
                      Console.Write("2: Two Players")
                      Console.SetCursorPosition(0,4)
                      Console.Write("3: Exit")
                      state
    | PlayerTwoWon -> Console.SetCursorPosition(0,0)
                      Console.Write("Player Two won")
                      Console.SetCursorPosition(0,2)
                      Console.Write("1: Single Player")
                      Console.SetCursorPosition(0,3)
                      Console.Write("2: Two Players")
                      Console.SetCursorPosition(0,4)
                      Console.Write("3: Exit")
                      state
    | MoveTo(RunningTwoPlayer) -> Console.Clear ()
                                  state
    | MoveTo(RunningSinglePlayer) -> Console.Clear ()
                                     state
    | MoveTo(PlayerOneWon) -> Console.Clear ()
                              state
    | MoveTo(PlayerTwoWon) -> Console.Clear ()
                              state

    | _ -> state