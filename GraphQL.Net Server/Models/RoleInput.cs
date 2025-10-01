public class RoleInput
{
    public string? Id { get; set; } = string.Empty;
    public string? Title { get; set; } = string.Empty;
    public List<SkillRequiredInput>? SkillRequired { get; set; } = new();
}