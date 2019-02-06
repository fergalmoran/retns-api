using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using retns.api.Data.Settings;
using retns.api.Services.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HtmlAgilityPack;
using System.Linq;

namespace retns.homework.api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class GetHomeworkController : ControllerBase {
        private readonly StorageSettings _storageSettings;
        private readonly HttpClient _httpClient;

        public GetHomeworkController(IOptions<StorageSettings> storageSettings,
        IHttpClientFactory httpClientFactory) {
            this._storageSettings = storageSettings.Value;
            this._httpClient = httpClientFactory.CreateClient("AzureClient");
        }
        [HttpGet]
        public async Task<IActionResult> GetCurrentWeek() {
            var fileName =
                DateTime.Now.GetThisMonday(DayOfWeek.Monday)
                    .ToString("ddMMyy") +
                    ".html";

            using (var wc = new System.Net.WebClient()) {
                var result = await _httpClient.GetAsync($"html/{fileName}");
                if (result.IsSuccessStatusCode) {
                    Stream stream = await result.Content.ReadAsStreamAsync();

                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(stream);
                    var html = doc.DocumentNode.SelectSingleNode("//body");
                    html.Descendants("div")
                        .Where(n => n.Attributes["title"] != null && n.Attributes["title"].Value == "header")
                        .FirstOrDefault().RemoveAll();

                    foreach (var node in html.SelectNodes("//table")) {
                        node.Attributes.Append("class", "table table-striped");
                    }
                    return Ok(html.InnerHtml);
                }
            }
            return NotFound();
        }
    }
}
