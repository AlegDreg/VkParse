using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkParse
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ParseImages parseImages = new ParseImages("vkAnyAccessToken", 
                @"C:\Users\oliso\Desktop\arh2\messages", 
                @"C:\Users\oliso\Desktop\out", 
                new List<string> { "100000001" });

            parseImages.Start();

            Console.ReadKey();
        }
    }
}
