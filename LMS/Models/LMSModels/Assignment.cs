using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public string Name { get; set; } = null!;
        public ushort Points { get; set; }
        public string Contents { get; set; } = null!;
        public DateTime Due { get; set; }
        public int AId { get; set; }
        public int AcId { get; set; }

        public virtual AssignmentCategory Ac { get; set; } = null!;
    }
}
