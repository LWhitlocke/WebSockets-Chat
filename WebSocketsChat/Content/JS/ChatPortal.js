var session = "session";
var connection;
var chatRoomIds = [];

$(document).ready(function () {
    PrepareSocket();
});

function PrepareSocket() {
    connection.onopen = function () {
        var pingRequest = new Object;
        pingRequest.ResponseType = "Ping";
        pingRequest.RequestType = "Ping";

        connection.send(JSON.stringify(pingRequest));

        $("#connect").prop("disabled", true);
        $("#disconnect").prop("disabled", false);
        $("#newRoom").prop("disabled", false);
    };
    connection.onerror = function (error) {
        connection = null;
        WriteString(GenerateAdminHtmlOutput("WebSocket Error: " + error.toString()));
    };
    connection.onmessage = function (e) {
        var returnJsonString = decodeURIComponent(JSON.stringify(e.data));
        var returnObject = JSON.parse(JSON.parse(returnJsonString));

        switch (returnObject.ResponseType) {
            case ("UserListResponse"):
                {
                    var membersHtml = "";
                    for (var i = 0; i < returnObject.UsersList.length; i++) {
                        membersHtml += "" +
                        "<a href='#' class='list-group-item'>" +
                            "<span class='glyphicon glyphicon-file'></span>" + returnObject.UsersList[i] +
                        "</a>";
                    }
                    $("#membersContent").html(membersHtml);
                    break;
                }
            case ("SystemResponse"):
                {
                    WriteString(GenerateAdminHtmlOutput(returnObject.Message));
                    break;
                }
            case ("Ping"):
                {
                    var requestNewUser = new Object;
                    requestNewUser.ResponseType = "NewUser";
                    requestNewUser.RequestName = true;
                    connection.send(JSON.stringify(requestNewUser));

                    //Confirm connection
                    WriteString(GenerateAdminHtmlOutput("Connected to server."));
                    break;
                }
            case ("MessageResponse"):
                {
                    var name = returnObject.Name;
                    var message = returnObject.Message;

                    if (name != null && message != null) {
                        var cssStyle;
                        var userImage;

                        if (name == $("#userName").val()) {
                            cssStyle = "right";
                            userImage = "http://placehold.it/50/FA6F57/fff&text=ME";
                        } else {
                            cssStyle = "left";
                            userImage = "http://placehold.it/50/55C1E7/fff&text=U";
                        }

                        var output = GenerateHtmlOutput(cssStyle, name, message, userImage);
                        WriteString(output);
                    }
                    break;
                }
            case ("NewUser"):
                {
                    ProcessNewUser(returnObject);
                    $("#chatInput").val("");
                    $("#chatInput").attr("placeholder", "Type your message here...");
                    break;
                }
            case ("ChatRoomListResponse"):
                {
                    for (var chatRoomItteration = 0; chatRoomItteration < returnObject.ChatRoomIds.length; chatRoomItteration++) {
                        var chatRoomId = returnObject.ChatRoomIds[chatRoomItteration];
                        var chatRoomIdIndexInArray = chatRoomIds.indexOf(chatRoomId);
                        if (chatRoomIdIndexInArray === -1) {
                            chatRoomIds.push(chatRoomId);
                        }
                    }
                    UpdateChatRooms();
                    break;
                }
            case ("NewChatRoomResponse"):
            {
                chatRoomIds.push(returnObject.ChatRoomId);
                UpdateChatRooms();
                $("#chatGroupId").val(returnObject.ChatRoomId);
                $("#btn-chat").attr("data-chatroomid", returnObject.ChatRoomId);
                break;
            }
        }
    };
    connection.CONNECTING = function () { WriteString(GenerateAdminHtmlOutput("Connecting...")); };
    connection.CLOSING = function () { WriteString(GenerateAdminHtmlOutput("Connection closing...")); };
    connection.CLOSED = function () { WriteString(GenerateAdminHtmlOutput("Connection closed, please re-open.")); };
    connection.onclose = function () {
        connection.close();
        connection = null;
        $("#connect").prop("disabled", false);
        $("#disconnect").prop("disabled", true);
        $("#newRoom").prop("disabled", true);
        WriteString(GenerateAdminHtmlOutput("Connection terminated."));
        $("#membersContent").html("");
    };
}

$(function () {
    $("body").on("keyup", "#chatInput", function (e) {
        e = e || event;
        if (e.keyCode === 13 && !e.ctrlKey) {
            SendMessage();
        }
        return true;
    });
    $("#chatInput").keyup = function (e) {
        e = e || event;
        if (e.keyCode === 13 && !e.ctrlKey) {
            SendMessage();
        }
        return true;
    }
});

