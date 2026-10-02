using System.ComponentModel.DataAnnotations;

namespace WebApiPrac4.DTOs;

public class TeamInputDto
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}
