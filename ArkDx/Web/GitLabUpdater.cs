using System;
using System.Collections.Generic;
using System.Linq;
using ArkDx.WPF;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace ArkDx
{
    public class GitLabUpdater
    {
        private Logger log = new Logger();
        private IList<Release> releases;
        private string gitlab;
        private string download;
        string json = "";
        public void GetReleases(string url, string version)
        {
            try
            {
                gitlab = url;
                download = "";

                //  Gets the releases page's json
                using (WebClient web = new WebClient())
                {
                    json = web.DownloadString($"{gitlab}/-/releases.json");
                }

                //  Json is sandwiched with brackets, have to be removed
                int ida = json.IndexOf('[') + 1;
                int idb = json.LastIndexOf(']') - 1;
                json = json.Substring(ida, idb);

                //  Enables multiple content support, required for multiple objects
                JsonTextReader reader = new JsonTextReader(new StringReader(json));
                reader.SupportMultipleContent = true;

                //  Creates a new "release" object of each json object
                releases = new List<Release>();
                while (true)
                {
                    try
                    {
                        if (!reader.Read())
                        {
                            break;
                        }
                        JsonSerializer serializer = new JsonSerializer();
                        Release rel = serializer.Deserialize<Release>(reader);
                        releases.Add(rel);
                    }
                    catch (Exception e)
                    {
                        log.BadLog($"Failed to convert json:\n{e.Message}");
                    }
                }

                //  Checks if the current version is the latest compared to the gitlab releases
                Release latest = releases.FirstOrDefault();
                if (latest != null)
                {
                    if (latest.Tag != version)
                    {
                        //  Tries to split download link from the description
                        CropInfo(latest);
                    }
                    else
                    {
                        log.MsgLog("No updates found");
                    }
                }
                else
                {
                    log.BadLog("No releases found");
                }
            }
            catch (Exception e)
            {
                log.BadLog($"Failed to check for updates:\n{e.Message}");
            }
        }

        //  Shows all releases
        public void ListAll()
        {
            Console.WriteLine($"All releases of {gitlab}");
            if (releases.Count > 0)
            {
                foreach (Release rel in releases)
                {
                    CropInfo(rel);
                }
            }
            else
            {
                log.BadLog("No releases found");
            }
        }

        public void ShowJson()
        {
            log.MsgLog(json);
        }

        //  Tries to separate the description and download to make it easier to download the updated release, may not work for all
        void CropInfo(Release rel)
        {
            string description = rel.Description;
            string link = $"{gitlab}/-/releases/{rel.Tag}";

            //  Gets the last download URL for easy download
            try
            {
                int id1 = description.LastIndexOf("](") + 2;
                int id2 = description.LastIndexOf(')');
                download = gitlab + description.Substring(id1, id2 - id1);
            }
            catch (Exception e)
            {
                log.ErrorMsg($"Failed to get download link:\n{e.Message}\nPlease go to '{link}' in your browser");
            }

            //  Checks so the last link is a file hosted on gitlab, if not it will show the entire description
            if (download.Contains("/uploads/"))
            {
                link = download;
                description = description.Substring(0, description.LastIndexOf('['));
            }
            log.UpdateMsg(description, rel.Tag, link);
        }
    }

    public class Release
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public string Sha { get; set; }
        public string ReleasedAt { get; set; }
        public string Json { get; set; }
    }
}
