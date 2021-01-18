(function(Global)
{
 "use strict";
 var FAkka,Shared,UI,Server,Name,User,Client,JHelper,SC$1,ClientServer_JsonDecoder,ClientServer_JsonEncoder,GeneratedPrintf,WebSharper,Unchecked,JavaScript,Pervasives,Html,Client$1,Tags,Concurrency,$,Utils,AspNetCore,WebSocket,Client$2,WithEncoding,JSON,IntelliFactory,Runtime,Remoting,AjaxRemotingProvider,UI$1,Var$1,Strings,Arrays,Submitter,View,Seq,MatchFailureException,Doc,AttrProxy,Client$3,Templates,Date,ClientSideJson,Provider;
 FAkka=Global.FAkka=Global.FAkka||{};
 Shared=FAkka.Shared=FAkka.Shared||{};
 UI=Shared.UI=Shared.UI||{};
 Server=UI.Server=UI.Server||{};
 Name=Server.Name=Server.Name||{};
 User=Server.User=Server.User||{};
 Client=UI.Client=UI.Client||{};
 JHelper=Client.JHelper=Client.JHelper||{};
 SC$1=Global.StartupCode$ClientServer$Client=Global.StartupCode$ClientServer$Client||{};
 ClientServer_JsonDecoder=Global.ClientServer_JsonDecoder=Global.ClientServer_JsonDecoder||{};
 ClientServer_JsonEncoder=Global.ClientServer_JsonEncoder=Global.ClientServer_JsonEncoder||{};
 GeneratedPrintf=Global.GeneratedPrintf=Global.GeneratedPrintf||{};
 WebSharper=Global.WebSharper;
 Unchecked=WebSharper&&WebSharper.Unchecked;
 JavaScript=WebSharper&&WebSharper.JavaScript;
 Pervasives=JavaScript&&JavaScript.Pervasives;
 Html=WebSharper&&WebSharper.Html;
 Client$1=Html&&Html.Client;
 Tags=Client$1&&Client$1.Tags;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 $=Global.jQuery;
 Utils=WebSharper&&WebSharper.Utils;
 AspNetCore=WebSharper&&WebSharper.AspNetCore;
 WebSocket=AspNetCore&&AspNetCore.WebSocket;
 Client$2=WebSocket&&WebSocket.Client;
 WithEncoding=Client$2&&Client$2.WithEncoding;
 JSON=Global.JSON;
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 UI$1=WebSharper&&WebSharper.UI;
 Var$1=UI$1&&UI$1.Var$1;
 Strings=WebSharper&&WebSharper.Strings;
 Arrays=WebSharper&&WebSharper.Arrays;
 Submitter=UI$1&&UI$1.Submitter;
 View=UI$1&&UI$1.View;
 Seq=WebSharper&&WebSharper.Seq;
 MatchFailureException=WebSharper&&WebSharper.MatchFailureException;
 Doc=UI$1&&UI$1.Doc;
 AttrProxy=UI$1&&UI$1.AttrProxy;
 Client$3=UI$1&&UI$1.Client;
 Templates=Client$3&&Client$3.Templates;
 Date=Global.Date;
 ClientSideJson=WebSharper&&WebSharper.ClientSideJson;
 Provider=ClientSideJson&&ClientSideJson.Provider;
 Name.New=function(FirstName,LastName)
 {
  return{
   "first-name":FirstName,
   LastName:LastName
  };
 };
 User.New=function(name,age)
 {
  return{
   name:name,
   age:age
  };
 };
 JHelper.append=function(o,key,arr)
 {
  o[key]=JHelper.add((Unchecked.Equals(Pervasives.GetJS(o,[key]),void 0)?o[key]=[]:void 0,o[key]),arr);
  return o;
 };
 JHelper.add=function(l,r)
 {
  return l.concat(r);
 };
 Client.WS=function(endpoint)
 {
  var container,b;
  function writen(fmt)
  {
   return fmt(function(s)
   {
    container.Dom.appendChild(self.document.createTextNode(s+"\n"));
   });
  }
  container=Tags.Tags().NewTag("pre",[]);
  Concurrency.Start((b=null,Concurrency.Delay(function()
  {
   var r;
   writen(function($1)
   {
    return $1("Checking regression #4...");
   });
   $.ajax((r={},r.url="/ws.txt",r.method="GET",r.success=function(x)
   {
    return(writen(function($1)
    {
     return function($2)
     {
      return $1(Utils.toSafe($2));
     };
    }))(x);
   },r.error=function(a,a$1,e)
   {
    return(writen(function($1)
    {
     return function($2)
     {
      return $1("KO: "+Utils.toSafe($2)+".");
     };
    }))(e);
   },r));
   return Concurrency.Bind(WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((ClientServer_JsonEncoder.j())(a));
   },function(a)
   {
    return(ClientServer_JsonDecoder.j())(JSON.parse(a));
   },endpoint,function()
   {
    var b$1;
    b$1=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$2;
       b$2=null;
       return Concurrency.Delay(function()
       {
        var data;
        return msg.$==3?(writen(function($1)
        {
         return $1("WebSocket connection closed.");
        }),Concurrency.Return(state)):msg.$==2?(writen(function($1)
        {
         return $1("WebSocket connection open.");
        }),Concurrency.Return(state)):msg.$==1?(writen(function($1)
        {
         return $1("WebSocket connection error!");
        }),Concurrency.Return(state)):(data=msg.$0,Concurrency.Combine(data.$==0?(((writen(Runtime.Curried3(function($1,$2,$3)
        {
         return $1("Response2 "+Global.String($2)+" (state: "+Global.String($3)+")");
        })))(data.$0))(state),Concurrency.Zero()):data.$==2?((writen(function($1)
        {
         return function($2)
         {
          return $1("Resp3 "+GeneratedPrintf.p($2));
         };
        }))(data.$0),Concurrency.Zero()):data.$==3?((writen(function($1)
        {
         return function($2)
         {
          return $1("MessageFromServer_String "+Utils.prettyPrint($2));
         };
        }))(data.$0),Concurrency.Zero()):(((writen(Runtime.Curried3(function($1,$2,$3)
        {
         return $1("Response1 "+Utils.toSafe($2)+" (state: "+Global.String($3)+")");
        })))(data.$0))(state),Concurrency.Zero()),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        })));
       });
      };
     }]);
    });
   }),function(a)
   {
    var conn;
    conn=a.get_Connection();
    return Concurrency.While(function()
    {
     return true;
    },Concurrency.Delay(function()
    {
     return Concurrency.Bind(Concurrency.Sleep(1000),function()
     {
      conn.send(JSON.stringify((ClientServer_JsonEncoder.j())({
       $:2,
       $0:User.New(Name.New("John00","Doe"),42)
      })));
      return Concurrency.Zero();
     });
    }));
   });
  })),null);
  return container;
 };
 Client.fsiCmd$268$26=Runtime.Curried3(function(filterResultFlattened,$1,$2)
 {
  return $("#filteredResult").val(filterResultFlattened.Get());
 });
 Client.fsiCmd=function(urlStr)
 {
  var inputPath,outFile,ifReadAllMode,f1,f2,sl1,sl2,p1,p2,dtStart,dtEnd,rvInput,rvHisCmd,filterResultFlattened,nScript,singleCommand,webSocket2,curPos,submit,submitSingle,hisCmd,vReversed,filterBox;
  function submitFun(a)
  {
   var input,b;
   return a!=null&&a.$==1?(input=a.$0,(rvHisCmd.Set(rvHisCmd.Get().concat([input])),curPos.Set(curPos.Get()+1),(new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.fsiExecute:1074071197",[input]))):(b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.fsiSeq:-71915202",[]),function(a$1)
    {
     return Concurrency.Return(["",a$1]);
    });
   }));
  }
  inputPath=Var$1.Create$1("v:\\");
  outFile=Var$1.Create$1("v:\\result.csv");
  ifReadAllMode=Var$1.Create$1(false);
  f1=Var$1.Create$1("5");
  f2=Var$1.Create$1("6");
  sl1=Var$1.Create$1("8");
  sl2=Var$1.Create$1("9");
  p1=Var$1.Create$1("3");
  p2=Var$1.Create$1("4");
  dtStart=Var$1.Create$1("20200712");
  dtEnd=Var$1.Create$1("20200720");
  rvInput=Var$1.Create$1("");
  rvHisCmd=Var$1.Create$1([]);
  filterResultFlattened=Var$1.Lens(Client.filterResult(),function(arr)
  {
   var reg;
   reg=new Global.RegExp(Client.filterKeyWord().Get());
   return Strings.concat("",Arrays.filter(function(s)
   {
    return reg.test(s);
   },arr));
  },function(n,s)
  {
   return n.concat([s]);
  });
  nScript=Var$1.Create$1("named script");
  singleCommand=Var$1.Create$1("");
  webSocket2=Var$1.Create$1(urlStr);
  curPos=Var$1.Create$1(0);
  submit=Submitter.CreateOption(rvInput.get_View());
  submitSingle=Submitter.CreateOption(singleCommand.get_View());
  hisCmd=Submitter.CreateOption(rvHisCmd.get_View());
  vReversed=View.Map(function(strSeq)
  {
   var p,p$1;
   p=Seq.nth(0,strSeq);
   p$1=Seq.nth(1,strSeq);
   return p[1]>p$1[1]?p[0]:p$1[0];
  },View.Sequence([View.MapAsync(submitFun,submit.view),View.MapAsync(submitFun,submitSingle.view)]));
  View.MapAsync(function(a)
  {
   var v,b;
   if(a!=null&&a.$==1)
   {
    if(Arrays.length(a.$0)===0)
     return(new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.getHisCmds:-1916767346",[]);
    else
     if(a!=null&&a.$==1)
      {
       v=a.$0;
       b=null;
       return Concurrency.Delay(function()
       {
        return Concurrency.Return(v);
       });
      }
     else
      throw new MatchFailureException.New("Client.fs",253,33);
   }
   else
    return(new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.getHisCmds:-1916767346",[]);
  },hisCmd.view);
  filterBox=Doc.Input([AttrProxy.Create("id","fKW"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","1")],Client.filterKeyWord()).on("blur",function()
  {
   return $("#filteredResult").val(filterResultFlattened.Get());
  });
  return Doc.Element("div",[],[Doc.Element("div",[],[Doc.Button("Send",[],function()
  {
   submit.Trigger();
  }),Doc.Button("SendSingle",[],function()
  {
   submitSingle.Trigger();
  }),Doc.Button("Clear Console",[],function()
  {
   $("#console").empty();
  }),Doc.Button("Clear Command",[],function()
  {
   $("#fsiCmd").val("");
  }),Doc.Button("Last Command",[],function()
  {
   var b;
   Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.getHisCmd:2048691399",[]),function(a)
    {
     $("#fsiCmd").val(a);
     return Concurrency.Zero();
    });
   })),null);
  }),Doc.Button("Previous Command",[],function()
  {
   var b;
   Arrays.length(rvHisCmd.Get())===0?Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.getHisCmds:-1916767346",[]),function(a)
    {
     rvHisCmd.Set(a);
     return Concurrency.Combine(Arrays.length(rvHisCmd.Get())>0?(curPos.Set(Arrays.length(rvHisCmd.Get())-1),Concurrency.Zero()):Concurrency.Zero(),Concurrency.Delay(function()
     {
      rvInput.Set(Arrays.get(rvHisCmd.Get(),curPos.Get()));
      return Concurrency.Zero();
     }));
    });
   })),null):(curPos.Get()===0?curPos.Set(Arrays.length(rvHisCmd.Get())-1):curPos.Set(curPos.Get()-1),rvInput.Set(Arrays.get(rvHisCmd.Get(),curPos.Get())));
   hisCmd.Trigger();
  }),Doc.Button("Get Script",[],function()
  {
   var b;
   Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.getNamedScript:1890332917",[Global.String($("#nScript").val())]),function(a)
    {
     $("#fsiCmd").val(a);
     rvInput.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  }),Doc.Button("Save Script",[],function()
  {
   var b;
   Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.upsertNamedScript:60259177",[Global.String($("#nScript").val()),Global.String($("#fsiCmd").val())]),function(a)
    {
     $("#fsiCmd").val(a);
     return Concurrency.Zero();
    });
   })),null);
  }),Doc.Button("List Script",[],function()
  {
   var b;
   Concurrency.Start((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.listNamedScripts:-1916767346",[]),function(a)
    {
     var s;
     s=Arrays.fold(function(str,item)
     {
      return str!==""?str+"\r\n"+item:item;
     },"",a);
     $("#nScript").val("");
     $("#fsiCmd").val(s);
     return Concurrency.Zero();
    });
   })),null);
  }),Doc.Button("Clear Result Cache",[],function()
  {
   Client.filterResult().Set([]);
  }),Doc.Element("br",[],[]),Doc.Button("ConnectTo",[],function()
  {
   Concurrency.Start(Client.Send3(Global.String($("#webSocket2").val())),null);
  }),Doc.Input([AttrProxy.Create("id","fast1"),AttrProxy.Create("style","width: 50px")],f1),Doc.Input([AttrProxy.Create("id","fast2"),AttrProxy.Create("style","width: 50px")],f2),Doc.Input([AttrProxy.Create("id","slow1"),AttrProxy.Create("style","width: 50px")],sl1),Doc.Input([AttrProxy.Create("id","slow2"),AttrProxy.Create("style","width: 50px")],sl2),Doc.Input([AttrProxy.Create("id","period1"),AttrProxy.Create("style","width: 50px")],p1),Doc.Input([AttrProxy.Create("id","period2"),AttrProxy.Create("style","width: 50px")],p2),Doc.Element("br",[],[]),Doc.Element("div",[AttrProxy.Create("style","display:inline-block")],[Doc.Element("div",[AttrProxy.Create("style","display:inline-block")],[Doc.TextNode("folder of input files: ")]),Doc.Input([AttrProxy.Create("id","ipp"),AttrProxy.Create("style","width: 440px; display:inline-block")],inputPath)]),Doc.Element("div",[],[Doc.Element("div",[AttrProxy.Create("style","display:inline-block")],[Doc.TextNode("output file: ")]),Doc.Input([AttrProxy.Create("id","opf"),AttrProxy.Create("style","width: 440px; display:inline-block")],outFile)]),Doc.CheckBox([AttrProxy.Create("id","ifReadAllMode")],ifReadAllMode),Doc.Button("RunColdFar",[],function()
  {
   var f1v,f2v,s1v,s2v,p1v,p2v,cmd;
   f1v=f1.Get();
   f2v=f2.Get();
   s1v=sl1.Get();
   s2v=sl2.Get();
   p1v=p1.Get();
   p2v=p2.Get();
   cmd="#r @\".\\dowSim002.exe\"\n"+"open dowSim002\n"+"open System.Reflection\n"+"main [| @\""+inputPath.Get()+"\"; \""+f1v+"\"; \""+f2v+"\"; \""+s1v+"\"; \""+s2v+"\"; \""+p1v+"\"; \""+p2v+"\"; @\""+outFile.Get()+"\""+(!ifReadAllMode.Get()?"":"; \""+dtStart.Get()+"\""+"; \""+dtEnd.Get()+"\"")+" |]";
   rvInput.Set(cmd);
   $("#fsiCmd").val(cmd);
  }),Doc.Element("br",[],[]),Doc.InputArea([AttrProxy.Create("id","webSocket2"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","1")],webSocket2),Doc.Element("br",[],[]),filterBox,Doc.InputArea([AttrProxy.Create("id","nScript"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","1")],nScript),Doc.InputArea([AttrProxy.Create("id","fsiCmd"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","10"),AttrProxy.Create("value","printfn \"orz\"")],rvInput),Doc.InputArea([AttrProxy.Create("id","singleCmd"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","1")],singleCommand)]),Doc.Element("hr",[],[]),Doc.InputArea([AttrProxy.Create("id","filteredResult"),AttrProxy.Create("style","width: 880px"),AttrProxy.Create("class","input"),AttrProxy.Create("rows","10")],filterResultFlattened),Doc.Element("h4",[AttrProxy.Create("class","text-muted")],[Doc.TextNode("The server responded:")]),Doc.Element("div",[],[Doc.Element("h1",[],[Doc.TextView(vReversed)])])]);
 };
 Client.Send3=function(uri)
 {
  var b;
  !Unchecked.Equals(Client.socketServer().Get(),null)?Client.socketServer().Get().$0.get_Connection().close():void 0;
  b=null;
  return Concurrency.Delay(function()
  {
   var b$1,b$2;
   return Concurrency.Bind(Concurrency.Parallel([(b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.MD5Hash:1890332917",[uri]),function(a)
    {
     return Concurrency.Return({
      $:0,
      $0:a
     });
    });
   })),(b$2=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.getPort:929898232",[uri]),function(a)
    {
     return Concurrency.Return({
      $:1,
      $0:a
     });
    });
   }))]),function(a)
   {
    var $1,c,hostHash;
    if(!Unchecked.Equals(a,null)&&a.length===2&&(Arrays.get(a,0).$==0&&(Arrays.get(a,1).$==1&&($1=[Arrays.get(a,1).$0,Arrays.get(a,0).$0],true))))
     {
      c=$1[0];
      hostHash=$1[1];
      $("#console").empty().ready(function()
      {
       return Client.Send2(c,hostHash);
      });
      return Concurrency.Zero();
     }
    else
     throw new MatchFailureException.New("Client.fs",151,16);
   });
  });
 };
 Client.Send2=function(serverReceive,hostHash)
 {
  var b;
  function padding(s)
  {
   return Strings.PadLeftWith(Global.String(s),2,"0");
  }
  function writen(fmt)
  {
   return fmt(function(s)
   {
    var tr;
    Var$1.Set(Client.filterResult(),Client.filterResult().Get().concat([s+"\n"]));
    tr=Doc.TextNode(s+"\n");
    Templates.LoadLocalTemplates("");
    Doc.RunAppendById("console",tr);
   });
  }
  Concurrency.Start((b=null,Concurrency.Delay(function()
  {
   return Concurrency.Bind(WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((ClientServer_JsonEncoder.j())(a));
   },function(a)
   {
    return(ClientServer_JsonDecoder.j())(JSON.parse(a));
   },serverReceive,function()
   {
    var b$1;
    b$1=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$2;
       b$2=null;
       return Concurrency.Delay(function()
       {
        var data,jd;
        return msg.$==3?(writen(function($1)
        {
         return $1("WebSocket connection closed.");
        }),Concurrency.Return(state)):msg.$==2?(writen(function($1)
        {
         return $1("WebSocket connection open.");
        }),Concurrency.Return(state)):msg.$==1?(writen(function($1)
        {
         return $1("WebSocket connection error!");
        }),Concurrency.Return(state)):(data=msg.$0,Concurrency.Combine(data.$==3?(jd=Date.now(),(((((((writen(Runtime.Curried(function($1,$2,$3,$4,$5,$6,$7)
        {
         return $1("["+Utils.toSafe($2)+"]["+Utils.toSafe($3)+Utils.toSafe($4)+Utils.toSafe($5)+"]["+Global.String($6)+"] "+Utils.toSafe($7));
        },7)))(hostHash))(padding((new Date(jd)).getHours())))(padding((new Date(jd)).getMinutes())))(padding((new Date(jd)).getSeconds())))(state))(data.$0),Concurrency.Zero())):(writen(function($1)
        {
         return $1("invalidMessage");
        }),Concurrency.Zero()),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        })));
       });
      };
     }]);
    });
   }),function(a)
   {
    Client.socketServer().Set({
     $:1,
     $0:a
    });
    a.Post({
     $:3,
     $0:"kickOff"
    });
    return Concurrency.Zero();
   });
  })),null);
 };
 Client.socketServer=function()
 {
  SC$1.$cctor();
  return SC$1.socketServer;
 };
 Client.content=function()
 {
  SC$1.$cctor();
  return SC$1.content;
 };
 Client.filterKeyWord=function()
 {
  SC$1.$cctor();
  return SC$1.filterKeyWord;
 };
 Client.filterResult=function()
 {
  SC$1.$cctor();
  return SC$1.filterResult;
 };
 Client.m2=function()
 {
  var varTxt,vLength,vWords;
  varTxt=Var$1.Create$1("orz");
  vLength=View.Map(function(l)
  {
   return(function($1)
   {
    return function($2)
    {
     return $1("You entered "+Global.String($2)+" characters.");
    };
   }(Global.id))(l);
  },View.Map(Strings.length,varTxt.get_View()));
  vWords=Doc.BindView(function(words)
  {
   return Doc.Concat(Arrays.map(function(w)
   {
    return Doc.Element("li",[],[Doc.TextNode(w)]);
   },words));
  },View.Map(function(s)
  {
   return Strings.SplitChars(s,[" "],0);
  },varTxt.get_View()));
  return Doc.Element("div",[],[Doc.Element("div",[],[Doc.Input([],varTxt),Doc.TextView(vLength)]),Doc.Element("div",[],[Doc.TextNode("You entered the following words:"),Doc.Element("ul",[],[vWords])])]);
 };
 Client.Main=function()
 {
  var rvInput,submit,vReversed;
  rvInput=Var$1.Create$1("");
  submit=Submitter.CreateOption(rvInput.get_View());
  vReversed=View.MapAsync(function(a)
  {
   var b;
   return a!=null&&a.$==1?(new AjaxRemotingProvider.New()).Async("ClientServer:FAkka.Shared.UI.Server.DoSomething:1890332917",[a.$0]):(b=null,Concurrency.Delay(function()
   {
    return Concurrency.Return("");
   }));
  },submit.view);
  return Doc.Element("div",[],[Doc.Input([],rvInput),Doc.Button("Send",[],function()
  {
   submit.Trigger();
  }),Doc.Element("hr",[],[]),Doc.Element("h4",[AttrProxy.Create("class","text-muted")],[Doc.TextNode("The server responded:")]),Doc.Element("div",[AttrProxy.Create("class","jumbotron")],[Doc.Element("h1",[],[Doc.TextView(vReversed)])])]);
 };
 SC$1.$cctor=function()
 {
  SC$1.$cctor=Global.ignore;
  SC$1.filterResult=Var$1.Create$1([]);
  SC$1.filterKeyWord=Var$1.Create$1("");
  SC$1.content=Var$1.Create$1("");
  SC$1.socketServer=Var$1.Create$1(null);
 };
 ClientServer_JsonDecoder.j=function()
 {
  return ClientServer_JsonDecoder._v?ClientServer_JsonDecoder._v:ClientServer_JsonDecoder._v=(Provider.DecodeUnion(void 0,"type",[["int",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["name",[["$0","value",Provider.Id(),0]]],["msgStr",[["$0","value",Provider.Id(),0]]]]))();
 };
 ClientServer_JsonEncoder.j=function()
 {
  return ClientServer_JsonEncoder._v?ClientServer_JsonEncoder._v:ClientServer_JsonEncoder._v=(Provider.EncodeUnion(void 0,{
   cmd:3,
   age:2,
   "int":1,
   str:0
  },[["Request1",[["$0","str",Provider.EncodeArray(Provider.Id()),0]]],["Request2",[["$0","int",Provider.EncodeArray(Provider.Id()),0]]],["Req3",[[null,true,Provider.Id()]]],["MessageFromClient",[["$0","cmd",Provider.Id(),0]]]]))();
 };
 GeneratedPrintf.p=function($1)
 {
  return"{"+("FirstName = "+Utils.prettyPrint($1["first-name"]))+"; "+("LastName = "+Utils.prettyPrint($1.LastName))+"}";
 };
}(self));
