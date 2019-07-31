using BingoSpeedrun.Models;
using BingoSpeedrun.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BingoSpeedrun.Services
{
    public class BingoRoomManager : IBingoRoomManager
    {
        private const int RoomIDLength = 3;

        public Dictionary<string, BingoRoom> Rooms = new Dictionary<string, BingoRoom>();

        public BingoRoomManager() {}
        
        // creates a room with a random ID, and returns the room ID
        public string CreateRoom()
        {
            string roomID = null;
            do
            {
                roomID = BingoUtils.RandomString(RoomIDLength);
            } while (Rooms.ContainsKey(roomID));

            Rooms.Add(roomID, new BingoRoom(roomID));

            return roomID;
        }

        public BingoRoom GetRoom(string roomID)
        {
            return Rooms[roomID];
        }

        public string AddToRoom(string connectionID, string roomID, string username)
        {
            return Rooms[roomID].AddUser(connectionID, username);
        }

        public void RemoveFromRoom(string connectionID, string roomID, string username)
        {
            Rooms[roomID].RemoveUser(connectionID, username);
            // delete room if no users
            if(Rooms[roomID].Users.Count == 0)
            {
                Rooms.Remove(roomID);
            }
        }

        public Dictionary<string, BingoUser> GetRoomUsers(string roomID)
        {
            return Rooms[roomID].Users;
        }

        
    }
}