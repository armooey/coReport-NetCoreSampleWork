﻿using coReport.Data;
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

        public void AddManagerReviewMessage(Message message, short managerReportId, short receiverId)
        {
            try
            {
                var managerReport = _context.ManagerReports.FirstOrDefault(mr => mr.Id == managerReportId);
                if (managerReport != null)
                {
                    //Check if review message exists then update message
                    if (managerReport != null && _context.Messages.Any(m => m.Id == managerReport.ReviewMessageId))
                    {
                        _context.Messages.Where(m => m.Id == managerReport.ReviewMessageId)
                            .Update(m => new Message { Text = message.Text, Time = DateTime.Now });
                        _context.UserMessages.Where(um => um.MessageId == managerReport.ReviewMessageId
                                                            && um.ReceiverId == receiverId)
                            .Update(um => new UserMessage { IsViewd = false });
                    }
                    else //Create new review message
                    {
                        _context.Messages.Add(message);
                        _context.SaveChanges();
                        _context.UserMessages.Add(new UserMessage { MessageId = message.Id, ReceiverId = receiverId });
                        managerReport.ReviewMessageId = message.Id;
                        _context.ManagerReports.Update(managerReport);
                    }
                    _context.SaveChanges(); 
                }
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
                const short ADMIN_ID = 1;
                _context.Messages.Add(message);
                _context.SaveChanges();
                _context.UserMessages.Add(new UserMessage { MessageId = message.Id, ReceiverId = ADMIN_ID });
                _context.SaveChanges();
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

        public void DeleteManagerReviewMessage(short managerReportId)
        {
            try
            {
                var managerReport = _context.ManagerReports.FirstOrDefault(mr => mr.Id == managerReportId);
                if(managerReport != null)
                {
                    _context.UserMessages.Where(um => um.Message.Id == managerReport.ReviewMessageId).Delete();
                    _context.Messages.Where(m => m.Id == managerReport.ReviewMessageId).Delete();
                    managerReport.ReviewMessageId = 0;
                    _context.ManagerReports.Update(managerReport);
                    _context.SaveChanges(); 
                }
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

        public IEnumerable<UserMessage> GetWarnings()
        {
            return _context.UserMessages.Where(um => um.Message.Type == MessageType.Warning)
                                        .Include(um => um.Message)
                                        .Include(um => um.Receiver)
                                        .OrderByDescending(um => um.Message.Time);
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
