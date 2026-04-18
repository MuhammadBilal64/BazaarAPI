using E_Commerce_BackendAPI.Model;

namespace E_Commerce_BackendAPI.Services
{
    public interface IIdempotencyService
    {
        Task<IdempotencyRecord?> GetExistingRecordAsync(string idempotencyKey); 
        Task<bool> TryCreateRecordAsync(IdempotencyRecord record);  
        Task UpdateRecordAsync(IdempotencyRecord record);  
        string ComputeRequestHash(string requestBody);
        Task CleanupExpiredRecordsAsync();
    }
}
