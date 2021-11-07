using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ArkDx.Data;
using ArkDx.Models;
using ArkDx.WPF;

namespace ArkDx.Logic
{
    public class CarnageController
    {
        private MainController main = new MainController();
        private Sound sound = new Sound();
        private Logger log = new Logger();
        private Shisno shisno = new Shisno();

        public void ShortenCarnage(string path, string file)                //  Saves XML file to the archive
        {
            string full = path + file;
            DateTime time = File.GetLastWriteTime(full);
            string newFile = $@"{path}\read\{file}";
            if (!File.Exists(newFile))
            {
                string player = "";
                if (!file.Contains("mpc"))
                {
                    player = "PlayerInfo";
                }
                else
                {
                    player = "Player";
                }
                File.Copy(full, newFile);
                XmlShortener(newFile, player);

                sound.CarnageSound();
                Storage.Carnages.Add(newFile);
            }
            else
            {
                log.MsgLog("Carnage has already been saved");
            }
            try
            {
                File.SetCreationTime(newFile, time);
            }
            catch (Exception e)
            {
                log.BadLog(e.Message);
                sound.SoftError();
            }
        }

        void XmlShortener(string file, string player)                       //  Shortens XML by removing un-earned medals. -360> lines per player
        {
            XDocument doc = XDocument.Load(file);
            List<XElement> players = doc.Root.Element("Players").Elements(player).ToList();

            foreach (XElement plr in players)
            {
                List<XElement> medals = plr.Element("MedalsCount").Elements("Medal").ToList();
                foreach (XElement medal in medals)
                {
                    if (medal.Attribute("mCount").Value == "0")
                    {
                        medal.Remove();
                    }
                }
            }
            doc.Save(file);
        }

        public Carnage GetCarnage(string file, string gt)                   //  Checks if the carnage report is of type campaign/firefight(PVE) or Multiplayer(PVP), different formats in the file
        {
            try
            {
                switch (file)
                {
                    case string f when f.Contains("mpc"):
                        return MP(file, gt);
                    case string f when f.Contains("surv") || f.Contains("cam"):
                        return PVE(file);
                    default:
                        return null;
                }
            }
            catch(Exception e)
            {
                shisno.ShisnoBag.Add(e.Message);
                return null;
            }
        }

        MPCarnage MP(string file, string gt)                                //  Reads the MP/PVP carnage and saves it as MPCarnage
        {
            MPCarnage mp;
            XDocument doc = XDocument.Load(file);
            List<XElement> xPlayers = doc.Root.Element("Players").Elements("Player").ToList();
            List<Player> players = Players(xPlayers, "mpmedals");
            Player player = players.Where(p => p.GamerTag.ToLower() == Settings.GamerTag.ToLower()).FirstOrDefault();
            if (player == null)
                player = players.FirstOrDefault();
            mp = new MPCarnage()
            {
                Score = player.Score,
                Player = player,
                Game = doc.Root.Element("GameEnum").Attribute("mGameEnum").Value,
                GameMode = doc.Root.Element("GameTypeName").Attribute("GameTypeName").Value,
                Players = players,
                Date = File.GetLastWriteTime(file),
                Time = TimeSpan.FromSeconds(int.Parse(xPlayers.FirstOrDefault().Attribute("mSecondsPlayed").Value))
            };
            return mp;
        }

        PVECarnage PVE(string file)                                         //  Reads the PVE carnage and saves it as PVECarnage
        {
            PVECarnage carnage;
            string mode = "Campaign";
            string medals = "spmedals";
            XDocument doc = XDocument.Load(file);
            XElement data = doc.Root.Element("GeneralData");
            List<XElement> xPlayers = doc.Root.Element("Players").Elements("PlayerInfo").ToList();
            XElement xPlayer = xPlayers.FirstOrDefault();
            if (file.Contains("surv"))
            {
                mode = doc.Root.Element("GameTypeInfo").Attribute("GameTypeName").Value;
                medals = "mpmedals";
            }
            List<Player> players = Players(xPlayers, medals);
            Player player = players.Where(p => p.GamerTag.ToLower() == Settings.GamerTag.ToLower()).FirstOrDefault();
            if (player == null)
                player = players.FirstOrDefault();
            try
            {
                carnage = new PVECarnage()
                {
                    Score = data.Attribute("TotalScoreText").Value,
                    Time = TimeSpan.Parse(data.Attribute("Time").Value),
                    Game = main.Translate(data.Attribute("GameId").Value, "game"),
                    Difficulty = File.ReadAllLines(@"Library/diff.txt")[int.Parse(data.Attribute("DiffId").Value)],
                    Players = players,
                    Skulls = Skulls(doc.Root.Element("Skulls")),
                    Date = File.GetLastWriteTime(file),
                    GameMode = mode,
                    Player = player,
                    Enemies = Enemies(xPlayer.Element("Kills"))
                };
            }
            catch (Exception e)
            {
                carnage = null;
                shisno.ShisnoBag.Add(e.Message);
            }
            return carnage;
        }

