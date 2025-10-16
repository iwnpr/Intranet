using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain_lib.Gitlab.Post
{
    /// <summary>
    /// Модель для создания комментария в треде
    /// </summary>
    public class NoteToCreate
    {
        /// <summary>
        /// Тело комментария
        /// </summary>
        [JsonPropertyName("body")]
        public string Body { get; set; } = null!;
        /// <summary>
        /// id комментария на который отвечаем
        /// Обязательный, если Reply!
        /// </summary>
        [JsonPropertyName("note_id")]
        public long? ReplyNoteId { get; set; }
    }
}
