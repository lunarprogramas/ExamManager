using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamManager.Modules.Info
{
    class LockedMenus
    {
        public object GetLockedMenus()
        {
            return (
                CandidateManagement: 3,
                AboutPage: 2
                );
        }
    }
}
