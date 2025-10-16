using Domain_lib.Entities;

namespace Domain_lib.Gitlab.Get
{
    public class GitUser
    {
        public long id { get; set; }
        public string username { get; set; }
        public string? email { get; set; }
        public string name { get; set; } = null!;
        public string state { get; set; } = null!;
        public bool? locked { get; set; }
        public string? avatar_url { get; set; }
        public string? web_url { get; set; }
        public DateTime? created_at { get; set; }
        public bool? is_admin { get; set; }
        public string? bio { get; set; }
        public string? location { get; set; }
        public string? skype { get; set; }
        public string? linkedin { get; set; }
        public string? twitter { get; set; }
        public string? discord { get; set; }
        public string? github { get; set; }
        public string? website_url { get; set; }
        public string? organization { get; set; }
        public string? job_title { get; set; }
        public DateTime? last_sign_in_at { get; set; }
        public DateTime? confirmed_at { get; set; }
        public int theme_id { get; set; }
        public string? last_activity_on { get; set; }
        public int color_scheme_id { get; set; }
        public int projects_limit { get; set; }
        public DateTime? current_sign_in_at { get; set; }
        public string? note { get; set; }
        public List<Identity>? identities { get; set; }
        public bool? can_create_group { get; set; }
        public bool? can_create_project { get; set; }
        public bool? two_factor_enabled { get; set; }
        public bool? external { get; set; }
        public bool? private_profile { get; set; }
        public string? current_sign_in_ip { get; set; }
        public string? last_sign_in_ip { get; set; }
        public int namespace_id { get; set; }
        public DateTime? email_reset_offered_at { get; set; }

        public TdUser MapTdUser()
        {
            return new()
            {
                Email = email,
                GitId = id,
                IsSystem = ((email?.Contains("gitlab-new") ?? false) || (email?.Contains("example.com") ?? false)),
                Login = username,
                StatusId = 1,
                UserName = name
            };
        }
    }

    public class Identity
    {
        public string? provider { get; set; }
        public string? extern_uid { get; set; }
    }
}