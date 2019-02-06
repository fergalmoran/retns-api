using System;
using System.Threading.Tasks;
using retns.api.Data;
using retns.api.Data.Models;
using retns.api.Services.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace retns.homework.api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase {
        private readonly HomeworkService _service;

        public CalendarController(HomeworkService service) {
            this._service = service;
        }

        [HttpGet]
        public async Task<ActionResult<HomeworkWeek>> Get() {
            var key = DateTime.Now.GetThisMonday()
                .ToString("ddMMyy");
            var week = await _service.Get(key);
            return Ok(week);
        }
    }
}
