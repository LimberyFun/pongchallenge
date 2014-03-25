module PongServer
open PongMessage
open NetMQ

type receivedMessage = 
    {Address:byte[];MessageType:string;MessageBody:byte[]}
     override x.ToString () = sprintf "%A - %s - %s" x.Address x.MessageType (System.Text.Encoding.UTF8.GetString(x.MessageBody))

let createDecodedMessage (msg:NetMQMessage) =
    let address = (msg.Pop ()).Buffer
    (msg.Pop ()) |> ignore
    let messagetype = (msg.Pop ()).ConvertToString ()
    let msgBody = (msg.Pop ()).Buffer
    {receivedMessage.Address = address; MessageType = messagetype; MessageBody=msgBody}
 
let encode = string >> System.Text.Encoding.UTF8.GetBytes
let decode = System.Text.Encoding.UTF8.GetString

let sendStartHostingMessage address (socket:NetMQSocket) =
    let msgtype = StartHostingNeworkGame |> getMessageType |> encode
    let msg = new NetMQMessage([|address;msgtype|])
    printfn "sending host"
    socket.SendMessage(msg)

let sendConnectToGameMessage addess ip (socket:NetMQSocket) =
    let msgType = ConnectToNetworkGame(decode ip) |> getMessageType |> encode
    let msg = new NetMQMessage([|addess;msgType;ip|])
    printfn "sending connect"
    socket.SendMessage(msg)

let rec mainloop (socket:NetMQSocket) (waitingPlayers: receivedMessage list) =
    let msg = socket.ReceiveMessage() |> createDecodedMessage
    printfn "msg %s received" (msg.ToString ())
    let newWaitingPlayers = msg :: waitingPlayers
    match newWaitingPlayers with
    | [] -> mainloop socket newWaitingPlayers
    | [_] -> mainloop socket newWaitingPlayers
    | [playertwo;playerOne] ->   sendStartHostingMessage playerOne.Address socket
                                 sendConnectToGameMessage playertwo.Address playertwo.MessageBody socket
                                 printfn "Connected %A and %A" playerOne.Address playertwo.Address
                                 mainloop socket List.empty<receivedMessage>
    | _ -> failwith "OOps more than two players waiting"                                

[<EntryPoint>]
let main argv =  
    printfn "hello"
    use context = NetMQContext.Create ()
    use server = context.CreateRouterSocket ()
    server.Bind("tcp://127.0.0.1:9999")
    let waitingPlayers = List.empty<receivedMessage>
    mainloop server waitingPlayers
            
    0 // return an integer exit code
