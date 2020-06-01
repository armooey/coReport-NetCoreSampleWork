using coReport.Data;
using coReport.Models.ActivityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public class ActivityData : IActivityData
    {
        private ApplicationDbContext _context;
        private ILogService _logger;

        public ActivityData(ApplicationDbContext context, ILogService logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool Add(Activity activity)
        {
            try 
            {
                _context.Activities.Add(activity);
                _context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                _logger.Log("Error in Creating new activity.", e);
                return false;
            }
        }

        public bool Delete(short id)
        {
            try
            {
                var activity = _context.Activities.Where(a => a.Id == id && !a.IsDeleted)
                    .Include(a => a.SubActivities).FirstOrDefault();
                if (activity != null)
                {
                    activity.IsDeleted = true;
                    _context.Activities.Update(activity);
                    foreach (var subactivity in activity.SubActivities)
                    {
                        subactivity.IsDeleted = true;
                        _context.Activities.Update(subactivity);
                    }

                    _context.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Log("Error in deleting activity.", e);
                return false;
            }
        }

        public Activity GetActivity(short id)
        {
            return _context.Activities.Where(a => a.Id == id)
                .Include(a => a.SubActivities).FirstOrDefault();
        }

        public IEnumerable<Activity> GetAllActivities()
        {
            return _context.Activities.Where(a => a.ParentActivityId == null && !a.IsDeleted)
                .Include(a => a.SubActivities);
        }

        public IEnumerable<Activity> GetParentActivities()
        {
            return _context.Activities.Where(a => a.ParentActivityId == null && !a.IsDeleted);
        }

        public void InitializeActivities()
        {
            try
            {
                if (!_context.Activities.Any())
                {
                    var onSiteContent = new Activity { Name = "تولید محتوای سایت" };
                    var telegramContent = new Activity { Name = "تولید محتوای کانال تلگرام" };
                    var instagramContent = new Activity { Name = "تولید محتوای اینستاگرام" };
                    var executiveAffairs = new Activity { Name = "امور اجرایی" };
                    var otherActivities = new Activity { Name = "سایر فعالیت‌ها" };
                    _context.Activities.AddRange(new List<Activity> { onSiteContent, telegramContent, instagramContent,
                                        executiveAffairs, otherActivities});
                    _context.SaveChanges();
                    _context.Activities.Add(new Activity { Name = "ارسال مطلب مشاوره‌ای", ParentActivityId = onSiteContent.Id });
                    _context.Activities.Add(new Activity { Name = "آپلود فیلم", ParentActivityId = onSiteContent.Id });
                    _context.Activities.Add(new Activity { Name = "تولید درسنامه", ParentActivityId = onSiteContent.Id });
                    _context.Activities.Add(new Activity { Name = "موارد دیگر", ParentActivityId = onSiteContent.Id });

                    _context.Activities.Add(new Activity { Name = "تولید درسنامه", ParentActivityId = telegramContent.Id});
                    _context.Activities.Add(new Activity { Name = "تولید ریزنکته", ParentActivityId = telegramContent.Id});
                    _context.Activities.Add(new Activity { Name = "موارد دیگر", ParentActivityId = telegramContent.Id});

                    _context.Activities.Add(new Activity { Name = "پاسخگویی به دایرکت", ParentActivityId = instagramContent.Id});
                    _context.Activities.Add(new Activity { Name = "پاسخگویی به کامنت‌ها", ParentActivityId = instagramContent.Id});
                    _context.Activities.Add(new Activity { Name = "هماهنگی برای لایو", ParentActivityId = instagramContent.Id});
                    _context.Activities.Add(new Activity { Name = "برگذاری لایو", ParentActivityId = instagramContent.Id});
                    _context.Activities.Add(new Activity { Name = "تولید درسنامه", ParentActivityId = instagramContent.Id});
                    _context.Activities.Add(new Activity { Name = "آپلود پست یا استوری", ParentActivityId = instagramContent.Id});

                    _context.Activities.Add(new Activity { Name = "هماهنگی و برگذاری جلسه", ParentActivityId = executiveAffairs.Id});
                    _context.Activities.Add(new Activity { Name = "شرکت در جلسه", ParentActivityId = executiveAffairs.Id});
                    _context.Activities.Add(new Activity { Name = "پیگیری فعالیت‌های محول شده و هماهنگی‌ها", ParentActivityId = executiveAffairs.Id});
                    _context.Activities.Add(new Activity { Name = "تهیه و تنظیم صورت‌جلسه", ParentActivityId = executiveAffairs.Id});
                    _context.Activities.Add(new Activity { Name = "سایر امور", ParentActivityId = executiveAffairs.Id});

                    _context.SaveChanges();

                }
            }
            catch (Exception e)
            {
                _logger.Log("Error in initializing activities.", e);
            }
        }
    }
}
