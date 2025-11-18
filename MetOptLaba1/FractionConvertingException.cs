using System;

namespace MetOptLaba1
{
    public class FractionConvertingException : Exception 
    {
        public readonly string value;

        public FractionConvertingException(string value)
        : base($"{value} can't be converted to Fraction") {
            this.value = value;
        }
    }
}
