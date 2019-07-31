using BingoSpeedrun.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BingoSpeedrun.Models
{
    public class BingoRoom
    {
        public string RoomID { get; private set; }

        public BingoBoard Board { get; private set; }

        private List<string> _possibleHexColours = new List<string>()
        {
            "#CC0000", "#9500E0", "#00BF00", "#008FE8"
        };
        // connectionID -> BingoUser
        public Dictionary<string, BingoUser> Users = new Dictionary<string, BingoUser>();

        public BingoRoom(string roomID)
        {
            RoomID = roomID;
            Board = new BingoBoard();
        }

        // creates a BingoUser with random colour and adds it to this room
        // returns hex colour
        public string AddUser(string connectionID, string username)
        {
            string hexColour = BingoUtils.PickAndRemoveString(_possibleHexColours);
            Users.Add(connectionID, new BingoUser(connectionID, username, hexColour));
            return hexColour;
        }

        // removes user from room and adds their colour back to the pool
        public void RemoveUser(string connectionID, string username)
        {
            _possibleHexColours.Add(Users[username].HexColour);
            Users.Remove(connectionID);
        }

        /*
         *  [
         *      {
         *          username: "Connor",
         *          colour: "#ABCDEF"
         *      },
         *      { ... }
         *  ]
         */
        public string UsersToJSON()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("[");

            BingoUser[] userArray = Users.Values.ToArray();
            for(int i = 0; i < userArray.Length - 1; i++)
            {
                builder.Append("{\"username\":")
                    .Append(HttpUtility.JavaScriptStringEncode(userArray[i].Username, true))
                    .Append(",\"colour\":\"")
                    .Append(userArray[i].HexColour)
                    .Append("\"},");
            }
            builder.Append("{\"username\":")
                .Append(HttpUtility.JavaScriptStringEncode(userArray[userArray.Length - 1].Username, true))
                .Append(",\"colour\":\"")
                .Append(userArray[userArray.Length - 1].HexColour)
                .Append("\"}");

            builder.Append("]");
            return builder.ToString();
        }

    }
}
