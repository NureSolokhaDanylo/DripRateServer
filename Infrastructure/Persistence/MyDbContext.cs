using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class MyDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }
}
