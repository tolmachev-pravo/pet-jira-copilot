using System;
using System.Collections.Generic;
using System.Text;

namespace Pet.Jira.Adapter
{
    public interface IJiraConfiguration
    {
        string Url { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
    }
}
