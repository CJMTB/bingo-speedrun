using BingoSpeedrun.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoSpeedrun.Models
{
    public class BingoUser
    {
        public string ConnectionID { get; private set; }
        public string Username { get; private set; }
        public string HexColour { get; private set; }

        // Creates user with random colour from PossibleHexColours
        public BingoUser(string connectionID, string username)
        {
            ConnectionID = connectionID;
            Username = username;
            HexColour = BingoUtils.RandomHexColour();
        }

        public BingoUser(string connectionID, string username, string hexColour)
        {
            ConnectionID = connectionID;
            Username = username;
            HexColour = hexColour;
        }
    }
}
