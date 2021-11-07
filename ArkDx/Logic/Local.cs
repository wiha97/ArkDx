using ArkDx.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArkDx.Data;
using ArkDx.WPF;

namespace ArkDx.Logic
{
    public class Local
    {
        Logger log = new Logger();
        //  Gets all games and their films, carnages and clips
        public void GetGames(string path)
        {
            FilmController film = new FilmController();
            GetCarnages(Settings.ArkPath);
            try
            {
                if (path.Contains("AppData"))
                {
                    foreach (string dir in Directory.GetDirectories(path + @"\UserContent\"))
                    {
                        if (Directory.Exists(dir + @"\Movie\") && Directory.GetFiles(dir + @"\Movie\").Count() > 0)
                        {
                            try
                            {
                                Storage.AppGames.Add(new Game() 
                                { 
                                    Name = Path.GetFileName(dir), 
                                    Films = film.GetLocalFilms(dir, Storage.Carnages) 
                                });
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
                else
                {
                    foreach (string dir in Directory.GetDirectories(path + @"\Theater\"))
                    {
                        try
                        {
                            Storage.ArkGames.Add(new Game() 
                            { 
                                Name = Path.GetFileName(dir), 
                                Films = film.GetLocalFilms(dir, Storage.Carnages) 
                            });
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }

        //      Gets all carnages from the archive and puts them in a list
        public void GetCarnages(string path)
        {
            try
            {
                if (Storage.Carnages.Count() == 0)
                {
                    foreach (string game in Directory.GetDirectories(path + "\\Carnages"))
                    {
                        foreach (string year in Directory.GetDirectories(game))
                        {
                            if (Settings.MultiThreading)
                            {
                                Parallel.ForEach(Directory.GetDirectories(year), date =>
                                {
                                    AddCarnages(date);
                                });
                            }
                            else
                            {
                                foreach (string date in Directory.GetDirectories(year))
                                {
                                    AddCarnages(date);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }
        }

        void AddCarnages(string date)
        {
            try
            {
                foreach (string file in Directory.GetFiles(date + @"\read"))
                {
                    Storage.Carnages.Add(file);
                }
            }
            catch(Exception e)
            {
                log.BadLog(e.Message);
            }
        }
    }
}
