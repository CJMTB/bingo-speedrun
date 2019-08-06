using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BingoSpeedrun.Models
{
    [DataContract]
    public class BingoRoomSettings
    {
        [DataMember]
        public List<string> CardsList = new List<string>();

        public BingoRoomSettings() {
            // create default cards
            for (int i = 0; i < 25; i++)
            {
                CardsList.Add("Example Bingo Card " + (i + 1));
            }
        }
    }
}
