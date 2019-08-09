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
        
        /// <summary>
        /// Creates a room with a random three character ID, and returns its room ID.
        /// </summary>
        /// <returns>The ID of the newly created room.</returns>
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

        /// <summary>
        /// Returns the room with the given roomID, or null if it does not exist.
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns>The room with the given roomID, or null if it does not exist.</returns>
        public BingoRoom GetRoom(string roomID)
        {
            if (Rooms.ContainsKey(roomID))
            {
                return Rooms[roomID];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Adds a user to the room with the given ID and returns their assigned colour.
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="connectionID"></param>
        /// <param name="username"></param>
        /// <returns>The user's assigned colour.</returns>
        public string AddUserToRoom(string roomID, string connectionID, string username)
        {
            BingoRoom room = GetRoom(roomID);
            if(room != null)
            {
                return room.AddUser(connectionID, username);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Removes a user with the given connectionID from the room.
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="roomID"></param>
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