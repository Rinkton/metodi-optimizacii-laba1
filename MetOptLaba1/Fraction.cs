using System;
using System.ComponentModel.Design.Serialization;

namespace MetOptLaba1
{
    public struct Fraction
    {
        public long Numerator;
        public long Denominator;

        public Fraction(long numerator, long denominator)
        {
            if(denominator == 0) {
                throw new ArgumentException("Делитель не может быть 0", nameof(denominator));
            }
            Numerator = numerator;
            Denominator = denominator;
            simplify();
        }

        /* TODO: Возможно стоит сделать типа упрощение дроби после каждого изменения
         * В сеттерах сверху */
        public static Fraction operator +(Fraction a, Fraction b)
        {
            long newNumerator = (a.Numerator * b.Denominator) + (b.Numerator * a.Denominator);
            long newDenominator = a.Denominator * b.Denominator;
            Fraction fraction = new Fraction(newNumerator, newDenominator);
            fraction.simplify();
            return fraction;
        }

        public static Fraction operator -(Fraction a, Fraction b)
        {
            long newNumerator = (a.Numerator * b.Denominator) - (b.Numerator * a.Denominator);
            long newDenominator = a.Denominator * b.Denominator;
            Fraction fraction = new Fraction(newNumerator, newDenominator);
            fraction.simplify();
            return fraction;
        }

        public static Fraction operator *(Fraction a, Fraction b)
        {
            long newNumerator = a.Numerator * b.Numerator;
            long newDenominator = a.Denominator * b.Denominator;
            Fraction fraction = new Fraction(newNumerator, newDenominator);
            fraction.simplify();
            return fraction;
        }

        public static Fraction operator /(Fraction a, Fraction b)
        {
            long newNumerator = a.Numerator * b.Denominator;
            long newDenominator = a.Denominator * b.Numerator;
            Fraction fraction = new Fraction(newNumerator, newDenominator);
            fraction.simplify();
            return fraction;
        }

        public static bool operator ==(Fraction a, Fraction b)
        {
            return a.Numerator == b.Numerator && a.Denominator == b.Denominator;
        }

        public static bool operator !=(Fraction a, Fraction b)
        {
            return !(a == b);
        }

        public static bool operator <(Fraction a, Fraction b)
        {
            return (a - b).Numerator < 0;
        }

        public static bool operator >(Fraction a, Fraction b)
        {
            return (a - b).Numerator > 0;
        }

        public static bool operator <=(Fraction a, Fraction b)
        {
            return a < b || a == b;
        }

        public static bool operator >=(Fraction a, Fraction b)
        {
            return a > b || a == b;
        }

        private static long GetGreatestCommonDivisor(long a, long b)
        {
            while(b != 0) {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static Fraction FromString(string s)
        {
            // Классическая дробь
            if (s.Contains("/")) {
                string[] split = s.Split('/');
                if (split.Length == 2) {
                    try {
                        long l1 = long.Parse(split[0]);
                        long l2 = long.Parse(split[1]);
                        return new Fraction(l1, l2);
                    }
                    catch (FormatException) {
                        throw new FractionConvertingException(s);
                    }
                }
                else {
                    throw new FractionConvertingException(s);
                }
            }
            // Десятичная дробь
            else if (s.Contains(".")) {
                string[] split = s.Split(".");
                if (split.Length == 2) {
                    try {
                        string stringBeforeDot = split[0];
                        string stringAfterDot = split[1];
                        string a = stringBeforeDot + stringAfterDot;
                        long numerator = long.Parse(a);
                        long denominator = (long)Math.Pow(10, stringAfterDot.Length);

                        return new Fraction(numerator, denominator);
                    }
                    catch(FormatException) {
                        throw new FractionConvertingException(s);
                    }
                }
                else {
                    throw new FractionConvertingException(s);
                }
            }
            // Целое число
            else {
                try {
                    long l = long.Parse(s);
                    return new Fraction(l, 1);
                }
                catch (FormatException) {
                    throw new FractionConvertingException(s);
                }
            }
        }

        public static Fraction GetZero()
        {
            return new Fraction(0, 1);
        }
        
        public override string ToString()
        {
            if (Denominator != 1) {
                return $"{Numerator}/{Denominator}";
            }
            else {
                return $"{Numerator}";
            }
        }

        public double ToDouble()
        {
            double d = (double)Numerator / Denominator;
            return d;
        }

        private void simplify()
        {
            long gcd = GetGreatestCommonDivisor(Math.Abs(Numerator), Math.Abs(Denominator));
            Numerator = Numerator / gcd;
            Denominator = Denominator / gcd;
            // Не хотим, чтобы знаменатель был отрицательным
            if (Denominator < 0) {
                Numerator *= -1;
                Denominator *= -1;
            }
        }
    }
}
