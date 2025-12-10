using System;

namespace MetOptLaba1
{
    public struct AllowableElementData
    {
        public readonly int row;
        public readonly int column;
        public readonly bool best;

        public AllowableElementData(int row, int column, bool best)
        {
            this.row = row;
            this.column = column;
            this.best = best;
        }
    }
}
