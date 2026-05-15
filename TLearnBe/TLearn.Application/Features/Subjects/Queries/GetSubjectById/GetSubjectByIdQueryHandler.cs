using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Queries.GetSubjectById;

public class GetSubjectByIdQueryHandler : IRequestHandler<GetSubjectByIdQuery, Result<SubjectDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<GetSubjectByIdQueryHandler> _logger;

    public GetSubjectByIdQueryHandler(TLearnDbContext context, ILogger<GetSubjectByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<SubjectDto>> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var subject = await _context.Subjects
                .Include(s => s.Materials)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (subject == null)
                return Result<SubjectDto>.Failure($"Subject with id '{request.Id}' was not found.");

            // Check permission: user can view if it's public or they own it
            if (!subject.IsPublic && subject.UserId != request.UserId)
                return Result<SubjectDto>.Failure("You don't have permission to view this subject.");

            return Result<SubjectDto>.Success(new SubjectDto
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description,
                Color = subject.Color,
                Icon = subject.Icon,
                IsPublic = subject.IsPublic,
                MaterialCount = subject.Materials.Count,
                CreatedAt = subject.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting subject {SubjectId}", request.Id);
            return Result<SubjectDto>.Failure("An error occurred while retrieving the subject.");
        }
    }
}