using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identify;

public class IdentityDesignTimeDbContextFactory: IDesignTimeDbContextFactory<IdDbContext>
{
    public IdDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<IdDbContext> builder = new();
        string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:Default");
        builder.UseSqlServer(connStr);
        return new IdDbContext(builder.Options);
    }
}