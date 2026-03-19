public class AddEmployeeInput
{
    public string? Name { get; set; }
    
    public RoleInput? CurrentRole { get; set; }

    public List<SkillProficiencyInput>? Skills { get; set; } = new();

    public RoleInput? DesiredRole { get; set; }

    public bool? IsMentor { get; set; }
}
