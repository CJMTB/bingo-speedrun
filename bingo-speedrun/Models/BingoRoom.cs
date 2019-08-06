using BingoSpeedrun.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BingoSpeedrun.Models
{
    [DataContract]
    public class BingoRoom
    {
        [DataMember]
        public string RoomID { get; private set; }

        [DataMember]
        public BingoRoomSettings Settings { get; set; }

        [DataMember]
        public BingoBoard Board { get; private set; }

        [IgnoreDataMember]
        private List<string> _possibleHexColours = new List<string>()
        {
            "#CC0000", "#9500E0", "#00BF00", "#008FE8"
        };

        // connectionID -> BingoUser
        [DataMember]
        public Dictionary<string, BingoUser> Users = new Dictionary<string, BingoUser>();

        public BingoRoom(string roomID)
        {
            RoomID = roomID;
            Board = new BingoBoard();

            Settings = new BingoRoomSettings();
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
        public void RemoveUser(string connectionID)
        {
            _possibleHexColours.Add(Users[connectionID].Colour);
            Users.Remove(connectionID);
        }

        // resets the board using random cards in room settings
        public void ResetBoard()
        {
            List<string> cards = new List<string>(Settings.CardsList);
            Board.Cards = cards.OrderBy(x => BingoUtils.Rand.Next()).Take(25).ToArray();
            for(int i = 0; i < 25; i++)
            {
                Board.Colours[i].Clear();
            }
        }

        public string BoardToJSON()
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BingoBoard));
            ser.WriteObject(stream, Board);
            stream.Position = 0;
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

    }
}
