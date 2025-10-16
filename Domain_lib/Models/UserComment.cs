namespace Domain_lib.Models
{
    public class UserComment
    {
        //public string Body { get; set; } = string.Empty;
        private string _body = string.Empty;
        public string Body
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
        public long GitIssueId { get; set; }
        public long GitProjectId { get; set; }
        public string? AuthorName { get; set; }
        public long UserKeyId { get; set; }
        public string? ReplyContext { get; set; }
        public long? ReplyedId { get; set; }
        public bool IsReplyed => ReplyContext != null;
    }
}
