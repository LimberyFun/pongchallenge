open NetMQ
open PongMessage

let encode = string >> System.Text.Encoding.UTF8.GetBytes
let decode = System.Text.Encoding.UTF8.GetString

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    use context = NetMQContext.Create ()
    use client = context.CreateDealerSocket ()
    client.Connect("tcp://127.0.0.1:9999")
    let msg2 = new NetMQMessage([|(encode (System.Guid.NewGuid().ToString())); (encode "rqnetworkgame");(encode "127.0.0.1:9997")|])

    client.SendMessage(msg2)


  
    let msgrcv2 = client.ReceiveMessage ()
    //printfn "%A" msgrcv1
    printfn "%A" msgrcv2
    System.Console.ReadLine () |> ignore
    0 // return an integer exit code
