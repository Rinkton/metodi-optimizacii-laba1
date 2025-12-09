using System;

namespace Test
{
    public class SpecialGaussTest
    {
        [Test]
        public void GetHandledMatr()
        {
            SimplexTableContentFormer simplex = new SimplexTableContentFormer();
            Fraction[,] matr =
            {
                {
                    new Fraction(1, 1),
                    new Fraction(2, 1),
                    new Fraction(3, 1),
                    new Fraction(2, 1),
                    new Fraction(0, 1)
                },
                {
                    new Fraction(4, 1),
                    new Fraction(5, 1),
                    new Fraction(6, 1),
                    new Fraction(3, 1),
                    new Fraction(2, 1)
                },
                {
                    new Fraction(7, 1),
                    new Fraction(6, 1),
                    new Fraction(4, 1),
                    new Fraction(1, 1),
                    new Fraction(2, 1)
                },
            };
            int[] basis = { 0, 1, 3 };
            Fraction[,] expectedMatr =
            {
                {
                    new Fraction(1, 1),
                    new Fraction(0, 1),
                    new Fraction(-5, 1),
                    new Fraction(0, 1),
                    new Fraction(-12, 1)
                },
                {
                    new Fraction(0, 1),
                    new Fraction(1, 1),
                    new Fraction(7, 1),
                    new Fraction(0, 1),
                    new Fraction(16, 1)
                },
                {
                    new Fraction(0, 1),
                    new Fraction(0, 1),
                    new Fraction(-3, 1),
                    new Fraction(1, 1),
                    new Fraction(-10, 1)
                },
            };
            Assert.That(SpecialGauss.GetHandledMatrix(matr, basis),
                Is.EqualTo(expectedMatr));
        }
    }
}
