using BingoSpeedrun.Models;
using BingoSpeedrun.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/* TODO:
 * Validate user input (eg no blank username)
 * Custom JSON to remove connection ID
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
            string userColour = _roomManager.AddUserToRoom(roomID, Context.ConnectionId, username);
            await Clients.Caller.SendAsync("ReceiveFirstJoinRoom", _roomManager.RoomToJSON(roomID), userColour);
        }

        public async Task SendJoinRoom(string username, string roomID)
        {
            try
            {
                string userColour = _roomManager.AddUserToRoom(roomID, Context.ConnectionId, username);
                await Clients.Caller.SendAsync("ReceiveFirstJoinRoom", _roomManager.RoomToJSON(roomID), userColour);

                Dictionary<string, BingoUser> usersInRoom = _roomManager.GetRoomUsers(roomID);
                foreach (KeyValuePair<string, BingoUser> entry in usersInRoom)
                {
                    if (Context.ConnectionId != entry.Key)
                    {
                        await Clients.Client(entry.Key).SendAsync("ReceiveOtherJoinRoom", username, userColour);
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
            _roomManager.Rooms[roomID].Board.Update(Int32.Parse(tileID), userColour);

            Dictionary<string, BingoUser> usersInRoom = _roomManager.GetRoomUsers(roomID);
            foreach (KeyValuePair<string, BingoUser> entry in usersInRoom)
            {
                await Clients.Client(entry.Key).SendAsync("ReceiveBoardUpdate", tileID, userColour);
            }
        }

        public async Task SendUpdateRoomSettings(string roomID, string roomSettingsJSON)
        {
            BingoRoomSettings settings = new BingoRoomSettings();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(roomSettingsJSON));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(settings.GetType());
            settings = ser.ReadObject(stream) as BingoRoomSettings;
            stream.Close();

            _roomManager.GetRoom(roomID).Settings = settings;

            Dictionary<string, BingoUser> usersInRoom = _roomManager.GetRoomUsers(roomID);
            foreach (KeyValuePair<string, BingoUser> entry in usersInRoom)
            {
                await Clients.Client(entry.Key).SendAsync("ReceiveUpdateRoomSettings", roomSettingsJSON);
            }
        }

        public async Task SendResetBoard(string roomID)
        {
            BingoRoom room = _roomManager.GetRoom(roomID);
            room.ResetBoard();
            foreach (KeyValuePair<string, BingoUser> entry in room.Users)
            {
                await Clients.Client(entry.Key).SendAsync("ReceiveResetBoard", room.BoardToJSON());
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

                    room.Value.RemoveUser(Context.ConnectionId);

                    if(room.Value.Users.Count == 0)
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
