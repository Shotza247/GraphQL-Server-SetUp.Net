using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class SkillProficiency
{
    [BsonElement("skillname")]
    public string? SkillName { get; set; }
    [BsonElement("sfia_level")]
    [GraphQLName("sfia_Level")]
    public int? sfia_Level { get; set; }
    [BsonElement("skills")]
    public Skill? Skills { get; set; }

}   