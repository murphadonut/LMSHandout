using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrollments = new HashSet<Enrollment>();
        }

        public ushort Year { get; set; }
        public string Season { get; set; } = null!;
        public int CId { get; set; }
        public int ClassId { get; set; }
        public string Location { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Teacher { get; set; } = null!;

        public virtual Course CIdNavigation { get; set; } = null!;
        public virtual Professor TeacherNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
