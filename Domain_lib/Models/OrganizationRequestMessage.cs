using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain_lib.Models
{
    public class OrganizationRequestMessage
    {
        public string? OrganizationName { get; set; }

        public string? Inn { get; set; }

        public string? Ogrn { get; set; }

        public IReadOnlyCollection<string> WhiteListIps { get; set; }

        public IReadOnlyCollection<string> Services { get; set; }

    }
}
