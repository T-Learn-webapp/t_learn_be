using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Commands.DeleteSubject;

public class DeleteSubjectCommand : IRequest<Result<bool>>

{
    public Guid SubjectId { get; set; }

    public DeleteSubjectCommand(Guid subjectId)

    {
        SubjectId = subjectId;
    }
}