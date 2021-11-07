using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ArkDx.Data
{
    public class Library
    {
        string lib = @"Library/";
        string[] games = { "0,haloce", "1,halo2c", "2,halo3", "4,halo4", "5,halo3odst", "6,haloreach", "8,halo2a" };

        public void GenerateLibrary()   //  Fills in the library files that are missing
        {
            CreateFolder();
            CreateMedals();
            CreateGames();
            CreateMaps();
            CreateTeams();
            CreateDifficulties();
        }

        void CreateFolder()
        {
            if (!Directory.Exists("Library"))
            {
                Directory.CreateDirectory("Library");
            }
        }

        void CreateMedals()
        {
            string mPath = lib + "mpmedals.txt";
            string sPath = lib + "spmedals.txt";
            if (IsNewFile(mPath))
            {
                FillFile(mPath, 383, null);
            }
            if (IsNewFile(sPath))
            {
                FillFile(sPath, 30, null);
            }
        }

        void CreateGames()
        {
            string path = lib + "games.txt";
            if (IsNewFile(path))
            {
                FillFile(path, 0, games);
            }
        }

        void CreateDifficulties()
        {
            string path = lib + "diff.txt";
            if (IsNewFile(path))
            {
                string[] diffs = { "Easy", "Normal", "Heroic", "Legendary" };
                FillFile(path, 0, diffs);
            }
        }

        void CreateMaps()
        {
            foreach (string game in games)
            {
                string g = game.Substring(2);
                string path = lib + g + ".txt";
                if (IsNewFile(path))
                {
                    string[] maps = { "" };
                    bool dontFill = false;
                    switch (g)
                    {
                        case "halo2a":
                            maps = new string[] { "coagul,Bloodline", "ascens,Zenith", "lockou,Lockdown", "sanctu,Shrine", "forge,[Some Forge Map]", "zanzib,Stonetown", "warloc,Warlord", "relic,Remnant" };
                            break;
                        case "halo3":
                            maps = new string[] { "chillou,Cold Storage", "lockout,Blackout", "chill,Narrows", "guard,Guardian", "ware,Foundry", "const,Construct", "midship,Heretic", "bunker,Standoff", "sidewin,Avalanche", "zanziba,Last Resort", "sandbox,Sandbox", "river,Valhalla", "shrine,Sandtrap", "cyberdy,The Pit", "voi,The Storm", "hal,Halo", "jun,Sierra 117", "was,The Ark", "cit,The Covenant", "bas,Crow's Nest", "out,Tsavo Highway", "salvati,Epitaph", "deadloc,High Ground", "ghost,Ghost Town", "isolati,Isolation", "space,Orbital", "fortres,Citadel", "descent,Assembly", "snow,Snowbound", "armory,Rat's Nest", "hc,Cortana", "docks,Longshore", "wat,Waterfall", "tur,Icebox", "edg,Edge" };
                            break;
                        case "halo3odst":
                            maps = new string[] { "h100,Mombasa Streets", "c100,Tayari Plaza", "c110,Uplift Reserve", "c120,Kizingo Boulevard", "c130,ONI Alpha Site", "c140,NMPD HQ", "c150,Kikowani Station", "l200,Data Hive", "l300,Coastal Highway" };
                            break;
                        case "halo4":
                            maps = new string[] { "warhou,Adrift", "blood,Exile", "wrap,Haven", "gore,Longbow", "canyon,Meltdown", "valha,Ragnarok", "forge,Impact", "port,Landfall", "rattle,Skyline", "high,Perdition", "creep,Pitfall" };
                            break;
                        case "haloreach":
                            maps = new string[] { "launch,Countdown", "timbe,Ridgeline", "forge,Forge World", "panopt,Boardwalk", "mediu,Tempest", "invas,Breakout", "trainingp,Highlands", "boneya,Boneyard", "settle,Powerhouse", "ivory,Reflection", "island,Spire", "aftshi,Zealot", "sword,Sword Base", "dlc_slaye,Anchor 9", "condemned,Condemned", "beave,Battle Canyon", "damn,Penance", "priso,Solitary", "hange,High Noon", "headl,Breakneck", "ha_,Installation 01", "uneart,Unearthed", "hold,Holdout", "corv,Corvette", "prot,Overlook", "glac,Glacier", "bonus,Lone Wolf", "m30,Nightfall", "m35,Tip of The Spear", "m50,Exodus", "m52,New Alexandria", "m70,Pillar of Autumn" };
                            break;
                        default:
                            dontFill = true;
                            break;
                    }
                    if (!dontFill)
                    {
                        FillFile(path, 0, maps);
                    }
                }
            }
        }

        void CreateTeams()
        {
            string path = lib + "teams.txt";
            if (IsNewFile(path))
            {
                string[] teams = { "-1,FFA", "0,Red", "1,Blue" };
                FillFile(path, 0, teams);
            }
        }

        bool IsNewFile(string file)
        {
            if (!File.Exists(file))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void FillFile(string path, int maxNum, string[] strings)
        {
            List<string> lines = new List<string>();
            if (strings != null)
            {
                lines = strings.ToList();
            }
            else
            {
                for (int i = 0; i <= maxNum; i++)
                {
                    lines.Add(i.ToString());
                }
            }
            File.WriteAllLines(path, lines);
        }
    }
}
