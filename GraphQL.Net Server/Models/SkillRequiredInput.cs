public class SkillRequiredInput
{
    public SkillInput? Skill { get; set; } = default!;
    [GraphQLName("sfia_Level")]
    public int? sfia_Level { get; set; }
}