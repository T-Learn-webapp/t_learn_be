using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.TodoList.Commands.DeleteTodoItem;

public class DeleteTodoCommand : IRequest<Result<bool>>

{
    public Guid TodoId { get; set; }

    public DeleteTodoCommand(Guid todoId)

    {
        TodoId = todoId;
    }
}