using System;
using System.Linq;
using System.IO;
using ArkDx.Data;
using System.Xml.Linq;
using ArkDx.WPF;
using ArkDx.Models;

namespace ArkDx.Logic
{
    public class FilesController
    {
        DateTime carnageTime = new DateTime();
        CarnageController carnage = new CarnageController();
        MainController main = new MainController();
        Sound sound = new Sound();
        Logger log = new Logger();

        public void CheckFilms(int files)
        {
            string userContent = $@"{Settings.AppData}\UserContent\";
            if (files >= Settings.AppMin)
            {
                MoveFiles(userContent);
            }
        }
        public bool CheckCarnage()
        {
            bool newCar = false;
            string firstCarnage = Directory.GetFiles(Settings.AppData).OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();
            DateTime newTime = File.GetLastWriteTime(firstCarnage);

            //  Checks if the last write time of the most recent file is newer than the latest stored one
            if (newTime > carnageTime)
            {
                try
                {
                    string file = $@"{File.GetLastWriteTime(firstCarnage).ToString("HHmm_")}{Path.GetFileName(firstCarnage)}";
                    string targetPath = string.Format(@"{0}\Carnages\{1}\{2}\", Settings.ArkPath, Game(firstCarnage), File.GetLastWriteTime(firstCarnage).ToString(@"yyyy\\MM-dd"));
                    if(CheckCopy(file, firstCarnage, targetPath))
                    {
                        CreateDir(targetPath + @"\read\");
                        carnage.ShortenCarnage(targetPath, file);
                        log.MsgLog($"Saved and shortened carnage: {file}");
                        newCar = true;
                    }
                    else
                    {
                        log.Log($"Carnage has already been saved: {file}");
                    }
                    carnageTime = newTime;
                }
                catch (Exception e)
                {
                    log.BadLog(e.Message);
                }
            }
            return newCar;
        }

        string Game(string file)    //  Gets the game's ID
        {
            XDocument doc = XDocument.Load(file);
            string game = "";
            if (file.Contains("mpc"))
            {
                game = main.Translate(doc.Root.Element("GameEnum").Attribute("mGameEnum").Value, "MPGame");
            }
            else
            {
                game = main.Translate(doc.Root.Element("GeneralData").Attribute("GameId").Value, "MPGame");
            }
            return game;
        }

        public bool CheckCopy(string name, string source, string target)
        {
            CreateDir(target);
            if (!File.Exists($@"{target}\{name}"))
            {
                File.Copy(source, $@"{target}\{name}");
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CopyFilm(Film film)
        {
            try
            {
                CreateDir(film.TargetPath);
                string target = film.TargetFile;
                if (target.Contains("AppData")) { target = "..." + target.Substring(Settings.AppData.Length); }
                else { target = target.Substring(Settings.ArkPath.Length); }
                if (!File.Exists(film.TargetFile))
                {
                    File.Copy(film.Source, film.TargetFile);
                    log.MsgLog($@"Copied film {film.Name} to {target}");
                }
                else
                {
                    log.Log($@"File '{target}' already exists");
                }
            }
            catch(Exception e)
            {
                log.BadLog(e.Message);
            }
        }

        public void DeleteFile(string target)
        {
            if (File.Exists(target))
            {
                File.Delete(target);
            }
        }

        public int FileCount(string directory)
        {
            int num = 0;
            foreach (string gameDir in Directory.GetDirectories(directory))
            {
                if (Directory.Exists($@"{gameDir}\Movie\"))
                {
                    num += Directory.GetFiles($@"{gameDir}\Movie\").Count();
                }
            }
            return num;
        }

        void MoveFiles(string directory)
        {
            foreach (string gameDir in Directory.GetDirectories(directory))
            {
                string movie = $@"{gameDir}\Movie\";
                if (Directory.Exists(movie))
                {
                    foreach (string file in Directory.GetFiles(movie))
                    {
                        string target = $@"{Settings.ArkPath}\Theater\{Path.GetFileName(gameDir)}\{File.GetCreationTime(file).ToString(@"yyyy\\MM-dd")}\";
                        try
                        {
                            if(CheckCopy(Path.GetFileName(file), file, target))
                            {
                                DeleteFile(file);
                                sound.FilmSound();
                                log.MsgLog($"Moved {Path.GetFileName(file)} to archive");
                            }
                        }
                        catch(Exception e)
                        {
                            log.BadLog(e.Message);
                        }
                    }
                }
            }
        }

        void CreateDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
