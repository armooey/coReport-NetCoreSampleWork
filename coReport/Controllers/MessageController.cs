using System;
using coReport.Models.HomeViewModels;
using coReport.Services;
using Microsoft.AspNetCore.Mvc;

namespace coReport.Controllers
{
    public class MessageController : Controller
    {
        private IMessageService _messageService;

        public MessageController( IMessageService messageService)
        {
            _messageService = messageService;
        }
        public IActionResult DeleteMessage(short id)
        {
            var result = _messageService.Delete(id);
            return Json(result);
        }

        //Marks the message as read
        public IActionResult SetViewed(short id)
        {
            var result = _messageService.SetViewed(id);
            return Json(result);
        }
    }
}