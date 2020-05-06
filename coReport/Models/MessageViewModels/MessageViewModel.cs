using coReport.Models.MessageModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.MessageViewModels
{
    public class MessageViewModel
    {
        public short Id { get; set; }
        public String AuthorName { get; set; }
        public String Title { get; set; }
        public DateTime Time { get; set; }
        public String Text { get; set; }
        public bool IsViewed { get; set; }
        public MessageType Type { get; set; }
    }
}
