using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using coReport.Auth;
using coReport.Models.HomeViewModels;
using coReport.Models.MessageModels;
using coReport.Operations;
using coReport.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace coReport.Controllers
{
    public class MessageController : Controller
    {
        private IMessageService _messageService;
        private UserManager<ApplicationUser> _userManager;

        public MessageController( IMessageService messageService, UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _userManager = userManager;
        }
        public IActionResult DeleteMessage(short id)
        {
            var result = _messageService.Delete(id);
            return Json(result);
        }


        public async Task<IActionResult> SendMessage(String messageTitle, IEnumerable<short> receivers, String messageText, String returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            var message = new Message { 
                Title = messageTitle,
                SenderId = user.Id,
                Type = MessageType.Message,
                Text = messageText,
                Time = DateTime.Now
            };
            
            var result = _messageService.Add(message, receivers);
            if (!result)
            {
                var model = new ErrorViewModel { Error = "مشکل در ارسال پیام" };
                return RedirectToAction("Error", "Home", model);
            }
            return RedirectToLocal(returnUrl);
        }


        //Get message text and flag message as viewed
        public async Task<IActionResult> ViewMessage(short id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var message = _messageService.Get(id);
            string senderName = string.Empty;
            if (message.Type == MessageType.Message ||
                    message.Type == MessageType.Manager_Review_Notification)
                senderName = message.Sender.FirstName + " " + message.Sender.LastName;
            else if (message.Type == MessageType.Warning)
                senderName = "اخطار سیستمی";
            else if (message.Type == MessageType.System_Notification)
                senderName = "پیام سیستمی";

            var messageData = new List<string> { message.Title, senderName , message.Text};
            var result = _messageService.SetViewed(id, user.Id);
            if (!result)
                return Json(null);
            return Json(messageData);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}