namespace Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
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
    }
}