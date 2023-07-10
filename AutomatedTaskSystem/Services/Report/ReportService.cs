using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Report;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.ReportService;

public class ReportService : IReportService
{
    private readonly DataContext _context;

    public ReportService(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<ResponseService<List<GetReportDto>>>> GetAllProjectsReports(
        DateTime? start,
        DateTime? end
    )
    {
        var projects = await _context.Projects
            .Where(p => !p.Archived)
            .Include(p => p.Year)
            .Include(p => p.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .ToListAsync();

        var res = new List<GetReportDto> { };

        if (start is not null && end is not null && (start == end || start > end))
            return new BadRequestObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = "Start date cannot be earlier than or equal to End date"
                }
            );

        foreach (var project in projects)
            res.Add(createReport(project, start, end));

        return new ResponseService<List<GetReportDto>>
        {
            Data = res,
            Error = false,
            Message = "List of all project reports"
        };
    }

    public async Task<ActionResult<ResponseService<GetProjectReportDto>>> GetProjectReport(int id)
    {
        var project = await _context.Projects
            .Where(p => p.Id == id && !p.Archived)
            .Include(p => p.Year)
            .Include(p => p.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives)
            .FirstOrDefaultAsync();

        if (project is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Project is not found" }
            );

        var res = new GetProjectReportDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Term = project.Term ? "Term 2" : "Term 1",
            Year = project.Year.Number
        };

        foreach (var unit in project.Units)
        {
            if (unit.Archived)
                continue;
            var unitRes = new GetUnitDto { Name = unit.Name, Id = unit.Id };
            res.Units.Add(unitRes);
            foreach (var lesson in unit.Lessons)
            {
                if (lesson.Archived)
                    continue;
                var lessonRes = new GetLessonDto { Name = lesson.Name, Id = lesson.Id };
                unitRes.Lessons.Add(lessonRes);
                foreach (var learningObjective in lesson.LearningObjectives)
                    if (!learningObjective.Archived)
                    {
                        if (learningObjective.DoneAt is not null)
                            lessonRes.DoneLearningObjectives++;
                        else if (learningObjective.StartedAt is not null)
                            lessonRes.RunningLearningObjectives++;
                        else
                            lessonRes.IdleLearningObjectives++;
                        lessonRes.LearningObjectives.Add(
                            new GetLearningObjectiveDto
                            {
                                Id = learningObjective.Id,
                                Name = learningObjective.Name,
                                Done = learningObjective.DoneAt,
                                Started = learningObjective.StartedAt
                            }
                        );
                    }
                unitRes.RunningLearningObjectives += lessonRes.RunningLearningObjectives;
                unitRes.DoneLearningObjectives += lessonRes.DoneLearningObjectives;
                unitRes.IdleLearningObjectives += lessonRes.IdleLearningObjectives;
            }
            res.RunningLearningObjectives += unitRes.RunningLearningObjectives;
            res.DoneLearningObjectives += unitRes.DoneLearningObjectives;
            res.IdleLearningObjectives += unitRes.IdleLearningObjectives;
        }

        return new ResponseService<GetProjectReportDto>
        {
            Message = "Report for project",
            Error = false,
            Data = res
        };
    }

    private GetReportDto createReport(Project project, DateTime? start, DateTime? end)
    {
        var report = new GetReportDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Year = project.Year.Number,
            Term = project.Term ? "Term 2" : "Term 1",
        };

        foreach (var unit in project.Units)
        {
            if (unit.Archived)
                continue;

            foreach (var lesson in unit.Lessons)
            {
                if (lesson.Archived)
                    continue;

                foreach (var lo in lesson.LearningObjectives)
                {
                    if (lo.Archived)
                        continue;
                    report.TotalLearningObjectives++;

                    if (start is null && end is null)
                        handleNoDate(report, lo);
                    else if (start is null && end is not null)
                        handleEndDate(report, lo, (DateTime)end);
                    else if (start is not null && end is null)
                        handleStartDate(report, lo, (DateTime)start);
                    else
                        handleStartAndEndDate(report, lo, (DateTime)start!, (DateTime)end!);
                }
            }
        }

        return report;
    }

    private void handleNoDate(GetReportDto report, LearningObjective lo)
    {
        if (lo.StartedAt is null)
            report.IdleLearningObjectives++;
        else if (lo.DoneAt is null)
            report.RunningLearningObjectives++;
        else
            report.DoneLearningObjectives++;
    }

    private void handleStartAndEndDate(
        GetReportDto report,
        LearningObjective lo,
        DateTime start,
        DateTime end
    )
    {
        if (lo.CreateAt < start)
            return;

        handleEndDate(report, lo, end);
    }

    private void handleStartDate(GetReportDto report, LearningObjective lo, DateTime start)
    {
        if (lo.CreateAt < start)
            return;

        handleNoDate(report, lo);
    }

    private void handleEndDate(GetReportDto report, LearningObjective lo, DateTime end)
    {
        if (lo.CreateAt >= end)
            return;

        if (lo.StartedAt is not null && lo.DoneAt is not null && lo.DoneAt < end)
            report.DoneLearningObjectives++;
        else if (lo.StartedAt is null || lo.StartedAt < end)
            report.RunningLearningObjectives++;
        else
            report.IdleLearningObjectives++;
    }
}
