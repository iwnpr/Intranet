using System.Text.Json;

namespace Domain_lib.Models
{
    public record HistorySnap
    {
        private HistorySnap() { }
        public List<SnapValue> SnapValues { get; init; } = [];
        public bool HasChanges => SnapValues.Count > 0; 

        public static HistorySnap Create()
        {
            return new HistorySnap();
        }
        public bool CheckForChangesAndAdd(string fieldName, string? oldFieldValue = "", string? newFieldValue = "")
        {
            if(oldFieldValue != newFieldValue)
            {
                SnapValues.Add(SnapValue.Create(fieldName, oldFieldValue, newFieldValue));
                return true;
            }
            return false;
        }
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public record SnapValue
    {
        public string FieldName { get; private set; }
        public string? OldFieldValue { get; set; }
        public string? NewFieldValue { get; set; }

        private SnapValue(string fieldName, string? oldFieldValue, string? newFieldValue)
        {
            FieldName = fieldName;
            OldFieldValue = oldFieldValue;
            NewFieldValue = newFieldValue;
        }

        public static SnapValue Create(string fieldName, string? oldFieldValue, string? newFieldValue)
        {
            return new(fieldName, oldFieldValue, newFieldValue);
        }
    }
}
