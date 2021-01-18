(function(Global)
{
 "use strict";
 var WebSharper,AspNetCore,WebSocket,Async,Obj,WebSocketEndpoint,Client,Message,WebSocketServer,WithEncoding,Control,MailboxProcessor,Concurrency,IntelliFactory,Runtime,JSON,Json,Seq;
 WebSharper=Global.WebSharper=Global.WebSharper||{};
 AspNetCore=WebSharper.AspNetCore=WebSharper.AspNetCore||{};
 WebSocket=AspNetCore.WebSocket=AspNetCore.WebSocket||{};
 Async=WebSocket.Async=WebSocket.Async||{};
 Obj=WebSharper&&WebSharper.Obj;
 WebSocketEndpoint=WebSocket.WebSocketEndpoint=WebSocket.WebSocketEndpoint||{};
 Client=WebSocket.Client=WebSocket.Client||{};
 Message=Client.Message=Client.Message||{};
 WebSocketServer=Client.WebSocketServer=Client.WebSocketServer||{};
 WithEncoding=Client.WithEncoding=Client.WithEncoding||{};
 Control=WebSharper&&WebSharper.Control;
 MailboxProcessor=Control&&Control.MailboxProcessor;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 JSON=Global.JSON;
 Json=WebSharper&&WebSharper.Json;
 Seq=WebSharper&&WebSharper.Seq;
 Async.FoldAgent=function(initState,f)
 {
  return MailboxProcessor.Start(function(inbox)
  {
   function loop(state)
   {
    var b;
    b=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Bind(inbox.Receive(null),function(a)
     {
      return Concurrency.Bind(f(state,a),function(a$1)
      {
       return loop(a$1);
      });
     });
    });
   }
   return loop(initState);
  },null);
 };
 WebSocketEndpoint=WebSocket.WebSocketEndpoint=Runtime.Class({
  get_JsonEncoding:function()
  {
   return this.JsonEncoding;
  },
  get_URI:function()
  {
   return this.uri;
  }
 },Obj,WebSocketEndpoint);
 WebSocketEndpoint.New=Runtime.Ctor(function(uri)
 {
  Obj.New.call(this);
  this.uri=uri;
  this.JsonEncoding={
   $:0
  };
 },WebSocketEndpoint);
 Message.Close={
  $:3
 };
 Message.Open={
  $:2
 };
 Message.Error={
  $:1
 };
 WebSocketServer=Client.WebSocketServer=Runtime.Class({
  Post:function(msg)
  {
   this.conn.send(this.encode(msg));
  },
  get_Connection:function()
  {
   return this.conn;
  }
 },Obj,WebSocketServer);
 WebSocketServer.New=Runtime.Ctor(function(conn,encode)
 {
  Obj.New.call(this);
  this.conn=conn;
  this.encode=encode;
 },WebSocketServer);
 WithEncoding.Connect=function(encode,decode,endpoint,agent)
 {
  return WithEncoding.FromWebSocket(encode,decode,new Global.WebSocket(endpoint.get_URI()),agent,endpoint.get_JsonEncoding());
 };
 WithEncoding.ConnectStateful=function(encode,decode,endpoint,agent)
 {
  return WithEncoding.FromWebSocketStateful(encode,decode,new Global.WebSocket(endpoint.get_URI()),agent,endpoint.get_JsonEncoding());
 };
 WithEncoding.FromWebSocket=function(encode,decode,socket,agent,jsonEncoding)
 {
  var p,decode$1,flush,server,b;
  p=Client.getEncoding(encode,decode,jsonEncoding);
  decode$1=p[1];
  flush=Client.cacheSocket(socket,decode$1);
  server=new WebSocketServer.New(socket,p[0]);
  b=null;
  return Concurrency.Delay(function()
  {
   return Concurrency.Bind(agent(server),function(a)
   {
    function a$1(ok,ko)
    {
     var isOpen;
     isOpen=flush(a);
     socket.onopen=function()
     {
      a(Message.Open);
      return ok(server);
     };
     socket.onclose=function()
     {
      return a(Message.Close);
     };
     socket.onmessage=function(msg)
     {
      return a({
       $:0,
       $0:decode$1(msg)
      });
     };
     socket.onerror=function()
     {
      a(Message.Error);
      return ko(new Global.Error("Could not connect to the server."));
     };
     isOpen?ok(server):void 0;
    }
    return Concurrency.FromContinuations(function($1,$2,$3)
    {
     return a$1.apply(null,[$1,$2,$3]);
    });
   });
  });
 };
 WithEncoding.FromWebSocketStateful=function(encode,decode,socket,agent,jsonEncoding)
 {
  var p,decode$1,flush,server,b;
  p=Client.getEncoding(encode,decode,jsonEncoding);
  decode$1=p[1];
  flush=Client.cacheSocket(socket,decode$1);
  server=new WebSocketServer.New(socket,p[0]);
  b=null;
  return Concurrency.Delay(function()
  {
   return Concurrency.Bind(agent(server),function(a)
   {
    var func,agent$1;
    function a$1(ok,ko)
    {
     var isOpen;
     isOpen=flush(function(a$2)
     {
      agent$1.mailbox.AddLast(a$2);
      agent$1.resume();
     });
     socket.onopen=function()
     {
      agent$1.mailbox.AddLast(Message.Open);
      agent$1.resume();
      return ok(server);
     };
     socket.onclose=function()
     {
      agent$1.mailbox.AddLast(Message.Close);
      return agent$1.resume();
     };
     socket.onmessage=function(msg)
     {
      agent$1.mailbox.AddLast({
       $:0,
       $0:decode$1(msg)
      });
      return agent$1.resume();
     };
     socket.onerror=function()
     {
      agent$1.mailbox.AddLast(Message.Error);
      agent$1.resume();
      return ko(new Global.Error("Could not connect to the server."));
     };
     isOpen?ok(server):void 0;
    }
    func=a[1];
    agent$1=Async.FoldAgent(a[0],function($1,$2)
    {
     return(func($1))($2);
    });
    return Concurrency.FromContinuations(function($1,$2,$3)
    {
     return a$1.apply(null,[$1,$2,$3]);
    });
   });
  });
 };
 Client.getEncoding=function(encode,decode,jsonEncoding)
 {
  var p,decode$1;
  function f(a)
  {
   return JSON.parse(a);
  }
  p=jsonEncoding.$==0?[function(a)
  {
   return JSON.stringify(a);
  },function(x)
  {
   return Json.Activate(f(x));
  }]:[encode,decode];
  decode$1=p[1];
  return[p[0],function(msg)
  {
   return decode$1(msg.data);
  }];
 };
 Client.cacheSocket=function(socket,decode)
 {
  var cache,isOpen;
  cache=[];
  isOpen=[false];
  socket.onopen=function()
  {
   cache.push(Message.Open);
   isOpen[0]=true;
  };
  socket.onclose=function()
  {
   return cache.push(Message.Close);
  };
  socket.onmessage=function(msg)
  {
   return cache.push({
    $:0,
    $0:decode(msg)
   });
  };
  socket.onerror=function()
  {
   return cache.push(Message.Error);
  };
  return function(post)
  {
   Seq.iter(post,cache);
   return isOpen[0];
  };
 };
}(self));
