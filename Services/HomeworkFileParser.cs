using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using retns.api.Data.Models;
using System.Linq;
using System.Collections.Generic;
using retns.api.Data;
using System.IO;
using System.Globalization;
using retns.api.Services.Extensions;
using Microsoft.Extensions.Logging;

namespace retns.api.Services {
    public class HomeworkFileParser {
        private readonly AzureHelper _fileUploader;
        private readonly HomeworkService _service;
        private readonly ILogger<HomeworkFileParser> _logger;

        public HomeworkFileParser(AzureHelper fileUploader, HomeworkService service, ILogger<HomeworkFileParser> logger) {
            this._logger = logger;
            this._fileUploader = fileUploader;
            this._service = service;
        }
        public async Task<bool> ProcessFromUrl(string url, string sourceFilename) {

            var extension = new FileInfo(sourceFilename).Extension;
            using (var wc = new System.Net.WebClient()) {

                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + $".{extension}";
                await wc.DownloadFileTaskAsync(new Uri(url), fileName);
                if (System.IO.File.Exists(fileName)) {

                    if (extension.Equals(".doc")) {
                        fileName = await DocHelpers.ConvertToDocx(fileName);
                    }

                    if (System.IO.File.Exists(fileName)) {
                        var id = DateTime.Now.GetNextWeekday(DayOfWeek.Monday).ToString("ddMMyy");
                        await _createHtmlFile(id, fileName);
                        var week = await _processFile(id, fileName);
                        if (week != null) {
                            Console.WriteLine("Sending to cosmos");
                            await _service.Create(week);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private async Task<byte[]> _downloadFile(string url) {
            using (var client = new HttpClient()) {
                using (var result = await client.GetAsync(url)) {
                    if (result.IsSuccessStatusCode) {
                        return await result.Content.ReadAsByteArrayAsync();
                    }
                }
            }
            return null;
        }
        private static DateTime _parseOrdinalDateTime(string dt) {
            string[] expectedFormats = new[]
            {
                    "d'st' MMMM yyyy",
                    "d'nd' MMMM yyyy",
                    "d'rd' MMMM yyyy",
                    "d'th' MMMM yyyy"
                };

            try {
                return DateTime.ParseExact(dt, expectedFormats, null, DateTimeStyles.None);
            } catch (Exception e) {
                throw new InvalidOperationException("Not a valid DateTime string", e);
            }
        }
        private void _processTable(Table node, ref HomeworkWeek week) {
            //parse the subjects
            week.Subjects = node.Descendants<TableRow>()
                .First()
                .Skip(2)
                .Select(r => r.InnerText)
                .ToList();
            short dayIndex = 0;
            foreach (var dow in Enum.GetValues(typeof(DayOfWeek))
                .OfType<DayOfWeek>()
                .ToList()
                .Skip(1).Take(4)) {

                var entries = node.Descendants<TableRow>()
                    .Skip(dayIndex + 1)
                    .First()
                    .Skip(2)
                    .Select(r => r.InnerText)
                    .ToArray();
                var days = new Dictionary<string, string>();
                short entryIndex = 0;
                foreach (var entry in entries) {
                    try {
                        var subject = week.Subjects[entryIndex++];
                        days.Add(subject, entry);
                    } catch (Exception ex) {
                        Console.Error.WriteLine(ex.Message);
                    }
                }
                week.Days.Add(dow, days);
                dayIndex++;
            }
        }
        private Task<HomeworkWeek> _processFile(string id, string fileName) {
            return Task.Run(() => {
                var fi = new FileInfo(fileName);

                var sb = new StringBuilder();
                using (var doc = WordprocessingDocument.Open(fileName, false)) {
                    var week = new HomeworkWeek(id);
                    week.WeekCommencing = DateTimeExtensions.GetNextWeekday(DateTime.Now, DayOfWeek.Monday);
                    var notes = new StringBuilder();

                    var paragraphs = doc.MainDocumentPart.Document.Body.Elements().OfType<Paragraph>();
                    foreach (var paragraph in paragraphs) {
                        try {
                            //check if we got the date
                            var date = _parseOrdinalDateTime(paragraph.InnerText);
                            week.WeekCommencing = date;
                            continue;
                        } catch (Exception /*ex*/) {
                            // _logger.LogError(ex.Message);
                        }
                        notes.Append(paragraph.InnerText);
                        notes.Append(Environment.NewLine);
                        notes.Append(Environment.NewLine);
                    }

                    week.Notes = notes.ToString();
                    var table = doc.MainDocumentPart.Document.Body.Elements().OfType<Table>().FirstOrDefault();
                    if (table == null) return null;

                    _processTable(table, ref week);
                    return week;
                }
            });
        }

        private async Task<bool> _createHtmlFile(string id, string fileName) {
            var outputFile = id + ".html";
            var tempFile = Path.Combine(Path.GetTempPath(), outputFile);
            var htmlFile = await DocHelpers.ConvertToHtml(fileName, tempFile);
            if (File.Exists(htmlFile)) {
                await _fileUploader.UploadFile(htmlFile, "html", outputFile, "text/html");
                return true;
            }
            return false;
        }
    }
}