using System.Threading.Tasks;
using retns.api.Services;
using retns.api.Services.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace retns.homework.api.Controllers {
    [Route("api/[controller]")]
    [ApiController]

    public class DocUploadController : ControllerBase {
        private readonly ILogger<DocUploadController> _logger;
        private readonly HomeworkFileParser _parser;

        public DocUploadController(ILogger<DocUploadController> logger, HomeworkFileParser parser) {
            this._logger = logger;
            this._parser = parser;
        }
        // POST api/values
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult> Post([FromForm] string url, [FromForm] string filename) {
            _logger.LogDebug($"New event received\n\tFile url: {url}\n\tFile name: {filename}");
            if (!filename.EndsWith("doc") && !filename.EndsWith("docx")) {
                filename = $"{filename.TrimEnd('.')}.doc";
            }
            if (!string.IsNullOrEmpty(url)) {
                if (await _parser.ProcessFromUrl(url, filename)) {
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
