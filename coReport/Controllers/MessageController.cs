using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coReport.Models;
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
            try
            { 
                _messageService.Delete(id);
            }
            catch (Exception e)
            {
                var errorModel = new ErrorViewModel
                {
                    Error = e.Message
                };
                return RedirectToAction("Error", "Home", errorModel);
            }
            return RedirectToAction("ManageReports", "Account");
        }

        public IActionResult SetViewed(short id)
        {
            try
            {
                _messageService.SetViewed(id);
            }
            catch (Exception e)
            {
                var errorModel = new ErrorViewModel
                {
                    Error = e.Message
                };
                return RedirectToAction("Error", "Home", errorModel);
            }
            return RedirectToAction("ManageReports", "Account");
        }
    }
}