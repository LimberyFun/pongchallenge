module PongNetworking
open NetMQ
open PongTypes
open System
open System.Net

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

let getMessageTypeFromMessage = Seq.head

let startPongNetworking state (socket:NetMQSocket) =
    socket.Connect("tcp://" + state.ServerAddressAndPort)
    socket.SendMore(System.Guid.NewGuid().ToString()).SendMore("rqnetworkplay").Send(state.OwnIpAddress + ":" + state.NetworkPort)
    let msg = socket.ReceiveStringMessages()
    msg |> Seq.iter (fun e -> printfn "%s" e)

let processMessage message state =
    match (getMessageTypeFromMessage message) with
    | "startHostingNetworkgame" -> {state with Status = RunningNetworkPlayerAsHost}
    

    