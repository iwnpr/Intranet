using Common_lib.Models.ServiceModels;
using Domain_lib.Models;

namespace Application_lib.Gitlab
{
    /// <summary>
    /// Сервис для работы с гитом
    /// </summary>
    public interface IGitService
    {
        /// <summary>
        /// Синхронизироваться с гитом
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result> SynchronizeGitlab(long userId);
        /// <summary>
        /// Создать карточку задачи в бэклог
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result> CreateIssue(IssueAgg issueCard);
        /// <summary>
        /// Обновить данные внутри задачи
        /// </summary>
        /// <param name="issueAgg">Агрегат задачи</param>
        /// <returns>Результат</returns>
        Task<Result> UpdateIssue(IssueAgg issueAgg);
        /// <summary>
        /// Синхронизация комментариев
        /// </summary>
        /// <param name="issue">Задача</param>
        /// <param name="userId">id пользователя</param>
        /// <returns>Результат</returns>
        Task<Result> SyncIssueDiscussions(IssueAgg issueCard);
        /// <summary>
        /// Изменение состояния задачи и установка тега колонки
        /// </summary>
        /// <param name="card"></param>
        /// <param name="columnFrom"></param>
        /// <param name="columnTo"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<Result> ChangeIssueColumn(IssueAgg issueCard);
        /// <summary>
        /// Создать новый комментарий
        /// </summary>
        /// <param name="commentText">Текст комментария</param>
        /// <param name="issueIid">id задачи</param>
        /// <param name="projectId">id проекта</param>
        /// <returns>Результат создания</returns>
        Task<Result> CreateComment(UserComment comment);
    }
}
