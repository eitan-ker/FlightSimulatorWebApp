using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightControlWeb.Commons
{
    public static class  RandomGenerator
    {
     
 
        // Generate a random string with a given size, and at the end add to the string an additional random number between 1 and 999    
        public static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            string generatedNumber = (random.Next(1, 999)).ToString();
            builder.Append(generatedNumber);
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }
}
