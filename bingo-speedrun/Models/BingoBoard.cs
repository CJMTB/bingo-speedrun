using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BingoSpeedrun.Models
{
    [DataContract]
    public class BingoBoard
    {
        [DataMember]
        public string[] Cards = new string[25];
        [DataMember]
        public List<string>[] Colours = new List<string>[25];

        public BingoBoard()
        {
            for(int i = 0; i < 25; i++)
            {
                Cards[i] = "Example Bingo Card " + (i + 1);
                Colours[i] = new List<string>();
            }
        }

        // add colour to tile if not present, remove it otherwise
        public void Update(int tileID, string colour)
        {
            if(Colours[tileID].Contains(colour))
            {
                Colours[tileID].Remove(colour);
            }
            else
            {
                Colours[tileID].Add(colour);
            }
        }
    }
}
