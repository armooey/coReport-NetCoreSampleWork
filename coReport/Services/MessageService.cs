using coReport.Data;
using coReport.Models.MessageModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace coReport.Services
{
    public class MessageService : IMessageService
    {
        private ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }



        public void Add(Message message, IEnumerable<short> receivers)
        {
            try
            {
                _context.Messages.Add(message);
                _context.SaveChanges();
                foreach (var receiver in receivers)
                {
                    _context.UserMessages.Add(new UserMessage { MessageId = message.Id, ReceiverId = receiver, IsViewd = false });
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddManagerReviewMessage(Message message, short receiverId)
        {
            try
            {
                if (_context.Messages.Any(m => m.HelperId == message.HelperId && m.Title == "گزارش مدیر"))
                    _context.Messages.Where(m => m.HelperId == message.HelperId).Update(m => new Message { Text = message.Text, Time = DateTime.Now });
                else
                {
                    _context.Messages.Add(message);
                    _context.SaveChanges();
                    _context.UserMessages.Add(new UserMessage { MessageId = message.Id, ReceiverId = receiverId });
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddSystemNotificationForAdmin(Message message)
        {
            try
            {
                _context.Messages.Add(message);
                _context.SaveChanges();
                _context.UserMessages.Add(new UserMessage { MessageId = message.Id, ReceiverId = 1 });
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void CreateSystemNotifications()
        {
            try
            {
                var unseenAlerts = _context.UserMessages.Where(um => um.Message.Type == MessageType.Warning && um.IsViewd == false
                                                                     && um.Message.Time <= DateTime.Now.AddHours(-12))
                                                        .Include(um => um.Receiver).Include(um => um.Message).ToList();

                foreach (var unseenAlert in unseenAlerts)
                {
                    if (!_context.Messages.Any(m => m.HelperId == unseenAlert.Message.HelperId && m.Title == "اخطار مشاهده نشده"))
                    {
                        var alertReceiver = unseenAlert.Receiver;
                        var message = new Message
                        {
                            Title = "اخطار مشاهده نشده",
                            Text = String.Format("کاربر {0} یک اخطار مشاهده نشده با عنوان {1} دارد که از زمان آن 12 ساعت می‌گذرد.",
                                    String.Concat(alertReceiver.FirstName, " ", alertReceiver.LastName), unseenAlert.Message.Title),
                            Time = DateTime.Now,
                            Type = MessageType.System_Notification,
                            SenderId = 1,
                            HelperId = unseenAlert.Message.HelperId
                        };
                        _context.Messages.Add(message);
                        _context.SaveChanges();
                        _context.UserMessages.Add(new UserMessage { MessageId = message.Id, ReceiverId = 1, IsViewd = false });
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Delete(short id)
        {
            try
            {
                _context.UserMessages.Where(um => um.MessageId == id).Delete();
                _context.Messages.Where(m => m.Id == id).Delete();
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void DeleteManagerReviewMessage(String helpertId)
        {
            try
            {
                _context.UserMessages.Where(um => um.Message.HelperId == helpertId && um.Message.Title == "گزارش مدیر").Delete();
                _context.Messages.Where(m => m.HelperId == helpertId && m.Title == "گزارش مدیر").Delete();
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public Message Get(short id)
        {
            return _context.Messages.Where(m => m.Id == id)
                .Include(m => m.Sender).FirstOrDefault();
        }

        public IEnumerable<UserMessage> GetReceivedMessages(short userId)
        {
            return _context.UserMessages.Where(um => um.ReceiverId == userId)
                .Include(um => um.Message)
                    .ThenInclude(m => m.Sender)
                .OrderByDescending(m => m.Message.Time);
        }

        public int GetWarningsCount(short userId)
        {
            return _context.UserMessages.Count(um => um.ReceiverId == userId && um.IsViewd == false && um.Message.Type == MessageType.Warning);
        }

        public void SetViewed(short id)
        {
            _context.UserMessages.Where(um => um.MessageId == id).Update(m => new UserMessage { IsViewd = true });
            _context.SaveChanges();
        }
    }
}
