using coReport.Models.ProjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IProjectService
    {
        public bool Add(Project project);
        public IEnumerable<Project> GetAll();
        public bool EndProject(short id);
        public IEnumerable<Project> GetInProgressProjects();
    }
}
