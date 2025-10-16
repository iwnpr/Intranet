using Application_lib.Gitlab;
using Common_lib.Models.ServiceModels;
using Domain_lib.Entities;
using Domain_lib.Gitlab.Get;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Domain_lib;
using Domain_lib.Models;

namespace Infrastructure_lib.Gitlab
{
    public class GitDBSync(AppDBContext context, ILogger<GitDBSync> logger, IConfiguration config) : IGitDBService
    {
        private readonly AppDBContext _context = context;
        private readonly ILogger<GitDBSync> _logger = logger;
        private readonly IConfiguration _config = config;
        private readonly string[] _labels = config.GetSection("App:Kanban:InitialColumns").Get<string[]>() ?? [];


        // Синхронизация пользователей с БД
        public async Task<Result> SyncUsers(List<GitUser> gitUsers, long userId)
        {
            // Валидация входных данных
            if (gitUsers == null || gitUsers.Count == 0)
            {
                _logger.LogWarning("Пустой список пользователей для синхронизации");
                return Result.Success();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Получаем существующих пользователей одним запросом
                var gitUsersIds = gitUsers.Select(g => g.username).ToList();
                // Получаем список пользователей из БД связанных с гитом
                var existingUsers = await _context.TdUsers.Where(x => x.Login != "admin").ToDictionaryAsync(x => x.Login);
                var usersToRemove = existingUsers.Where(x => !gitUsersIds.Contains(x.Key)).Select(x => x.Value).ToList();

                foreach (var gitUser in gitUsers)
                {
                    if (string.IsNullOrEmpty(gitUser.username))
                    {
                        _logger.LogWarning("Пользователь с ID {UserId} имеет пустой username, пропускаем", gitUser.id);
                        continue;
                    }

                    if (existingUsers.TryGetValue(gitUser.username, out var existingUser))
                    {
                        // Обновляем существующего пользователя
                        await UpdateExistingUser(existingUser, gitUser, userId);
                        _context.TdUsers.Update(existingUser);
                        _logger.LogInformation("Обновление пользователя {user}", existingUser.Keyid);
                    }
                    else
                    {
                        // Добавляем нового пользователя
                        var newUser = gitUser.MapTdUser();
                        await _context.TdUsers.AddAsync(newUser);
                        _logger.LogInformation("Добавление нового пользователя {username}", gitUser.username);
                    }
                }

                // Пакетные операции
                if (usersToRemove.Count != 0)
                    _context.TdUsers.RemoveRange(usersToRemove);

                // Сохраняем все изменения одним вызовом
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation("Синхронизировано {changes} пользователей", changes);

                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "{method}: Ошибка синхронизации с БД", StaticExtensions.GetCurrentMethodName());
                return Result.Error(-1, ex.Message);
            }
        }
        private async Task UpdateExistingUser(TdUser existing, GitUser gitUser, long userId)
        {
            var hsnap = HistorySnap.Create();

            if (existing.GitId != gitUser.id)
                existing.GitId = gitUser.id;

            // Обновляем только измененные поля
            if (hsnap.CheckForChangesAndAdd(nameof(existing.Email), existing.Email, gitUser.email))
                existing.Email = gitUser.email;

            if (hsnap.CheckForChangesAndAdd(nameof(existing.UserName), existing.UserName, gitUser.name))
                existing.UserName = gitUser.name;

            if (hsnap.CheckForChangesAndAdd(nameof(existing.Login), existing.Login, gitUser.username))
                existing.Login = gitUser.username;

            if (hsnap.HasChanges)
                await _context.TdHistories.AddAsync(new()
                {
                    IsSystem = true,
                    Action = "Обновлено",
                    EntityId = existing.Keyid,
                    Description = hsnap.ToJson(),
                    EntityType = nameof(existing),
                    UserId = userId
                });
        }


