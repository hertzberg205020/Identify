using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JwtRevoke.Identity;

public class IdentityDesignTimeDbContextFactory: IDesignTimeDbContextFactory<IdDbContext>
{
    public IdDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<IdDbContext> builder = new();
        string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:Demo3");
        builder.UseSqlServer(connStr);
        return new IdDbContext(builder.Options);
    }
}