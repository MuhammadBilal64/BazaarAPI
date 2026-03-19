namespace E_Commerce_BackendAPI.Exceptions
{
    /// <summary>Throw this to have the global exception middleware return 403.</summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}

