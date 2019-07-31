using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BingoSpeedrun.Utils
{
    public class BingoUtils
    {
        public static Random Rand = new Random();

        // generates random string of given length
        public static string RandomString(int length)
        {
            const string possible = "ABCDEFGHJKLMNPQRSTUVWXYZ123456789"; // exclude I, O and 0
            char[] stringChars = new char[length];

            for(int i = 0; i < length; i++)
            {
                stringChars[i] = possible[Rand.Next(possible.Length)];
            }

            return new string(stringChars);
        }

        public static string RandomHexColour()
        {
            string[] possibleHexColours =
            {
                "#E6194B", "#3CB44B", "#FFE119", "#4363D8", "#F58231",
                "#911EB4", "#42D4F4", "#F032E6", "#BFEF45", "#FABEBE",
                "#469990", "#E6BEFF", "#9A6324", "#FFFAC8", "#800000",
                "#AAFFC3", "#808000", "#FFD8B1", "#000075", "#A9A9A9"
            };

            int index = Rand.Next(possibleHexColours.Length);
            return possibleHexColours[index];
        }

        public static string PickAndRemoveString(List<string> stringArray)
        {
            int index = Rand.Next(stringArray.Count);
            string colour = stringArray[index];
            stringArray.RemoveAt(index);
            return colour;
        }
    }
}
