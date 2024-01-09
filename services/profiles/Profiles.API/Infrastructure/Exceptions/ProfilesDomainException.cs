using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Infrastructure.Exceptions
{

    /// <summary>
    /// Exception type for app exceptions
    /// </summary>
    public class ProfilesDomainException : Exception
    {
        public ProfilesDomainException()
        { }

        public ProfilesDomainException(string message)
            : base(message)
        { }

        public ProfilesDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
