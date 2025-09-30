public class Role
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<SkillRequired> SkillRequired { get; set; } = new List<SkillRequired>();

}
