module PongServer
open PongMessage
open PongNetworking
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

let sendStartHostingMessage address (socket:NetMQSocket) =
    let msgtype = StartHostingNetworkGame |> getMessageType |> encode
    let msg = new NetMQMessage([|address;msgtype|])
    printfn "sending host"
    socket.SendMessage(msg)

let sendConnectToGameMessage addess ip (socket:NetMQSocket) =
    let msgType = ConnectToNetworkGame(decode ip) |> getMessageType |> encode
    let msg = new NetMQMessage([|addess;msgType;ip|])
    printfn "sending connect"
    socket.SendMessage(msg)

let rec runServer (waitingPlayers: receivedMessage list) (socket:NetMQSocket)  =
    let msg = socket.ReceiveMessage() |> createDecodedMessage
    printfn "msg %s received" (msg.ToString ())
    let newWaitingPlayers = msg :: waitingPlayers
    match newWaitingPlayers with
    | [] -> runServer newWaitingPlayers socket
    | [_] -> runServer newWaitingPlayers socket
    | [playertwo;playerOne] ->   sendStartHostingMessage playerOne.Address socket
                                 sendConnectToGameMessage playertwo.Address playerOne.MessageBody socket
                                 printfn "Connected %A and %A" playerOne.Address playertwo.Address
                                 runServer List.empty<receivedMessage> socket
    | _ -> failwith "OOps more than two players waiting"      