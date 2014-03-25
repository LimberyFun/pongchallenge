﻿module Pong
open System
open PongTypes
open PongEngine
open PongAi
open PongRenderer
open PongNetworking
open PongServer
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
      NetworkPort = ""
      OwnIpAddress = ""
      ServerAddressAndPort = ""
      NetworkGameHostAddressAndPort = ""
      PaddleHeight = 3.0
    }

let rec gameloop (context:NetMQContext) (socket:Option<NetMQSocket>) state = 
    if (state.Status = RunningNetworkPlayerAsClient) = false then Threading.Thread.Sleep (16)
    match state.Status with
    | Exit -> ()
    | MoveTo(RunningTwoPlayer) -> gameloop context None (resetState RunningTwoPlayer) 
    | MoveTo(RunningSinglePlayer) -> gameloop context None (resetState RunningSinglePlayer) 
    | MoveTo(PlayerOneWon) -> gameloop context None {state with Status = PlayerOneWon} 
    | MoveTo(PlayerTwoWon) -> gameloop context None {state with Status = PlayerTwoWon} 
    | MoveTo(GetOwnPortAndIp next) -> gameloop context None {state with Status = GetOwnPortAndIp(next)}
    | MoveTo(GetServerAddressAndPort) -> gameloop context None {state with Status = GetServerAddressAndPort}
    | MoveTo(RunningNetworkPlayerAsHost) -> let newSocket = context.CreateResponseSocket () |> bindServer ("tcp://*:" + state.NetworkPort)
                                            newSocket |> waitForStartGame (resetState state.Status) |> gameloop context (Some(newSocket))
    | MoveTo(RunningNetworkPlayerAsClient) -> let newSocket = context.CreateRequestSocket () |> connectToServer ("tcp://" + state.NetworkGameHostAddressAndPort)
                                              newSocket |> sendStartGame (resetState state.Status) |> gameloop context (Some(newSocket))   
    | MoveTo(NetworkGameOver winner) -> gameloop context None {state with Status = NetworkGameOver(winner)}                                         
    | GetOwnPortAndIp next -> {(state |> render |> setOwnIpAddress |> setOwnPort) with Status = MoveTo(next)} |> gameloop context None
    | MoveTo(WaitingForPartner) -> gameloop context (Some(context.CreateDealerSocket ())) {state with Status = WaitingForPartner} 
    | MoveTo(BePongServer) -> gameloop context (Some(context.CreateRouterSocket ())) {state with Status = BePongServer}
    | RunningTwoPlayer -> state |> processPlayerOneInput |> processPlayerTwoInput |> moveBall |> bounceBall |> bounceOfPaddle |> detectScore|>winPlayer |> render |> gameloop context None
    | RunningSinglePlayer -> state |> processPlayerTwoInput |> computerMove |> moveBall |> bounceBall |> bounceOfPaddle |> detectScore|>winPlayer |> render |> gameloop context None
    | BePongServer -> state  |> render |> ignore
                      socket |> getSocket |> bindServer ("tcp://*:" + state.NetworkPort) |> runServer List.empty<receivedMessage>
    | GetServerAddressAndPort -> {(state |> render |> setServerAddressAndPort ) with Status = MoveTo(GetOwnPortAndIp(WaitingForPartner))} |> render |> gameloop context None
    | WaitingForPartner ->  state |> render |> ignore
                            socket |> getSocket |> startPongNetworkingGame state |> render |> gameloop context None
    | RunningNetworkPlayerAsHost -> state 
                                    |> sendGameStateMessage (getSocket socket) 
                                    |> moveRemotePlayer (getSocket socket) 
                                    |> processPlayerTwoInput 
                                    |> moveBall
                                    |> bounceBall
                                    |> bounceOfPaddle
                                    |> detectScore
                                    |> winPlayer
                                    |> sendEndOfGameMessage (getSocket socket)
                                    |> render |> gameloop context socket
    | RunningNetworkPlayerAsClient -> state |> receiveGameUpdateMessage (getSocket socket) |> render |> sendControlMessage (getSocket socket) |> gameloop context socket
    | _ -> processBlockingInput state |> render |> gameloop context None


[<EntryPoint>]
[<STAThreadAttribute>]
let main argv =
    Console.CursorVisible = false |> ignore
    let initialState = resetState InitScreen
    use context = NetMQContext.Create ()
    gameloop context None initialState 
    0 // return an integer exit code
