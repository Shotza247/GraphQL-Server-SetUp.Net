using MongoDB.Bson;
using MongoDB.Driver;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Data;


var builder = WebApplication.CreateBuilder(args);
const string connectionUri = "mongodb+srv://jabulanindlovu360_db_user:xVcjEhfrZ3Rf2Kye@sbsa-test.wmi9dqy.mongodb.net/?retryWrites=true&w=majority&appName=SBSA-Test";

// Setup MongoDB client
var settings = MongoClientSettings.FromConnectionString(connectionUri);
settings.ServerApi = new ServerApi(ServerApiVersion.V1);

var client = new MongoClient(settings);

try
{
    var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
    Console.WriteLine("Successfully connected to MongoDB!");
}
catch (Exception ex)
{
    Console.WriteLine($"MongoDB connection failed: {ex}");
}

// Register MongoClient + Database in DI
builder.Services.AddSingleton<IMongoClient>(client);
builder.Services.AddSingleton(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase("SBSA-Test"); // 
});

//registering services for graphql
builder.Services.AddGraphQLServer().AddQueryType<Query>().AddFiltering().AddSorting();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL();
app.Run();
