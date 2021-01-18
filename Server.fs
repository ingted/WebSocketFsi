namespace FAkka.Shared.UI
open FAkka.Shared.Util
open System.Threading
open System.Reactive
open FSharp.Control.Reactive
open System.IO
open System.Text
//open FSharp.Compiler.SourceCodeServices
//open FSharp.Compiler.Interactive.Shell
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Server

open FSharp.Compiler
open FSharp.Compiler.AbstractIL
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Interactive.Shell.Settings
open Swensen.Unquote.Extensions
open System
open System.IO
open System.Reflection
open System.Text
open System.Collections.Concurrent
open System.Reactive.Subjects
open FSharp.Control.Reactive

type TwOrSub =
| TW of TextWriter
| SUB of ReplaySubject<string>
type ConsoleTextWriter (tos: TwOrSub) = 
    inherit TextWriter ()
    let mutable enc = Encoding.Default
    let queue = new ConcurrentDictionary<int, ConcurrentQueue<char> * ConcurrentQueue<char>>()
    let mutable cw = Unchecked.defaultof<string -> unit>
    do 
        ConsoleTextWriter.saveDefaultWriter ()
        cw <- 
            match tos with 
            | TW tw -> fun (str:string) -> tw.WriteLine str
            | SUB sub -> fun (str:string) -> sub.OnNext str
    member this.curWrite 
        with get () = cw
        and set value = cw <- value
                
                
    //override this.NewLine =
    //    Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ": "
    override this.Write(value:char) =
        let tid = Threading.Thread.CurrentThread.ManagedThreadId
        let preCharQ, allQ =
            queue.GetOrAdd(
                tid
                , (ConcurrentQueue<char>(), ConcurrentQueue<char>())
            )
        let preChar = ref ' '
        if preCharQ.IsEmpty then () 
        else
            ignore <| preCharQ.TryDequeue(preChar)
            //this.curWriter.WriteLine (sprintf "dqIt %A %A" dq preChar)
        
        match preChar.Value, value with
        | ('\013', '\010') ->
            let aq = allQ.ToArray() |> Array.take (allQ.Count - 1)
            let qq = ref (ConcurrentQueue<char>(), ConcurrentQueue<char>())

            ignore <| queue.TryRemove(tid, qq)
            let str = String.Join(null, aq)
            if str.Trim() <> "" then
                let s = sprintf "%d: %s" tid str
                //this.curWriter.WriteLine s
                this.curWrite s
        | _ ->
            //this.curWriter.WriteLine (sprintf "queueIt %A" value)
            preCharQ.Enqueue value
            allQ.Enqueue value
    override this.WriteLine(value:string) =
        
        let str = value.Replace("\r\n", "\r\n|")
        if str.Trim() <> "" then
            let tid = Threading.Thread.CurrentThread.ManagedThreadId
            let s = sprintf "%d: %s" tid str
            //this.curWriter.WriteLine s
            this.curWrite s

    override this.Encoding 
        with get () = enc
    static member val locker = new Object() with get
    static member val ifSaveDefaultWritter = false with get, set
    static member val defaultWriter = Unchecked.defaultof<TextWriter> with get, set
    static member saveDefaultWriter () =
        lock ConsoleTextWriter.locker (fun () ->
            if ConsoleTextWriter.ifSaveDefaultWritter = false then
                ConsoleTextWriter.defaultWriter <- Console.Out
                ConsoleTextWriter.ifSaveDefaultWritter <- true
            )

