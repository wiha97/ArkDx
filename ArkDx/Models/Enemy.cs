using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkDx.Models
{
    public class Enemy
    {
        public string EnemyType { get; set; }
        public string EnemyClass { get; set; }
        public int Count { get; set; }
        public bool Betray { get; set; }
    }
}
