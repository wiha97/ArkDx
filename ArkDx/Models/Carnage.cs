using System;
using System.Collections.Generic;

namespace ArkDx.Models
{
    public class Carnage
    {
        public string Game { get; set; }
        public string GameMode { get; set; }
        public string Score { get; set; }
        public Player Player { get; set; }
        public List<Player> Players { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime Date { get; set; }
    }

    public class PVECarnage : Carnage
    {
        public string Difficulty { get; set; }
        public List<string> Skulls { get; set; }
        public List<Enemy> Enemies { get; set; }
    }

    public class MPCarnage : Carnage
    {
        //  MOVED TO PLAYER
    }
}
