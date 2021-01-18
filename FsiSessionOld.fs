module FsiSession
open System
open System.Globalization
open System.IO
open System.Reflection
open System.Threading
open System.Runtime.CompilerServices
open FSharp.Compiler
open FSharp.Compiler.AbstractIL
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Interactive.Shell.Settings

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
    let defaultFSharpBinariesDir =
        let d = System.Reflection.Assembly.GetExecutingAssembly() //IO.Directory.GetCurrentDirectory()
        let fi = IO.FileInfo(d.Location)
        fi.DirectoryName            
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
