using System.ComponentModel.DataAnnotations;

namespace AutomatedTaskSystem.Models.YearModel;

public class Year
{
    public int Id { get; set; }

    [MinLength(1)]
    public string Number { get; set; } = "";
    public bool Active { get; set; } = true;
}
