using Domain_lib.Entities;
using System.Text.Json.Serialization;

namespace Domain_lib.Gitlab.Get
{
    public class GitDiscussion
    {
        [JsonPropertyName("id")]
        public string ContextId { get; set; } = null!;
        public bool individual_note { get; set; }
        public Note[] notes { get; set; } = [];
    }

    public class Note
    {
        public long id { get; set; }
        public string? type { get; set; }
        //public string body { get; set; } = string.Empty;\
        public string _body = string.Empty;
        public string body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value.Replace("\n", "<br>");
            }
        }

        public Author author { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool system { get; set; }
        public int noteable_id { get; set; }
        public string? noteable_type { get; set; }
        public int project_id { get; set; }
        public bool resolvable { get; set; }
        public bool confidential { get; set; }
        public bool _internal { get; set; }
        public long noteable_iid { get; set; }

        public TdComment MapTdComment(string contextId, long cardId, List<TdUser> users, long? userId = null)
        {
            return new TdComment()
            {
                Card = cardId,
                GitId = id,
                Created = created_at,
                IsSystem = system,
                CommentText = body,
                UserId = userId ?? users.FirstOrDefault(x => x.GitId == author.id)?.Keyid ?? 1,
                Context = contextId
            };
        }
    }
}
