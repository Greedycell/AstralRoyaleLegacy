using System;
using System.Collections.Generic;

namespace ClashRoyale.Logic.Home
{
    public class Achievements : List<Achievement>
    {
        public new void Add(Achievement achievement)
        {
            if (!Contains(achievement))
                base.Add(achievement);
        }
    }
}