using ArkDx.Data;
using ArkDx.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArkDx.WPF;

namespace ArkDx.Logic
{
    public class FilmController
    {
        CarnageController carnage = new CarnageController();
        private Shisno shisno = new Shisno();
        public IEnumerable<Film> GetLocalFilms(string path, ConcurrentBag<string> reports)
        {
            ConcurrentBag<Film> filmBag = new ConcurrentBag<Film>();
            string game = Path.GetFileName(path);
            try
            {
                if (path.Contains("AppData"))
                {
                    if (Settings.MultiThreading)
                    {
                        MultiThreadFilms(path + @"\Movie", filmBag, reports, game, true);
                    }
                    else
                    {
                        SingleThreadFilms(path + @"\Movie", filmBag, reports, game, true);
                    }
                }
                else
                {
                    foreach (string yr in Directory.GetDirectories(path))
                    {
                        if (Settings.MultiThreading)
                        {
                            Parallel.ForEach(Directory.GetDirectories(yr), date =>
                            {
                                MultiThreadFilms(date, filmBag, reports, game, false);
                            });
                        }
                        else
                        {
                            foreach (string date in Directory.GetDirectories(yr))
                            {
                                SingleThreadFilms(date, filmBag, reports, game, false);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                shisno.ShisnoBag.Add($"Failed to get films:\n{e.Message}");
            }
            if (shisno.ShisnoBag.Count() > 0)
            {
                shisno.LogErrors();
            }
            return filmBag.ToList().OrderByDescending(f => f.Date);
        }

        ConcurrentBag<Film> MultiThreadFilms(string dir, ConcurrentBag<Film> filmBag, ConcurrentBag<string> reports, string game, bool isApp)
        {
            try
            {
                Parallel.ForEach(Directory.GetFiles(dir), film =>
                {
                    AddFilm(film, reports, filmBag, game, isApp);
                });
            }
            catch (Exception e)
            {
                shisno.ShisnoBag.Add(e.Message);
            }
            return filmBag;
        }

        ConcurrentBag<Film> SingleThreadFilms(string dir, ConcurrentBag<Film> filmBag, ConcurrentBag<string> reports, string game, bool isApp)
        {
            try
            {
                foreach (string film in Directory.GetFiles(dir))
                {
                    AddFilm(film, reports, filmBag, game, isApp);
                }
            }
            catch (Exception e)
            {

            }
            return filmBag;
        }

        void AddFilm(string film, ConcurrentBag<string> reports, ConcurrentBag<Film> filmBag, string game, bool isApp)
        {
            //  Checks so the film is above a minimum size limit in an attempt to reduce clutter
            long vol = new FileInfo(film).Length;
            if (vol / 1024 > Settings.FilmSizeLimit)
            {
                MainController main = new MainController();
                Carnage report = null;
                TimeSpan time = TimeSpan.FromMinutes(8);
                if (reports != null)
                {
                    try
                    {
                        //  Tries to pair carnages and films using datetime
                        DateTime filmTime = File.GetLastWriteTime(film);
                        List<string> gameRep = reports.Where(r => r.Contains(game.ToLower())).ToList();
                        foreach (string rep in gameRep)
                        {
                            try
                            {
                                if (rep.Contains(filmTime.ToString(@"yyyy\\MM-dd")))
                                {
                                    DateTime repTime = File.GetCreationTime(rep);
                                    if (repTime.Subtract(filmTime) <= TimeSpan.FromMinutes(2) && repTime.Subtract(filmTime) >= TimeSpan.FromMinutes(-1))
                                    {
                                        try
                                        {
                                            report = carnage.GetCarnage(rep, Settings.GamerTag, shisno);
                                            time = report.Time;
                                            if (report.Time > TimeSpan.FromMinutes(1))
                                            {
                                                break;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            shisno.ShisnoBag.Add($"Failed to add report:\n{e.Message}");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                shisno.ShisnoBag.Add(e.Message);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        shisno.ShisnoBag.Add(e.Message);
                    }
                }
                DateTime date = File.GetLastWriteTime(film);
                DateTime clipDate = date.Subtract(time);
                try
                {
                    string targetPath = "";
                    if (isApp)
                    {
                        targetPath = string.Format(@"{0}\Theater\{1}\{2}\{3}\", Settings.ArkPath, game, date.ToString(@"yyyy"), date.ToString("MM-dd"));
                    }
                    else
                    {
                        targetPath = string.Format(@"{0}\UserContent\{1}\Movie\", Settings.AppData, game);
                    }
                    if (report != null)
                    {
                        date = clipDate;
                    }
                    filmBag.Add(new Film
                    {
                        Name = main.Translate(Path.GetFileName(film), Path.GetFileName(game)),
                        Date = date,
                        Source = film,
                        TargetFile = targetPath + Path.GetFileName(film),
                        TargetPath = targetPath,
                        Report = report,
                        Clips = Clips(clipDate, time)
                    });
                }
                catch (Exception e)
                {
                    shisno.ShisnoBag.Add($"Failed to add film:\n{e.Message}");
                }
            }
            else
            {
                string name = film + vol;
            }
        }

        List<Clip> Clips(DateTime start, TimeSpan dur)  //  Checks the clips directories for a clip saved during the round then pairs it with the film
        {
            List<Clip> clips = new List<Clip>();
            IEnumerable<FileInfo> files;
            try
            {
                foreach (string path in Settings.Clips)
                {
                    files = new DirectoryInfo(path).GetFiles()
                        .Where(c => c.LastWriteTime > start && c.LastWriteTime < start + dur + TimeSpan.FromSeconds(30));
                    foreach (FileInfo clip in files)
                    {
                        clips.Add(new Clip()
                        {
                            Name = clip.Name,
                            Path = clip.FullName
                        });
                    }
                }
            }
            catch (Exception e)
            {
                shisno.ShisnoBag.Add($"Failed to add clip:\n{e.Message}");
            }
            return clips;
        }
    }
}
