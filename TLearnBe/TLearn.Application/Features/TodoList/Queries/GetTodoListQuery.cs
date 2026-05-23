using MediatR;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Domain.Entities;

namespace TLearn.Application.Features.TodoList.Queries;

public class GetTodoListQuery
    : IRequest<Result<PagedResult<TodoListDto>>>
{
    public TodoQueryParams Params { get; set; } = new();
}