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
        public string title { get; set; }

        [Required]
        [ForeignKey("meetId")]
        public IList<Dates> datesList { get; set; }

        [Required]
        [ForeignKey("meetId")]
        public IList<Member> membersList { get; set; }

        public bool fixedDate { get; set; }

        public string state { get; set; }
    }
}
