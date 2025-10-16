using Domain_lib.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain_lib.Gitlab.Get
{
    public class GitProject
    {
        public long id { get; set; }
        public string? description { get; set; }
        public string name { get; set; }
        public string? name_with_namespace { get; set; }
        public string? path { get; set; }
        public string? path_with_namespace { get; set; }
        public DateTime created_at { get; set; }
        public string? default_branch { get; set; }
        public string[]? tag_list { get; set; }
        public string[]? topics { get; set; }
        public string? ssh_url_to_repo { get; set; }
        public string? http_url_to_repo { get; set; }
        public string? web_url { get; set; }
        public string? readme_url { get; set; }
        public int forks_count { get; set; }
        public int star_count { get; set; }
        public DateTime last_activity_at { get; set; }
        public Namespace? _namespace { get; set; }
        public _Links? _links { get; set; }
        public bool packages_enabled { get; set; }
        public bool empty_repo { get; set; }
        public bool archived { get; set; }
        public string? visibility { get; set; }
        public bool resolve_outdated_diff_discussions { get; set; }
        public Container_Expiration_Policy? container_expiration_policy { get; set; }
        public bool issues_enabled { get; set; }
        public bool merge_requests_enabled { get; set; }
        public bool wiki_enabled { get; set; }
        public bool jobs_enabled { get; set; }
        public bool snippets_enabled { get; set; }
        public bool container_registry_enabled { get; set; }
        public bool service_desk_enabled { get; set; }
        public bool can_create_merge_request_in { get; set; }
        public string? issues_access_level { get; set; }
        public string? repository_access_level { get; set; }
        public string? merge_requests_access_level { get; set; }
        public string? forking_access_level { get; set; }
        public string? wiki_access_level { get; set; }
        public string? builds_access_level { get; set; }
        public string? snippets_access_level { get; set; }
        public string? pages_access_level { get; set; }
        public string? analytics_access_level { get; set; }
        public string? container_registry_access_level { get; set; }
        public string? security_and_compliance_access_level { get; set; }
        public string? releases_access_level { get; set; }
        public string? environments_access_level { get; set; }
        public string? feature_flags_access_level { get; set; }
        public string? infrastructure_access_level { get; set; }
        public string? monitor_access_level { get; set; }
        public bool emails_disabled { get; set; }
        public bool emails_enabled { get; set; }
        public bool shared_runners_enabled { get; set; }
        public bool lfs_enabled { get; set; }
        public int creator_id { get; set; }
        public string? import_status { get; set; }
        public int open_issues_count { get; set; }
        public string? description_html { get; set; }
        public DateTime updated_at { get; set; }
        public int ci_default_git_depth { get; set; }
        public bool ci_forward_deployment_enabled { get; set; }
        public bool ci_forward_deployment_rollback_allowed { get; set; }
        public bool ci_job_token_scope_enabled { get; set; }
        public bool ci_separated_caches { get; set; }
        public bool ci_allow_fork_pipelines_to_run_in_parent_project { get; set; }
        public string? build_git_strategy { get; set; }
        public bool keep_latest_artifact { get; set; }
        public bool restrict_user_defined_variables { get; set; }
        public string? runners_token { get; set; }
        public bool group_runners_enabled { get; set; }
        public string? auto_cancel_pending_pipelines { get; set; }
        public int build_timeout { get; set; }
        public bool auto_devops_enabled { get; set; }
        public string? auto_devops_deploy_strategy { get; set; }
        public bool public_jobs { get; set; }
        public bool only_allow_merge_if_pipeline_succeeds { get; set; }
        public bool request_access_enabled { get; set; }
        public bool only_allow_merge_if_all_discussions_are_resolved { get; set; }
        public bool remove_source_branch_after_merge { get; set; }
        public bool printing_merge_request_link_enabled { get; set; }
        public string? merge_method { get; set; }
        public string? squash_option { get; set; }
        public bool enforce_auth_checks_on_uploads { get; set; }
        public string? issue_branch_template { get; set; }
        public bool autoclose_referenced_issues { get; set; }
        public string? repository_storage { get; set; }
        public Permissions? permissions { get; set; }
        public Owner? owner { get; set; }

        public TdProject MapTdProject(string[] columns)
        {
            return new()
            {
                OwnerId = owner?.id,
                ProjectName = name,
                GitId = id,
                TdColumns = InitColumns(columns)
            };
        }
        private static List<TdColumn> InitColumns(string[] columns)
        {
            List<TdColumn> columnList = [];

            for (int i = 0; i < columns.Length; i++)
            {
                columnList.Add(new()
                {
                    ColumnOrder = i+1,
                    ColumnName = columns[i]
                });
            }


            return columnList;
        }
    }

    public class Namespace
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? path { get; set; }
        public string? kind { get; set; }
        public string? full_path { get; set; }
        public int? parent_id { get; set; }
        public string? avatar_url { get; set; }
        public string? web_url { get; set; }
    }

    public class Container_Expiration_Policy
    {
        public string? cadence { get; set; }
        public bool enabled { get; set; }
        public int keep_n { get; set; }
        public string? older_than { get; set; }
        public string? name_regex { get; set; }
        public DateTime next_run_at { get; set; }
    }

    public class Permissions
    {
        public Project_Access? project_access { get; set; }
        public Group_Access? group_access { get; set; }
    }

    public class Project_Access
    {
        public int access_level { get; set; }
        public int notification_level { get; set; }
    }

    public class Group_Access
    {
        public int access_level { get; set; }
        public int notification_level { get; set; }
    }

    public class Owner
    {
        public int id { get; set; }
        public string? username { get; set; }
        public string? name { get; set; }
        public string? state { get; set; }
        public string? avatar_url { get; set; }
        public string? web_url { get; set; }
    }


}
