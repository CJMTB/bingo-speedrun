using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BingoSpeedrun.Models
{
    public class BingoBoard
    {
        public string[] cards = new string[25];
        public List<string>[] colours = new List<string>[25];

        public BingoBoard()
        {
            for(int i = 0; i < 25; i++)
            {
                cards[i] = "";
                colours[i] = new List<string>();
            }
        }

        // add colour to tile if not present, remove it otherwise
        public void Update(int tileID, string colour)
        {
            if(colours[tileID].Contains(colour))
            {
                colours[tileID].Remove(colour);
            }
            else
            {
                colours[tileID].Add(colour);
            }
        }


        /*
        [
			{
                colours: [],
				card: "Example Bingo Card"
			},
			{...}
		]
        */
        public string ToJSON()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            for(int i = 0; i < 24; i++)
            {
                builder.Append("{");
                BuildColourCardJSON(builder, i);
                builder.Append("},");
            }
            builder.Append("{");
            BuildColourCardJSON(builder, 24);
            builder.Append("}");
            builder.Append("]");
            return builder.ToString();
        }

        private void BuildColourCardJSON(StringBuilder builder, int tileID)
        {
            builder.Append("\"colours\":[");
            int count = colours[tileID].Count;
            if(count > 0)
            {
                if(count > 1)
                {
                    for (int i = 0; i < colours[tileID].Count - 1; i++)
                    {
                        // "#ABCDEF",
                        builder.Append("\"")
                            .Append(colours[tileID][i])
                            .Append("\",");
                    }
                }
                // last colour without comma
                // "#ABCDEF"
                builder.Append("\"")
                    .Append(colours[tileID][colours[tileID].Count - 1])
                    .Append("\"");
            }
            builder.Append("],\"card\":")
                .Append(HttpUtility.JavaScriptStringEncode(cards[tileID], true));
        }
    }
}
