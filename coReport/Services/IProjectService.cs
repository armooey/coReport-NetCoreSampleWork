using coReport.Models.ProjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IProjectService
    {
        public void Add(Project project);
        public IEnumerable<Project> GetAll();
        public void EndProject(short id);
        public IEnumerable<Project> GetInProgressProjects();
    }
}
