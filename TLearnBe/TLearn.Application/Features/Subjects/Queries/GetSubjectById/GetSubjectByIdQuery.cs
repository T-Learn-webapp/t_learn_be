using MediatR;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Queries.GetSubjectById;

public class GetSubjectByIdQuery : IRequest<Result<SubjectDto>>
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
}