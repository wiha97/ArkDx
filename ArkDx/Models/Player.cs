using System.Collections.Generic;

namespace ArkDx.Models
{
    public class Player
    {

        public string GamerTag { get; set; }        //  Gamertag/username of player
        public string XboxID { get; set; }          //  XBOX ID for potential API integration or something
        public string ServiceID { get; set; }       //  Clan tag of player
        public int Spree { get; set; }              //  Highest spree
        public int Kills { get; set; }              //  Total kill count
        public int Deaths { get; set; }             //  Total death count
        public int Assists { get; set; }            //  Total assist count
        public string Kdr { get; set; }             //  Kill/death ratio with double precision
        public string Nemesis { get; set; }         //  Index of the nemesis
        public string Score { get; set; }
        public string Team { get; set; }
        public List<Medal> Medals { get; set; }
        
    }
}
