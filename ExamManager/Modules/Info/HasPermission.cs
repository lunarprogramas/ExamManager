using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamManager.Modules.Info;

// Our beloved permission checker

// LEVELS:
// 4 = godded
// 3 = ADMIN
// 2 = EXAMINER
// 1 = GUEST/VISITOR

namespace ExamManager.Modules.Info
{
    class HasPermission
    {
        private LockedMenus _lockedMenus;
        public void Initialize()
        {
            _lockedMenus = new LockedMenus();
        }
        public bool CheckPermission(int userLevel, string origin)
        {
            if (origin.Contains("Menu"))
            {
                var configuredLevels = ((int CandidateManagement, int AboutPage, int ExamHallManagement, int ExaminerManagement))_lockedMenus.GetLockedMenus();

                string[] menuSelected = origin.Split(":");

                string key = menuSelected.Length > 1 ? menuSelected[1] : null;

                if (key == "CandidateManagement" && userLevel >= configuredLevels.CandidateManagement)
                {
                    return true;
                } else if (key == "AboutPage" && userLevel >= configuredLevels.AboutPage)
                {
                    return true;
                } else if (key == "ExamHallManagement" && userLevel >= configuredLevels.ExamHallManagement)
                {
                    return true;
                } else if (key == "ExaminerManagement" && userLevel >= configuredLevels.ExaminerManagement)
                {
                    return true;
                }

                return false;
            } else
            {
                if (userLevel == int.Parse(origin))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
