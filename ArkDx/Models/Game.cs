using System.Collections.Generic;

namespace ArkDx.Models
{
    public class Game
    {
        public string Name { get; set; }
        public IEnumerable<Film> Films { get; set; }
    }
}
