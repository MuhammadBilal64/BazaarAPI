namespace E_Commerce_BackendAPI.Utilities
{
    public enum UserRole { Admin, Customer }
    public enum OrderStatus { Created, Paid, Shipped, Cancelled }
    public enum PaymentStatus { Pending, Completed, Failed }
    public enum CartStatus { Active, Ordered, Failed }
}
