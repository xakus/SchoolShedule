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
        Task<List<Schedule>>GetScheduleByClass(int classId);
        Task<List<Schedule>> GetScheduleByTeacher(int teacherId);
        Task<byte[]> GetSchedulePdfByClass(int classId);
        Task<byte[]> GetSchedulePdfByTeacher(int teacherId);
        Task<List<Schedule>> GenerateSchedule(); // Генерация расписания с проверкой коллизий
        Task<List<Schedule>>RegenerateSchedule(); // Перегенерация расписания
    }
}
