using System.Security.Cryptography;
using System.Text;
using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Model;  
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IdempotencyService> _logger;

        public IdempotencyService(AppDbContext context, ILogger<IdempotencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IdempotencyRecord?> GetExistingRecordAsync(string idempotencyKey)
        {
            return await _context.IdempotencyRecords
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey);
        }

        public async Task<bool> TryCreateRecordAsync(IdempotencyRecord record)
        {
            try
            {
                var exists = await _context.IdempotencyRecords
                    .AnyAsync(r => r.IdempotencyKey == record.IdempotencyKey);

                if (exists)
                    return false;

                await _context.IdempotencyRecords.AddAsync(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Concurrency issue while creating idempotency record for key {Key}", record.IdempotencyKey);
                return false;
            }
        }

        public async Task UpdateRecordAsync(IdempotencyRecord record)
        {
            _context.IdempotencyRecords.Update(record);
            await _context.SaveChangesAsync();
        }

        public string ComputeRequestHash(string requestBody)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(requestBody);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public async Task CleanupExpiredRecordsAsync()
        {
            var expired = DateTime.UtcNow;
            var deleted = await _context.IdempotencyRecords
                .Where(r => r.ExpiresAt < expired)
                .ExecuteDeleteAsync();

            if (deleted > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired idempotency records", deleted);
            }
        }
    }
    }