using Microsoft.AspNetCore.Mvc;
using SchoolService.DTOs;
using SchoolService.Helpers;
using SchoolService.Services;

namespace SchoolService.Controllers
{
    [ApiController]
    [Route("class")]
    public class ClassController : ControllerBase
    {
        private readonly ClassService _service;
        public ClassController(ClassService service)
        {
            _service = service;
        }

        // POST: class/add
        [HttpPost("add")]
        public async Task<IActionResult> AddClass([FromBody] CreateClassRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var klass = await _service.AddClassWithDtoAsync(request, user.Id);
            return Ok(klass);
        }

        // PUT: class/edit_name
        [HttpPut("edit_name")]
        public async Task<IActionResult> EditNameClass([FromBody] EditNameClassRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var klass = await _service.EditClassNameWithDtoAsync(request, user.Id);
            return Ok(klass);
        }

        // PUT: class/edit_day_week
        [HttpPut("edit_day_week")]
        public async Task<IActionResult> EditDayWeekClass([FromBody] EditDayWeekClassRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var klass = await _service.EditClassDayWeekWithDtoAsync(request, user.Id);
            return Ok(klass);
        }

        // GET: class/get_all?schoolId=1
        [HttpGet("get_all")]
        public async Task<IActionResult> GetAll([FromQuery] int schoolId)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var classes = await _service.GetClassesBySchoolDtoAsync(schoolId, user.Id);
            return Ok(classes);
        }

        // GET: class/get_all_class_subjects?classId=1
        [HttpGet("get_all_class_subjects")]
        public async Task<IActionResult> GetAllClassSubjects([FromQuery] int classId)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var classes = await _service.GetClassesSubjectsByClassIdDtoAsync(classId, user.Id);
            return Ok(classes);
        }

        // GET: class/get?id=1
        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] int id)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var c = await _service.GetClassDtoAsync(id, user.Id);
            if (c == null) return NotFound();
            return Ok(c);
        }

        // DELETE: class/delete?id=1
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            var result = await _service.DeleteClassAsync(id, user.Id);
            if (!result) return NotFound();
            return Ok();
        }

        // POST: class/add_subject
        [HttpPost("add_subject")]
        public async Task<IActionResult> AddSubject([FromBody] ClassSubjectRequest request)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            await _service.AddSubjectToClassAsync(request.ClassId, request.SubjectId, request.HoursPerWeek, user.Id);
            return Ok();
        }

        // DELETE: class/delete_subject
        [HttpDelete("delete_subject")]
        public async Task<IActionResult> DeleteSubject([FromQuery] int classLessosnId)
        {
            var user = UserContextHelper.GetCurrentUser(HttpContext);
            if (user == null) return Unauthorized();
            await _service.RemoveSubjectFromClassAsync(classLessosnId, user.Id);
            return Ok();
        }

    }
}
