﻿using coReport.Data;
using coReport.Models.ProjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace coReport.Services
{
    public class ProjectData : IProjectData
    {
        private ApplicationDbContext _context;
        private ILogService _logger;

        public ProjectData(ApplicationDbContext context, ILogService logger)
        {
            _context = context;
            _logger = logger;
        }
        public bool Add(Project project)
        {
            try
            {
                _context.Projects.Add(project);
                _context.SaveChanges();
                return true;
            }
            catch(Exception e)
            {
                _logger.Log("Error in saving project", e);
                return false;
            }
        }

        public bool EndProject(short id)
        {
            try
            {
                _context.Projects.Where(p => p.Id == id).Update(p => new Project { EndDate = DateTime.Now});
                _context.SaveChanges();
                return true;
            }
            catch(Exception e)
            {
                _logger.Log("Error in flagging project as ended", e);
                return false;
            }
        }

        public IEnumerable<Project> GetAll()
        {
            return _context.Projects.OrderBy(p => p.EndDate).ThenBy(p => p.CreateDate);
        }

        public IEnumerable<Project> GetInProgressProjects()
        {
            return _context.Projects.Where(p => p.EndDate == null);
        }
    }
}
