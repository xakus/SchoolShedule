using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolService.DTOs;
using SchoolService.Helpers;
using SchoolService.Models;
using SchoolService.Services;

namespace SchoolService.Controllers
{
    [ApiController]
    [Route("subject")]
    public class SubjectController : ControllerBase
    {
        private readonly SubjectService _service;
        public SubjectController(SubjectService service)
        {
            _service = service;
        }

        // POST: subject/add
        [HttpPost("add")]
        public async Task<IActionResult> AddSubject([FromBody] CreateSubjectRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var id = await _service.AddSubjectWithDtoAsync(request, user.Id);
            return Ok(new { id });
        }

        // GET: subject/get_all?schoolId=1
        [HttpGet("get_all")]
        public async Task<IActionResult> GetAll([FromQuery] int schoolId)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var subjects = await _service.GetSubjectsBySchoolDtoAsync(schoolId, user.Id);
            return Ok(subjects);
        }

        [HttpPut("edit_name")]
        public async Task<IActionResult> EditName([FromBody] EditSubjectNameRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var subjects = await _service.EditNameSubjectAsync(request, user.Id);
            return Ok(subjects);
        }

        // DELETE: subject/delete?id=1
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var result = await _service.DeleteSubjectAsync(id,user.Id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
