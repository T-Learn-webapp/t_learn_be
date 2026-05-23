using MediatR;
using Microsoft.EntityFrameworkCore;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.TodoList.Queries;

public class GetTodoListHandler
    : IRequestHandler<GetTodoListQuery, Result<PagedResult<TodoListDto>>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTodoListHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<TodoListDto>>> Handle(
        GetTodoListQuery request,
        CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;

        var query = _context.TodoItems
            .AsNoTracking()
            .Include(x => x.LearningMaterial)
                .ThenInclude(x => x.Subject)
            .Include(x => x.Assignments)
            .AsQueryable();

        // ===== Filter Subject =====

       
            query = query.Where(x =>
                x.LearningMaterial.SubjectId == request.Params.SubjectId);
        // ===== Filter Material =====
            query = query.Where(x =>
                x.LearningMaterialId == request.Params.LearningMaterialId);
        // ===== Filter Mine =====

        if (request.Params.Filter == TodoFilterType.Mine)
        {
            query = query.Where(x =>
                x.Assignments.Any(a => a.UserId == currentUserId));
        }

        // ===== Filter Status =====

        if (request.Params.Status.HasValue)
        {
            query = query.Where(x =>
                x.Assignments.Any(a =>
                    a.Status == request.Params.Status));
        }

        // ===== Search =====

        if (!string.IsNullOrWhiteSpace(request.Params.Search))
        {
            var keyword = request.Params.Search.Trim();

            query = query.Where(x =>
                x.Title.Contains(keyword) ||
                (x.Description != null &&
                 x.Description.Contains(keyword)));
        }

        // ===== Sorting =====

        query = request.Params.SortBy?.ToLower() switch
        {
            "title" => request.Params.IsDescending
                ? query.OrderByDescending(x => x.Title)
                : query.OrderBy(x => x.Title),

            "duedate" => request.Params.IsDescending
                ? query.OrderByDescending(x => x.DueDate)
                : query.OrderBy(x => x.DueDate),

            _ => request.Params.IsDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt)
        };

        // ===== Total Count =====

        var totalCount = await query.CountAsync(ct);

        // ===== Pagination =====

        var items = await query
            .Skip((request.Params.PageNumber - 1)
                  * request.Params.PageSize)
            .Take(request.Params.PageSize)
            .Select(x => new TodoListDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                DueDate = x.DueDate,
                LearningMaterialId = x.LearningMaterialId,
                LearningMaterialTitle = x.LearningMaterial.Title,
                SubjectId = x.LearningMaterial.Subject.Id,
                SubjectName = x.LearningMaterial.Subject.Name,
                CreatedByUserId = x.CreatedByUserId,
                CreatedAt = x.CreatedAt,

                // status của current user
                Status = x.Assignments
                    .Where(a => a.UserId == currentUserId)
                    .Select(a => a.Status)
                    .FirstOrDefault(),

                AssignedUsers = x.Assignments
                    .Select(a => new TodoAssignedUserDto
                    {
                        UserId = a.UserId,
                        Status = a.Status
                    })
                    .ToList()
            })
            .ToListAsync(ct);

        var result = new PagedResult<TodoListDto>(
            items,
            totalCount,
            request.Params.PageNumber,
            request.Params.PageSize);

        return Result<PagedResult<TodoListDto>>.Success(result);
                
            
    }
}