using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public DateTime SubmittedOn { get; set; }
        public ushort? Score { get; set; }
        public string? Contents { get; set; }
        public int AId { get; set; }
        public string Student { get; set; } = null!;

        public virtual Assignment AIdNavigation { get; set; } = null!;
        public virtual Student StudentNavigation { get; set; } = null!;
    }
}
