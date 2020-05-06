using coReport.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.MessageModels
{
    public class Message
    {
        public short Id { get; set; }
        public String Title { get; set; }
        public short SenderId { get; set; }
        public DateTime Time { get; set; }
        public ApplicationUser Sender { get; set; }
        public MessageType Type { get; set; }
        public String Text { get; set; }
        public String HelperId { get; set; } //Used for purposes like manager report notifiacations
        public ICollection<UserMessage> Receivers { get; set; }
    }
}
