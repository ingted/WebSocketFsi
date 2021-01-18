namespace FAkka.Shared.Util



module FsiSession =

    open System

    open System.Globalization
    open System.IO
    open System.Reflection
    open System.Threading
    //open System.Runtime.CompilerServices
    open FSharp.Compiler.SourceCodeServices
    //open FSharp.Compiler
    //open FSharp.Compiler.AbstractIL
    open FSharp.Compiler.Interactive.Shell
    //open FSharp.Compiler.Interactive.Shell.Settings

    #nowarn "55"
    #nowarn "40" // let rec on value 'fsiConfig'


    let SetCurrentUICultureForThread (lcid : int option) =
        let culture = Thread.CurrentThread.CurrentUICulture
        match lcid with
        | Some n -> Thread.CurrentThread.CurrentUICulture <- new CultureInfo(n)
        | None -> ()
        { new IDisposable with member x.Dispose() = Thread.CurrentThread.CurrentUICulture <- culture }

    let callStaticMethod (ty:Type) name args =
        ty.InvokeMember(name, (BindingFlags.InvokeMethod ||| BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic), null, null, Array.ofList args,Globalization.CultureInfo.InvariantCulture)

    /// Starts the remoting server to handle interrupt requests from a host tool.
    let StartServer (fsiSession : FsiEvaluationSession) (fsiServerName) = 
        ignore (fsiSession, fsiServerName)

    //----------------------------------------------------------------------------
    // GUI runCodeOnMainThread
    //----------------------------------------------------------------------------

    // Create the console reader
