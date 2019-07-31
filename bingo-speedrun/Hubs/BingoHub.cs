using BingoSpeedrun.Models;
using BingoSpeedrun.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

/* TODO:
 * Remove user when they close website
 * Room settings
 * Cards
 */

namespace BingoSpeedrun.Hubs
{

    public class BingoHub : Hub
    {
        private IBingoRoomManager _roomManager;

        public BingoHub(IBingoRoomManager roomManager) {
            _roomManager = roomManager;
        }

        public async Task SendCreateRoom(string username)
        {
            string roomID = _roomManager.CreateRoom();
            string hexColour = _roomManager.AddToRoom(Context.ConnectionId, roomID, username);
            string boardJSON = _roomManager.GetRoom(roomID).Board.ToJSON();
            string usersJSON = _roomManager.GetRoom(roomID).UsersToJSON();
            Debug.WriteLine("HEX COLOR: " + hexColour);
            Debug.WriteLine("USERS: " + usersJSON);
            await Clients.Caller.SendAsync("ReceiveFirstJoinRoom", roomID, hexColour, boardJSON, usersJSON);
        }

        public async Task SendJoinRoom(string username, string roomID)
        {
            string hexColour = _roomManager.AddToRoom(Context.ConnectionId, roomID, username);
            string boardJSON = _roomManager.GetRoom(roomID).Board.ToJSON();
            string usersJSON = _roomManager.GetRoom(roomID).UsersToJSON();
            Debug.WriteLine(usersJSON);
            await Clients.Caller.SendAsync("ReceiveFirstJoinRoom", roomID, hexColour, boardJSON, usersJSON);

            Dictionary<string, BingoUser> usersInRoom = _roomManager.GetRoomUsers(roomID);
            foreach (KeyValuePair<string, BingoUser> entry in usersInRoom)
            {
                if(Context.ConnectionId != entry.Key)
                {
                    await Clients.Client(entry.Key).SendAsync("ReceiveOtherJoinRoom", username, hexColour);
                }
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
    }
}
