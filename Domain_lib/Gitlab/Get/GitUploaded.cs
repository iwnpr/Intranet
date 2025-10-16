using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain_lib.Gitlab.Get
{
    public class GitUploaded
    {
        public string? alt { get; set; }
        public string? url { get; set; }
        public string? full_path { get; set; }
        public string? markdown { get; set; }
    }
}
