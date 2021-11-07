using System;
using System.IO;

namespace ArkDx.Logic
{
    public class MainController
    {
        public string Translate(string name, string file)   //  Checks the string before the comma for reference then replaces it with the string to the right
        {
            file = string.Format(@"Library\{0}.txt", file);
            if (File.Exists(file))
            {
                try
                {
                    foreach (string line in File.ReadLines(file))
                    {
                        if (name.Contains(line.Split(',')[0]))
                        {
                            name = line.Split(',')[1];
                            break;
                        }
                    }
                }
                catch(Exception e)
                {

                }
            }

            return name;
        }
    }
}
