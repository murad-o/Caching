using System;

namespace Caching.Exceptions
{
    public abstract class BaseException : Exception
    {
        protected BaseException()
        {
        }

        protected BaseException(string errorMessage)
            : base(errorMessage)
        {
        }

        protected BaseException(string errorMessage, string informationMessage)
            : base(errorMessage)
        {
            InformationMessage = informationMessage;
        }

        protected BaseException(string errorMessage, string informationTitle, string informationMessage)
            : base(errorMessage)
        {
            InformationTitle = informationTitle;
            InformationMessage = informationMessage;
        }

        protected BaseException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }

        protected BaseException(string errorMessage, string informationMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
            InformationMessage = informationMessage;
        }

        protected BaseException(string errorMessage, string informationTitle, string informationMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
            InformationTitle = informationTitle;
            InformationMessage = informationMessage;
        }

        public virtual string InformationTitle { get; }

        public virtual string InformationMessage { get; }
    }
}
