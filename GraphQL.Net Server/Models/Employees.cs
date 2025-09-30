public class Employees
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Role CurrentRole { get; set; }
    public List<SkillProficiency> Skills { get; set; } = new List<SkillProficiency>();
    public Role DesiredRole { get; set; }
    public bool IsMentor { get; set; }
}
