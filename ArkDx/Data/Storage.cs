using ArkDx.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ArkDx.Data
{
    public static class Storage
    {
        private static List<Game> appGames = new List<Game>();
        private static List<Game> arkGames = new List<Game>();
        private static ConcurrentBag<string> carnages = new ConcurrentBag<string>();

        public static List<Game> AppGames
        {
            get { return appGames; }
            set { appGames = value; }
        }
        public static List<Game> ArkGames
        {
            get { return arkGames; }
            set { arkGames = value; }
        }
        public static ConcurrentBag<string> Carnages
        {
            get { return carnages; }
            set { carnages = value; }
        }
    }
}
