using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Commands.DeleteSubject;

public class DeleteSubjectCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}