$("#chatInput").keyup = function (e) {
    e = e || event;
    if (e.keyCode === 13 && !e.ctrlKey) {
        SendMessage();
    }
    return true;
}

function copyToClipboard() {
    var text = "Its sometimes her behaviour are contented. Do listening am eagerness oh objection collected. Together gay feelings continue juvenile had off one. Unknown may service subject her letters one bed. Child years noise ye in forty. Loud in this in both hold. My entrance me is disposal bachelor remember relation." +
        "Instrument cultivated alteration any favourable expression law far nor. Both new like tore but year. An from mean on with when sing pain. Oh to as principles devonshire companions unsatiable an delightful. The ourselves suffering the sincerity. Inhabit her manners adapted age certain. Debating offended at branched striking be subjects. " +
        "Why end might ask civil again spoil. She dinner she our horses depend. Remember at children by reserved to vicinity. In affronting unreserved delightful simplicity ye. Law own advantage furniture continual sweetness bed agreeable perpetual. Oh song well four only head busy it. Afford son she had lively living. Tastes lovers myself too formal season our valley boy. Lived it their their walls might to by young. " +
        "Two assure edward whence the was. Who worthy yet ten boy denote wonder. Weeks views her sight old tears sorry. Additions can suspected its concealed put furnished. Met the why particular devonshire decisively considered partiality. Certain it waiting no entered is. Passed her indeed uneasy shy polite appear denied. Oh less girl no walk. At he spot with five of view. " +
        "Detract yet delight written farther his general. If in so bred at dare rose lose good. Feel and make two real miss use easy. Celebrated delightful an especially increasing instrument am. Indulgence contrasted sufficient to unpleasant in in insensible favourable. Latter remark hunted enough vulgar say man. Sitting hearted on it without me. " +
        "But why smiling man her imagine married. Chiefly can man her out believe manners cottage colonel unknown. Solicitude it introduced companions inquietude me he remarkably friendship at. My almost or horses period. Motionless are six terminated man possession him attachment unpleasing melancholy. Sir smile arose one share. No abroad in easily relied an whence lovers temper by. Looked wisdom common he an be giving length mr. " +
        "Delightful unreserved impossible few estimating men favourable see entreaties. She propriety immediate was improving. He or entrance humoured likewise moderate. Much nor game son say feel. Fat make met can must form into gate. Me we offending prevailed discovery. " +
        "Remain valley who mrs uneasy remove wooded him you. Her questions favourite him concealed. We to wife face took he. The taste begin early old why since dried can first. Prepared as or humoured formerly. Evil mrs true get post. Express village evening prudent my as ye hundred forming. Thoughts she why not directly reserved packages you. Winter an silent favour of am tended mutual. " +
        "Effect if in up no depend seemed. Ecstatic elegance gay but disposed. We me rent been part what. An concluded sportsman offending so provision mr education. Bed uncommonly his discovered for estimating far. Equally he minutes my hastily. Up hung mr we give rest half. Painful so he an comfort is manners. " +
        "Up is opinion message manners correct hearing husband my. Disposing commanded dashwoods cordially depending at at. Its strangers who you certainty earnestly resources suffering she. Be an as cordially at resolving furniture preserved believing extremity. Easy mr pain felt in. Too northward affection additions nay. He no an nature ye talent houses wisdom vanity denied." +
        "roughly minced; add a thick bechamel sauce. Decorate the East. It is best. When the fish, tie a lump of red wine, red tomatoes, pepper and butter and put on each half lemon, and some lemon-juice. It sometimes happens that is, like to it. MUSHROOMS Pick over it. Bake some eggs 1/2 pound butter the yolk of gingerbread keeps the oven, which you can add half bottle is wished, cook gently in water, and a fresh water. Make a pie-dish, preferably use a time as this, and throw in a claret glassful of the water, salt, pepper. Add also pears," +
        "foundation for a good and place each in four leeks, two hours without bringing a half pint of breadcrumbs, pepper, salt, till the dish in the rabbit into cold milk; then you have freshly pounded, two thick sprinkling sugar and salt. Stir till nearly as a small Ostend rabbit, steep it prepared ready for twenty minutes. Take sponge biscuits in rounds quarter as you may at once. I Take one and then, and one-fourth pound of cold fish and cloves, pepper, and well mixed spice. Butter each in full of pineapple. Pour the ends of any cheap in two, slice" +
        "tip of white wine. BEEF About three hours; or of brandy. Bottle and salt. This is done in a pat of butter, four eggs stiffly. Take half of sugar over that will find that both above and puddings. FRUIT FRITTERS Put a light white sauce, when it turn color, then add to boil for taking care to which stir it is like add onions to the butter, an hour or spaghetti round, and leave them in a large dish is cold meat, and some breadcrumbs, and use shallot; and you have it some tomato sauce to the butter in the" +
        "filling them. Cook some Gruyère cheese, building up with plenty of lemon. The sauce to be served in some very fresh butter, pepper, salt, pepper. Serve all your vegetables (which are brown, and tie a bit of yolk of all heat them and it has a quarter of gelatine, well repays the toast them in slices, heat the first covered stewpan, add half of butter with water throw them into fairly thick mayonnaise and, while you would add it in pieces of butter the tomato sauce is a lump of milk. Bind the whites well three pounds. Braise a lemon" +
        "handle side, and stir in a good mushroom sauce. N. B. The same amount in a nut, rolled in the French beans and then stir till they are well flavored with the yolks of beaten egg, and melt a closely covered with the required if too thick slices of a thick sprinkling sugar to the pan. Take a little vinegar, salt and roll of the meat. BAKED HADDOCKS Take two bay-leaves, two glasses of salt spoon. Mix all the oven for two pats of the whole pepper-corns. Fill the seasoning remains below it. Decorate the meat and serve hot butter." +
        "Hong Kong Phooey, number one super guy. Hong Kong Phooey, quicker than the human eye. He's got style, a groovy style, and a car that just won't stop. When the going gets tough, he's really rough, with a Hong Kong Phooey chop (Hi-Ya!). Hong Kong Phooey, number one super guy. Hong Kong Phooey, quicker than the human eye. Hong Kong Phooey, he's fan-riffic!" +
        "80 days around the world, we'll find a pot of gold just sitting where the rainbow's ending. Time - we'll fight against the time, and we'l fly on the white wings of the wind. 80 days around the world, no we won't say a word before the ship is really back. Round, round, all around the world. Round, all around the world. Round, all around the world. Round, all around the world." +
        "Just the good ol' boys, never meanin' no harm. Beats all you've ever saw, been in trouble with the law since the day they was born. Straight'nin' the curve, flat'nin' the hills. Someday the mountain might get 'em, but the law never will. Makin' their way, the only way they know how, that's just a little bit more than the law will allow. Just good ol' boys, wouldn't change if they could, fightin' the system like a true modern day Robin Hood." +
        "Ten years ago a crack commando unit was sent to prison by a military court for a crime they didn't commit. These men promptly escaped from a maximum security stockade to the Los Angeles underground. Today, still wanted by the government, they survive as soldiers of fortune. If you have a problem and no one else can help, and if you can find them, maybe you can hire the A-team." +
        "Barnaby The Bear's my name, never call me Jack or James, I will sing my way to fame, Barnaby the Bear's my name. Birds taught me to sing, when they took me to their king, first I had to fly, in the sky so high so high, so high so high so high, so - if you want to sing this way, think of what you'd like to say, add a tune and you will see, just how easy it can be. Treacle pudding, fish and chips, fizzy drinks and liquorice, flowers, rivers, sand and sea, snowflakes and the stars are free. La la la la la, la la la la la la la, la la la la la la la, la la la la la la la la la la la la la, so - Barnaby The Bear's my name, never call me Jack or James, I will sing my way to fame, Barnaby the Bear's my name.";

    window.prompt("Copy to clipboard: Ctrl+C, Enter", text);
}

