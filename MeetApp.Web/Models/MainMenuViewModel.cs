using MeetApp.DAL.Models;
using System;
using System.Collections.Generic;

namespace MeetApp.Models
{
    public class MainMenuViewModel
    {
        public string UserLogin { get; set; }
        public IList<Meet> OwnedMeets { get; set; }
        public IList<Meet> MemberMeets { get; set; }
        public string AlertMessage { get; set; }
    }
}
