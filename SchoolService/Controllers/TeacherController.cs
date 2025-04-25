using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolService.DTOs;
using SchoolService.Helpers;
using SchoolService.Models;
using SchoolService.Services;

namespace SchoolService.Controllers
{
    [ApiController]
    [Route("teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly TeacherService _service;
        public TeacherController(TeacherService service)
        {
            _service = service;
        }

        // POST: teacher/add
        [HttpPost("add")]
        public async Task<IActionResult> AddTeacher([FromBody] CreateTeacherRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var teacher = await _service.AddTeacherWithDtoAsync(request, user.Id);
            return Ok(teacher);
        }

        // PUT: teacher/edit
        [HttpPut("edit")]
        public async Task<IActionResult> EditTeacherName([FromBody] EditTeacherNameRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var teacher = await _service.EditTeacherNameWithDtoAsync(request, user.Id);
            return Ok(teacher);
        }

        // GET: teacher/get_all?schoolId=1
        [HttpGet("get_all")]
        public async Task<IActionResult> GetAll([FromQuery] int schoolId)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var teachers = await _service.GetTeachersBySchoolDtoAsync(schoolId, user.Id);
            return Ok(teachers);
        }

        // GET: teacher/get_all?schoolId=1
        [HttpGet("get_all_teacher_subjects")]
        public async Task<IActionResult> GetAllTeacherSubjects([FromQuery] int teacherId)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var teachers = await _service.GetTeacherSubjectsByTheacherIdDtoAsync(teacherId, user.Id);
            return Ok(teachers);
        }

        // GET: teacher/get?id=1
        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] int id)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var teacher = await _service.GetTeacherDtoAsync(id, user.Id);
            if (teacher == null) return NotFound();
            return Ok(teacher);
        }

        // DELETE: teacher/delete?id=1
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var result = await _service.DeleteTeacherAsync(id, user.Id);
            if (!result) return NotFound();
            return Ok();
        }

        // POST: teacher/add_subject
        [HttpPost("add_subject")]
        public async Task<IActionResult> AddSubject([FromBody] TeacherSubjectRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            await _service.AddSubjectToTeacherAsync(request.TeacherId, request.SubjectId, user.Id);
            return Ok();
        }

        // DELETE: teacher/delete_subject
        [HttpDelete("delete_subject")]
        public async Task<IActionResult> DeleteSubject([FromQuery] int teacherSubjectId)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            await _service.RemoveSubjectFromTeacherAsync(teacherSubjectId, user.Id);
            return Ok();
        }

    }
}
