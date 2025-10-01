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
    [GraphQLName("sfia_Code")]
    public string? sfia_Code { get; set; }
}