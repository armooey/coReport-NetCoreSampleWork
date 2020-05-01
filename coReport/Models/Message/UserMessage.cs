using coReport.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.Message
{
    public class UserMessage
    {
        public short MessageId { get; set; }
        public Message Message { get; set; }
        public short ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }
        public bool IsViewd { get; set; }
    }
}
