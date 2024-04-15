﻿using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Student
    {
        public Student()
        {
            Enrollments = new HashSet<Enrollment>();
            Submissions = new HashSet<Submission>();
        }

        public string UId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly Dob { get; set; }
        public string Major { get; set; } = null!;

        public virtual Department MajorNavigation { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
