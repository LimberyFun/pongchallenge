module PongNetworking
open NetMQ
open PongTypes
open PongMessage
open PongEngine
open System
open System.Net
open System.Windows.Input

let setOwnIpAddress state = 
    let ip = (Dns.GetHostEntry(Dns.GetHostName()).AddressList
    |> Array.find (fun e -> e.AddressFamily.ToString() = "InterNetwork")).ToString()
    {state with OwnIpAddress = ip}

let setOwnPort state =
    let port = Console.ReadLine ()
    Console.CursorVisible <- false
    {state with NetworkPort = port}

let setServerAddressAndPort state =
    let serverAddressAndPort = Console.ReadLine()
    Console.CursorVisible <- false
    {state with ServerAddressAndPort = serverAddressAndPort}
    
let encode = string >> System.Text.Encoding.UTF8.GetBytes
let decode = System.Text.Encoding.UTF8.GetString

let getSocket (socket:Option<NetMQSocket>) =
    match socket with 
    | None -> failwith "Socket missing"
    | Some s -> s

let bindServer address (socket:NetMQSocket)  =
    Console.WriteLine("Binding to " + address)
    socket.Bind(address)
    socket

let connectToServer address (socket:NetMQSocket) =
    socket.Connect(address)
    socket

let getMessageTypeFromMessage = Seq.head

let getHostAddressAndPortFromMessage = Seq.skip 1 >> Seq.head

let waitForStartGame state (socket:NetMQSocket) =
    let msg = socket.ReceiveStringMessages() |> Seq.toList
    if getMessageTypeFromMessage msg = "startgame" then 
        {state with Status = RunningNetworkPlayerAsHost}
    else
        failwith "Invalid message"

let sendStartGame state (socket:NetMQSocket) =
    socket.Send("startgame")
    {state with Status = RunningNetworkPlayerAsClient}



let sendGameStateMessage (socket:NetMQSocket) state  =
    socket.SendMore("gameupdate")
          .SendMore(convertToRemoteHorizontalValue state.BallPosition.left)
          .SendMore(convertToRemoteVerticalValue state.BallPosition.top)
          .SendMore(convertToRemoteVerticalValue state.PlayerOnePaddlePosition)
          .SendMore(convertToRemoteVerticalValue state.PlayerTwoPaddlePosition)
          .SendMore(convertToRemoteVerticalValue state.PaddleHeight)
          .SendMore(state.PlayerOneScore.ToString())
          .Send(state.PlayerTwoScore.ToString())
    state

let getMessagePart partNumber (message:string seq) =
    message |> Seq.skip partNumber |> Seq.head

let getBallPositiionLeft messages = getMessagePart 1 messages |> convertToLocalHorizontalValue
let getBallPositionTop messages = getMessagePart 2 messages |> convertToLocalVerticalValue
let getBallPosition message =
    {BallPosition.left = getBallPositiionLeft message; top = getBallPositionTop message}

let getPlayerOnePaddlePosition messages = getMessagePart 3 messages |> convertToLocalVerticalValue
let getPlayerTwoPaddlePosition messages = getMessagePart 4 messages |> convertToLocalVerticalValue
let getPaddleHeight messages = getMessagePart 5 messages |> convertToLocalVerticalValue
let getPlayerOneScore message = (getMessagePart 6 message).ToString() |> Convert.ToInt32
let getPlayerTwoScore message = (getMessagePart 7 message).ToString() |> Convert.ToInt32
let getWinnerFromMessage message = getMessagePart 1 message
let getControlCommand message = getMessagePart 1 message

let getGameStateFromMessage state message =
    {state with 
        BallPosition = getBallPosition message
        PreviousBallPosition = state.BallPosition
        PlayerOnePaddlePosition = getPlayerOnePaddlePosition message
        PlayerTwoPaddlePosition = getPlayerTwoPaddlePosition message
        PreviousPlayerOnePaddlePosition = state.PlayerOnePaddlePosition
        PreviousPlayerTwoPaddlePosition = state.PlayerTwoPaddlePosition
        PlayerOneScore = getPlayerOneScore message
        PlayerTwoScore = getPlayerTwoScore message

        PaddleHeight = getPaddleHeight message
    }
   
let CreateControlMessageBody () =
    match (Keyboard.IsKeyDown(Key.A), Keyboard.IsKeyDown(Key.Z)) with
    | (true,true) -> "0"
    | (true,false) -> "1"
    | (false,true) -> "2"
    | (false,false) -> "0"  

let sendControlMessage (socket:NetMQSocket) state =
    socket.SendMore("control")
        .Send((CreateControlMessageBody ()))
    state

let sendGameOverMessage (socket:NetMQSocket) winner =
    socket.SendMore("game over")
           .Send(winner + " won")

let receiveControlMessage (socket:NetMQSocket) =
    let msg = socket.ReceiveStringMessages() |> Seq.toList
    let msgtype = getMessageTypeFromMessage msg
    match msgtype with 
    | "control" -> getControlCommand msg
    | _ -> failwith ("Invalid message type: " + msgtype)

let moveRemotePlayer (socket:NetMQSocket) state =
    match receiveControlMessage socket with
    | "0" -> state
    | "1" -> playerOnePaddleUp state
    | "2" -> playerOnePaddleDown state
    | _ -> failwith "Invalid control command"

let receiveGameUpdateMessage (socket:NetMQSocket) state =
    let msg = socket.ReceiveStringMessages() |> Seq.toList
    let msgType = getMessageTypeFromMessage msg
    match msgType with
    | "gameupdate" -> getGameStateFromMessage state msg
    | "game over" -> {state with Status = MoveTo(NetworkGameOver(getWinnerFromMessage msg))}
    | _ -> failwith ("invalid message type: " + msgType)


let processMessage message state =
    let messagetype = (getMessageTypeFromMessage message)
    match messagetype with
    | "starthostingnetworkgame" -> {state with Status = MoveTo(RunningNetworkPlayerAsHost)}
    | "connecttoGame" -> {state with Status = MoveTo(RunningNetworkPlayerAsClient); NetworkGameHostAddressAndPort = getHostAddressAndPortFromMessage message}
    | _ -> failwith ("Invalid message: " + messagetype)

let sendEndOfGameMessage (socket:NetMQSocket) state =
    match state.Status with
    | MoveTo(PlayerOneWon) -> sendGameOverMessage socket "Player One" 
                              (socket :> IDisposable).Dispose ()
                              state
    | MoveTo(PlayerTwoWon) -> sendGameOverMessage socket "Player Two"
                              (socket :> IDisposable).Dispose ()
                              state
    | _ -> state
        

let startPongNetworkingGame state (socket:NetMQSocket) =
    socket.Connect("tcp://" + state.ServerAddressAndPort)
    socket.SendMore(System.Guid.NewGuid().ToString()).SendMore("rqnetworkplay").Send(state.OwnIpAddress + ":" + state.NetworkPort)
    let msg = socket.ReceiveStringMessages() |> Seq.toList
    msg |> Seq.iter (fun e -> printfn "%s" e)
    (socket :> IDisposable).Dispose ()
    processMessage msg state
     



    