using System;

namespace Caching.Exceptions
{
public class SettingsValidationExeption : BaseException
{
    public SettingsValidationExeption(string message)
        : base(message)
    {
    }

    public SettingsValidationExeption(string message, Exception exception)
        : base(message, exception)
    {
    }
}
}
