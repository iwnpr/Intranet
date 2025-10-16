using System.Text.RegularExpressions;
using Application_lib.Gitlab;
using Microsoft.Extensions.Configuration;

namespace Infrastructure_lib.Gitlab
{
    public partial class TextFormatterService(IConfiguration config) : ITextFormatterService
    {
        private readonly IConfiguration _config = config;

        public string FormatTextToHtml(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Сохраняем оригинальные переносы для обработки списков
            var lines = text.Split("<br>");
            var processedLines = new List<string>();

            var inUlList = false;
            var inOlList = false;

            foreach (var line in lines)
            {
                // Обработка чекбоксов
                if (GetCheckBox().IsMatch(line))
                {
                    var match = GetCheckBox().Match(line);
                    var isChecked = match.Groups[1].Value == "x";
                    var content = match.Groups[2].Value.Trim();
                    processedLines.Add($"<input type=\"checkbox\" {(isChecked ? "checked" : "")} disabled /> {content}<br>");
                    continue;
                }

                // Обработка маркированных списков
                if (GetUnorderedList().IsMatch(line))
                {
                    if (!inUlList)
                    {
                        processedLines.Add("<ul>");
                        inUlList = true;
                    }

                    var match = GetUnorderedList().Match(line);
                    var content = match.Groups[1].Value.Trim();
                    processedLines.Add($"<li>{ProcessLineContent(content.Trim('\n'))}</li>");
                    continue;
                }

                // Обработка нумерованных списков
                if (GetOrderedList().IsMatch(line))
                {
                    if (!inOlList)
                    {
                        processedLines.Add("<ol>");
                        inOlList = true;
                    }

                    var match = GetOrderedList().Match(line);
                    var content = match.Groups[1].Value.Trim();
                    processedLines.Add($"<li>{ProcessLineContent(content.Trim('\n'))}</li>");
                    continue;
                }

                // Закрываем списки если они активны
                if (inUlList || inOlList)
                {
                    if (inUlList)
                    {
                        processedLines.Add("</ul>");
                        inUlList = false;
                    }
                    if (inOlList)
                    {
                        processedLines.Add("</ol>");
                        inOlList = false;
                    }
                }

                // Обработка обычного текста
                processedLines.Add(ProcessLineContent(line));
            }

            // Закрываем списки в конце
            if (inUlList) processedLines.Add("</ul>");
            if (inOlList) processedLines.Add("</ol>");

            return string.Join("", processedLines);
        }

        private string ProcessLineContent(string content)
        {
            // Обработка ссылок в содержимом строки
            return GetHref().Replace(content, match =>
                $"<a href='{_config.GetValue<string>("GitClientSettings:BaseAddress")?[..27]}{match.Groups[2].Value}' class='text-primary'>{match.Groups[1].Value}</a>");
        }

        [GeneratedRegex(@"\[([^\]]+)\]\(([^)]+)\)")]
        public static partial Regex GetHref();

        [GeneratedRegex(@"- \[(x| )\] (.+)")]
        public static partial Regex GetCheckBox();

        [GeneratedRegex(@"- (.+)")]
        public static partial Regex GetUnorderedList();

        [GeneratedRegex(@"\d+\. (.+)")]
        public static partial Regex GetOrderedList();
    }
}
