using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Biblioteca.Infrastructure.Services
{
    public class RefreshTokenService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(string userEmail)
        {
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddDays(7); // 7 días de validez
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserEmail = userEmail,
                ExpiryDate = expiry,
                IsRevoked = false
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token && !x.IsRevoked);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}