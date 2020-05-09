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
        public void AddManagerReviewMessage(Message message,short managerReportId, short receiverId);
        public void AddSystemNotificationForAdmin(Message message);
        public void DeleteManagerReviewMessage(short managerReportId);
        public Message Get(short id);
        public IEnumerable<UserMessage> GetReceivedMessages(short userId);
        public void SetViewed(short id);
        public int GetWarningsCount(short userId);
        public IEnumerable<UserMessage> GetWarnings();
        public void Delete(short id);
    }
}
