using Common_lib.Models.ServiceModels;
using Domain_lib.Entities;
using Domain_lib.Gitlab.Get;
using Domain_lib.Models;

namespace Application_lib.Gitlab
{
    /// <summary>
    /// Сервис синхронизации Git с БД
    /// </summary>
    public interface IGitDBService
    {
        /// <summary>
        /// Синхронизировать пользователей
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result> SyncUsers(List<GitUser> users, long userKeyId);
        /// <summary>
        /// Синхронизировать группы
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result> SyncGroups(List<GitGroup> groups, long userKeyId);
        /// <summary>
        /// Синхронизировать проекты
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result> SyncProjects(List<GitProject> projects, long userKeyId);
        /// <summary>
        /// Синхронизировать задачи
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result> SyncIssues(List<GitIssue> issues, long userKeyId);
        /// <summary>
        /// Синхронизировать комментарии к задачам
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result> SyncDiscussions(List<GitDiscussion> comments, long userId, long? cardID = null);
        /// <summary>
        /// Добавить карточку с задачей из git в БД
        /// </summary>
        /// <param name="issue">Зача из git</param>
        /// <param name="userKeyId">Id пользователя который создал карточку</param>
        /// <returns>Результат</returns>
        Task<Result<TdCard>> AddIssue(GitIssue issue, long userKeyId);
        Task<Result> UpdateIssue(GitIssue issue, IssueAgg issueAgg);
        /// <summary>
        /// Добавление комментария в БД
        /// </summary>
        /// <param name="comment">Комментарий</param>
        /// <param name="userId">Id пользователя который добавил комментарий</param>
        /// <returns>Результат</returns>
        Task<Result<TdComment>> AddComment(GitDiscussion comment, UserComment userComment);
    }
}
