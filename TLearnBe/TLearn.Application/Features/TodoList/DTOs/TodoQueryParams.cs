using TLearn.Common.Pagination;
using TLearn.Domain.Entities;

namespace TLearn.Application.Features.TodoList.DTOs;

public class TodoQueryParams : PaginationParams

{

    public Guid SubjectId { get; set; }

    public Guid LearningMaterialId { get; set; }

    public TodoStatus? Status { get; set; }

    public TodoFilterType Filter { get; set; } = TodoFilterType.All;

    public string? Search { get; set; }

}