using System;
using System.Windows.Threading;
using ArkDx.Data;
using ArkDx.WPF;

namespace ArkDx.Logic
{
    public class Ticker
    {
        private int curFilms = 0;
        private int ival = 1;
        private int carnages = 0;
        private int warningCount = 0;
        private int critCount = 0;
        private DispatcherTimer timer;
        private FilesController files = new FilesController();
        private WPFWindow win;
        private Sound sound = new Sound();
        private Logger logger = new Logger();

        public void Timer(WPFWindow wpf)
        {
            win = wpf;
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0,0,ival);
            Start();
        }

        public void Start()
        {
            try
            {
                timer.Start();
            }
            catch(Exception e)
            {

            }
        }
        public void Stop()
        {
            timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Settings.ArkPath != "")
                {
                    int fileCount = files.FileCount($@"{Settings.AppData}\UserContent\");
                    //files.CheckApp(curFilms, fileCount, win);   //Checks for new files in AppData
                    if (fileCount != curFilms)
                    {
                        if (fileCount != 0)
                        {
                            logger.MsgLog("Found new film(s)");
                        }
                        files.CheckFilms(fileCount);
                        win.GetApps();
                        curFilms = fileCount;
                        sound.NewFilm();
                        carnages = 0;
                        warningCount = 0;
                        critCount = 0;
                    }
                    if (files.CheckCarnage())
                    {
                        carnages++;
                        warningCount++;
                        critCount++;
                    }
                    int limit = Settings.AppLimit - fileCount;
                    int min = Settings.AppMin - fileCount;
                    int remaining = Settings.AppLimit - (carnages + fileCount);
                    //if (carnages >= min)
                    //{
                    if (critCount >= limit)
                    {
                        logger.BadLog($"Warning:\n" +
                            $"Theater limit has been reached and The Ark has been unable to save films due to them not being loaded by the game.\n" +
                            $"Please go to 'My Files' from the main menu (options>MyFiles) to load the files\n" +
                            $"{carnages - Settings.AppLimit} films lost\n" +
                            $"Blame 343i for this one");
                        sound.ErrorSound();
                        critCount--;
                    }
                    else if (warningCount == min)
                    {
                        logger.Log($"{carnages} carnages has been saved with no new films, please manually load your films. Game will start removing films in {remaining} rounds\n" +
                            $"(Main Menu >> OPTIONS >> My Files >> Films)\n" +
                            $"Cannot be done mid-game!");
                        sound.SoftError();
                        warningCount--;
                    }
                    //}
                }
            }
            catch(Exception x)
            {
                logger.BadLog(x.Message);
            }

        }
    }
}
