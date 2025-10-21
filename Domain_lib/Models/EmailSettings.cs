using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain_lib.Models
{
    public class EmailSettings
    {
        public string? Host { get; set; }

        public int Port { get; set; } = 25;

        public bool EnableSsl { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? From { get; set; }

        public string? OrganizationRequestRecipient { get; set; }
    }
}
