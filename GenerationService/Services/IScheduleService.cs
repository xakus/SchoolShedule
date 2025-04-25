using System;
using System.Collections.Generic;
using GenerationService.Models;

namespace GenerationService.Services
{
    /// <summary>
    /// Интерфейс сервиса расписания
    /// </summary>
    public interface IScheduleService
    {
        List<Schedule> GetScheduleByClass(int classId);
        List<Schedule> GetScheduleByTeacher(int teacherId);
        byte[] GetSchedulePdfByClass(int classId);
        byte[] GetSchedulePdfByTeacher(int teacherId);
        List<Schedule> GenerateSchedule(); // Генерация расписания с проверкой коллизий
        List<Schedule> RegenerateSchedule(); // Перегенерация расписания
    }
}
