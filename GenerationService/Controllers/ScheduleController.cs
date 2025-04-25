using Microsoft.AspNetCore.Mvc;
using GenerationService.Models;
using GenerationService.Data;
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

        [HttpGet("class/{classId}")]
        public IActionResult GetScheduleByClass(int classId)
        {
            var schedule = _scheduleService.GetScheduleByClass(classId);
            return Ok(schedule);
        }

        [HttpGet("teacher/{teacherId}")]
        public IActionResult GetScheduleByTeacher(int teacherId)
        {
            var schedule = _scheduleService.GetScheduleByTeacher(teacherId);
            return Ok(schedule);
        }

        [HttpGet("class/{classId}/pdf")]
        public IActionResult GetSchedulePdfByClass(int classId)
        {
            var pdf = _scheduleService.GetSchedulePdfByClass(classId);
            return File(pdf, "application/pdf", $"schedule_class_{classId}.pdf");
        }

        [HttpGet("teacher/{teacherId}/pdf")]
        public IActionResult GetSchedulePdfByTeacher(int teacherId)
        {
            var pdf = _scheduleService.GetSchedulePdfByTeacher(teacherId);
            return File(pdf, "application/pdf", $"schedule_teacher_{teacherId}.pdf");
        }

        [HttpPost("generate")]
        public IActionResult GenerateSchedule()
        {
            try
            {
                var schedule = _scheduleService.GenerateSchedule();
                if (schedule == null || !schedule.Any())
                    return NotFound("Нет данных для генерации расписания.");
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("regenerate")]
        public IActionResult RegenerateSchedule()
        {
            try
            {
                var schedule = _scheduleService.RegenerateSchedule();
                if (schedule == null || !schedule.Any())
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
