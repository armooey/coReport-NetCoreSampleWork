using coReport.Data;
using coReport.Models.ProjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace coReport.Services
{
    public class ProjectService : IProjectService
    {
        private ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Add(Project project)
        {
            try
            {
                _context.Projects.Add(project);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void EndProject(short id)
        {
            try
            {
                _context.Projects.Where(p => p.Id == id).Update(p => new Project { EndDate = DateTime.Now});
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<Project> GetAll()
        {
            return _context.Projects.OrderBy(p => p.CreateDate).ToList();
        }
    }
}
