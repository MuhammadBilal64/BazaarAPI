using E_Commerce_BackendAPI.Utilities;

namespace E_Commerce_BackendAPI.Dtos
{
    public class PaymentSimulationResultDto
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