        // Синхронизация проектов с БД
        public async Task<Result> SyncProjects(List<GitProject> gitProjects, long userId)
        {
            // Валидация входных данных
            if (gitProjects == null || gitProjects.Count == 0)
            {
                _logger.LogWarning("Пустой список проектов для синхронизации");
                return Result.Success();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Получаем ID всех проектов из GitLab
                var GitIds = gitProjects.Select(g => g.id).ToList();

                // Получаем список проектов из БД связанных с гитом
                var existingProjects = await _context.TdProjects.Where(x => x.GitId != null).ToDictionaryAsync(x => x.GitId ?? 0);
                var projectsToRemove = existingProjects.Where(x => !GitIds.Contains(x.Key)).Select(x => x.Value).ToList();

                foreach (var gitProject in gitProjects)
                {
                    if (string.IsNullOrEmpty(gitProject.name))
                    {
                        _logger.LogWarning("Проект с ID {project_id} имеет пустое имя, пропускаем", gitProject.id);
                        continue;
                    }

                    if (existingProjects.TryGetValue(gitProject.id, out var existingProject))
                    {
                        // Обновляем существующий проект
                        await UpdateExistingProject(existingProject, gitProject, userId);
                        _context.Entry(existingProject).State = EntityState.Modified;
                        _logger.LogInformation("Обновление проекта {project_id}", existingProject.Keyid);
                    }
                    else
                    {
                        // Добавляем новый проект
                        var newProject = gitProject.MapTdProject(_config.GetSection("App:Kanban:InitialColumns").Get<string[]>() ?? []);
                        await _context.TdProjects.AddAsync(newProject);
                        _logger.LogInformation("Добавление нового проекта {project_name}", gitProject.name);
                    }
                }

                // Пакетные операции
                if (projectsToRemove.Count != 0)
                    _context.TdProjects.RemoveRange(projectsToRemove);

                // Сохраняем все изменения одним вызовом
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation("Синхронизировано {changes} проектов", GitIds.Count);

                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "{method}: Ошибка синхронизации проектов с БД", StaticExtensions.GetCurrentMethodName());
                return Result.Error(-1, ex.Message);
            }
        }
        private async Task UpdateExistingProject(TdProject existing, GitProject gitProject, long userId)
        {
            var hsnap = HistorySnap.Create();

            // Обновляем только измененные поля
            if (hsnap.CheckForChangesAndAdd(nameof(existing.ProjectName), existing.ProjectName, gitProject.name))
                existing.ProjectName = gitProject.name;

            if (hsnap.CheckForChangesAndAdd(nameof(existing.OwnerId), existing.OwnerId.ToString(), gitProject.owner?.id.ToString() ?? ""))
                existing.OwnerId = gitProject.owner?.id;

            if (hsnap.HasChanges)
                await _context.TdHistories.AddAsync(new()
                {
                    IsSystem = true,
                    Action = "Обновлено",
                    EntityId = existing.Keyid,
                    Description = hsnap.ToJson(),
                    EntityType = nameof(existing),
                    UserId = userId
                });
        }


        // Синхронизация групп с БД
        public async Task<Result> SyncGroups(List<GitGroup> gitGroups, long userId)
        {
            // Валидация входных данных
            if (gitGroups == null || gitGroups.Count == 0)
            {
                _logger.LogWarning("Пустой список групп для синхронизации");
                return Result.Success();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Получаем ID всех групп из GitLab
                var GitIds = gitGroups.Select(g => g.id).ToList();
                // Получаем существующие группы одним запросом
                var existingGroups = await _context.TdGroups.Where(x => x.GitId != null).ToDictionaryAsync(x => x.GitId ?? 0);
                var groupsToRemove = existingGroups.Where(x => !GitIds.Contains(x.Key)).Select(x => x.Value).ToList();

                foreach (var gitGroup in gitGroups)
                {
                    if (string.IsNullOrEmpty(gitGroup.name))
                    {
                        _logger.LogWarning("Группа с ID {Groupid} имеет пустое имя, пропускаем", gitGroup.id);
                        continue;
                    }

                    if (existingGroups.TryGetValue(gitGroup.id, out var existingGroup))
                    {
                        // Обновляем существующую группу
                        await UpdateExistingGroup(existingGroup, gitGroup, userId);
                        _context.Entry(existingGroup).State = EntityState.Modified;
                        _logger.LogInformation("Обновление группы {Groupid}", existingGroup.Keyid);
                    }
                    else
                    {
                        // Добавляем новую группу
                        var newGroup = gitGroup.MapTdGroup();
                        await _context.TdGroups.AddAsync(newGroup);
                        _logger.LogInformation("Добавление новой группы {Group_name}", gitGroup.name);
                    }
                }

                // Пакетные операции
                if (groupsToRemove.Count != 0)
                    _context.TdGroups.RemoveRange(groupsToRemove);

                // Сохраняем все изменения одним вызовом
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation("Синхронизировано {changes} групп", changes);

                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "{method}: Ошибка синхронизации групп с БД", StaticExtensions.GetCurrentMethodName());
                return Result.Error(-1, ex.Message);
            }
        }
        private async Task UpdateExistingGroup(TdGroup existing, GitGroup gitGroup, long userId)
        {
            var hsnap = HistorySnap.Create();

            // Обновляем только измененные поля
            if (hsnap.CheckForChangesAndAdd(nameof(existing.GroupName), existing.GroupName, gitGroup.name))
                existing.GroupName = gitGroup.name;

            if (hsnap.CheckForChangesAndAdd(nameof(existing.GitId), existing.GitId.ToString(), gitGroup.id.ToString()))
                existing.GitId = gitGroup.id;

            if (hsnap.HasChanges)
                await _context.TdHistories.AddAsync(new()
                {
                    IsSystem = true,
                    Action = "Обновлено",
                    EntityId = existing.Keyid,
                    Description = hsnap.ToJson(),
                    EntityType = nameof(existing),
                    UserId = userId
                });
        }


        // Синхронизация задач с БД
        public async Task<Result> SyncIssues(List<GitIssue> gitIssues, long userId)
        {
            // Валидация входных данных
            if (gitIssues == null || gitIssues.Count == 0)
            {
                _logger.LogWarning("Пустой список задач для синхронизации");
                return Result.Success();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Получаем ID всех задач из GitLab
                var gitIssueIds = gitIssues.Select(g => g.id).ToList();
                // Получаем существующие карточки одним запросом
                var existingCards = await _context.TdCards.ToDictionaryAsync(x => x.GitId);
                var cardsToRemove = existingCards.Where(x => !gitIssueIds.Contains(x.Key)).Select(x => x.Value).ToList();

                // Получаем всех возможных пользователей (авторов и исполнителей) одним запросом
                var allUserIds = gitIssues
                    .SelectMany(i => new[] { i.author.id, i.assignee?.id ?? 0 })
                    .Where(id => id != 0)
                    .Distinct()
                    .ToList();

                var existingUsers = await _context.TdUsers
                    .Where(u => u.GitId.HasValue)
                    .ToDictionaryAsync(u => u.GitId!.Value);

                foreach (var gitIssue in gitIssues)
                {
                    if (string.IsNullOrEmpty(gitIssue.title))
                    {
                        _logger.LogWarning("Задача с ID {IssueId} имеет пустой заголовок, пропускаем", gitIssue.id);
                        continue;
                    }

                    // Проверяем существование связанных пользователей
                    if (!existingUsers.ContainsKey(gitIssue.author.id))
                    {
                        _logger.LogWarning("Автор задачи {Issueid} (GitId: {gitIssueAuthorId}) не найден в БД", gitIssue.id, gitIssue.author.id);
                        continue;
                    }

                    var columns = _context.TdColumns.Where(x => x.Project.GitId == gitIssue.project_id);

                    if (existingCards.TryGetValue(gitIssue.id, out var existingCard))
                    {
                        // Обновляем существующую карточку
                        await UpdateExistingCard(existingCard, gitIssue, existingUsers, userId);
                        _context.Entry(existingCard).State = EntityState.Modified;
                        _logger.LogInformation("Обновление карточки {existingCardid}", existingCard.Keyid);

                        switch (gitIssue.state)
                        {
                            case "opened":
                                if (gitIssue.labels?.Any(x => x == "Бэклог") ?? false)
                                    existingCard.ColumnId = columns.First(x => x.ColumnName == "Бэклог").Keyid;
                                else if (!gitIssue.labels?.Any(x => _labels.Any(l => l == x)) ?? false)
                                    existingCard.ColumnId = columns.First(x => x.ColumnName == "Открыто").Keyid;
                                break;
                            case "closed":
                                if (!gitIssue.labels?.Any(x => _labels.Any(l => l == x)) ?? false)
                                    existingCard.ColumnId = columns.First(x => x.ColumnName == "Закрыто").Keyid;
                                else if (!gitIssue.labels?.Any(x => x == "Тестирование") ?? false)
                                    existingCard.ColumnId = columns.First(x => x.ColumnName == "Тестирование").Keyid;
                                else if (!gitIssue.labels?.Any(x => x == "Передано в публикацию") ?? false)
                                    existingCard.ColumnId = columns.First(x => x.ColumnName == "Передано в публикацию").Keyid;
                                else if (!gitIssue.labels?.Any(x => x == "") ?? false)
                                    existingCard.ColumnId = columns.First(x => x.ColumnName == "Опубликовано").Keyid;
                                break;
                            default:
                                existingCard.ColumnId = columns.First(x => x.ColumnName == "Бэклог").Keyid;
                                break;
                        }
                    }
                    else
                    {
                        // Добавляем новую карточку
                        var newCard = gitIssue.MapTdCard();

                        // Устанавливаем связь с пользователями
                        newCard.Author = existingUsers[gitIssue.author.id];
                        if (gitIssue.assignee != null && existingUsers.TryGetValue(gitIssue.assignee.id, out var assignee))
                        {
                            newCard.Assigned = assignee;
                        }

                        switch (gitIssue.state)
                        {
                            case "opened":
                                if (gitIssue.labels?.Any(x => x == "Бэклог") ?? false)
                                    newCard.ColumnId = columns.First(x => x.ColumnName == "Бэклог").Keyid;
                                else if (!gitIssue.labels?.Any(x => x == "Бэклог") ?? false)
                                    newCard.ColumnId = columns.First(x => x.ColumnName == "Открыто").Keyid;
                                break;
                            case "closed":
                                if (gitIssue.labels?.Any(x => x == "Тестирование") ?? false)
                                    newCard.ColumnId = columns.First(x => x.ColumnName == "Тестирование").Keyid;
                                else if (gitIssue.labels?.Any(x => x == "Тестирование завершено") ?? false)
                                    newCard.ColumnId = columns.First(x => x.ColumnName == "Передано в публикацию").Keyid;
                                else if (gitIssue.labels?.Any(x => x == "Опубликовано") ?? false)
                                    newCard.ColumnId = columns.First(x => x.ColumnName == "Опубликовано").Keyid;
                                else if (!gitIssue.labels?.Any(x => _labels.Any(l => l == x)) ?? false)
                                    newCard.ColumnId = columns.First(x => x.ColumnName == "Закрыто").Keyid;
                                break;
                        }

                        _logger.LogDebug("Загружено columnid: {id} CardName: {name}", newCard.ColumnId, newCard.CardName);

                        await _context.TdCards.AddAsync(newCard);
                        _logger.LogInformation("Добавление новой карточки {IssueTitle}", gitIssue.title);
                    }
                }

                // Пакетные операции
                if (cardsToRemove.Count != 0)
                    _context.TdCards.RemoveRange(cardsToRemove);

                // Сохраняем все изменения одним вызовом
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation("Синхронизировано {changes} карточек", changes);

                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "{method}: Ошибка синхронизации задач с БД", StaticExtensions.GetCurrentMethodName());
                return Result.Error(-1, ex.Message);
            }
        }
        public async Task<Result<TdCard>> AddIssue(GitIssue gitIssue, long userId)
        {
            try
            {
                var existingUsers = await _context.TdUsers
                        .Where(u => u.GitId.HasValue)
                        .ToDictionaryAsync(u => u.GitId!.Value);

                // Добавляем новую карточку
                var newCard = gitIssue.MapTdCard();

                // Устанавливаем связь с пользователями
                newCard.Author = existingUsers[gitIssue.author.id];
                if (gitIssue.assignee != null && existingUsers.TryGetValue(gitIssue.assignee.id, out var assignee))
                {
                    newCard.Assigned = assignee;
                }

                newCard.ColumnId = (await _context.TdColumns
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Project.GitId == gitIssue.project_id && x.ColumnName == "Бэклог")).Keyid;

                _logger.LogDebug("Загружено columnid: {id} CardName: {name}", newCard.ColumnId, newCard.CardName);

                await _context.TdCards.AddAsync(newCard);
                _logger.LogInformation("Добавление новой карточки {IssueTitle}", gitIssue.title);

                await _context.SaveChangesAsync();

                return Result<TdCard>.Success(newCard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: Ошибка добавления задачи в БД", StaticExtensions.GetCurrentMethodName());
                return Result<TdCard>.Error(-1, ex.Message);
            }
        }
        public async Task<Result> UpdateIssue(GitIssue gitIssue, IssueAgg issueAgg)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                var existingCard = issueAgg.Card;
                var existingUsers = await _context.TdUsers
                    .Where(u => u.GitId.HasValue)
                    .ToDictionaryAsync(u => u.GitId!.Value);

                // Обновляем существующую карточку
                await UpdateExistingCard(existingCard, gitIssue, existingUsers, issueAgg.User.Keyid);
                _context.Entry(existingCard).State = EntityState.Modified;
                _logger.LogInformation("Обновление карточки {existingCardid}", existingCard.Keyid);

                // Сохраняем все изменения одним вызовом
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation("Синхронизировано {changes} карточек", changes);

                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: Ошибка обновления задачи в БД", StaticExtensions.GetCurrentMethodName());
            }

            return Result.Error(-1, $"Ошибка обновления project {gitIssue.id} issue {gitIssue.iid} в БД");
        }
        private async Task UpdateExistingCard(TdCard existing, GitIssue gitIssue, Dictionary<long, TdUser> existingUsers, long userId)
        {
            var hsnap = HistorySnap.Create();

            // Обновляем только измененные поля
            if (hsnap.CheckForChangesAndAdd(nameof(existing.CardName), existing.CardName, gitIssue.title))
                existing.CardName = gitIssue.title;

            if (hsnap.CheckForChangesAndAdd(nameof(existing.Description), existing.Description, gitIssue.description))
                existing.Description = gitIssue.description;

            if (hsnap.CheckForChangesAndAdd(nameof(existing.Duedate), existing.Duedate.ToString(), gitIssue.due_date.ToString()))
                existing.Duedate = gitIssue.due_date;

            // Обновляем исполнителя, если он изменился
            if (gitIssue.assignee != null
                && existingUsers.TryGetValue(gitIssue.assignee.id, out var assignee)
                && hsnap.CheckForChangesAndAdd(nameof(existing.AssignedId),
                                                existing.AssignedId.ToString(),
                                                assignee.Keyid.ToString()))
                existing.Assigned = assignee;

            if (hsnap.HasChanges)
                await _context.TdHistories.AddAsync(new()
                {
                    IsSystem = true,
                    Action = "Обновлено",
                    EntityId = existing.Keyid,
                    Description = hsnap.ToJson(),
                    EntityType = nameof(existing),
                    UserId = userId
                });
        }


        // Синхронизация переписок/комментариев с БД
        public async Task<Result> SyncDiscussions(List<GitDiscussion> gitDiscussions, long userId, long? cardID = null)
        {
            // Валидация входных данных
            if (gitDiscussions == null || gitDiscussions.Count == 0)
            {
                _logger.LogWarning("Пустой список обсуждений для синхронизации");
                return Result.Success();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Получаем ID всех комментариев из GitLab
                var incommingIds = gitDiscussions.SelectMany(g => g.notes.Select(a => a.id)).ToList();

                // Получаем существующие комментарии одним запросом
                Dictionary<long, TdComment> existingComments;

                if (cardID is null)
                    existingComments = await _context.TdComments
                        .Where(x => x.GitId.HasValue)
                        .ToDictionaryAsync(x => x.GitId!.Value);
                else
                {
                    existingComments = await _context.TdComments
                        .Include(x => x.CardNavigation)
                        .Where(x => x.GitId.HasValue && x.CardNavigation.GitId == cardID)
                        .ToDictionaryAsync(x => x.GitId!.Value);
                }

                var existingGitUsers = await _context.TdUsers.Where(x => x.GitId != null).ToListAsync();
                var cards = await _context.TdCards.Include(x => x.Column).ThenInclude(x => x.Project).ToListAsync();
                string commentText = string.Empty;
                long? commentUserId = null;

                foreach (var gitDiscussion in gitDiscussions)
                {
                    foreach (var note in gitDiscussion.notes) // .Where(x => !x.system) Хз че делать с системными логами пока
                    {
                        bool IsFormattedComment = note.author.username == "CommentBot" &&
                            GitApiData.TryParseCommentInfo(note.body, out commentUserId, out commentText);

                        if (IsFormattedComment)
                            note.body = commentText;

                        if (existingComments.TryGetValue(note.id, out var existingDiscussion))
                        {
                            // Обновляем существующий комментарий
                            await UpdateExistingComment(existingDiscussion, note, userId);
                            _context.Entry(existingDiscussion).State = EntityState.Modified;
                            _logger.LogInformation("Обновление комментария {Commentid}", existingDiscussion.Keyid);
                        }
                        else
                        {
                            var card = cards.FirstOrDefault(x =>
                            x.GitId == note.noteable_id &&
                            x.Column.Project.GitId == note.project_id);

                            // Добавляем новый комментарий
                            if (card is null)
                            {
                                _logger.LogWarning("Ошибка добавления нового комментария {GitContext} в noteId {NoteId}, не найдена карточка с cardId {cardId} в проекте {project_id}", gitDiscussion.ContextId, note.id, note.noteable_id, note.project_id);
                                continue;
                            }

                            var newComment = note.MapTdComment(gitDiscussion.ContextId, card.Keyid, existingGitUsers, IsFormattedComment ? commentUserId : null);
                            await _context.TdComments.AddAsync(newComment);
                            _logger.LogInformation("Добавление нового комментария {context}", newComment.Context);
                        }
                    }
                }

                // Пакетные операции
                var commentsToRemove = existingComments.Where(x => !incommingIds.Contains(x.Key)).Select(x => x.Value).ToList();

                foreach (var comment in commentsToRemove)
                {
                    comment.Deleted = true;
                    _context.Entry(comment).State = EntityState.Modified;
                }

                // Сохраняем все изменения одним вызовом
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation("Синхронизировано {count} комментариев", incommingIds.Count);

                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "{method}: Ошибка синхронизации комментариев с БД", StaticExtensions.GetCurrentMethodName());
                return Result.Error(-1, ex.Message);
            }
        }
        public async Task<Result<TdComment>> AddComment(GitDiscussion discussion, UserComment comment)
        {
            try
            {
                // Добавление комментария в БД
                var note = discussion.notes.First();
                var existingGitUsers = await _context.TdUsers.Where(x => x.GitId != null).ToListAsync();
                var cards = await _context.TdCards.Include(x => x.Column).ThenInclude(x => x.Project).ToListAsync();
                var card = cards.FirstOrDefault(x => x.GitId == note.noteable_id && x.Column.Project.GitId == note.project_id);
                string commentText = string.Empty;
                long? commentUserId = null;
                bool IsFormattedComment = note.author.username == "CommentBot" &&
                    GitApiData.TryParseCommentInfo(note.body, out commentUserId, out commentText);

                if (IsFormattedComment)
                    note.body = commentText;

                if (card is null)
                {
                    _logger.LogError("Ошибка добавления нового комментария {GitContext} в noteId {NoteId}, не найдена карточка с cardId {cardId} в проекте {project_id}", discussion.ContextId, note.id, note.noteable_id, note.project_id);
                    return Result<TdComment>.Error(-1, "Ошибка добавления нового комментария в БД");
                }

                var newComment = note.MapTdComment(discussion.ContextId, card.Keyid, existingGitUsers, commentUserId);
                await _context.TdComments.AddAsync(newComment);
                _logger.LogInformation("Добавление нового комментария {context}", newComment.Context);

                await _context.SaveChangesAsync();
                return Result<TdComment>.Success(newComment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: Ошибка добавления задачи в БД", StaticExtensions.GetCurrentMethodName());
                return Result<TdComment>.Error(-1, ex.Message);
            }
        }
        private async Task UpdateExistingComment(TdComment existing, Note gitDiscussion, long userId)
        {
            var hsnap = HistorySnap.Create();

            string? oldtext = null;
            // Обновляем только измененные поля
            if (hsnap.CheckForChangesAndAdd(nameof(existing.CommentText), existing.CommentText, gitDiscussion.body))
            {
                oldtext = existing.CommentText;
                existing.CommentText = gitDiscussion.body;
            }

            if (hsnap.HasChanges)
                await _context.TdHistories.AddAsync(new()
                {
                    Action = "Обновлено",
                    EntityId = existing.Keyid,
                    EntityType = nameof(existing),
                    Description = hsnap.ToJson(),
                    OldState = oldtext,
                    UserId = userId
                });
        }
    }
}
