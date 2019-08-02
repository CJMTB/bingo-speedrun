using BingoSpeedrun.Models;
using BingoSpeedrun.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

/* TODO:
 * Room settings
 * Cards
 * Validate user input (eg no blank username)
 */

namespace BingoSpeedrun.Hubs
{

    public class BingoHub : Hub
    {
        private BingoRoomManager _roomManager;

        public BingoHub(BingoRoomManager roomManager) {
            _roomManager = roomManager;
        }

        public async Task SendCreateRoom(string username)
        {
            string roomID = _roomManager.CreateRoom();
            string hexColour = _roomManager.AddToRoom(Context.ConnectionId, roomID, username);
            string boardJSON = _roomManager.GetRoom(roomID).Board.ToJSON();
            string usersJSON = _roomManager.GetRoom(roomID).UsersToJSON();
            await Clients.Caller.SendAsync("ReceiveFirstJoinRoom", roomID, hexColour, boardJSON, usersJSON);
        }

        public async Task SendJoinRoom(string username, string roomID)
        {
            try
            {
                string hexColour = _roomManager.AddToRoom(Context.ConnectionId, roomID, username);
                string boardJSON = _roomManager.GetRoom(roomID).Board.ToJSON();
                string usersJSON = _roomManager.GetRoom(roomID).UsersToJSON();
                await Clients.Caller.SendAsync("ReceiveFirstJoinRoom", roomID, hexColour, boardJSON, usersJSON);

                Dictionary<string, BingoUser> usersInRoom = _roomManager.GetRoomUsers(roomID);
                foreach (KeyValuePair<string, BingoUser> entry in usersInRoom)
                {
                    if (Context.ConnectionId != entry.Key)
                    {
                        await Clients.Client(entry.Key).SendAsync("ReceiveOtherJoinRoom", username, hexColour);
                    }
                }
            }
            catch(KeyNotFoundException)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Room " + roomID + " not found");
            }
        }

        public async Task SendBoardUpdate(string roomID, string tileID, string userColour)
        {
            _roomManager.GetRoom(roomID).Board.Update(Int32.Parse(tileID), userColour);

            Dictionary<string, BingoUser> usersInRoom = _roomManager.GetRoomUsers(roomID);
            foreach (KeyValuePair<string, BingoUser> entry in usersInRoom)
            {
                await Clients.Client(entry.Key).SendAsync("ReceiveBoardUpdate", tileID, userColour);
            }
            
        }

        // Remove user from room.
        // Send all other users a message.
        // Delete room if no users left.
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            foreach (KeyValuePair<string, BingoRoom> room in _roomManager.Rooms)
            {
                if (room.Value.Users.ContainsKey(Context.ConnectionId))
                {
                    // room found

                    string roomID = room.Key;
                    string username = room.Value.Users[Context.ConnectionId].Username;

                    _roomManager.Rooms[roomID].RemoveUser(Context.ConnectionId);

                    if(_roomManager.Rooms[roomID].Users.Count == 0)
                    {
                        _roomManager.Rooms.Remove(roomID);
                    }
                    else
                    {
                        Dictionary<string, BingoUser> usersInRoom = _roomManager.GetRoomUsers(roomID);
                        foreach (KeyValuePair<string, BingoUser> users in usersInRoom)
                        {
                            await Clients.Client(users.Key).SendAsync("ReceiveLeaveRoom", username);

                        }
                    }
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
