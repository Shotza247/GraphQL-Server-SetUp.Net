public class AddEmployeeInput
{
    public string? Name { get; set; }
    public Role? CurrentRole { get; set; }
    public List<SkillProficiency>? Skills { get; set; } = new();
    public Role? DesiredRole { get; set; }
    public bool? IsMentor { get; set; }
}
