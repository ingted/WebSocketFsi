namespace FAkka.Shared.UI

open WebSharper
open WebSharper.Sitelets
open Microsoft.Extensions.Logging
open WebSharper.AspNetCore
open WebSharper.AspNetCore.WebSocket

//open WebSharper.UI.Next
//open WebSharper.UI.Next.Server



type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About
    | [<EndPoint "/websocket">] WS
    | [<EndPoint "/fsi">] FSI
    

module Templating =
    open WebSharper.UI
    open WebSharper.UI.Html
    open WebSharper.UI.Server

    type MainTemplate = Templating.Template<"main.html">
    //type FSTemplate = Templating.Template<"main2.html">
    type FSIndex = Templating.Template<"index.html">

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             li [if endpoint = act then yield attr.``class`` "active"] [
                a [attr.href (ctx.Link act)] [text txt]
             ]
        [
            "Home" => EndPoint.Home
            "About" => EndPoint.About
            "WebSocket" => EndPoint.WS
            "FSI" => EndPoint.FSI
            
        ]

    let Main ctx endPoint (title: string) (body: Doc list) =
        let mt = 
            MainTemplate()                
                .Title(title)
                .MenuBar(MenuBar ctx endPoint)
                .Body(body)
                .Doc()
        Content.Page mt


    let FSI (ctx: Context<EndPoint>) endPoint (title: string) (main: Doc list) =
        Content.Page(
            FSIndex()
                .main(main)
                //.Title(title)
                //.Body(body)
                .Doc()
                //.Body(body)
                //.Doc()
        )

module Site =
    open WebSharper
    open WebSharper.UI
    open WebSharper.UI.Html
    open WebSharper.UI.Server
    open WebSharper.Sitelets

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            h1 [] [text "Say Hi to the server!"]
            div [] [client <@ Client.Main() @>]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [] [text "About"]
            p [] [text "This is a template WebSharper client-server application."]
            div [] [client <@ Client.m2() @>]
        ]


    let FSIPage ctx =
        let docList = Templating.MenuBar ctx EndPoint.FSI 
        let urlStr = ctx.RequestUri.ToString().Replace("fsi", "")

        let wc = 
            div [] [
                br [][]
                div [] [client <@ Client.fsiCmd urlStr @>]
                div [
                    Attr.Create "id" "fsiResult" 
                ] [
                    div [attr.id "consoleWC"] [
                        textarea [attr.id "console"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "10"] []
                    ]
                ]
            ]
        Content.Page(
            Templating.MainTemplate()
                .Title("wsFsi")
                .MenuBar(docList)
                .Body(wc)
                .Doc()
        )

    let Socketing send receive ctx =
        let docList = Templating.MenuBar ctx EndPoint.WS 
        let ws = ClientSide <@ Client.WS send @>
        let ws2 = ClientSide <@ Client.WS receive @>
        let wc = 
            div [] [
                div [] [Doc.WebControl ws; Doc.WebControl ws2]
                div [] []
            ]
        Content.Page(
            Templating.MainTemplate()
                .Title("wsInAspNetCore")
                .MenuBar(docList)
                .Body(wc)
                .Doc()
        )

    
    type MyWebsite(logger: ILogger<MyWebsite>) =
        inherit SiteletService<EndPoint>()
        override this.Sitelet = 
            Application.MultiPage (fun (ctx: Context<_>) (endpoint: EndPoint) ->
                let urlStr = ctx.RequestUri.ToString()
                let connPort1 = 
                    WebSocketEndpoint.Create(urlStr, "/WS", JsonEncoding.Readable)
                    
                let connPort2 = 
                    WebSocketEndpoint.Create(urlStr, "/WS2", JsonEncoding.Readable)
                match endpoint with
                | EndPoint.Home -> HomePage ctx
                | EndPoint.About -> AboutPage ctx
                | EndPoint.WS -> Socketing connPort1 connPort2 ctx
                | EndPoint.FSI -> FSIPage ctx
                
            )
    
