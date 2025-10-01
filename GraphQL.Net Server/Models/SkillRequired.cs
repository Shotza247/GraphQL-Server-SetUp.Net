using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class SkillRequired
{
    [BsonElement("skill")]
    public Skill? Skill { get; set; }
    [BsonElement("sfia_level")]
    [GraphQLName("sfia_Level")]
    public int? sfia_Level { get; set; }
}