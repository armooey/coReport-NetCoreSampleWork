using coReport.Models.MessageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IMessageService
    {
        public void Add(Message message, IEnumerable<short> receivers);
        public void AddManagerReviewMessage(Message message, short receiverId);
        public void AddSystemNotificationForAdmin(Message message);
        public void CreateSystemNotifications();
        public void DeleteManagerReviewMessage(String helperId);
        public Message Get(short id);
        public IEnumerable<UserMessage> GetReceivedMessages(short userId);
        public void SetViewed(short id);
        public int GetWarningsCount(short userId);
        public void Delete(short id);
    }
}
