using System.Collections.Generic;

namespace E_Commerce_BackendAPI.Dtos
{
    public class CartResponseDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItemCount { get; set; }
        public decimal CartTotal { get; set; }
    }
}

