<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChatPortal.aspx.cs" Inherits="WebSocketsChat.ChatPortal" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Websocket chat</title>
    <link rel="shortcut icon" href="Content/Images/favicon.ico" />
    <link href="Content/CSS/ChatPortal.css" rel="stylesheet" />
    <link href="Content/bootstrap-theme.css" rel="stylesheet" />
    <link href="Content/bootstrap.css" rel="stylesheet" />
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js"></script>
    <script src="/Content/JS/ChatPortal.js"></script>
    <script type="text/javascript">
        var server = '<%=(ConfigurationManager.AppSettings["location"]) %>';
        var port = '<%=(ConfigurationManager.AppSettings["port"])%>';
    </script>
</head>
<body>
    <nav class="navbar navbar-inverse navbar-fixed-top">
        <div class="container">
            <div class="navbar-header">
                <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false" aria-controls="navbar">
                    <span class="sr-only">Toggle navigation</span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                </button>
                <a class="navbar-brand" href="#">Websocket Chat Client</a>
            </div>
            <div id="navbar" class="navbar-collapse collapse">
                <form class="navbar-form navbar-right">
                    <div class="form-group">
                        <input type="text" placeholder="Email" class="form-control" />
                    </div>
                    <div class="form-group">
                        <input type="password" placeholder="Password" class="form-control" />
                    </div>
                    <button type="submit" class="btn btn-success">Sign in</button>
                </form>
            </div>
        </div>
    </nav>
    <div class="jumbotron">
        <div class="container">
            <h1>Web Socket Chat Client</h1>
            <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla posuere volutpat turpis, id finibus libero. Pellentesque malesuada nulla libero, non vehicula est efficitur ac. Nam sit amet arcu lacinia, pharetra quam ac, faucibus ipsum. Mauris finibus consectetur massa, consequat molestie lectus egestas sit amet. Aliquam efficitur sollicitudin lectus, at dignissim lectus mattis a. Nullam posuere vel odio vel ultrices.</p>
        </div>
    </div>
    <div class="container">
        <div id="Menu" style="margin-bottom: 10px;">
            <input class="input-group" type="text" id="chatGroupId" readonly="readonly" style="float: left;" />
            |
            <input class="btn btn-success" type="button" id="connect" value="Open Connection" onclick=" OpenConnection(); " />
            |
            <input class="btn btn-danger" type="button" id="disconnect" value="Disconnect" onclick=" Disconnect(); " disabled="disabled" />
            |
            <input class="btn btn-danger" type="button" id="newRoom" value="Create new room" onclick=" CreateNewRoom(); " disabled="disabled" />

            <input class="btn btn-info" type="button" id="CopyText" value="Copy Text" onclick=" copyToClipboard()" style="float: right;" />
        </div>
        <div class="row">
            <div class="col-md-8">
                <div class="panel panel-primary">
                    <div class="panel-heading">
                        <span class="glyphicon glyphicon-comment"></span> Chat
                        <div class="btn-group pull-right">
                            <button type="button" class="btn btn-default btn-xs dropdown-toggle" data-toggle="dropdown-menu">
                                <span class="glyphicon glyphicon-chevron-down"></span>
                            </button>
                            <ul class="dropdown-menu slidedown">
                                <li><a href="#"><span class="glyphicon glyphicon-refresh"></span>Refresh</a></li>
                                <li><a href="#"><span class="glyphicon glyphicon-ok-sign"></span>Available</a></li>
                                <li><a href="#"><span class="glyphicon glyphicon-remove"></span>Busy</a></li>
                                <li><a href="#"><span class="glyphicon glyphicon-time"></span>Away</a></li>
                                <li class="divider"></li>
                                <li><a href="#"><span class="glyphicon glyphicon-off"></span>Sign Out</a></li>
                            </ul>
                        </div>
                    </div>
                    <div id="chatPanelBody" class="chatpanel-body">
                        <ul class="chat" id="chatBody">

                        </ul>
                    </div>
                    <div class="panel-footer">
                        <div class="form-group">
                            <div class="col-sm-2 text-left">
                                <input id="userName" type="text" class="form-control input-sm" disabled="disabled" value="name" />
                            </div>
                            <div class="col-sm-9">
                                <input id="chatInput" type="text" class="form-control input-sm" placeholder="Type your message here..." onkeydown="if (event.keyCode == 13) { SendMessage(); }" />
                            </div>
                            <div class="col-sm-1 text-right">
                                <span class="input-group-btn">
                                    <button class="btn btn-warning btn-sm" id="btn-chat" data-chatroomid="" onclick="SendMessage();">Send</button>
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div id="Members" class="panel panel-primary">
                    <div class="panel-heading">
                        Active Users
                    </div>
                    <div class="panel-body">
                        <div class="list-group"id="membersContent">
                            
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-md-4 pull-right">
                <div id="ChatRooms" class="panel panel-primary">
                    <div class="panel-heading">
                        Your Chat Rooms
                    </div>
                    <div class="panel-body">
                        <div class="list-group"id="chatRoomsList">
                            
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>