module PongNetworking
open NetMQ
open PongTypes

let serverAddress = "tcp://127.0.0.1:9999"
let ownAddress = "127.0.0.1:9998"

let getSocket (socket:Option<NetMQSocket>) =
    match socket with 
    | None -> failwith "Socket missing"
    | Some s -> s

let getMessageType = Seq.head

let startPongNetworking (socket:NetMQSocket) =
    socket.Connect(serverAddress)
    socket.SendMore(System.Guid.NewGuid().ToString()).SendMore("rqnetworkplay").Send(ownAddress)
    let msg = socket.ReceiveStringMessages()
    msg |> Seq.iter (fun e -> printfn "%s" e)

let processMessage message state =
    match (getMessageType message) with
    | "startHostingNetworkgame" -> {state with Status = RunningNetworkPlayerAsHost}
    

    