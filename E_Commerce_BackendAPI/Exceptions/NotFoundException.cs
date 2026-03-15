namespace E_Commerce_BackendAPI.Exceptions
{
    /// <summary>Throw this to have the global exception middleware return 404.</summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
