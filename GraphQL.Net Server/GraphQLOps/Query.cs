using MongoDB.Driver;

public class Query
{
    // Fetch all employees
    public async Task<List<Employees>> GetEmployees([Service] IMongoDatabase database)
    {
        var collection = database.GetCollection<Employees>("Employees");
        return await collection.Find(_ => true).ToListAsync();
    }

    // Fetch one employee by Id
    public async Task<Employees?> GetEmployeeById(string id, [Service] IMongoDatabase database)
    {
        var collection = database.GetCollection<Employees>("Employees");
        return await collection.Find(e => e.Id == id).FirstOrDefaultAsync();
    }
}
