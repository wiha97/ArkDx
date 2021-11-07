using ArkDx.Data;
using ArkDx.Logic;
using ArkDx.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArkDx.WPF
{
    public class WPFWindow
    {
        private GitLabUpdater update = new GitLabUpdater();
        private Local local = new Local();
        private Colour color = new Colour();
        private MainWindow win;
        private Logger log = new Logger();
        private Ticker tick = new Ticker();
        private Library lib = new Library();
        private Sound sound = new Sound();

        public void Startup()
        {
            win = Application.Current.Windows.OfType<MainWindow>().First();
            FirstLaunch();
            Settings.ReadSettings();            //  Configures The Ark to use the stored settings
            lib.GenerateLibrary();              //  Checks if library files are missing, generates new ones if they are
            local.GetGames(Settings.AppData);   //  Gets the films currently in AppData
            ListGames(Settings.AppData);                          //  Lists the films stored in AppData
            Update();
            win.medalCombo.ItemsSource = File.ReadAllLines(Settings.MedalSource);
        }
        public void Update()
        {
            try
            {
                update.GetReleases(Settings.ProjectUrl, Settings.Version);  //  Checks for updates
            }
            catch (Exception e)
            {
                log.BadLog($"Failed to update:\n{e.Message}");
            }
        }
        void FirstLaunch()
        {
            //  If no 'settings.xml' file is found, settings expander will be expanded and a message will popup asking to fill it out
            if (!File.Exists("settings.xml"))
            {
                log.FirstMsg();
                win.setPander.IsExpanded = true;
                win.setPander.Width = 219;
                win.setPander.Height = 480;
            }
        }
        public void ApplySettings()
        {
            bool thread = false;
            bool auto = false;
            if (win.autoBox.IsChecked == true)
                auto = true;
            if (win.multiBox.IsChecked == true)
                thread = true;
            Settings.ApplySettings(
                Convert.ToInt32(win.appSlide.Value), 
                win.pathBox.Text, 
                win.gtBox.Text, 
                Convert.ToInt32(win.fileSizeSlide.Value), 
                win.urlBox.Text, 
                thread, 
                auto
                );
        }

        public async void GetArk()
        {
            await Task.Run(() =>
            {
                win.Dispatcher.Invoke(() =>
                {
                    log.Log("Getting archive...");
                });
            });
            await Task.Run(() =>
            {
                Storage.ArkGames.Clear();
                if (Storage.Carnages.Count == 0)
                    local.GetCarnages(Settings.ArkPath);
                local.GetGames(Settings.ArkPath);
            });
            await Task.Run(() =>
            {
                win.Dispatcher.Invoke(() =>
                {
                    log.Log("Done!\nGetting UI...");
                });
            });
            await Task.Run(() =>
            {
                win.Dispatcher.Invoke(() =>
                {
                    win.arkStack.Children.Clear();
                    ListGames(Settings.ArkPath);
                });
            });
            await Task.Run(() =>
            {
                win.Dispatcher.Invoke(() =>
                {
                    log.Log("Done!");
                });
            });
        }

        public void ListGames(string path)
        {
            try
            {
                List<Game> games = Storage.AppGames;
                StackPanel basePanel = win.appStack;
                bool isApp = true;
                if (path == Settings.ArkPath)
                {
                    basePanel = win.arkStack;
                    games = Storage.ArkGames;
                    isApp = false;
                }

                foreach (Game game in games)
                {
                    StackPanel gamePanel = new StackPanel();
                    AddToStack(game, gamePanel, isApp);
                    basePanel.Children.Add(gamePanel);
                }
            }
            catch (Exception e)
            {
                log.BadLog(e.Message);
            }
        }

        public void AddToStack(Game game, StackPanel gamePanel, bool isApp)
        {
            try
            {
                ScrollViewer scroll = new ScrollViewer();
                StackPanel filmPanel = new StackPanel();
                DockPanel titlePanel = new DockPanel();
                Label title = new Label();
                Label count = new Label();
                scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                title.Content = game.Name;
                title.FontSize = 28;
                title.FontFamily = new FontFamily("Halo");
                title.Foreground = color.Cyan();
                count.Content = game.Films.Count();
                count.Foreground = color.White();
                count.HorizontalAlignment = HorizontalAlignment.Right;
                ListFilms(game.Films, filmPanel, isApp);
                titlePanel.Children.Add(title);
                titlePanel.Children.Add(count);
                gamePanel.Children.Add(titlePanel);
                scroll.Content = filmPanel;
                gamePanel.Children.Add(scroll);
            }
            catch
            {

            }
        }

        private void ListFilms(IEnumerable<Film> films, StackPanel listPanel, bool isApp)
        {
            try
            {
                if (isApp)
                {
                    AddFilms(films, listPanel);
                }
                else
                {
                    AddDate(films, listPanel);
                }
            }
            catch
            {

            }
        }

        void AddFilms(IEnumerable<Film> films, StackPanel listPanel)
        {
            ScrollViewer scroll = new ScrollViewer();
            foreach (Film film in films)
            {
                StackPanel filmPanel = new StackPanel();
                Films(film, filmPanel);
                listPanel.Children.Add(filmPanel);
            }
        }

        void AddDate(IEnumerable<Film> films, StackPanel listPanel)
        {
            foreach (var year in films.GroupBy(f => f.Date.ToString("yyyy")))
            {
                StackPanel yearPan = new StackPanel();
                DockPanel yearDock = new DockPanel();
                Expander yr = new Expander();
                yearPan.Orientation = Orientation.Horizontal;
                yr.Header = year.Key;
                if (year.Key == DateTime.Now.ToString("yyyy")) { yr.IsExpanded = true; }
                var dates = year.GroupBy(f => f.Date.ToString("MM-dd")).OrderByDescending(group => group.Key);
                foreach (var group in dates)
                {
                    try
                    {
                        string datePath = Path.GetDirectoryName(films.Where(f => f.Source.Contains(group.Key)).FirstOrDefault().Source);                                    //  Gets the path to the directory
                        string carPath = $"{Settings.ArkPath}/Carnages/{datePath.Substring(datePath.IndexOf(Settings.ArkPath) + Settings.ArkPath.Length + 8)}/read";      //  Gets the path to the carnages
                        ScrollViewer scroll = new ScrollViewer();
                        StackPanel datePanel = new StackPanel();
                        DockPanel titlePan = new DockPanel();
                        DockPanel btnPan = new DockPanel();
                        StackPanel conPan = new StackPanel();
                        StackPanel filmPanel = new StackPanel();
                        Button filmsBtn = new Button() { Background = color.Gray(), Foreground = color.Cyan(), BorderBrush = color.DarkGray() };
                        Button carBtn = new Button() { Background = color.Gray(), Foreground = color.Cyan(), BorderBrush = color.DarkGray() };
                        Label date = new Label();
                        carBtn.Content = "Reports";
                        carBtn.Tag = carPath;
                        carBtn.Click += CarBtn_Click;
                        //carBtn.HorizontalAlignment = HorizontalAlignment.Right;
                        filmsBtn.Content = "Films";
                        filmsBtn.Tag = datePath;
                        filmsBtn.Click += DateBtn_Click;
                        //filmsBtn.HorizontalAlignment = HorizontalAlignment.Right;
                        datePanel.Orientation = Orientation.Horizontal;
                        scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                        scroll.MaxHeight = 500;
                        date.Foreground = color.White();
                        date.Content = group.Key;
                        date.Background = color.Gray();
                        date.HorizontalContentAlignment = HorizontalAlignment.Center;
                        foreach (Film film in group.OrderByDescending(f => f.Date))
                        {
                            Films(film, filmPanel);
                        }
                        titlePan.Children.Add(date);
                        btnPan.Children.Add(filmsBtn);
                        btnPan.Children.Add(carBtn);
                        btnPan.HorizontalAlignment = HorizontalAlignment.Right;
                        titlePan.Children.Add(btnPan);
                        titlePan.Background = color.Gray();
                        conPan.Children.Add(titlePan);
                        scroll.Content = filmPanel;
                        conPan.Children.Add(scroll);
                        datePanel.Children.Add(conPan);
                        yearPan.Children.Add(datePanel);
                    }
                    catch (Exception e)
                    {
                        //log.Error(e.Message);
                        //log.SoftError();
                    }
                }
                yr.Content = yearPan;
                yearDock.Children.Add(yr);
                listPanel.Children.Add(yearDock);
            }
        }

        private void CarBtn_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            try
            {
                Process.Start(b.Tag.ToString());
            }
            catch (Exception x)
            {
                log.BadLog($"Exception: {x.Message}\n{b.Tag.ToString()}");
                sound.SoftError();
            }
        }

        private void DateBtn_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Process.Start(b.Tag.ToString());
        }

        private void Films(Film film, StackPanel filmPanel)
        {
            DockPanel titlePanel = new DockPanel();
            DockPanel buttonPanel = new DockPanel();
            Label time = new Label();
            Label title = new Label();
            Button moveBtn = new Button();
            Button delBtn = new Button();
            title.Foreground = color.White();
            time.Content = string.Format("[{0}]", film.Date.ToString("HH:mm"));
            time.Foreground = color.LightGray();
            titlePanel.Children.Add(time);
            moveBtn.Content = "Copy";
            delBtn.Content = "X";
            moveBtn.Height = 20;
            moveBtn.Opacity = 0.3;
            delBtn.Width = 20;
            delBtn.Height = 20;
            delBtn.Opacity = 0.3;
            delBtn.Tag = film.Source;
            delBtn.Click += DelBtn_Click;
            moveBtn.DataContext = film;
            moveBtn.Click += MoveBtn_Click;
            moveBtn.Background = color.Black();
            delBtn.Background = color.Red();
            moveBtn.Foreground = color.White();
            delBtn.Foreground = color.White();
            buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;
            if (film.Report != null)
            {
                Expander rePander = new Expander();
                string name;
                if (film.Clips.Count() > 0)
                    name = "[>] " + film.Name;
                else
                    name = film.Name;
                rePander.Header = name;
                rePander.Foreground = color.Cyan();
                rePander.Content = Carnage(film.Report, film.Clips);
                titlePanel.Children.Add(rePander);
            }
            else
            {
                title.Content = film.Name;
                if (film.Clips.Count() > 0)
                {
                    titlePanel.Children.Add(VXpander(film.Clips, "[>] " + film.Name));
                }
                else
                {
                    titlePanel.Children.Add(title);
                }
            }
            buttonPanel.Children.Add(moveBtn);
            buttonPanel.Children.Add(delBtn);
            titlePanel.Children.Add(buttonPanel);
            filmPanel.Children.Add(titlePanel);
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            File.Delete(b.Tag.ToString());
            GetApps();
        }

        private void MoveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FilesController files = new FilesController();
                Button b = sender as Button;
                files.CopyFilm((Film)b.DataContext);
                GetApps();
            }
            catch (Exception x)
            {
                //log.Error("Couldn't copy file: " + x.Message);
            }
        }

        public void GetApps()
        {
            win.appStack = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault().appStack;
            win.appStack.Children.Clear();
            Storage.AppGames.Clear();
            local.GetGames(Settings.AppData);
            ListGames(Settings.AppData);
        }

        Expander VXpander(List<Clip> clips, string name)
        {
            Expander vPander = new Expander();
            StackPanel vPanel = new StackPanel();
            ListView clipList = new ListView();
            vPander.Header = name;
            vPander.Foreground = color.White();
            clipList.DataContext = clips;
            clipList.ItemsSource = clips;
            foreach (Clip clip in clips)
            {
                vPanel.Children.Add(Video(clip));
            }
            vPander.Content = vPanel;
            return vPander;
        }

        Button Video(Clip clip)
        {
            Button btn = new Button();
            btn.Background = new SolidColorBrush(Colors.Black);
            btn.Foreground = new SolidColorBrush(Colors.White);
            btn.Opacity = 0.3;
            btn.Height = 20;
            btn.Content = clip.Name;
            btn.Tag = clip.Path;
            btn.Click += new RoutedEventHandler(vidClick);
            return btn;
        }

        private void vidClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button b = sender as Button;
                Process.Start(b.Tag.ToString());
            }
            catch (Exception eX)
            {
                //log.Error(eX.Message);
            }
        }

        private StackPanel Carnage(Carnage report, List<Clip> clips)
        {
            StackPanel mainPanel = new StackPanel();
            DockPanel listPanel = new DockPanel();
            mainPanel.Background = color.Gray();
            mainPanel.Children.Add(CarnageBodyMain(report, clips));
            mainPanel.Children.Add(listPanel);
            return mainPanel;
        }
        private StackPanel CarnageBodyMain(Carnage report, List<Clip> clips)
        {
            StackPanel mainPanel = new StackPanel();
            Expander headPanel = new Expander();
            StackPanel bigPanel = new StackPanel();
            StackPanel contentPanel = new StackPanel();
            DockPanel modePanel = new DockPanel();
            StackPanel infoPanel = new StackPanel();
            DockPanel statsPanel = new DockPanel();
            DockPanel listPanel = new DockPanel();
            DockPanel dePanel = new DockPanel();
            Label mode = new Label() { Padding = new Thickness(0, 0, 0, 0), Foreground = color.White() };
            Label time = new Label() { Padding = new Thickness(0, 0, 0, 0), Foreground = color.Cyan(), Content = report.Time };
            Label team = new Label() { Padding = new Thickness(0, 0, 0, 0), Foreground = color.Cyan() };

            if (report.Player != null)
            {
                team.Content = report.Player.Team;
                Label score = new Label() { Padding = new Thickness(0, 0, 0, 0), BorderBrush = color.Cyan(), BorderThickness = new Thickness(0, 0, 0, 1), Foreground = color.Cyan(), Content = $"{report.Player.Score}Pts" };
                statsPanel.Children.Add(PlayerPanel(report.Player));
                infoPanel.Children.Add(score);
                infoPanel.Children.Add(team);
            }
            switch (report.GetType().Name)
            {
                case "PVECarnage":
                    PVECarnage pve = (PVECarnage)report;
                    mode.Content = pve.Difficulty;
                    dePanel = PVEPanel(pve);
                    break;
                case "MPCarnage":
                    MPCarnage pvp = (MPCarnage)report;
                    mode.Content = pvp.GameMode;
                    break;
            }
            modePanel.Children.Add(mode);
            headPanel.Header = modePanel;
            headPanel.IsExpanded = true;
            infoPanel.Children.Add(time);
            contentPanel.Children.Add(infoPanel);
            statsPanel.HorizontalAlignment = HorizontalAlignment.Right;
            contentPanel.Children.Add(statsPanel);
            contentPanel.Orientation = Orientation.Horizontal;
            bigPanel.Children.Add(contentPanel);
            if (report.Players.Count() > 1)
            {
                Expander players = new Expander() { Foreground = color.White() };
                StackPanel playerPan = new StackPanel();
                players.Content = Players(report);
                players.Header = "Players";
                listPanel.Children.Add(players);
            }
            if (report.Player.Medals.Count() > 0)
            {
                Expander expander = new Expander() { Header = "Medals", Foreground = color.White() };
                ListView medals = new ListView() { Background = color.DarkGray(), Foreground = color.White() };
                foreach (Medal medal in report.Player.Medals.OrderByDescending(m => m.Group))
                {
                    string mdl = $"{medal.Count}x {medal.Name}";
                    //if (medal.Group != null)
                    //    mdl += string.Format(" <{0}>", medal.Group);
                    medals.Items.Add(mdl);
                }
                expander.Content = medals;
                listPanel.Children.Add(expander);
            }
            if (clips.Count > 0)
            {
                listPanel.Children.Add(VXpander(clips, "Clips"));
            }
            bigPanel.Children.Add(dePanel);
            bigPanel.Children.Add(listPanel);
            headPanel.Content = bigPanel;
            mainPanel.Children.Add(headPanel);
            return mainPanel;
        }
        private DockPanel PVEPanel(PVECarnage report)
        {
            DockPanel mainDock = new DockPanel();
            if (report.Enemies != null)
            {
                mainDock.Children.Add(Enemies(report.Enemies));
            }
            if (report.Skulls != null)
            {
                mainDock.Children.Add(Skulls(report.Skulls));
            }
            return mainDock;
        }

        private StackPanel PlayerPanel(Player player)
        {
            StackPanel mainStack = new StackPanel();
            DockPanel scorePanel = new DockPanel();
            DockPanel kdrPanel = new DockPanel();
            DockPanel top = new DockPanel();
            DockPanel center = new DockPanel();
            DockPanel bottom = new DockPanel();
            Label kdr = new Label() { Padding = new Thickness(0, 0, 0, 0), Foreground = color.Cyan(), Content = string.Format("{0}", player.Kdr), HorizontalAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Top };
            Label kills = new Label() { Padding = new Thickness(0, 0, 0, 0), Foreground = color.Cyan(), Content = string.Format("{0}<K/", player.Kills) };
            Label deaths = new Label() { Padding = new Thickness(0, 0, 0, 0), Foreground = color.Cyan(), Content = string.Format("D>{0}", player.Deaths) };
            Label assists = new Label() { Padding = new Thickness(0, 0, 0, 0), Foreground = color.Cyan(), Content = string.Format("{0}", player.Assists), HorizontalAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Bottom };
            top.Children.Add(assists);
            center.Children.Add(kills);
            center.Children.Add(deaths);
            bottom.Children.Add(kdr);
            StackPanel stack = new StackPanel();
            stack.Children.Add(top);
            stack.Children.Add(center);
            stack.Children.Add(bottom);
            scorePanel.HorizontalAlignment = HorizontalAlignment.Center;
            stack.HorizontalAlignment = HorizontalAlignment.Center;
            mainStack.Children.Add(scorePanel);
            mainStack.Children.Add(stack);
            return mainStack;
        }

        private StackPanel Players(Carnage rep)
        {
            StackPanel mainPanel = new StackPanel();
            DockPanel topPan = new DockPanel();
            DockPanel botPan = new DockPanel();
            ListView players = new ListView();
            ListView reds = new ListView();
            ListView blues = new ListView();
            ListView grif = new ListView();
            ListView green = new ListView();
            string name = "";
            if (rep.Players.GroupBy(p => p.Team).Count() > 1)
            {
                foreach (Player player in rep.Players)
                {
                    name = PlayerNameCheck(rep, player, name);
                    switch (player.Team)
                    {
                        case "Red":
                            reds.Items.Add(name);
                            break;
                        case "Blue":
                            blues.Items.Add(name);
                            break;
                        case "Orange":
                            grif.Items.Add(name);
                            break;
                        case "Green":
                            green.Items.Add(name);
                            break;
                        default:
                            players.Items.Add(name);
                            break;
                    }
                }
                reds.Background = color.Red();
                reds.Foreground = color.White();
                blues.Background = color.Blue();
                blues.Foreground = color.White();
                grif.Background = color.Orange();
                grif.Foreground = color.White();
                green.Background = color.Green();
                green.Foreground = color.White();
                topPan.Children.Add(reds);
                topPan.Children.Add(blues);
                botPan.Children.Add(grif);
                botPan.Children.Add(green);
                mainPanel.Children.Add(topPan);
                mainPanel.Children.Add(botPan);
            }
            else
            {
                players.Background = color.DarkGray();
                players.BorderBrush = color.DarkGray();
                players.Foreground = color.White();
                foreach (Player player in rep.Players)
                {
                    name = PlayerNameCheck(rep, player, name);
                    players.Items.Add(name);
                }
                mainPanel.Children.Add(players);
            }
            return mainPanel;
        }

        string PlayerNameCheck(Carnage rep, Player player, string name)
        {
            if (player.GamerTag == Settings.GamerTag)
            {
                name = "You";
            }
            else
            {
                name = player.GamerTag;
            }
            return name;
        }

        private Expander Enemies(List<Enemy> enemies)
        {
            Expander ePan = new Expander();
            ListView eList = new ListView();
            foreach (Enemy enemy in enemies)
            {
                eList.Items.Add($"{enemy.Count}X {enemy.EnemyType}.{enemy.EnemyClass}");
            }
            eList.Foreground = color.White();
            eList.Background = color.DarkGray();
            eList.BorderBrush = color.DarkGray();
            ePan.Foreground = color.White();
            ePan.Header = "Enemies";
            ePan.Content = eList;
            return ePan;
        }

        private Expander Skulls(List<string> skulls)
        {
            Expander skPan = new Expander();
            ListView sList = new ListView();
            foreach (string skull in skulls)
            {
                sList.Items.Add(skull);
            }
            sList.Foreground = color.White();
            sList.Background = color.DarkGray();
            sList.BorderBrush = color.DarkGray();
            skPan.Foreground = color.White();
            skPan.Header = "Skulls";
            skPan.Content = sList;
            return skPan;
        }

        public void TimerOn()
        {
            tick.Timer(this);
            win.autoLab.Foreground = color.Green();
            win.autoLab.Content = "Autorefresh enabled";
        }
        public void TimerOff()
        {
            tick.Stop();
            win.autoLab.Foreground = color.LightGray();
            win.autoLab.Content = "Autorefresh disabled";
        }

        public void MultiOn()
        {
            Settings.MultiThreading = true;
            win.multiLab.Foreground = color.Green();
            win.multiLab.Content = "Multithreading enabled";
        }
        public void MultiOff()
        {
            Settings.MultiThreading = false;
            win.multiLab.Foreground = color.LightGray();
            win.multiLab.Content = "Multithreading disabled";
        }

        public void SetSound(string item)
        {
            try
            {
                sound.Custom(win.soundBox.Text);
                Settings.SetSound(item, win.soundBox.Text);
                log.MsgLog($"Set audio for '{item}' to: '{win.soundBox.Text}'");
            }
            catch (Exception e)
            {
                log.BadLog($"Failed to set audio:\n{e.Message}\nMake sure the file is '.wav'.");
            }
        }
    }
}
