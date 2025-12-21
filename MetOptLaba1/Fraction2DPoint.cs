using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetOptLaba1
{
    /// <summary>
    /// Точка на двумерной плоскости, которая использует Fraction в качестве своих
    /// координат. 
    /// По сути нужна сугубо для удобства
    /// </summary>
    public class Fraction2DPoint
    {
        public Fraction X {  get; private set; }
        public Fraction Y { get; private set; }

        public Fraction2DPoint(Fraction x, Fraction y)
        {
            X = x;
            Y = y;
        }
    }
}
