using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingApp.DAL.Models
{
    public class Meeting
    {
        public Guid Id { get; set; }

        [Required]
        public string title { get; set; }

        [Required]
        [ForeignKey("MeetingId")]
        public IList<Dates> datesList { get; set; }

        public DateTime dateFinal { get; set; }

        public int user_Id { get; set; }

        public string state { get; set; }
    }
}
