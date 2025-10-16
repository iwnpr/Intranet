using Domain_lib.Entities;

namespace Domain_lib.Models
{
    public class IssueAgg
    {
        private IssueAgg(TdCard card, long projectId, TdUser? user, TdColumn? columnFrom, TdColumn? columnTo)
        {
            Card = card;
            ProjectId = projectId;
            ColumnFrom = columnFrom;
            ColumnTo = columnTo;
            User = user;
        }

        public static IssueAgg Create(TdCard card, long projectId, TdUser? user, TdColumn? columnFrom = null, TdColumn? columnTo = null)
        {
            return new(card, projectId, user, columnFrom, columnTo);
        }

        public TdCard Card { get; private set; }
        public TdColumn? ColumnFrom { get; private set; }
        public TdColumn? ColumnTo { get; private set; }
        public long ProjectId { get; private set; }
        public TdUser? User { get; private set; }
    }
}
