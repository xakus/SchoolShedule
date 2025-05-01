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
        public async Task<IActionResult> GetScheduleByClass(int classId)
        {
            var schedule = await _scheduleService.GetScheduleByClass(classId);
            return Ok(schedule);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetScheduleByTeacher(int teacherId)
        {
            var schedule = await _scheduleService.GetScheduleByTeacher(teacherId);
            return Ok(schedule);
        }

        [HttpGet("class/{classId}/pdf")]
        public async Task<IActionResult> GetSchedulePdfByClass(int classId)
        {
            var pdf = await _scheduleService.GetSchedulePdfByClass(classId);
            return File(pdf, "application/pdf", $"schedule_class_{classId}.pdf");
        }

        [HttpGet("teacher/{teacherId}/pdf")]
        public async Task<IActionResult> GetSchedulePdfByTeacher(int teacherId)
        {
            var pdf = await _scheduleService.GetSchedulePdfByTeacher(teacherId);
            return File(pdf, "application/pdf", $"schedule_teacher_{teacherId}.pdf");
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSchedule()
        {
            try
            {
                var schedule = await _scheduleService.GenerateSchedule();
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
        public async Task<IActionResult> RegenerateSchedule()
        {
            try
            {
                var schedule = await _scheduleService.RegenerateSchedule();
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
