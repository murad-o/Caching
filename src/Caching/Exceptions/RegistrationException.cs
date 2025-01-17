using System;

namespace Caching.Exceptions;

public class RegistrationException : BaseException
{
    public RegistrationException(string errorMessage)
        : this(errorMessage, null, null)
    {
    }

    public RegistrationException(string errorMessage, string informationMessage)
        : this(errorMessage, informationMessage, null)
    {
    }

    public RegistrationException(string errorMessage, Exception innerException)
        : base(errorMessage, null, innerException)
    {
    }

    public RegistrationException(string errorMessage, string informationMessage, Exception innerException)
        : base(errorMessage, informationMessage, innerException)
    {
    }
}
