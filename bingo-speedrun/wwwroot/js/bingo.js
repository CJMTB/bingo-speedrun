/*
 * FINAL COLOUR LIST
 * var colours = ["CC0000", "9500E0", "00BF00", "008FE8"];
 * 
 * NICE LOOKING COLOURS
 * var colours = ["3c1f55", "ff6223", "a53e5a", "fecc74"];
 * 
 * 
 */

var board = [];
var username = null;
var userColour = null;
var roomCode = null;

for (var i = 0; i < 25; i++) {
	board[i] = {
		colours: [],
		card: ""
	};
}

const connection = new signalR.HubConnectionBuilder()
	.withUrl("/bingoHub")
	.build();

connection.on("ReceiveError", function (errorMessage) {
	$("#bingo-login-error").text(errorMessage).show();
});

connection.on("ReceiveBoardUpdate", function (tileID, userColour) {
	updateBoard(tileID, userColour);
});

connection.on("ReceiveFirstJoinRoom", function (roomID, assignedColour, boardJSON, usersJSON) {
	roomCode = roomID;
	userColour = assignedColour;
	board = JSON.parse(boardJSON);
	var users = JSON.parse(usersJSON);

	$("#room-code").text(roomCode);

	// remove all table rows other than header
	$("#room-players tbody").find("tr:gt(0)").remove();
	// add users
	for (var i = 0; i < users.length; i++) {
		var newRow = "<tr style='background-color: " + users[i].colour + "'><td>" + users[i].username + "</td></tr>";
		$("#room-players tbody").append(newRow);
	}

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

// remove user from list when they leave
connection.on("ReceiveLeaveRoom", function (playerUsername) {
	$("#room-players tr td").each(function () {
		if ($(this).text() == playerUsername) {
			$(this).parent().remove();
		}
	});
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
			connection.invoke("SendBoardUpdate", roomCode, tileID, userColour).catch(function (err) {
				console.error(err.toString());
			});
		}
		else {
			console.log("User colour null");
		}
	});


});

function updateBoard(tileID, colour) {
	var square = parseInt(tileID);
	var index = board[square].colours.indexOf(colour);
	if (index > -1) {
		board[square].colours.splice(index, 1);
	}
	else {
		board[square].colours.push(colour);
	}
	renderBoard();
}

function renderBoard() {
	for (var i = 0; i < 25; i++) {
		$("#" + i + " > .card > span").text(board[i].card);
		setColours(i, board[i].colours);
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