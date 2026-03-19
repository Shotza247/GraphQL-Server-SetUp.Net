using MongoDB.Bson;
using MongoDB.Driver;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Data;


var builder = WebApplication.CreateBuilder(args);
const var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"]
    ?? throw new InvalidOperationException(
        "MongoDB:ConnectionString is not configured. Use User Secrets or environment variables.");
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"]
    ?? "Client_CollectionDB";
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

// Register MongoClient + Database
builder.Services.AddSingleton<IMongoClient>(client);
builder.Services.AddSingleton(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase(mongoDatabaseName); // 
});

//registering services for graphql
builder.Services.AddGraphQLServer()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment())
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddFiltering()
    .AddSorting();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL();
app.Run();
