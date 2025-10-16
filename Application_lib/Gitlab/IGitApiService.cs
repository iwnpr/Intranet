using Common_lib.Models.ServiceModels;
using Domain_lib.Gitlab.Get;
using Domain_lib.Models;

namespace Application_lib.Gitlab
{
    /// <summary>
    /// Сервис для работы с гитом
    /// </summary>
    public interface IGitApiService
    {
        /// <summary>
        /// Получить всех пользователей из Git
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result<List<GitUser>>> GetUsers();
        /// <summary>
        /// Получить все группы из Git
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result<List<GitGroup>>> GetGroups();
        /// <summary>
        /// Получить все проекты из Git
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result<List<GitProject>>> GetProjects();
        /// <summary>
        /// Получить все задачи из Git
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result<List<GitIssue>>> GetIssues();
        /// <summary>
        /// Получить все комментарии из Git
        /// </summary>
        /// <returns>Результат</returns>
        Task<Result<List<GitDiscussion>>> GetDiscussions(List<GitIssue> issues);
        /// <summary>
        /// Создание карточки с задачей
        /// </summary>
        /// <returns>Создание issue в БД</returns>
        Task<Result<GitIssue>> CreateIssue(IssueAgg issueAgg);
        /// <summary>
        /// Обновить сущность
        /// </summary>
        /// <param name="issueAgg">Агрегат задачи</param>
        /// <returns>Результат</returns>
        Task<Result<GitIssue>> UpdateIssue(IssueAgg issueAgg);
        /// <summary>
        /// Создание нового комментария
        /// </summary>
        /// <param name="comment">Комменатрий</param>
        /// <returns>Обсуждение</returns>
        Task<Result<GitDiscussion>> CreateComment(UserComment comment);
        /// <summary>
        /// Создание ответного комментария
        /// </summary>
        /// <param name="comment">Комментарий</param>
        /// <returns>Обсуждение</returns>
        Task<Result<GitDiscussion>> ReplyComment(UserComment comment);
        /// <summary>
        /// Добавить массив тегов к задаче
        /// </summary>
        /// <param name="issueId">id задачи</param>
        /// <param name="projectId">id проекта</param>
        /// <param name="labels">Массив тегов</param>
        Task<Result> AddLabels(long? issueId, long? projectId, string[] labels);
        /// <summary>
        /// Удаление стандартных тегов из задачи (По дефолту забираются из конфига)
        /// </summary>
        /// <param name="issueId">id задачи</param>
        /// <param name="projectId">id проекта</param>
        /// <returns>Результат</returns>
        Task<Result> RemoveDefaultLabels(long? issueId, long? projectId);
        /// <summary>
        /// Добавить Тег к задаче
        /// </summary>
        /// <param name="issueId">id задачи</param>
        /// <param name="projectId">id проекта</param>
        /// <param name="label">Тег</param>
        /// <returns>Результат</returns>
        Task<Result> AddLabel(long? issueId, long? projectId, string label);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="issueId"></param>
        /// <param name="projectId"></param>
        /// <param name="IsOpened"></param>
        /// <returns></returns>
        Task<Result> SetIssueState(long? issueId, long? projectId, bool IsSameState, bool IsOpened);

    }
}
