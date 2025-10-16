using Application_lib.Gitlab;
using Common_lib.Models.ServiceModels;
using Domain_lib;
using Domain_lib.Entities;
using Domain_lib.Gitlab.Get;
using Domain_lib.Gitlab.Post;
using Domain_lib.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure_lib.Gitlab
{
    /// <inheritdoc/>
    public class GitApiData(ILogger<GitApiData> logger, IHttpClientFactory httpClientFactory, IConfiguration config) : IGitApiService
    {
        private readonly ILogger<GitApiData> _logger = logger;
        private readonly IConfiguration _config = config;
        private readonly HttpClient _gitClient = httpClientFactory.CreateClient("GitClient");
        private readonly string[] _labelsList = config.GetSection("App:Kanban:InitialColumns").Get<string[]>() ?? [];

        public static string AddCommentInfo(string commentText, string user, long dbUserId)
        {
            return $"[Tasker_{dbUserId}]{user}: {commentText}";
        }
        public static bool TryParseCommentInfo(string commentInfo, out long? dbUserId, out string commentText)
        {
            dbUserId = null;
            commentText = string.Empty;

            if (string.IsNullOrEmpty(commentInfo))
                return false;

            // Шаблон для разбора строки [Tasker_{dbUserId}]{user}: {commentText}
            var pattern = @"\[Tasker_(?<dbUserId>\d+)\](?<user>[^:]+):\s?(?<commentText>[\s\S]*)";
            var match = Regex.Match(commentInfo, pattern);

            if (!match.Success)
                return false;

            // Парсим dbUserId
            if (!long.TryParse(match.Groups["dbUserId"].Value, out var UserId))
                return false;
            else
                dbUserId = UserId;

            // Получаем commentText
            commentText = match.Groups["commentText"].Value;

            return true;
        }
        public async Task<Result> SetIssueState(long? issueId, long? projectId, bool IsFromOpenedState, bool IsOpened)
        {
            // Если состояние задачи не изменилось, то и дублировать запрос нет смысла
            if (IsOpened == IsFromOpenedState)
            {
                return Result.Success();
            }

            string error;
            if (issueId is null)
                return Result.Error(-1, "id задачи не заполнено");

            if (projectId is null)
                return Result.Error(-1, "id проекта не заполнено");

            try
            {
                var result = await _gitClient.PutAsync($"projects/{projectId}/issues/{issueId}?state_event={(IsOpened ? "reopen" : "close")}", null);

                if (result.IsSuccessStatusCode)
                    return Result.Success();

                error = $"Ошибка изменения статуса задачи projectId:{projectId} issueId:{issueId} message:{await result.Content.ReadAsStringAsync()}";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка изменения статуса задачи projectId:{projectId} issueId:{issueId}", StaticExtensions.GetCurrentMethodName(), projectId, issueId);
            }
            return Result.Error(-1, error);
        }

        /* Создание одиночных сущностей
         * Карточки задач
         * Загрузка файлов
         * Создание комментариев
         * Создание и удаление лейблов
         */
        public async Task<Result> RemoveDefaultLabels(long? issueId, long? projectId)
        {
            string error;
            if (issueId is null)
                return Result.Error(-1, "id задачи не заполнено");

            if (projectId is null)
                return Result.Error(-1, "id проекта не заполнено");

            try
            {

                var removeLabels = string.Join(',', _labelsList);
                var result = await _gitClient.PutAsync($"projects/{projectId}/issues/{issueId}?remove_labels={removeLabels}", null);

                if (result.IsSuccessStatusCode)
                    return Result.Success();

                error = $"Ошибка очистки тегов в projectId:{projectId} issueId:{issueId} message:{await result.Content.ReadAsStringAsync()}";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка очистки тегов в projectId:{projectId} issueId:{issueId}", StaticExtensions.GetCurrentMethodName(), projectId, issueId);
            }
            return Result.Error(-1, error);
        }
        public async Task<Result> AddLabel(long? issueId, long? projectId, string label)
        {
            return await AddLabels(issueId, projectId, [label]);
        }
        public async Task<Result> AddLabels(long? issueId, long? projectId, string[] labels)
        {
            string error;
            if (issueId is null)
                return Result.Error(-1, "id задачи не заполнено");

            if (projectId is null)
                return Result.Error(-1, "id проекта не заполнено");

            try
            {
                var addLabels = string.Join(",", labels);
                var result = await _gitClient.PutAsync($"projects/{projectId}/issues/{issueId}?add_labels={addLabels}", null);

                if (result.IsSuccessStatusCode)
                    return Result.Success();

                error = $"Ошибка добавления labels в projectId:{projectId} issueId:{issueId} labels:[{addLabels}] message:{await result.Content.ReadAsStringAsync()}";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка добавления labels [{labels}] в projectId:{projectId} issueId:{issueId}", StaticExtensions.GetCurrentMethodName(), string.Join(',', labels), projectId, issueId);
            }
            return Result.Error(-1, error);
        }
        public async Task<Result<GitIssue>> CreateIssue(IssueAgg issueAgg)
        {
            string error;
            try
            {
                var apiIssue = new IssueToCreate()
                {
                    AssigneeId = issueAgg.User.GitId,
                    Description = issueAgg.Card.Description,
                    DueDate = issueAgg.Card.Duedate?.ToString("yyyy-MM-dd"),
                    ProjectId = issueAgg.ProjectId,
                    IssueType = "issue",
                    Labels = "Бэклог",
                    Title = issueAgg.Card.CardName
                };
                var result = await _gitClient.PostAsJsonAsync($"projects/{apiIssue.ProjectId}/issues", apiIssue);

                if (!result.IsSuccessStatusCode)
                    return Result<GitIssue>.Error(-1, $"Ошибка получения данных StatusCode:{result.StatusCode}");

                var issueObject = await JsonSerializer.DeserializeAsync<GitIssue>(await result.Content.ReadAsStreamAsync());

                if (issueObject is null)
                    return Result<GitIssue>.Error(-1, $"Ошибка сериализации ответа api");

                return Result<GitIssue>.Success(issueObject);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка добавления карточки в гит", StaticExtensions.GetCurrentMethodName());
            }

            return Result<GitIssue>.Error(-1, error);
        }
        public async Task<Result<GitIssue>> UpdateIssue(IssueAgg issueAgg)
        {
            string error;
            try
            {
                var apiIssue = new IssueToCreate()
                {
                    AssigneeId = issueAgg.User.GitId,
                    Description = issueAgg.Card.Description?.Replace("\n","<br>"),
                    DueDate = issueAgg.Card.Duedate?.ToString("yyyy-MM-dd"),
                    Title = issueAgg.Card.CardName
                };
                var result = await _gitClient.PutAsJsonAsync($"projects/{issueAgg.ProjectId}/issues/{issueAgg.Card.GitIid}", apiIssue);

                if (!result.IsSuccessStatusCode)
                    return Result<GitIssue>.Error(-1, $"Ошибка получения данных StatusCode:{result.StatusCode}");

                var issueObject = await JsonSerializer.DeserializeAsync<GitIssue>(await result.Content.ReadAsStreamAsync());


                if (issueObject is null)
                    return Result<GitIssue>.Error(-1, $"Ошибка сериализации ответа api");

                issueObject.description = issueObject.description?.Replace("<br>", "\n");

                return Result<GitIssue>.Success(issueObject);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка добавления карточки в гит", StaticExtensions.GetCurrentMethodName());
            }

            return Result<GitIssue>.Error(-1, error);
        }
        public async Task<Result<GitDiscussion>> CreateComment(UserComment comment)
        {
            string error;
            try
            {
                var apiNote = new NoteToCreate()
                {
                    Body = AddCommentInfo(comment.Body,
                                          comment.AuthorName ?? "Undefined",
                                          comment.UserKeyId)
                };
                var result = await _gitClient.PostAsJsonAsync($"projects/{comment.GitProjectId}/issues/{comment.GitIssueId}/discussions", apiNote);

                if (!result.IsSuccessStatusCode)
                    return Result<GitDiscussion>.Error(-1, $"Ошибка получения данных StatusCode:{result.StatusCode}");

                var issueObject = await JsonSerializer.DeserializeAsync<GitDiscussion>(await result.Content.ReadAsStreamAsync());

                if (issueObject is null)
                    return Result<GitDiscussion>.Error(-1, $"Ошибка сериализации ответа api");

                return Result<GitDiscussion>.Success(issueObject);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка добавления карточки в гит", StaticExtensions.GetCurrentMethodName());
            }
            return Result<GitDiscussion>.Error(-1, error);
        }
        public async Task<Result<GitDiscussion>> ReplyComment(UserComment comment)
        {
            string error;
            try
            {
                var apiNote = new NoteToCreate()
                {
                    Body = AddCommentInfo(comment.Body, comment.AuthorName ?? "Undefined", comment.UserKeyId),
                    ReplyNoteId = comment.ReplyedId
                };
                var result = await _gitClient.PostAsJsonAsync($"projects/{comment.GitProjectId}/issues/{comment.GitIssueId}/discussions/{comment.ReplyContext}/notes", apiNote);

                if (!result.IsSuccessStatusCode)
                    return Result<GitDiscussion>.Error(-1, $"Ошибка получения данных StatusCode:{result.StatusCode}");

                var issueObject = await JsonSerializer.DeserializeAsync<Note>(await result.Content.ReadAsStreamAsync());

                if (issueObject is null)
                    return Result<GitDiscussion>.Error(-1, $"Ошибка сериализации ответа api");

                return Result<GitDiscussion>.Success(new() { ContextId = comment.ReplyContext!, notes = [issueObject] });
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка добавления карточки в гит", StaticExtensions.GetCurrentMethodName());
            }
            return Result<GitDiscussion>.Error(-1, error);
        }
        public async Task<Result> UploadFile(long projectId, Stream[] files)
        {
            string error;
            GitUploaded? gitUploaded = null;
            List<GitUploaded> gitFiles = [];
            MultipartFormDataContent content;
            HttpResponseMessage result;

            try
            {
                foreach (var fileStream in files)
                {
                    content = [new StreamContent(fileStream)];
                    result = await _gitClient.PostAsync($"projects/{projectId}/uploads", content);
                    gitUploaded = await JsonSerializer.DeserializeAsync<GitUploaded>(await result.Content.ReadAsStreamAsync());

                    if (gitUploaded != null)
                        gitFiles.Add(gitUploaded);
                }

                //if (result.IsSuccessStatusCode)
                //    return Result.Success();

                error = $"Ошибка получения данных IsSuccess ";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка добавления карточки в гит", StaticExtensions.GetCurrentMethodName());
            }

            return Result<GitIssue>.Error(-1, error);
        }

        /* Глобальная синхронизация всех сущностей
         * Пользователей
         * Групп
         * Проектов
         * Задач
         * Комментариев и переписок
         */
        public async Task<Result<List<GitUser>>> GetUsers()
        {
            string error;

            try
            {
                var result = await GetFromGit<GitUser>("users", new() { { "page", "1" } });

                if (result.IsSuccess)
                    return result;

                error = $"Ошибка получения данных IsSuccess {result.IsSuccess}";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка получения данных метод", StaticExtensions.GetCurrentMethodName());
            }
            return Result<List<GitUser>>.Error(-1, error);
        }
        public async Task<Result<List<GitGroup>>> GetGroups()
        {
            string error;

            try
            {
                var result = await GetFromGit<GitGroup>("groups", new() { { "page", "1" } });

                if (result.IsSuccess)
                    return result;

                error = $"Ошибка получения данных IsSuccess {result.IsSuccess}";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка получения данных метод", StaticExtensions.GetCurrentMethodName());
            }
            return Result<List<GitGroup>>.Error(-1, error);
        }
        public async Task<Result<List<GitProject>>> GetProjects()
        {
            string error;

            try
            {
                var result = await GetFromGit<GitProject>("projects", new() { { "page", "1" } });

                if (result.IsSuccess)
                    return result;

                error = $"Ошибка получения данных IsSuccess {result.IsSuccess}";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка получения данных метод", StaticExtensions.GetCurrentMethodName());
            }
            return Result<List<GitProject>>.Error(-1, error);
        }
        public async Task<Result<List<GitIssue>>> GetIssues()
        {
            string error;

            try
            {
                var result = await GetFromGit<GitIssue>("issues", new() { { "page", "1" }, { "scope", "all" } });

                if (result.IsSuccess)
                    return result;

                error = $"Ошибка получения данных IsSuccess {result.IsSuccess}";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка получения данных метод", StaticExtensions.GetCurrentMethodName());
            }
            return Result<List<GitIssue>>.Error(-1, error);
        }
        public async Task<Result<List<GitDiscussion>>> GetDiscussions(List<GitIssue> issues)
        {
            string error;

            try
            {
                List<GitDiscussion> entities = [];
                HttpResponseMessage? response = null;
                Dictionary<string, string> args = new() { { "page", "1" }, { "per_page", "50" } };
                string ArgsStrting = await CombinePathArgs(args);

                try
                {
                    foreach (var issue in issues)
                    {
                        _logger.LogInformation("Синхронизация комментариев проекта {project_id} в задаче {issueiid}", issue.project_id, issue.iid);
                        var route = $"projects/{issue.project_id}/issues/{issue.iid}/discussions";

                        // Запрос с первой страницы
                        response = await _gitClient.GetAsync($"{route}{ArgsStrting}");

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Ошибка запроса {route} в git StatusCode: {StatusCode}, Message: {Message}", route, response?.StatusCode, response?.Content.ReadAsStringAsync());
                            return Result<List<GitDiscussion>>.Error(2, $"Ошибка запроса {route} в git StatusCode: {response?.StatusCode}, Message: {response?.Content.ReadAsStringAsync()}");
                        }

                        await AddFromJsonToList(response, entities);

                        while (response.Headers.TryGetValues("X-Next-Page", out var nextPage) && !string.IsNullOrWhiteSpace(nextPage.FirstOrDefault()))
                        {
                            // Т.к. в аргументах указаны страницы.
                            // А из ответа мы считываем пагинацию,
                            // то меняем номер страницы для следующего
                            // запроса с каждой итерацией.
                            args["page"] = nextPage.First();

                            // Обработка пагинации
                            response = await _gitClient.GetAsync($"{route}{await CombinePathArgs(args)}");
                            await AddFromJsonToList(response, entities);
                        }
                    }

                    return Result<List<GitDiscussion>>.Success(entities);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{method}: Ошибка запроса {route} в git StatusCode: {StatusCode}", StaticExtensions.GetCurrentMethodName(), "discussions", response?.StatusCode);
                    return Result<List<GitDiscussion>>.Error(1, $"Ошибка запроса discussions в git");
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка получения данных метод", StaticExtensions.GetCurrentMethodName());
            }
            return Result<List<GitDiscussion>>.Error(-1, error);
        }

        /* Вспомогательные методы
         * Используются для сбора данных из ответа gitlab
         */
        /// <summary>
        /// Универсальный метод для разрешения пагинации.
        /// По русски: В ответе от гита, в хэдере возвращается
        /// тег X-Next-Page, который рабоатет указателем
        /// на следующую страницу. Этот метод стучится повторно
        /// за данными на следующей странцие, пока не соберет все.
        /// </summary>
        /// <typeparam name="T">Тип EntityExt</typeparam>
        /// <param name="route">Путь в апи, куда стучаться</param>
        /// <returns>Результат обработки</returns>
        private async Task<Result<List<T>>> GetFromGit<T>(string route, Dictionary<string, string> args)
        {
            List<T> entities = [];
            HttpResponseMessage? response = null;
            string ArgsStrting = await CombinePathArgs(args);
            bool argsHasPages = args.Keys.Any(x => x == "page");

            try
            {
                // Запрос с первой страницы
                response = await _gitClient.GetAsync($"{route}{ArgsStrting}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Ошибка запроса {route} в git StatusCode: {StatusCode}, Message: {Message}", route, response?.StatusCode, response?.Content.ReadAsStringAsync());
                    return Result<List<T>>.Error(2, $"Ошибка запроса {route} в git StatusCode: {response?.StatusCode}, Message: {response?.Content.ReadAsStringAsync()}");
                }

                await AddFromJsonToList(response, entities);

                while (argsHasPages && response.Headers.TryGetValues("X-Next-Page", out var nextPage) && !string.IsNullOrWhiteSpace(nextPage.FirstOrDefault()))
                {
                    // Т.к. в аргументах указаны страницы. А из ответа мы считываем пагинацию, то меняем номер страницы для следующего запроса с каждой итерацией.
                    args["page"] = nextPage.First();

                    // Обработка пагинации
                    response = await _gitClient.GetAsync($"{route}{await CombinePathArgs(args)}");
                    await AddFromJsonToList(response, entities);
                }

                return Result<List<T>>.Success(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: Ошибка запроса {route} в git StatusCode: {StatusCode}", StaticExtensions.GetCurrentMethodName(), route, response?.StatusCode);
                return Result<List<T>>.Error(1, $"Ошибка запроса {route} в git");
            }
        }
        /// <summary>
        /// Сборка аргументов запроса из словаря
        /// </summary>
        /// <param name="args">Аргументы</param>
        /// <returns>Срока в urlencoded формате</returns>
        private static async Task<string> CombinePathArgs(Dictionary<string, string> args)
        {
            string result = string.Empty;
            string argsPrefix = "?";

            using (var content = new FormUrlEncodedContent(args))
            {
                result = await content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(result))
                {
                    result = argsPrefix + result;
                }
            }

            return result;
        }
        /// <summary>
        /// Заполнение списка юзеров на основании ифнформации из ответа
        /// </summary>
        /// <param name="response">Ответ</param>
        /// <param name="users">Список пользователей</param>
        /// <returns>Дополненый список пользователей</returns>
        private async Task<List<T>> AddFromJsonToList<T>(HttpResponseMessage response, List<T> users)
        {
            try
            {
                T[]? entityArray = await JsonSerializer.DeserializeAsync<T[]?>(await response.Content.ReadAsStreamAsync());

                if (entityArray is not null)
                    users.AddRange(entityArray);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Ошибка десериализации git json.");
            }

            return users;
        }
    }
}
