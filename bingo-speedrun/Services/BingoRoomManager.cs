using BingoSpeedrun.Models;
using BingoSpeedrun.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;

namespace BingoSpeedrun.Services
{
    public class BingoRoomManager
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

        public void RemoveFromRoom(string connectionID, string roomID)
        {
            Rooms[roomID].RemoveUser(connectionID);
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

        public string RoomToJSON(string roomID)
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BingoRoom));
            ser.WriteObject(stream, Rooms[roomID]);
            stream.Position = 0;
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}