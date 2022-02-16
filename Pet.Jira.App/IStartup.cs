using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pet.Jira.App
{
    public interface IStartup
    {
        Task Run();
    }
}