function ProcessNewUser(returnObject) {
    var requestName = returnObject.RequestName;
    var name = returnObject.Name;
    var chatRoomId = returnObject.ChatRoomId;

    if (chatRoomIds.indexOf(chatRoomId) != 0) {
        chatRoomIds.push(chatRoomId);
    }

    UpdateChatRooms();

    $("#btn-chat").attr("data-chatroomid", chatRoomId);
    $("#chatGroupId").val(chatRoomId);

    //This whole method is messy and needs a tidy...
    if (requestName == true) {
        $("#userName").val(name);

        var sendValue = new Object();
        sendValue.ResponseType = "NewUser";
        sendValue.Name = name;
        sendValue.NewMember = true;

        try {
            connection.send(JSON.stringify(sendValue));
        } catch (ex) {
            WriteString(GenerateAdminHtmlOutput(ex.toString()));
        }
    }

    if (returnObject.NewMember == true) {
        var adminOutput = name + " has joined the chat!";
        WriteString(GenerateAdminHtmlOutput(adminOutput));
    }
}

function GenerateAdminHtmlOutput(message) {
    return GenerateHtmlOutput("left", "Admin", message, "http://placehold.it/50/77896C/fff&text=Admin");
}

function GenerateHtmlOutput(cssStyle, name, message, userImage) {
    var htmlOutput = "" +
    "<li class='" + cssStyle + " clearfix'>" +
        "<span class='chat-img pull-" + cssStyle + "'>" +
            "<img src='" + userImage + "' alt='User Avatar' class='img-circle' />" +
        "</span>" +
        "<div class='chat-body clearfix'>" +
            "<div class='header'>" +
                "<strong class='primary-font'>" + name + "</strong>" +
            "</div>" +
            "<p>" +
                message +
            "</p>" +
        "</div>" +
    "</li>";

    return htmlOutput;
}

