using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class SkillRequired
{
    [BsonElement("skill")]
    public Skill? Skill { get; set; }
    [BsonElement("sfia_level")]
    public int? SFIA_Level { get; set; }
}