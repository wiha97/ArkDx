using System;
using System.Collections.Generic;
using System.Linq;
using ArkDx.WPF;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace ArkDx.Web
{
    public class Updater
    {
        private Logger log = new Logger();
        public void GetReleases(string url, string version)
        {
            //  All-nighter for this gem, apparently most json libraries don't support multiple root objects
            try
            {
                string gitlab = url;
                string download = "";
                string data = "";

                //  Gets the releases page's json
                using (WebClient web = new WebClient())
                {
                    data = web.DownloadString($"{gitlab}/-/releases.json");
                }

                //  Json is sandwiched with brackets, have to be removed
                int ida = data.IndexOf('[') +1;
                int idb = data.LastIndexOf(']') -1;
                data = data.Substring(ida, idb);

                //  Enables multiple content support, required for multiple objects
                JsonTextReader reader = new JsonTextReader(new StringReader(data));
                reader.SupportMultipleContent = true;

                //  Creates a new "release" object of each json object
                IList<Release> releases = new List<Release>();
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
                    catch(Exception e)
                    {
                        log.BadLog($"Failed to convert json:\n{e.Message}");
                    }
                }

                //  Checks if the current version is the latest compared to the gitlab releases
                Release latest = releases.FirstOrDefault();
                if(latest.Tag != version)
                {
                    string description = latest.Description;
                    
                    //  Gets the download URL for easy download, hinges on the file being at the end of the description
                    try
                    {
                        int id1 = description.LastIndexOf("(/uploads/") +1;
                        int id2 = description.LastIndexOf(')');

                        download = gitlab + description.Substring(id1, id2 - id1);
                    }
                    catch (Exception e)
                    {
                        log.ErrorMsg($"Failed to get download link:\n{e.Message}\nPlease go to '{gitlab}/-/releases/{latest.Tag}' in your browser");
                    }
                    
                    //  Cuts out the download string from the description and lets the user know what the changes are before updating
                    description = description.Substring(0, description.IndexOf('['));
                    log.UpdateMsg(description, latest.Tag, download);
                }
                else
                {
                    log.MsgLog("No updates");
                }
            }
            catch (Exception e)
            {
                log.ErrorMsg($"Failed to check for updates:\n{e.Message}");
            }
        }
    }

    public class Release
    {
        //[JsonPropertyName("id")]
        public int Id { get; set; }
        //[JsonPropertyName("tag")]
        public string Tag { get; set; }
        //[JsonPropertyName("description")]
        public string Description { get; set; }
        //[JsonPropertyName("project_id")]
        public int ProjectId { get; set; }
        //[JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
        //[JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }
        //[JsonPropertyName("author_id")]
        public int AuthorId { get; set; }
        //[JsonPropertyName("name")]
        public string Name { get; set; }
        //[JsonPropertyName("sha")]
        public string Sha { get; set; }
        //[JsonPropertyName("released_at")]
        public string ReleasedAt { get; set; }
    }
}
