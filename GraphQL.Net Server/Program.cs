var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGraphQLServer();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL();
app.Run();