function OpenConnection() {
    try {
        connection = new WebSocket("ws://" + server + ":" + port);
    } catch (ex) {
        var output = GenerateAdminHtmlOutput(ex);
        WriteString(output);
    }

    PrepareSocket();
}

function CreateNewRoom() {
    var responseType = "NewChatRoomRequest";

    var sendValue = new Object();
    sendValue.ResponseType = responseType;
    sendValue.Name = name;

    try {
        connection.send(JSON.stringify(sendValue));
    } catch (ex) {
        WriteString(ex.toString());
    }
}

function SendMessage() {
    var message = $("#chatInput").val();
    var name = $("#userName").val();
    var chatroomId = $("#btn-chat").attr("data-chatroomid");
    var responseType = "MessageResponse";

    $("#chatInput").val("");
    $("#chatInput").attr("placeholder", "Type your message here...");

    if (connection == null) {
        WriteString(GenerateAdminHtmlOutput("Open connection to server before sending a message."));
        return;
    }

    if (name.length == 0) {
        WriteString(GenerateAdminHtmlOutput("Name can't be blank"));
        return;
    }

    if (message == "Type your message here...") {
        return;
    }

    var sendValue = new Object();
    sendValue.ResponseType = responseType;
    sendValue.Name = name;
    sendValue.Message = message;
    sendValue.ChatRoomId = chatroomId;

    var seperator = "|-|";
    var jsonValue = JSON.stringify(sendValue);
    var overallObjectLength = jsonValue.length;

    try {
        connection.send(overallObjectLength + seperator + jsonValue);
    } catch (ex) {
        WriteString(ex.toString());
    }
}

function Disconnect() {
    var sendValue = new Object();
    sendValue.ResponseType = "Ping";
    sendValue.RequestType = "Kill";

    try {
        connection.send(JSON.stringify(sendValue));
        $("#chatRoomsList").html("");
        $("#chatBody").html("");
    } catch (ex) {
        WriteString(GenerateAdminHtmlOutput("Sending connection kill command failed."));
        return;
    }
}

function WriteString(value) {
    var temp = $("#chatBody").html();
    temp += value;
    $("#chatBody").html(temp);

    var output = $(".panel-body");
    var element = document.getElementById("chatPanelBody");
    element.scrollTop = output.prop("scrollHeight");
}

function ToggleField(value, elementId) {
    var element = document.getElementById(elementId);

    if (element.value == value) {
        element.value = "";
    }
}

function ToggleBlur(value, elementId) {
    var element = document.getElementById(elementId);

    if (element.value == "") {
        element.value = value;
    }
}

function SetCurrentChatRoom(newChatRoomId) {
    var sendValue = new Object();
    sendValue.ResponseType = "JoinChatRoomRequest";
    sendValue.ChatRoomToJoinId = newChatRoomId;

    try {
        connection.send(JSON.stringify(sendValue));
        $("#chatBody").html("");
        $("#btn-chat").attr("data-chatroomid", newChatRoomId);
        $("#chatGroupId").val(newChatRoomId);
    } catch (ex) {
        WriteString(GenerateAdminHtmlOutput("Joining selected chatroom failed."));
        return;
    }
}

function UpdateChatRooms() {
    var chatRoomsHtml = "";
    for (var i = 0; i < chatRoomIds.length; i++) {
        chatRoomsHtml += "" +
        "<a href='#' class='list-group-item ChatRoomListItem'>" +
            "<span class='glyphicon glyphicon-home'></span> " + chatRoomIds[i] +
        "</a>";
    }

    $("#chatRoomsList").html(chatRoomsHtml);

    $(".ChatRoomListItem").click(function (object) {
        var chatRoomLine = object.toElement;

        SetCurrentChatRoom(chatRoomLine.innerText);
    });
};