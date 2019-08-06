var username = null;
var userColour = null;

/**
 *	{
 *		Board: {
 *			Cards: [ "card1", "card2", ... ],
 *			Colours: [ ["#ABCDEF", "#ABCDEF", "#ABCDEF", "#ABCDEF"], [...], ... ]
 *		},
 *		RoomID: "XYZ",
 *		Settings: {
 *			CardsList: [ "card1", "card2", ... ]
 *		},
 *		Users: [
 *			{
 *				Key: "connectionID",
 *				Value: {
 *					Colour: "#ABCDEF",
 *					Username: "username"
 *				}
 *			},
 *			{ ... }
 *		]
 *	}
 */

var room = null; // Users not kept updated

const connection = new signalR.HubConnectionBuilder()
	.withUrl("/bingoHub")
	.build();

connection.on("ReceiveError", function (errorMessage) {
	$("#bingo-login-error").text(errorMessage).show();
});

connection.on("ReceiveBoardUpdate", function (tileID, userColour) {
	updateBoard(tileID, userColour);
});

connection.on("ReceiveFirstJoinRoom", function (roomJSON, assignedColour) {
	room = JSON.parse(roomJSON);

	userColour = assignedColour;

	$("#room-code").text(room.RoomID);

	// remove all table rows other than header
	$("#room-players tbody").find("tr:gt(0)").remove();
	// add users
	for (var i = 0; i < room.Users.length; i++) {
		var newRow = "<tr style='background-color: " + room.Users[i].Value.Colour + "'><td>" + room.Users[i].Value.Username + "</td></tr>";
		$("#room-players tbody").append(newRow);
	}

	// show room settings
	var cardsStr = room.Settings.CardsList.join("\n");
	$("#room-cards-list").val(cardsStr);

	$("#bingo-login").hide();
	$("#bingo-login-error").hide();
	$("#bingo-game").show();

	renderBoard();
	textfill(30);
});

// add user to list when they join
connection.on("ReceiveOtherJoinRoom", function (playerUsername, playerColour) {
	var newRow = "<tr style='background-color: " + playerColour + "'><td>" + playerUsername + "</td></tr>";
	$("#room-players tbody").append(newRow);
});

connection.on("ReceiveUpdateRoomSettings", function (roomSettingsJSON) {
	console.log(roomSettingsJSON);
	var roomSettings = JSON.parse(roomSettingsJSON);
	var cardsStr = roomSettings.CardsList.join("\n");
	$("#room-cards-list").val(cardsStr);
});

// remove user from list when they leave
connection.on("ReceiveLeaveRoom", function (playerUsername) {
	$("#room-players tr td").each(function () {
		if ($(this).text() == playerUsername) {
			$(this).parent().remove();
		}
	});
});

connection.on("ReceiveResetBoard", function (boardJSON) {
	room.Board = JSON.parse(boardJSON);
	renderBoard();
	textfill(30);
});

// We need an async function in order to use await, but we want this code to run immediately,
// so we use an "immediately-executed async function"
(async () => {
    try {
        await connection.start();
    }
    catch (e) {
        console.error(e.toString());
    }
})();

jQuery(document).ready(function ($) {
	// pressing enter submits
	$("#username-input").keypress(function (e) {
		if (e.which == 13) {
			$("#bingo-login-submit").click();
			return false;
		}
	});

	$("#room-code-input").keypress(function (e) {
		if (e.which == 13) {
			$("#bingo-login-submit").click();
			return false;
		}
	});

	$("#bingo-login-submit").click(function () {
		var username = $.trim($("#username-input").val());
		var roomID = $.trim($("#room-code-input").val());
		// new room
		if (roomID == "") {
			connection.invoke("SendCreateRoom", username).catch(function (err) {
				console.error(err.toString());
			});
		}
		// join room
		else {
			connection.invoke("SendJoinRoom", username, roomID).catch(function (err) {
				console.error(err.toString());
			});
		}
	});

	$(".bingo-text").click(function () {
		if (userColour !== "" && userColour !== null /* && roomCode !== null*/) {
			var tileID = $(this).parent().attr('id');
			connection.invoke("SendBoardUpdate", room.RoomID, tileID, userColour).catch(function (err) {
				console.error(err.toString());
			});
		}
		else {
			console.log("User colour null");
		}
	});

	$("#room-confirm-settings").click(function () {
		var cardsList = $("#room-cards-list").val();
		var cardsArr = cardsList.split("\n");

		if (cardsArr.length >= 25) {
			var msg = {
				CardsList: cardsArr
			};

			connection.invoke("SendUpdateRoomSettings", room.RoomID, JSON.stringify(msg)).catch(function (err) {
				console.error(err.toString());
			});
		}
		else {
			alert("Not enough cards entered, 25 minimum");
		}
	});

	$("#room-new-game").click(function () {
		connection.invoke("SendResetBoard", room.RoomID).catch(function (err) {
			console.error(err.toString());
		});
	});


});

function updateBoard(tileID, colour) {
	var square = parseInt(tileID);
	var index = room.Board.Colours[square].indexOf(colour);
	if (index > -1) {
		room.Board.Colours[square].splice(index, 1);
	}
	else {
		room.Board.Colours[square].push(colour);
	}
	renderBoard();
}

function renderBoard() {
	for (var i = 0; i < 25; i++) {
		$("#" + i + " > .bingo-text > span").text(room.Board.Cards[i]);
		setColours(i, room.Board.Colours[i]);
	}
}

// sets colour(s) for a tile. Colours param should be array of hex colours, with length 0-4
function setColours(tileID, colours) {
	$("#" + tileID + " > .bingo-tile").removeClass("two-colours").removeClass("three-colours").removeClass("four-colours");
	$("#" + tileID + " > .bingo-tile").attr('style', '');
	switch (colours.length) {
		case 1:
			$("#" + tileID + " > .bingo-tile").css("background-color", colours[0]);
			break;
		case 2:
			$("#" + tileID + " > .bingo-tile").addClass("two-colours").css("background-color", colours[0]);
			$("#" + tileID + " > .bingo-tile div:first-child").css("background-color", colours[1]);
			break;
		case 3:
			$("#" + tileID + " > .bingo-tile").addClass("three-colours").css("background-color", colours[0]);
			$("#" + tileID + " > .bingo-tile div:first-child").css("background-color", colours[1]);
			$("#" + tileID + " > .bingo-tile div:last-child").css("background-color", colours[2]);
			break;
		case 4:
			$("#" + tileID + " > .bingo-tile").addClass("four-colours");
			$("#" + tileID + " > .bingo-tile").css("border-top-color", colours[0]);
			$("#" + tileID + " > .bingo-tile").css("border-right-color", colours[1]);
			$("#" + tileID + " > .bingo-tile").css("border-bottom-color", colours[2]);
			$("#" + tileID + " > .bingo-tile").css("border-left-color", colours[3]);
			break;
	}
}

// incrementally decreases font size until text fits
function textfill(maxFontSize) {
	$(".bingo-text > span").each(function () {
		$(this).css("fontSize", maxFontSize);
		var parent = $(this).parent();
		var maxHeight = parent.height();
		var maxWidth = parent.width();
		var currentSize = maxFontSize;
		// inefficient but works
		while (currentSize > 0 && ($(this).outerWidth() > maxWidth || $(this).outerHeight() > maxHeight)) {
			//console.log("Reducing size from " + currentSize + " to " + (currentSize - 1) + " as " +
			//	$(this).outerWidth() + " > " + maxWidth + " or " + $(this).outerHeight() + " > " + maxHeight);
			currentSize--;
			$(this).css("fontSize", currentSize);
		}
	});
}