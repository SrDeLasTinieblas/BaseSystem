using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Infrastructure.Persistence
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}