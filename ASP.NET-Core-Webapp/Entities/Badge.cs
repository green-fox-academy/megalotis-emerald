
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_Webapp.Entities
{
    public class Badge
    {
        public string Name { get; set; }
        public ICollection<BadgeLevel> Levels { get; set; }
        public string Tag { get; set; }
        public string Version { get; set; }

        public Badge()
        {
            this.Levels = new List<BadgeLevel>();
        }

        public Badge(string name)
        {
            this.Name = name;
            this.Levels = new List<BadgeLevel>();
        }
    }
}
