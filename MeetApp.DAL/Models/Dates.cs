using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetApp.DAL.Models
{
    public class Dates
    {
        public int Id { get; set; }
        public Guid meetId { get; set; }
        public Guid userId { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
    }
}
