using System;
using System.Collections.Generic;

namespace ArkDx.Models
{
    public class Film
    {
        public string Name { get; set; }        //  Name of file or translated name if translation exists
        public string Source { get; set; }      //  Source path of the file
        public string TargetFile { get; set; }  //  Full file path to where the file should be moved/copied
        public string TargetPath { get; set; }  //  Directory path to where the file should be moved/copied
        public DateTime Date { get; set; }      //  Time of creation
        public Carnage Report { get; set; }     //  Paired carnage report
        public List<Clip> Clips { get; set; }   //  Paired clips
    }
}