        List<Player> Players(List<XElement> xPlayers, string medals)
        {
            List<Player> players = new List<Player>();
            foreach (XElement player in xPlayers)
            {
                string team = "-1";
                int spree = 0;
                int assists = 0;
                string nemesis = null;
                string gamerTag = "GamertagText";
                string mKills = "mKillCount";
                string mDeaths = "mDeathCount";
                try
                {
                    //  If attribute "mGamertagText" exists, then it's assumed that the report is from PVP Multiplayer and not Campaign or Firefight
                    if (player.Attribute("mGamertagText") != null)
                    {
                        gamerTag = "mGamertagText";
                        assists = int.Parse(player.Attribute("mAssists").Value);
                        if(int.Parse(player.Attribute("mKilledMostPlayerIndex").Value) >= 0)
                        {
                            nemesis = xPlayers[int.Parse(player.Attribute("mKilledMostPlayerIndex").Value)]
                                .Attribute("mGamertagText").Value;
                        }
                        mKills = "mKills";
                        mDeaths = "mDeaths";
                        team = player.Attribute("mTeamId").Value;
                        spree = int.Parse(player.Attribute("mMostKillsInARow").Value);
                    }
                    int kills = int.Parse(player.Attribute(mKills).Value);
                    int deaths = int.Parse(player.Attribute(mDeaths).Value);
                    float kdr = 0;
                    if(kills > 0)
                    {
                        if (deaths > 0)
                            kdr = (float)kills / (float)deaths;
                        else
                            kdr = kills;
                    }
                    players.Add(new Player()
                    {
                        XboxID = player.Attribute("mXboxUserId").Value,
                        GamerTag = player.Attribute(gamerTag).Value,
                        ServiceID = player.Attribute("ServiceId").Value,
                        Team = main.Translate(team, "teams"),
                        Score = player.Attribute("Score").Value,
                        Spree = spree,
                        Kills = kills,
                        Deaths = deaths,
                        Medals = Medals(medals, player),
                        Kdr = kdr.ToString("0.00"),
                        Nemesis = nemesis,
                        Assists = assists
                    });
                }
                catch (Exception e)
                {

                }
            }
            return players;
        }

        List<Medal> Medals(string mode, XElement player)
        {
            List<Medal> medals = new List<Medal>();
            IEnumerable<XElement> meds = player.Element("MedalsCount").Elements("Medal").Where(m => m.Attribute("mCount").Value != "0");
            if (meds.Count() > 0)
            {
                foreach (XElement medal in meds)
                {
                    string line = File.ReadAllLines(string.Format(@"Library\{0}.txt", mode)).Skip(int.Parse(medal.Attribute("mId").Value)).Take(1).FirstOrDefault();
                    string name = line.Split(',')[0];
                    string group = null;
                    if (line.Contains(','))
                    {
                        group = main.Translate(line, mode);
                    }
                    medals.Add(new Medal()
                    {
                        Name = name,
                        Group = group,
                        Count = int.Parse(medal.Attribute("mCount").Value)
                    });
                }
            }
            return medals;
        }

        List<Enemy> Enemies(XElement kill)
        {
            List<Enemy> enemies = new List<Enemy>();
            IEnumerable<XElement> kills = kill.Elements("Kill");
            foreach (XElement k in kill.Elements("Kill"))
            {
                enemies.Add(new Enemy()
                {
                    EnemyType = k.Attribute("EnemyType").Value,
                    EnemyClass = k.Attribute("EnemyClass").Value,
                    Count = int.Parse(k.Attribute("Count").Value),
                    Betray = bool.Parse(k.Attribute("BetrayalKill").Value),
                    //Elite = bool.Parse(k.Attribute("PlayerElite").Value)
                });
            }
            return enemies;
        }

        List<string> Skulls(XElement skull)
        {
            List<string> skulls = new List<string>();
            foreach (XElement skl in skull.Elements("SkullInfo"))
            {
                skulls.Add(skl.Attribute("SkullImage").Value);
            }
            return skulls;
        }
    }
}
