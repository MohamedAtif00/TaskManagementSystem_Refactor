using System.ComponentModel.DataAnnotations;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Models;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.Permission
{
    public class CreatePermissionDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public PermissionType Type { get; set; }

        public string? Reason { get; set; } = "";

        [Required]
        public string From { get; set; } = "";

        [Required]
        public string To { get; set; } = "";

        [Required]
        public string PermissionDate { get; set; } = "";
    }

    public class GetPermissionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Reason { get; set; } = "";
        public string FromTime { get; set; } = "";
        public string ToTime { get; set; } = "";
        public string PermissionDate { get; set; } = "";
        public string Duration { get; set; } // Duration in minutes
        public string Status { get; set; } = "";
        public IDName User { get; set; } = new IDName();
        public string DateCreated { get; set; } = "";
    }

    public class GetSinglePermissionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Reason { get; set; } = "";
        public string FromTime { get; set; } = "";
        public string ToTime { get; set; } = "";
        public string PermissionDate { get; set; } = "";
        public string Duration { get; set; } // Duration in minutes
        public string Status { get; set; } = "";
        public string DateCreated { get; set; } = "";
        public UserDTO User { get; set; } = new UserDTO();
        public List<GetOpinion> Opinions { get; set; } = new();
    }

    public class UpdatePermissionDto
    {
        [Required]
        public int Id { get; set; }
        public PermissionType? Type { get; set; }
        public string? Reason { get; set; }
        public string? FromTime { get; set; }
        public string? ToTime { get; set; }
        public string? PermissionDate { get; set; }
    }

    public class ApprovePermissionDto
    {
        [Required]
        public int PermissionId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        public string Comment { get; set; } = "";
    }
}
