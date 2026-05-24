using TLearn.Domain.Entities;

namespace TLearn.Application.Features.Subjects.DTOs;

public class SubjectDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    

    public Guid OwnerId { get; set; }

    public string OwnerName { get; set; } = string.Empty;
    
    public string OwnerEmail { get; set; } = string.Empty;
    
    public int MaterialCount { get; set; }

    public DateTime CreatedAt { get; set; }

    // Thêm để FE phân biệt
    public bool IsOwner { get; set; }

    public bool IsMember { get; set; }

    public SubjectPermission? MyPermission { get; set; }

    public string Role { get; set; } = string.Empty;
}