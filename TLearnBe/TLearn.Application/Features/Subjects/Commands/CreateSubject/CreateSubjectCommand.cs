using MediatR;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Commands.CreateSubject;

public class CreateSubjectCommand : IRequest<Result<SubjectDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
   
    public Guid UserId { get; set; }
}