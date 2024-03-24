﻿using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
        }

        public int CId { get; set; }
        public string Name { get; set; } = null!;
        public ushort Number { get; set; }
        public string Listing { get; set; } = null!;

        public virtual Department ListingNavigation { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
