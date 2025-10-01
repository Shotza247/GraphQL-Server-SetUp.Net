public class SkillProficiencyInput
{
    public string? SkillName { get; set; } = string.Empty;
    [GraphQLName("sfia_Level")]
    public int? Sfia_Level { get; set; }
    
    public SkillInput? Skills { get; set; } = default!;
}