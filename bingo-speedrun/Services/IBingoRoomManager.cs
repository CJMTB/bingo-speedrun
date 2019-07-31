using BingoSpeedrun.Models;
using System.Collections.Generic;

namespace BingoSpeedrun.Services
{
    public interface IBingoRoomManager
    {
        // creates a room and returns the room ID
        string CreateRoom();

        BingoRoom GetRoom(string roomID);

        // adds user to room and returns user's assigned hex colour
        string AddToRoom(string connectionID, string roomID, string username);

        void RemoveFromRoom(string connectionID, string roomID, string username);

        Dictionary<string, BingoUser> GetRoomUsers(string roomID);
    }
}