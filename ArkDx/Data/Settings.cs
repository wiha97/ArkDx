using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Xml.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ArkDx.Data
{
    public static class Settings
    {
        #region variables
        private static string xml = "settings.xml";
        private static string[] sounds = { "SoftError", "Error", "NewFilm", "SavedFilm", "Delete", "Carnage" };
        private static string[] attributes = { "url", "version", "theaterLimit", "copyMin", "arkPath", "gamerTag", "auto", "multi", "sizeLimit" };
        public static string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string ProjectUrl { get; set; } = @"";
        public static int AppMin { get; set; } = 7;
        public static int AppLimit { get; set; } = 12;
        public static string AppData { get; set; } = "";
        public static string ArkPath { get; set; } = Directory.GetCurrentDirectory();
        public static string GamerTag { get; set; } = "";
        public static string MedalSource { get; set; } = @"Library\mpmedals.txt";
        public static bool AutoRefresh { get; set; } = false;
        public static bool MultiThreading { get; set; } = true;
        public static long FilmSizeLimit { get; set; } = 500;
        public static string SoftErrorSFX { get; set; } = @"C:\Windows\Media\chord.wav";
        public static string ErrorSFX { get; set; } = @"C:\Windows\Media\Windows Foreground.wav";
        public static string NewFilmSFX { get; set; } = @"C:\Windows\Media\notify.wav";
        public static string SavedFilmSFX { get; set; } = @"C:\Windows\Media\chimes.wav";
        public static string DeleteSFX { get; set; } = @"C:\Windows\Media\Windows Recycle.wav";
        public static string CarnageSFX { get; set; } = @"C:\Windows\Media\ding.wav";
        public static List<string> Clips { get; set; } = new List<string>();
        #endregion

        public static void GetApp() // Gets the path for the AppData directory
        {
            //  For potential Linux version
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string winUser = WindowsIdentity.GetCurrent().Name;
                winUser = winUser.Substring(winUser.LastIndexOf('\\') + 1);
                AppData = $@"C:\Users\{winUser}\AppData\LocalLow\MCC\Temporary";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //  Probably have to give WINE/Proton path manually
            }
        }

        public static void AddClip(string path) //  Adds a path to the clip list and saves it to the xml
        {

            XDocument doc = XDocument.Load(xml);
            doc.Root.Element("Clips").Add(
                new XElement("Clip",
                new XAttribute("path", path))
                );
            doc.Save(xml);
            Clips.Add(path);
            Fill();
        }

        public static void SetSound(string sound, string source)    //  Sets the sound effect for notifications
        {
            XDocument doc = XDocument.Load(xml);
            IEnumerable<XElement> snds = doc.Root.Element("Sounds").Elements("Sound");
            XElement snd = snds.Where(s => s.Attribute("sound").Value == sound).FirstOrDefault();
            snd.Attribute("source").Value = source;
            doc.Save(xml);
            ReadSettings();
        }

        public static void ApplySettings(int appLim, string arkPath, string gt, int sizeLim, string update, bool mt, bool auto) //  Applies the applicable settings and saves them to settings.xml
        {
            AppMin = appLim;
            ArkPath = arkPath;
            GamerTag = gt;
            FilmSizeLimit = sizeLim;
            ProjectUrl = update;
            MultiThreading = mt;
            AutoRefresh = auto;

            UpdateXml();
        }

        public static void ReadSettings()  // Reads the settings.xml file and saves its values
        {
            try
            {
                if (File.Exists(xml))
                {
                    XDocument doc = XDocument.Load(xml);
                    XElement set = doc.Root;
                    XElement snd = set.Element("Sounds");
                    AppMin = int.Parse(set.Attribute("copyMin").Value);
                    AppLimit = int.Parse(set.Attribute("theaterLimit").Value);
                    ArkPath = set.Attribute("arkPath").Value;
                    GamerTag = set.Attribute("gamerTag").Value;
                    AutoRefresh = Boolean.Parse(set.Attribute("auto").Value);
                    MultiThreading = Boolean.Parse(set.Attribute("multi").Value);
                    //Version = set.Attribute("version").Value;
                    ProjectUrl = set.Attribute("url").Value;
                    FilmSizeLimit = int.Parse(set.Attribute("sizeLimit").Value);

                    ReadSounds(snd);

                    foreach (XElement clip in doc.Root.Element("Clips").Elements("Clip"))
                    {
                        Clips.Add(clip.Attribute("path").Value);
                    }
                    //clipPath = set.Attribute("clips").Value;
                }
                else
                {
                    GenXML();
                }
                GetApp();
                Fill();
            }
            catch (Exception e)
            {
                GenXML();
                GetApp();
                Fill();
            }
        }

        static void ReadSounds(XElement snd)    //  Reads the saved sound paths from the xml and applies them
        {
            IEnumerable<XElement> snds = snd.Elements("Sound");

            foreach (string sound in sounds)
            {
                XElement sn = snds.Where(s => s.Attribute("sound").Value == sound).FirstOrDefault();
                string value = sn.Attribute("source").Value;
                switch (sound)
                {
                    case "SoftError":
                        SoftErrorSFX = value;
                        break;
                    case "Error":
                        ErrorSFX = value;
                        break;
                    case "NewFilm":
                        NewFilmSFX = value;
                        break;
                    case "SavedFilm":
                        SavedFilmSFX = value;
                        break;
                    case "Delete":
                        DeleteSFX = value;
                        break;
                    case "Carnage":
                        CarnageSFX = value;
                        break;
                    default:
                        break;
                }
            }
        }

        static void Fill()      // Fills in the GUI
        {
            MainWindow win = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            win.appSlide.Value = AppMin;
            win.pathBox.Text = ArkPath;
            win.gtBox.Text = GamerTag;
            win.urlBox.Text = ProjectUrl;
            win.clipList.Items.Clear();
            foreach (string clip in Clips)
            {
                win.clipList.Items.Add(clip);
            }
            if (AutoRefresh == true)
            {
                win.autoBox.IsChecked = true;
            }
            if (MultiThreading == true)
            {
                win.multiBox.IsChecked = true;
            }
            win.SoundCombo.ItemsSource = sounds;
            win.fileSizeSlide.Value = FilmSizeLimit;
            win.vLab.Content = Version;
        }

        public static void GenXML()    // Generates a new settings.xml file
        {
            if (!File.Exists(xml))
            {
                CreateXML(xml);
            }
            UpdateXml();
        }

        static void UpdateXml() //  Updates settings.xml with all changes
        {
            XDocument doc = XDocument.Load(xml);
            XElement set = doc.Root;
            XElement snd = doc.Root.Element("Sounds");
            if(snd == null)
            {
                set.Add(new XElement("Sounds"));
                snd = set.Element("Sounds");
            }
            AddAttributes(set);
            AddSounds(snd);
            doc.Save(xml);
        }

        static void AddAttributes(XElement element) //  Adds or updates attributes
        {
            foreach (string attribute in attributes)
            {
                try
                {
                    XAttribute att = element.Attribute(attribute);
                    string value = "";
                    switch (attribute)
                    {
                        case "url":
                            value = ProjectUrl;
                            break;
                        case "version":
                            value = Version;
                            break;
                        case "theaterLimit":
                            value = AppLimit.ToString();
                            break;
                        case "copyMin":
                            value = AppMin.ToString();
                            break;
                        case "arkPath":
                            value = ArkPath;
                            break;
                        case "gamerTag":
                            value = GamerTag;
                            break;
                        case "auto":
                            value = AutoRefresh.ToString();
                            break;
                        case "multi":
                            value = MultiThreading.ToString();
                            break;
                        case "sizeLimit":
                            value = FilmSizeLimit.ToString();
                            break;
                        default:
                            break;
                    }
                    if (att == null)
                    {
                        element.Add(new XAttribute(attribute, value));
                    }
                    else
                    {
                        att.Value = value;
                    }
                }
                catch
                {

                }
            }
        }

        static void AddSounds(XElement element) //  Fills in sounds
        {
            foreach (string sound in sounds)
            {
                XElement snd = element.Elements("Sound").Where(s => s.Attribute("sound").Value == sound).FirstOrDefault();
                string value = ErrorValue(sound);
                if (snd == null)
                {
                    try
                    {
                        element.Add(new XElement("Sound", new XAttribute("sound", sound), new XAttribute("source", value)));
                    }
                    catch(Exception e)
                    {

                    }
                }
                else
                {
                    snd.Attribute("source").Value = value;
                }
            }
        }

        public static string ErrorValue(string sound)
        {
            string value = "";
            switch (sound)
            {
                case "SoftError":
                    value = SoftErrorSFX;
                    break;
                case "Error":
                    value = ErrorSFX;
                    break;
                case "NewFilm":
                    value = NewFilmSFX;
                    break;
                case "SavedFilm":
                    value = SavedFilmSFX;
                    break;
                case "Delete":
                    value = DeleteSFX;
                    break;
                case "Carnage":
                    value = CarnageSFX;
                    break;
                default:
                    break;
            }
            return value;
        }

        static void CreateXML(string file)
        {
            XDocument doc = new XDocument(new XElement("Settings"));
            doc.Root.Add(new XElement("Clips"));
            doc.Save(file);
        }

    }
}
