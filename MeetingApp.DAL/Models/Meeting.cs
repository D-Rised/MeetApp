using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetingApp.DAL.Models
{
    public class Meeting
    {
        public Guid Id { get; set; }

        [Required]
        public string title { get; set; }

        [Required]
        [ForeignKey("meetingId")]
        public IList<Dates> datesList { get; set; }

        [Required]
        [ForeignKey("meetingId")]
        public IList<Members> membersList { get; set; }

        public DateTime dateFinal { get; set; }

        public string state { get; set; }
    }
}