module Server =
    open WebSharper
    
    let sbOut = new StringBuilder()
    let sbErr = new StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)
    let chrHisCmd = new IO.DirectoryInfo("./hisCmd")
    let chrNamedScripts = new IO.DirectoryInfo("./namedScripts")
    if not chrHisCmd.Exists then
        chrHisCmd.Create ()
    if not chrNamedScripts.Exists then
        chrNamedScripts.Create ()
                                         
    type UniversalDict () = 
        static member val cmdHistory = 
            //let cmdList = new ConcurrentQueue<string> ()
            let cd = new ConcurrentDictionary<int, string>()
            //let cq = new ConcurrentQueue<string>()
            
            let hisFiles = 
                chrHisCmd.GetFiles ()
                |> Array.sortBy (fun fi -> fi.Name)
            hisFiles
            |> Array.iteri (fun i fi ->
                let content = IO.File.ReadAllText fi.FullName
                cd.AddOrUpdate (
                    i,
                    (fun _ -> content),
                    (fun key curV -> content)
                ) |> ignore
            )
            printfn "cmdHisRoot: %A" chrHisCmd.FullName                                        
            cd with get,set

        static member val namedScripts = 
            let cd = new ConcurrentDictionary<string, string>()

            let namedScriptFiles = 
                chrNamedScripts.GetFiles ()
                |> Array.sortBy (fun fi -> fi.Name)
            namedScriptFiles
            |> Array.iter (fun fi ->
                let content = IO.File.ReadAllText fi.FullName
                cd.AddOrUpdate (
                    fi.Name.Replace("." + fi.Extension, ""),
                    (fun _ -> content),
                    (fun key curV -> content)
                ) |> ignore
            )
            cd with get,set

    
    [<Rpc>]
    let getHisCmd () =
        async {
            let c = 
                if UniversalDict.cmdHistory.Count = 0 then "history empty"
                else  UniversalDict.cmdHistory.Values |> Seq.last
            return c
        }

    [<Rpc>]
    let getHisCmds () =
        async {
            let c = 
                UniversalDict.cmdHistory 
                |> Seq.sortBy (fun kvp ->
                    kvp.Key
                )
                |> Seq.map (fun kvp ->
                    kvp.Value
                )
                |> Seq.toArray
            return c
        }

    // Build command line arguments & start FSI session
    let argv = [|"C:\\fsi.exe"|]
    let allArgs = Array.append argv [| "--noframework"; "--langversion:preview" |]
    [<JavaScript>]
    type Name = {
        [<Name "first-name">] FirstName: string
        LastName: string
    }

    [<JavaScript>]
    type User = {
        name: Name
        age: int
    }

    type [<JavaScript; NamedUnionCases>]
        C2SMessage =
        | Request1 of str: string[]
        | Request2 of int: int[]
        | Req3 of User
        | MessageFromClient of cmd : string
    
    and [<JavaScript; NamedUnionCases "type">]
        S2CMessage =
        | [<Name "int">] Response2 of value: int
        | [<Name "string">] Response1 of value: string
        | [<Name "name">] Resp3 of value: Name
        | [<Name "msgStr">] MessageFromServer_String of value: string

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }
    let obs = Subject<string>.replay
    let obsOrig = Subject<string>.broadcast
    let tw = new ConsoleTextWriter(SUB obs)

    let ooOrig = 
        obsOrig
        |> Observable.observeOn System.Reactive.Concurrency.Scheduler.CurrentThread
        |> Observable.subscribeOn System.Reactive.Concurrency.Scheduler.CurrentThread
        |> Observable.subscribe (fun o ->
            //let tid = Threading.Thread.CurrentThread.ManagedThreadId.ToString()
            ConsoleTextWriter.defaultWriter.WriteLine o
        )

    [<Rpc>]
    let getNamedScript name =
        async {
            let c = 
                if not <| UniversalDict.namedScripts.ContainsKey name then "script not existed"
                else
                    let s = ref ""
                    if UniversalDict.namedScripts.TryGetValue(name, s) then s.Value
                    else 
                        "script get failed"
            return c
        }

    [<Rpc>]
    let upsertNamedScript name script =
        async {
            let c = 
                try 
                    let fi = new FileInfo(chrNamedScripts.FullName + @"\" + name)
                    if fi.Exists then fi.Delete ()
                    File.WriteAllText(fi.FullName, script, Encoding.Unicode)
                    UniversalDict.namedScripts.AddOrUpdate(
                                                            name, 
                                                            (fun name -> script),
                                                            (fun _ _ -> script)
                                                            )
                with
                | exn ->
                    exn.Message
            return c
        }

    [<Rpc>]
    let listNamedScripts () =
        async {
            let c = 
                try 
                    UniversalDict.namedScripts.Keys |> Seq.toArray
                with
                | exn ->
                    printfn "%s" exn.Message
                    [|"NAMEDSCRIPT IS EMPTY"|]
            return c
        }

    [<Rpc>]
    let getPort (uriStr:string) =
        async {
            let connPort2 : WebSocketEndpoint<S2CMessage, C2SMessage> = 
                WebSocketEndpoint.Create(uriStr, "/WS2", JsonEncoding.Readable)
                //WebSharper.Owin.WebSocket.Endpoint.Create(uriStr, "/WS2", WebSharper.Owin.WebSocket.JsonEncoding.Readable)
            return connPort2
        }


    Console.SetOut tw
    //let fsiConfigOld = FsiEvaluationSession.GetDefaultConfiguration()
    //let fsiSessionOld = FsiEvaluationSession.Create(fsiConfigOld, allArgs, inStream, tw, tw)

    open FsiSession

    let (fsiSession, (cacheIt:(string -> obj -> obj)), (setEnvCd:(System.Collections.Concurrent.ConcurrentDictionary<string, obj> -> unit))) = 
        let fsi = 
            FsiEvaluationSession.Create (fsiConfig, [|"fsi.exe"; "/langversion:preview"|], 
                inStream, tw, tw, collectible=false, legacyReferenceResolver=legacyReferenceResolver)
        let cmd = 
            "let mutable env = new System.Collections.Concurrent.ConcurrentDictionary<string, obj>()\r\n" +
            "let auf : (string -> obj -> obj) = fun (k:string) (v:obj) -> env.AddOrUpdate(k, (fun _ -> v), (fun _ _ -> v))\r\n" + 
            "let setenv : (System.Collections.Concurrent.ConcurrentDictionary<string, obj> -> unit) = fun (cdObj:System.Collections.Concurrent.ConcurrentDictionary<string, obj>) -> env <- cdObj\r\n"
        fsi.EvalInteraction cmd
        
        let cacheIt : (string -> obj -> obj) = 
            unbox (fsi.EvalExpression "(auf: (string -> obj -> obj))").Value.ReflectionValue
        let setEnvCd : (System.Collections.Concurrent.ConcurrentDictionary<string, obj> -> unit) =
            unbox (fsi.EvalExpression "(setenv : (System.Collections.Concurrent.ConcurrentDictionary<string, obj> -> unit))").Value.ReflectionValue
        fsi, cacheIt, setEnvCd


    let locker = new Object()

    type FSCMD = string
    let fsiExecututor = 
        MailboxProcessor.Start(
            fun (agt:MailboxProcessor<FSCMD * AsyncReplyChannel<S2CMessage option>>) -> 
                let rec f () =
                    async {
                        let! (cmd, channel) = agt.Receive ()
                        UniversalDict.cmdHistory.TryAdd (UniversalDict.cmdHistory.Count, cmd) |> ignore
                        async {
                            let fis = chrHisCmd.GetFiles () |> Seq.sortByDescending (fun fi -> fi.Name) |> Seq.cache
                            let fNm = 
                                if fis |> Seq.length = 0 then 0
                                else 
                                    let maxCmd = fis |> Seq.head
                                    (Int32.Parse maxCmd.Name) + 1
                            IO.File.WriteAllText(chrHisCmd.FullName + @"\" + fNm.ToString (), cmd)
                            }
                            |> Async.Catch
                            |> Async.Ignore
                            |> Async.Start
                        let rst, err = fsiSession.EvalInteractionNonThrowing cmd
                        //let rst = fsiSession.EvalInteraction cmd
                        match rst with
                        | Choice1Of2 (Some value) ->
                            if value.ReflectionValue = null then channel.Reply <| None else
                            channel.Reply <| Some (MessageFromServer_String (value.ReflectionValue.ToString()))
                        | Choice1Of2 None ->
                            channel.Reply <| None
                        | Choice2Of2 exn ->
                            channel.Reply <| Some (MessageFromServer_String (exn.Message))
                        return! f ()
                    }
                f ()
    )
    let agtSeq = 
        let mutable exCnt = 0
        MailboxProcessor.Start (fun (mb:MailboxProcessor<unit * AsyncReplyChannel<int>>)  -> 
            let rec loop () = 
                async {
                    let! _, arc = mb.Receive()
                    arc.Reply(exCnt)
                    exCnt <- exCnt + 1
                    return! loop ()
                }            
            loop ()
        )
    [<Rpc>]
    let fsiSeq () =
        async {            
            return! agtSeq.PostAndAsyncReply (fun (channel:AsyncReplyChannel<int>) -> (), channel)
        }
            
    type STWrapper =
    | STSO of S2CMessage option
    | STInt of int
    | STUnit 

    let stsoa (a:Async<S2CMessage option>) = 
            async {
                let! v = a
                return STSO v
                }
    let stinta (a:Async<int>) = 
            async {
                let! v = a
                return STInt v
                }
    let stunita (a:Async<unit>) = 
            async {
                let! _ = a
                return STUnit 
                }

    let cmdShowA (cmd:string) = async {
        tw.WriteLine(cmd)
        return STUnit
    }

    [<Rpc>]
    let fsiExecute (input:string) =
        async {
            let replyA = fsiExecututor.PostAndAsyncReply (fun (channel:AsyncReplyChannel<S2CMessage option>) -> input, channel)
            let exCntA = agtSeq.PostAndAsyncReply (fun (channel:AsyncReplyChannel<int>) -> (), channel)
            let! s =
                seq[stsoa replyA; stinta exCntA; cmdShowA input]
                |> Async.Parallel
            let [|STSO reply; STInt exCnt; u |] = s    
            match reply with
            | Some (MessageFromServer_String v) -> return v,exCnt
            | None -> return "noReturn",exCnt
            | _ -> return "invalidMsg",exCnt
            }

    [<Rpc>]
    let MD5Hash (input : string) =
        async {
            use md5 = System.Security.Cryptography.MD5.Create()
            return (
                input
                |> System.Text.Encoding.ASCII.GetBytes
                |> md5.ComputeHash
                |> Seq.map (fun c -> c.ToString("X2"))
                |> Seq.reduce (+)
                |> fun s -> s.Substring(0,7)
            )
        }

    let mre = new ManualResetEvent (false)

    let Start i (cd:System.Collections.Concurrent.ConcurrentDictionary<string, obj>) : StatefulAgent<S2CMessage, C2SMessage, int> =
        setEnvCd cd
        /// print to debug output and stdout
        let dprintfn x =
            Printf.ksprintf (fun s ->
                System.Diagnostics.Debug.WriteLine s
                stdout.WriteLine s
            ) x
        let agt = 
            MailboxProcessor.Start(
                fun (agt:MailboxProcessor<string*Message<C2SMessage>*int*AsyncReplyChannel<S2CMessage option*int>>) -> 
                    let rec f () =
                        async {
                            let! (clientIp, msg, state, channel) = agt.Receive () 
                            //if i = 1 
                            //then Thread.Sleep 2000 
                            //else Thread.Sleep 1000
                            match msg with
                            | Message data -> 
                                match data with
                                | Request1 x -> 
                                    channel.Reply <| (Some (Response1 x.[0]), state + 1)
                                | Request2 x -> 
                                    channel.Reply <| (Some (Response2 x.[0]), state + 1)
                                | Req3 x ->
                                    channel.Reply <| (Some (Resp3 x.name), state + 1)
                                | MessageFromClient cmd ->
                                    let ll = if cmd.Length <= 7 then cmd.Length else 7
                                    channel.Reply <| (Some (MessageFromServer_String <| cmd.Substring(0, ll)), state + 1)
                            | Error exn -> 
                                dprintfn "Error in WebSocket server connected to %s: %s" clientIp exn.Message
                                //dprintfn "Error in WebSocket server connected: %s" exn.Message
                                channel.Reply <| (Some (Response1 ("Error: " + exn.Message)), state)
                            | Close ->
                                eprintfn "Closed connection to %d %s" i clientIp
                                //eprintfn "Closed connection to %d" i
                                channel.Reply <| (None, state)
                            return! f ()
                        }
                    f ()
        )
        fun client -> async {
            //let clientIp = client.Connection.Context.Request.RemoteIpAddress
            let clientIp = client.Connection.Context.Connection.RemoteIpAddress.ToString()            
            return 0, fun state msg -> async {
                eprintfn "%d Received message #%i from %s" i state clientIp
                //eprintfn "%d Received message #%i" i state
                //let tid = Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":"
                let! (msg2client, state) = 
                    agt.PostAndAsyncReply(
                        fun channel ->
                            (clientIp, msg, state, channel)
                    )
                match msg2client with
                | Some m ->
                    do! client.PostAsync m
                    let loopPost (cmdResult:string) = 
                        //sync {
                            //let! m = 
                            //return! loopPost ()
                            //loopPost ()
                            if cmdResult.Substring(0, 2) = "1:" then
                                obsOrig.OnNext cmdResult
                            client.PostAsync (S2CMessage.MessageFromServer_String cmdResult)
                            |> Async.Start
                            //printfn "cpIt"
                        //}
                    //do! loopPost ()

                    use oo = 
                        obs
                        //|> Observable.filter (fun o ->
                        //    o.Substring(0, 2) <> tid
                        //)
                        |> Observable.observeOn System.Reactive.Concurrency.Scheduler.CurrentThread
                        |> Observable.subscribeOn System.Reactive.Concurrency.Scheduler.CurrentThread
                        |> Observable.subscribe loopPost
                    
                    mre.WaitOne ()|> ignore
                    printfn "jumpOut"
                | None -> ()
                return state * 10
            }
        }

    //let wbSockIn i : StatefulAgent<S2CMessage, C2SMessage, int> =
    //    Console.SetOut (System.IO.TextWriter.Synchronized tw)
    //    let dprintfn x =
    //        Printf.ksprintf (fun s ->
    //            System.Diagnostics.Debug.WriteLine s
    //            stdout.WriteLine s
    //        ) x
    //    let fsiKickOffAgent = 
    //        MailboxProcessor.Start(
    //            fun (agt:MailboxProcessor<WebSocketClient<S2CMessage, C2SMessage> * Message<C2SMessage> * int * AsyncReplyChannel<S2CMessage option * int>>) -> 
    //                let rec f () =
    //                    async {
    //                        let! (client, msg, state, channel) = agt.Receive () 
    //                        //if i = 1 
    //                        //then Thread.Sleep 2000 
    //                        //else Thread.Sleep 1000
    //                        match msg with
    //                        | Message data -> 
    //                            match data with
    //                            | Request1 x -> 
    //                                channel.Reply <| (Some (Response1 x.[0]), state + 1)
    //                            | Request2 x -> 
    //                                channel.Reply <| (Some (Response2 x.[0]), state + 1)
    //                            | Req3 x ->
    //                                channel.Reply <| (Some (Resp3 x.name), state + 1)
    //                            | MessageFromClient cmd ->
    //                                let ll = if cmd.Length <= 5 then cmd.Length else 5
    //                                channel.Reply <| (Some (MessageFromServer_String <| cmd.Substring(0, ll)), state + 1)
    //                        | Error exn -> 
    //                            dprintfn "Error in WebSocket server connected to %s: %s" (client.Connection.Context.Connection.RemoteIpAddress.ToString()) exn.Message
    //                            channel.Reply <| (Some (Response1 ("Error: " + exn.Message)), state)
    //                        | Close ->
    //                            eprintfn "Closed connection to %d %s" i (client.Connection.Context.Connection.RemoteIpAddress.ToString())
    //                            channel.Reply <| (None, state)
    //                        return! f ()
    //                    }
    //                f ()
    //    )
    //    fun client -> async {
    //        //let clientIp = client.Connection.Context.Request.RemoteIpAddress
    //        let clientIp = client.Connection.Context.Connection.RemoteIpAddress.ToString() 
            
    //        return 0, fun state msg -> async {
    //            eprintfn "%d Received kickOff message #%i from %s" i state clientIp
    //            let! (msg2client, state) = 
    //                fsiKickOffAgent.PostAndAsyncReply(
    //                    fun channel ->
    //                        (client, msg, state, channel)
    //                )
    //            match msg2client with
    //            | Some m ->
    //                do! client.PostAsync m
    //                let loopPost cmdResult = 
    //                    //sync {
    //                        //let! m = 
    //                        //return! loopPost ()
    //                        //loopPost ()
    //                        client.PostAsync (S2CMessage.MessageFromServer_String cmdResult)
    //                        |> Async.Start
    //                        printfn "cpIt"
    //                    //}
    //                //do! loopPost ()

    //                use oo = 
    //                    obs
    //                    |> Observable.observeOn System.Reactive.Concurrency.Scheduler.CurrentThread
    //                    |> Observable.subscribeOn System.Reactive.Concurrency.Scheduler.CurrentThread
    //                    |> Observable.subscribe loopPost
                    
    //                mre.WaitOne ()|> ignore
    //                printfn "jumpOut"
    //            | None -> ()
    //            return state * 10
    //        }
    //    }
