using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class Role
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("title")]
    public string? Title { get; set; }
    [BsonElement("skillrequired")]
    public List<SkillRequired>? SkillRequired { get; set; } = new List<SkillRequired>();

}
