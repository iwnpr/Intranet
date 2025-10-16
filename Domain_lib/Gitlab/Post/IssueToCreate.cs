using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain_lib.Gitlab.Post
{
    /// <summary>
    /// Модель для создания задачи в гите
    /// </summary>
    public class IssueToCreate
    {
        /// <summary>
        /// Идентификатор проекта
        /// </summary>
        [JsonPropertyName("id")]
        public long ProjectId { get; set; }
        /// <summary>
        /// Индетификатор кому назначено
        /// </summary>
        [JsonPropertyName("assignee_id")]
        public long? AssigneeId { get; set; }

        [JsonPropertyName("description")]
        private string? _description;

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value?.Replace("\n", "<br>");
            }
        }
        /// <summary>
        /// Срок исполнения
        /// </summary>
        [JsonPropertyName("due_date")]
        public string? DueDate { get; set; }
        /// <summary>
        /// Тип задачи
        /// </summary>
        [JsonPropertyName("issue_type")]
        public string? IssueType { get; set; }
        /// <summary>
        /// Теги
        /// </summary>
        [JsonPropertyName("labels")]
        public string? Labels { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
    }

}
