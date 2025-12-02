namespace Test
{
    public class SimplexTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FormSimplexTable()
        {
            Simplex simplex = new Simplex();
            Fraction[] target = new Fraction[]
            {
                new Fraction(-2, 1),
                new Fraction(-1, 1),
                new Fraction(-3, 1),
                new Fraction(-1, 1),
                new Fraction(0, 1),
            };
            Fraction[,] constraints = new Fraction[,]
            {
                {
                    new Fraction(1, 1),
                    new Fraction(2, 1),
                    new Fraction(5, 1),
                    new Fraction(-1, 1),
                    new Fraction(4, 1),
                },
                {
                    new Fraction(1, 1),
                    new Fraction(-1, 1),
                    new Fraction(-1, 1),
                    new Fraction(2, 1),
                    new Fraction(1, 1),
                },
            };
            Fraction[] x0 =
            {
                new Fraction(0, 1),
                new Fraction(0, 1),
                new Fraction(1, 1),
                new Fraction(1, 1),
            };
            Fraction[,] actual = simplex.FormSimplexTable(target, constraints, x0);
            Fraction[,] expected =
            {
                {
                    new Fraction(1, 3),
                    new Fraction(1, 3),
                    new Fraction(1, 1),
                },
                {
                    new Fraction(2, 3),
                    new Fraction(-1, 3),
                    new Fraction(1, 1),
                },
                {
                    new Fraction(-1, 3),
                    new Fraction(-1, 3),
                    new Fraction(4, 1),
                },
            };
            Assert.That(actual,
                Is.EqualTo(expected));
        }

        [Test]
        public void getNonlinearConstraints()
        {
            Simplex simplex = new Simplex();
            Fraction[,] constraints =
            {
                { 
                    new Fraction(1, 1),
                    new Fraction(2, 1),
                },
                {
                    new Fraction(2, 1),
                    new Fraction(4, 1),
                },
            };
            Fraction[,] expectedConstraints =
            {
                {
                    new Fraction(2, 1),
                    new Fraction(4, 1),
                },
            };
            Assert.That(simplex.getNonlinearConstraints(constraints), 
                Is.EqualTo(expectedConstraints));
        }

        [Test]
        public void getLastSimplexTableRow()
        {
            Simplex simplex = new Simplex();
            Fraction[] target = new Fraction[]
            {
                new Fraction(1, 1),
                new Fraction(2, 1),
                new Fraction(3, 1),
                new Fraction(-4, 1),
                new Fraction(0, 1)
            };
            Fraction[,] matr =
            {
                {
                    new Fraction(2, 1),
                    new Fraction(2, 1),
                    new Fraction(-5, 1)
                },
                {
                    new Fraction(2, 1),
                    new Fraction(-2, 1),
                    new Fraction(1, 1)
                },
            };
            Fraction[] actual = simplex.getLastSimplexTableRow(target, matr, new int[] { 2, 3 });
            Fraction[] expected = new Fraction[] {
                new Fraction(-1, 1),
                new Fraction(16, 1),
                new Fraction(19, 1),
            };
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}