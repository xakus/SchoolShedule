using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolService.DTOs;
using SchoolService.Helpers;
using SchoolService.Services;

namespace SchoolService.Controllers
{
    [ApiController]
    [Route("school")]
    public class SchoolController : ControllerBase
    {
        private readonly SchoolSer _service;
        public SchoolController(SchoolSer service)
        {
            _service = service;
        }

        // POST: school/add
        [HttpPost("add")]
        public async Task<IActionResult> AddSchool([FromBody] CreateSchoolRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var school = await _service.AddSchoolWithDtoAsync(request,user);
            return Ok(school);
        }

        [HttpPut("edit_name")]
        public async Task<IActionResult> EditSchoolName([FromBody] EditSchoolName request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var school = await _service.EditSchoolNameAsync(request, user.Id);
            return Ok(school);
        }

        [HttpPut("edit_lessons_day")]
        public async Task<IActionResult> EditSchoolMaxLessosnsDay([FromBody] EditSchoolMaxLessonsDay request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var school = await _service.EditSchoolMaxLessosnsDayAsync(request, user.Id);
            return Ok(school);
        }

        // GET: school/get_all?userId=1
        [HttpGet("get_all")]
        public async Task<IActionResult> GetAll()
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var schools = await _service.GetSchoolsByUserDtoAsync(user.Id);
            return Ok(schools);
        }

        // GET: school/get?id=1&userId=1
        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] int id)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var school = await _service.GetSchoolDtoAsync(id, user.Id);
            if (school == null) return NotFound();
            return Ok(school);
        }

        // DELETE: school/delete?id=1&userId=1
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var result = await _service.DeleteSchoolAsync(id, user.Id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
