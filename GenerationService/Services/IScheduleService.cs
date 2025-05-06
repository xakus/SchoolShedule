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
        List<Schedule>GetScheduleByClass(long schoolId,int classId);
        List<Schedule> GetScheduleByTeacher(long schoolId,int teacherId);
        Task<byte[]> GetSchedulePdfByClass(long schoolId, int classId);
        Task<byte[]> GetSchedulePdfByTeacher(long schoolId, int teacherId);
        Task<List<Schedule>> GenerateSchedule(long schoolId); // Генерация расписания с проверкой коллизий
        Task<List<ScheduleResponse>> GenerateScheduleResponse(long schoolId); // Генерация расписания с проверкой коллизий
        Task<List<ScheduleResponse>>RegenerateSchedule(long schoolId); // Перегенерация расписания
    }
}
