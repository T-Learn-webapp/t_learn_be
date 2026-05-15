using MediatR;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Commands.UpdateSubject;

public class UpdateSubjectCommand : IRequest<Result<SubjectDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsPublic { get; set; }
    public Guid UserId { get; set; }
}