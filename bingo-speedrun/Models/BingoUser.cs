using BingoSpeedrun.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BingoSpeedrun.Models
{
    [DataContract]
    public class BingoUser
    {
        [IgnoreDataMember]
        public string ConnectionID { get; private set; }
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string Colour { get; private set; }

        public BingoUser(string connectionID, string username, string colour)
        {
            ConnectionID = connectionID;
            Username = username;
            Colour = colour;
        }
    }
}
