using Common_lib.Models.RequestXml;
using Domain_lib.Entities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domain_lib.Gitlab.Get
{
    public class GitGroup
    {
        public long id { get; set; }
        public string? web_url { get; set; }
        public string name { get; set; }
        public string? path { get; set; }
        public string? description { get; set; }
        public string? visibility { get; set; }
        public bool share_with_group_lock { get; set; }
        public bool require_two_factor_authentication { get; set; }
        public int two_factor_grace_period { get; set; }
        public string? project_creation_level { get; set; }
        public string? subgroup_creation_level { get; set; }
        public bool lfs_enabled { get; set; }
        public int default_branch_protection { get; set; }
        public string? avatar_url { get; set; }
        public bool request_access_enabled { get; set; }
        public string? full_name { get; set; }
        public string? full_path { get; set; }
        public DateTime created_at { get; set; }
        public int? parent_id { get; set; }
        public string? shared_runners_setting { get; set; }

        public TdGroup MapTdGroup()
        {
            return new()
            {
                GitId = id,
                GroupName = name
            };
        }
    }
}
