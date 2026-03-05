using System;

namespace StoreManagement.Services
{
    // Base domain exception for consistent handling
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ValidationException : DomainException
    {
        public ValidationException(string message) : base(message) { }
    }

    public class InsufficientStockException : DomainException
    {
        public InsufficientStockException(string message) : base(message) { }
    }
}
