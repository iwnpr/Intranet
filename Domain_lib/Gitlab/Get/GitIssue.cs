using Domain_lib.Entities;

namespace Domain_lib.Gitlab.Get
{
    public class GitIssue
    {
        public long id { get; set; }
        public long iid { get; set; }
        public long project_id { get; set; }
        public string title { get; set; }
        public string? description { get; set; }
        public string? state { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime? closed_at { get; set; }
        public Assignee? closed_by { get; set; }
        public string[]? labels { get; set; }
        //public string? milestone { get; set; }
        public Assignee[]? assignees { get; set; }
        public Author author { get; set; }
        public string? type { get; set; }
        public Assignee? assignee { get; set; }
        public int user_notes_count { get; set; }
        public int merge_requests_count { get; set; }
        public int upvotes { get; set; }
        public int downvotes { get; set; }
        public DateTime? due_date { get; set; }
        public bool confidential { get; set; }
        public string? issue_type { get; set; }
        public string? web_url { get; set; }
        public Time_Stats? time_stats { get; set; }
        public Task_Completion_Status? task_completion_status { get; set; }
        public bool has_tasks { get; set; }
        public string? task_status { get; set; }
        public _Links? _links { get; set; }
        public References? references { get; set; }
        public string? severity { get; set; }
        public int? moved_to_id { get; set; }

        public TdCard MapTdCard()
        {
            return new()
            {
                AssignedId = assignee?.id,
                AuthorId = author.id,
                CardName = title,
                Description = description?.Replace("\n", "<br>"),
                Duedate = due_date,
                GitId = id,
                GitIid = iid,
                CreatedAt = created_at,
                UpdatedAt = updated_at,
                ClosedAt = closed_at,
                ClosedBy = closed_by?.id
            };
        }
    }

    public class Author
    {
        public long id { get; set; }
        public string? username { get; set; }
        public string? name { get; set; }
        public string? state { get; set; }
        public string? avatar_url { get; set; }
        public string? web_url { get; set; }
    }

    public class Assignee
    {
        public long id { get; set; }
        public string? username { get; set; }
        public string? name { get; set; }
        public string? state { get; set; }
        public string? avatar_url { get; set; }
        public string? web_url { get; set; }
    }

    public class Time_Stats
    {
        public int time_estimate { get; set; }
        public int total_time_spent { get; set; }
        public string? human_time_estimate { get; set; }
        public string? human_total_time_spent { get; set; }
    }

    public class Task_Completion_Status
    {
        public int count { get; set; }
        public int completed_count { get; set; }
    }

    public class _Links
    {
        public string? self { get; set; }
        public string? notes { get; set; }
        public string? award_emoji { get; set; }
        public string? project { get; set; }
        public string? closed_as_duplicate_of { get; set; }
    }

    public class References
    {
        public string? _short { get; set; }
        public string? relative { get; set; }
        public string? full { get; set; }
    }
}
