using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Employees
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonElement("name")]
    public string Name { get; set; }
    [BsonElement("currentrole")]
    public Role CurrentRole { get; set; }
    [BsonElement("skills")]
    public List<SkillProficiency> Skills { get; set; } = new List<SkillProficiency>();
    [BsonElement("desiredrole")]
    public Role DesiredRole { get; set; }
    [BsonElement("ismentor")]
    public bool IsMentor { get; set; }
}