//#if NET50FSI  => search whyNoNET50FSI
    
    let assms = System.AppDomain.CurrentDomain.GetAssemblies()
    //assms
    //|> Array.choose (fun a -> try Some a.FullName with _ -> None) |> Array.sort
 
    let fsi = 
        try
            assms |> Array.filter (fun a -> a.GetName().Name = "fsi") |> Array.item 0
        with
        | exn ->
            try
                let privatec =
                    assms |> Array.filter (fun a -> a.GetName().Name = "FSharp.Compiler.Private") |> Array.item 0
                let fsiPath = 
                    (new IO.FileInfo(privatec.Location)).DirectoryName + "\\fsi.exe" 
                Assembly.LoadFile fsiPath
            with
            | exn2 ->
                printfn "%s" exn2.Message
                reraise()

    let ft = fsi.GetTypes()
    //ft|>Array.iter (fun t -> printfn "%s" t.Name)
    //let o = {| ft = ft |}
    let regex = System.Text.RegularExpressions.Regex "ReadLineConsole"
    let rlc0 = ft |> Array.filter (fun t -> regex.IsMatch t.Name ) 
    //fsi.GetType("ReadLineConsole")
    //open System
    //open System.Reflection

    //type O < ^T when ^T :(member ReadLine : unit -> string) > = {
    //    rlco :^T
    //}
    let rlc = rlc0.[0]
    let rlco = Activator.CreateInstance(rlc)
    let rl = rlc.GetMethod("ReadLine", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let console = {|ReadLine = fun () -> rl.Invoke(rlco, [||]).ToString()|}

//#else
      //whyNoNET50FSI
//    //use reflection instead of customized built FSharp.Core, so, no NET50FSI anymore
//    let console = new FSharp.Compiler.Interactive.ReadLineConsole()
//#endif
    //System.AppDomain.CurrentDomain.GetAssemblies()|>Seq.iter (fun a -> printfn "%s" a.FullName)
    //#r @"C:\Users\aniba\Downloads\mdcdev\fsharp\artifacts\bin\fsi\Debug\net48\fsi.exe";;
    // Define the function we pass to the FsiEvaluationSession
    let getConsoleReadLine (probeToSeeIfConsoleWorks) = 
        let consoleIsOperational =
          if probeToSeeIfConsoleWorks then 
            //if progress then fprintfn outWriter "probing to see if console works..."
            try
                // Probe to see if the console looks functional on this version of .NET
                let _ = Console.KeyAvailable 
                let _ = Console.ForegroundColor
                let _ = Console.CursorLeft <- Console.CursorLeft
                true
            with _ -> 
                //if progress then fprintfn outWriter "probe failed, we have no console..."
                false 
          else true
        if consoleIsOperational then 
            Some (fun () -> console.ReadLine())
        else
            None

    //#if USE_FSharp_Compiler_Interactive_Settings
    let fsiObjOpt = 
    #if INTERACTIVE
        let defaultFSharpBinariesDir = AppDomain.CurrentDomain.BaseDirectory
    #else
        let defaultFSharpBinariesDir =
            let d = System.Reflection.Assembly.GetExecutingAssembly() //IO.Directory.GetCurrentDirectory()        
            let fi = IO.FileInfo(d.Location)
            fi.DirectoryName  
    #endif
        // We use LoadFrom to make sure we get the copy of this assembly from the right load context
        let fsiAssemblyPath = Path.Combine(defaultFSharpBinariesDir,"FSharp.Compiler.Interactive.Settings.dll")
        let fsiAssembly = Assembly.LoadFrom(fsiAssemblyPath)
        if isNull fsiAssembly then 
            None
        else
            let fsiTy = fsiAssembly.GetType("FSharp.Compiler.Interactive.Settings")
            if isNull fsiAssembly then failwith "failed to find type FSharp.Compiler.Interactive.Settings in FSharp.Compiler.Interactive.Settings.dll"
            Some (callStaticMethod fsiTy "get_fsi" [  ])

    let fsiConfig0 = 
        match fsiObjOpt with 
        | None -> FsiEvaluationSession.GetDefaultConfiguration()
        | Some fsiObj -> FsiEvaluationSession.GetDefaultConfiguration(fsiObj, true)

    let legacyReferenceResolver = 
        LegacyMSBuildReferenceResolver.getResolver()


            // Update the configuration to include 'StartServer', WinFormsEventLoop and 'GetOptionalConsoleReadLine()'
    let rec fsiConfig = 
        { new FsiEvaluationSessionHostConfig () with 
            member __.FormatProvider = fsiConfig0.FormatProvider
            member __.FloatingPointFormat = fsiConfig0.FloatingPointFormat
            member __.AddedPrinters = fsiConfig0.AddedPrinters
                //Utilities.getInstanceProperty fsiObj "AddedPrinters"
            member __.ShowDeclarationValues = fsiConfig0.ShowDeclarationValues
            member __.ShowIEnumerable = fsiConfig0.ShowIEnumerable
            member __.ShowProperties = fsiConfig0.ShowProperties
            member __.PrintSize = fsiConfig0.PrintSize  
            member __.PrintDepth = fsiConfig0.PrintDepth
            member __.PrintWidth = fsiConfig0.PrintWidth
            member __.PrintLength = fsiConfig0.PrintLength
            member __.ReportUserCommandLineArgs args = fsiConfig0.ReportUserCommandLineArgs args
            member __.EventLoopRun() = 
                fsiConfig0.EventLoopRun()
            member __.EventLoopInvoke(f) = 
                fsiConfig0.EventLoopInvoke(f)
            member __.EventLoopScheduleRestart() = 
                fsiConfig0.EventLoopScheduleRestart()

            member __.UseFsiAuxLib = fsiConfig0.UseFsiAuxLib

            member __.StartServer(fsiServerName) = () //StartServer fsiSession fsiServerName
        
            // Connect the configuration through to the 'fsi' Event loop
            member __.GetOptionalConsoleReadLine(probe) = None //getConsoleReadLine(probe) 
        }

    // Create the console
    //let fsiSessionDefault : FsiEvaluationSession = FsiEvaluationSession.Create (fsiConfig, [|"fsi.exe"; "/langversion:preview"|], Console.In, Console.Out, Console.Error, collectible=false, legacyReferenceResolver=legacyReferenceResolver)

    //[<EntryPoint>]
    //let main argv =
    //    let a = 
    //        fsiSession.EvalInteractionNonThrowing """
    //#r "nuget: Akka"
    //open Akka
    //printfn "123"
    //        """
    //    let b = a
    //    printfn "read: %s" <| Console.ReadLine() 
    //    0 // return an integer exit code
    let evalProc (vHandler:obj->unit) (eHandler:exn -> unit) (r:Choice<FsiValue option, exn> * FSharpErrorInfo[]) =
        let c, _ = r
        match c with
        | Choice1Of2 (Some v) ->
            vHandler v.ReflectionValue
        | Choice1Of2 None ->
            ()
        | Choice2Of2 exn ->
            eHandler exn

    let evalIgnoreOK =
        evalProc (fun _ -> ()) 

    let evalRstProc h =
        evalProc h (fun exn -> raise exn) 

    let evalIgnoreErr h =
        evalProc h (fun _ -> ())