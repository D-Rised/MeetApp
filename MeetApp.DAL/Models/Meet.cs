using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetApp.DAL.Models
{
    public class Meet
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [ForeignKey("meetId")]
        public IList<Dates> DatesList { get; set; }

        [Required]
        [ForeignKey("meetId")]
        public IList<Member> MembersList { get; set; }

        public bool FixedDate { get; set; }

        public string State { get; set; }
    }
}
