using Microsoft.EntityFrameworkCore;

namespace SGH.Infrastructure.Persistence;

public sealed class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options)
    {
    }
}
