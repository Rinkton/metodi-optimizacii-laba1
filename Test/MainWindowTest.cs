using MetOptLaba1;
using System;
using System.Windows.Controls;

namespace Test
{
    public class MainWindowTest
    {
        [Test]
        public void GetSimplexTable()
        {
            Fraction[,] simplexTableContent =
            {
                {
                    new Fraction(1, 1),
                    new Fraction(1, 1),
                },
                {
                    new Fraction(-2, 3),
                    new Fraction(2, 1),
                },
                {
                    new Fraction(1, 1),
                    new Fraction(-12, 1),
                },
            };
            Fraction[] x0 = new Fraction[] { new Fraction(0, 1), new Fraction(1, 1), new Fraction(2, 1) };
            SimplexTable simplexTable = new SimplexTable(simplexTableContent, x0);
            DataGrid dg = simplexTable.getDataGrid(0);
        }
    }
}
