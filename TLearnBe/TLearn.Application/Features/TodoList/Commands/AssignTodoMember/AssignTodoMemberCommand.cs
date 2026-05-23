using MediatR;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.TodoList.Commands.AssignTodoMember;

public record AssignTodoMemberCommand
    : IRequest<Result<TodoItemDto>>
{
    public Guid TodoId { get; init; }
    

    public List<Guid> UserIds { get; init; } = [];
}