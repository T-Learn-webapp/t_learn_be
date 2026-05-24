using MediatR;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.TodoList.Queries.GetTodoDetails;

public class GetTodoDetailQuery
    : IRequest<Result<TodoDetailDto>>
{
    public Guid TodoItemId { get; set; }

    public GetTodoDetailQuery(Guid todoItemId)
    {
        TodoItemId = todoItemId;
    }
}