using System;

namespace MetOptLaba1
{
    public class UserException: Exception
    {
        public UserException(string message)
        : base(message) { }
    }
}
