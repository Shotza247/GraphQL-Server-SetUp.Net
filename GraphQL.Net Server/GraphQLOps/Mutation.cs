using MongoDB.Driver;

public class Mutation
{
    public async Task<Employees> AddEmployee(AddEmployeeInput input, [Service] IMongoDatabase database)
    {
        var collection = database.GetCollection<Employees>("Employees");

        var employee = new Employees
        {
            Name = input.Name,

            // Map CurrentRole
            CurrentRole = new Role
            {
                Id = input.CurrentRole.Id,
                Title = input.CurrentRole.Title,
                SkillRequired = input.CurrentRole.SkillRequired.Select(sr => new SkillRequired
                {
                    Skill = new Skill
                    {
                        Id = sr.Skill.Id,
                        Name = sr.Skill.Name,
                        sfia_Code = sr.Skill.sfia_Code
                    },
                    sfia_Level = sr.sfia_Level
                }).ToList()
            },

            // Map Skills
            Skills = input.Skills.Select(s => new SkillProficiency
            {
                SkillName = s.SkillName,
                sfia_Level = s.sfia_Level,
                Skills = new Skill
                {
                    Id = s.Skills.Id,
                    Name = s.Skills.Name,
                    sfia_Code = s.Skills.sfia_Code
                }
            }).ToList(),

            // Map DesiredRole
            DesiredRole = new Role
            {
                Id = input.DesiredRole.Id,
                Title = input.DesiredRole.Title,
                SkillRequired = input.DesiredRole.SkillRequired.Select(sr => new SkillRequired
                {
                    Skill = new Skill
                    {
                        Id = sr.Skill.Id,
                        Name = sr.Skill.Name,
                        sfia_Code = sr.Skill.sfia_Code
                    },
                    sfia_Level = sr.sfia_Level
                }).ToList()
            },

            IsMentor = input.IsMentor
        };

        await collection.InsertOneAsync(employee);

        return employee;
    }
}
