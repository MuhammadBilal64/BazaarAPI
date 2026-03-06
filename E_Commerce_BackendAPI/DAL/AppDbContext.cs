using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.DAL
{
    public class AppDbContext:DbContext
    {
    public    AppDbContext(DbContextOptions<AppDbContext>options):base(options)
        {
        }

    }
}
