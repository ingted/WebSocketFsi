namespace FAkka.Shared.UI


module FsiWeb =
    open System
    open Microsoft.AspNetCore
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Hosting
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Hosting
    open WebSharper.AspNetCore
    open WebSharper.AspNetCore.WebSocket
    open Microsoft.Extensions.Logging
    open NLog
    open NLog.Web
    open NLog.Web.AspNetCore
    let mutable startUpCd = new System.Collections.Concurrent.ConcurrentDictionary<string, obj>()
    type Startup() =
    
        member this.ConfigureServices(services: IServiceCollection) =
            services.AddSitelet<Site.MyWebsite>()
                    .AddAuthentication("WebSharper")
                    .AddCookie("WebSharper", fun options -> ())
            |> ignore
    
        member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment, cfg: IConfiguration) =
            if env.IsDevelopment() then app.UseDeveloperExceptionPage() |> ignore
    
            app.UseAuthentication()
                .UseWebSockets()
                .UseWebSharper(
                    fun ws ->
                        ws.UseWebSocket("WS", fun wsws -> 
                            let wuse = 
                                wsws.Use(Server.Start 1 startUpCd).JsonEncoding(JsonEncoding.Readable)
                            wuse |> ignore
                        )
                        |> ignore
                        ws.UseWebSocket("WS2", fun wsws -> 
                            let wuse = 
                                wsws.Use(Server.Start 2 startUpCd).JsonEncoding(JsonEncoding.Readable)
                            wuse |> ignore
                        )
                        |> ignore                    
                )
                .UseStaticFiles()
                .Run(fun context ->
                    context.Response.StatusCode <- 404
                    context.Response.WriteAsync("Fell through :("))
    
    let BuildWebHost args (cd:System.Collections.Concurrent.ConcurrentDictionary<string, obj>) =
        ConsoleTextWriter.saveDefaultWriter () 
        AppDomain.CurrentDomain.UnhandledException.AddHandler(
            new UnhandledExceptionEventHandler(fun eventObj eventArg ->
                ConsoleTextWriter.defaultWriter.WriteLine((eventArg.ExceptionObject :?> Exception).Message)
                )
            )
        let url =
            if args |> Array.length = 0 then 
                "http://*:5999/"
            else
                args.[0]
        if cd <> null then
            startUpCd <- cd
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            
            .UseUrls(url)
            .ConfigureLogging(fun logging ->                            
                logging.ClearProviders()
                |> ignore
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
                |> ignore
            )
            .UseNLog()
            .Build()
    let fsiWebRun args objCache = (BuildWebHost args objCache).Run()
    let fsiWebStart args objCache = (BuildWebHost args objCache).Start()
    let fsiWebStartAsync args objCache = (BuildWebHost args objCache).StartAsync()
    [<EntryPoint>]
    let main args =
               
        fsiWebRun args null
        0
