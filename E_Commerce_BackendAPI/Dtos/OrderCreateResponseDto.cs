namespace E_Commerce_BackendAPI.Dtos
{
    public class OrderCreateResponseDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

