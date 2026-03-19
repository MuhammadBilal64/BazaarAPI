using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Utilities;

namespace E_Commerce_BackendAPI.Services
{
    public interface IOrderService
    {
        Task<PaginationResponseDto<OrderListDto>> GetMyOrdersAsync(
            int userId,
            int page,
            int pageSize,
            string? status,
            DateTime? fromDate,
            DateTime? toDate);

        Task<OrderDetailDto> GetOrderByIdAsync(int id, int userId, bool isAdmin);
        Task<OrderCreateResponseDto> CreateOrderFromCartAsync(int userId);
        Task<OrderStatus> UpdateOrderStatusAsync(int id, string status, int? modifiedBy);
        Task<PaymentSimulationResultDto> SimulatePaymentAsync(int id, int userId, bool isAdmin, bool simulateSuccess);
        Task<PaginationResponseDto<OrderListDto>> GetAllOrdersAsync(
            int page,
            int pageSize,
            string? status,
            DateTime? fromDate,
            DateTime? toDate);
    }
}

