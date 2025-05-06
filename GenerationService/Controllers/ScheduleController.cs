using Microsoft.AspNetCore.Mvc;
using GenerationService.Services;

namespace GenerationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("class")]
        public Task<IActionResult> GetScheduleByClass([FromQuery] long schoolId, [FromQuery] int classId)
        {
            var schedule =  _scheduleService.GetScheduleByClass(schoolId,classId);
            return Task.FromResult<IActionResult>(Ok(schedule));
        }

        [HttpGet("teacher")]
        public Task<IActionResult> GetScheduleByTeacher([FromQuery] long schoolId, [FromQuery] int teacherId)
        {
            var schedule =  _scheduleService.GetScheduleByTeacher(schoolId,teacherId);
            return Task.FromResult<IActionResult>(Ok(schedule));
        }

        [HttpGet("class/pdf")]
        public Task<IActionResult> GetSchedulePdfByClass([FromQuery] long schoolId,[FromQuery] int classId)
        {
            var pdf =  _scheduleService.GetSchedulePdfByClass(schoolId,classId).Result;
            return Task.FromResult<IActionResult>(File(pdf, "application/pdf", $"schedule_class_{classId}.pdf"));
        }

        [HttpGet("teacher/pdf")]
        public async Task<IActionResult> GetSchedulePdfByTeacher([FromQuery] long schoolId,[FromQuery]  int teacherId)
        {
            var pdf = await _scheduleService.GetSchedulePdfByTeacher(schoolId,teacherId);
            return File(pdf, "application/pdf", $"schedule_teacher_{teacherId}.pdf");
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSchedule([FromQuery] long schoolId)
        {
            try
            {
                var schedule = await _scheduleService.GenerateScheduleResponse(schoolId);
                if (schedule.Count == 0)
                    return NotFound("Нет данных для генерации расписания.");
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("regenerate")]
        public async Task<IActionResult> RegenerateSchedule([FromQuery] long schoolId)
        {
            try
            {
                var schedule = await _scheduleService.RegenerateSchedule(schoolId);
                if (schedule.Count == 0)
                    return NotFound("Нет данных для генерации расписания.");
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
