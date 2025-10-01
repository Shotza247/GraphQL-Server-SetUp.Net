using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class Skill
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("name")]
    public string? Name { get; set; }
    [BsonElement("sfia_code")]
    public string? SFIA_Code { get; set; }
}