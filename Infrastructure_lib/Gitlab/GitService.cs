using Application_lib.Gitlab;
using Common_lib.Models.ServiceModels;
using Domain_lib.Entities;
using Domain_lib.Gitlab.Get;
using Domain_lib.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure_lib.Gitlab
{
    /// <inheritdoc/>>
    public class GitService(
        IGitApiService gitDataService,
        IGitDBService gitDBService,
        ILogger<GitService> logger,
        IConfiguration config) : IGitService
    {
        private readonly IGitApiService _gitApiService = gitDataService;
        private readonly IGitDBService _gitDBService = gitDBService;
        private readonly ILogger<GitService> _logger = logger;
        private readonly IConfiguration _config = config;
        public bool IsSyncronysing;

        public async Task<Result> CreateComment(UserComment comment)
        {
            // Создаем комментарий в гите
            _logger.LogInformation("Создаем комментарий в гите");
            Result<GitDiscussion> gitDiscussion;

            if (comment.IsReplyed)
                gitDiscussion = await _gitApiService.ReplyComment(comment);
            else
                gitDiscussion = await _gitApiService.CreateComment(comment);

            if (!gitDiscussion.IsSuccess)
                return Result.Error(gitDiscussion);

            // Синхронизируем с БД
            _logger.LogInformation("Синхронизируем с БД");
            var dbSyncResult = await _gitDBService.AddComment(gitDiscussion.Data, comment);

            if (!dbSyncResult.IsSuccess)
                return Result.Error(dbSyncResult);

            return Result.Success();
        }
        public async Task<Result> CreateIssue(IssueAgg issueAgg)
        {
            // Создаем карточку в гите
            _logger.LogInformation("Создаем карточку в гите");
            var gitIssue = await _gitApiService.CreateIssue(issueAgg);

            if (!gitIssue.IsSuccess)
                return Result.Error(gitIssue);

            // Синхронизируем с БД
            _logger.LogInformation("Синхронизируем с БД");
            var dbSyncResult = await _gitDBService.AddIssue(gitIssue.Data, issueAgg.User.Keyid);

            if (!dbSyncResult.IsSuccess)
                return Result.Error(dbSyncResult);

            return Result.Success();
        }
        public async Task<Result> UpdateIssue(IssueAgg issueAgg)
        {
            // Создаем карточку в гите
            _logger.LogInformation("Создаем карточку в гите");
            var gitIssue = await _gitApiService.UpdateIssue(issueAgg);

            if (!gitIssue.IsSuccess)
                return Result.Error(gitIssue);

            // Синхронизируем с БД
            _logger.LogInformation("Синхронизируем с БД");
            var dbSyncResult = await _gitDBService.UpdateIssue(gitIssue.Data, issueAgg);

            if (!dbSyncResult.IsSuccess)
                return Result.Error(dbSyncResult);

            return Result.Success();
        }
        public async Task<Result> SyncIssueDiscussions(IssueAgg issueAgg)
        {
            var gitComments = await _gitApiService.GetDiscussions([new() { project_id = issueAgg.ProjectId, iid = issueAgg.Card.GitIid }]);

            if (gitComments.IsSuccess)
                return await _gitDBService.SyncDiscussions(gitComments.Data, issueAgg.User?.Keyid ?? 1, issueAgg.Card.GitId);

            return gitComments;
        }
        public async Task<Result> ChangeIssueColumn(IssueAgg issueCard)
        {
            // Удалить все теги которые относятся к колонкам
            _logger.LogInformation("Удаление тегов колонок");
            var removeResult = await _gitApiService.RemoveDefaultLabels(issueCard.Card.GitIid, issueCard.ProjectId);

            if (!removeResult.IsSuccess)
                return Result.Error(removeResult);

            // Установка статуса задачи в гите
            var isOpened = issueCard.ColumnTo?.ColumnName is "Бэклог" or "Открыто";
            _logger.LogInformation("Установка статуса задачи в гите opened:{opened}", isOpened);

            var stateChangeResult = await _gitApiService.SetIssueState(
                issueCard.Card.GitIid,
                issueCard.ProjectId,
                issueCard.ColumnFrom?.ColumnName is "Бэклог" or "Открыто",
                isOpened);

            if (!stateChangeResult.IsSuccess)
                return Result.Error(stateChangeResult);

            _logger.LogInformation("Добавление тега колонки {tag}", issueCard.ColumnTo?.ColumnName);
            var addResult = await _gitApiService.AddLabel(issueCard.Card.GitIid, issueCard.ProjectId, issueCard.ColumnTo?.ColumnName ?? "Бэклог");
            if (!addResult.IsSuccess)
                return Result.Error(addResult);

            return Result.Success();
        }

        #region Синхронизация с гитом
        public async Task<Result> SynchronizeGitlab(long userId)
        {
            if (IsSyncronysing)
                return Result.Error(201, "В процессе синхронизации");

            try
            {
                IsSyncronysing = true;
                _logger.LogInformation("Синхронизируем пользователей");
                var SyncUsersResult = await SyncUsers(userId);
                if (!SyncUsersResult.IsSuccess)
                    return SyncUsersResult;

                _logger.LogInformation("Синхронизируем группы");
                var SyncGroupsResult = await SyncGroups(userId);
                if (!SyncGroupsResult.IsSuccess)
                    return SyncGroupsResult;

                _logger.LogInformation("Синхронизируем проекты");
                var SyncProjectsResult = await SyncProjects(userId);
                if (!SyncProjectsResult.IsSuccess)
                    return SyncProjectsResult;

                _logger.LogInformation("Синхронизируем задачи");
                return await SyncIssues(userId);
            }
            finally
            {
                IsSyncronysing = false;
            }
        }
        private async Task<Result> SyncUsers(long userId)
        {
            var gitUsers = await _gitApiService.GetUsers();

            if (gitUsers.IsSuccess)
                return await _gitDBService.SyncUsers(gitUsers.Data, userId);

            return gitUsers;
        }
        private async Task<Result> SyncGroups(long userId)
        {
            var gitGroups = await _gitApiService.GetGroups();

            if (gitGroups.IsSuccess)
                return await _gitDBService.SyncGroups(gitGroups.Data, userId);

            return gitGroups;
        }
        private async Task<Result> SyncProjects(long userId)
        {
            var gitProjects = await _gitApiService.GetProjects();

            if (gitProjects.IsSuccess)
                return await _gitDBService.SyncProjects(gitProjects.Data, userId);

            return gitProjects;
        }
        private async Task<Result> SyncIssues(long userId)
        {
            var gitIssues = await _gitApiService.GetIssues();

            if (!gitIssues.IsSuccess)
                return gitIssues;

            var dbsync = await _gitDBService.SyncIssues(gitIssues.Data, userId);

            if (!dbsync.IsSuccess)
                return dbsync;

            if (_config.GetValue<bool>("App:GitClientSettings:IsCommentsSync"))
            {
                _logger.LogInformation("Синхронизируем комментарии");
                return await SyncDiscussions(gitIssues.Data, userId);
            }

            return dbsync;
        }
        private async Task<Result> SyncDiscussions(List<GitIssue> issues, long userId)
        {
            var gitComments = await _gitApiService.GetDiscussions(issues);

            if (gitComments.IsSuccess)
                return await _gitDBService.SyncDiscussions(gitComments.Data, userId);

            return gitComments;
        }
        #endregion
    }
}
