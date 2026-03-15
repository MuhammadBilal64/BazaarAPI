namespace E_Commerce_BackendAPI.Dtos
{
    /// <summary>Request for mock payment. Set SimulateSuccess to true to mark order as Paid, false to simulate failure.</summary>
    public class SimulatePaymentRequest
    {
        /// <summary>When true, order is marked as Paid. When false, simulates payment failure.</summary>
        public bool SimulateSuccess { get; set; } = true;
    }
}
