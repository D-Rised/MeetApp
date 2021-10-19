using System;

namespace MeetApp.Models
{
    public class DatesViewModel
    {
        public int Id { get; set; }
        public Guid meetId { get; set; }
        public Guid memberId { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
        public string memberLogin { get; set; }
    }
}
