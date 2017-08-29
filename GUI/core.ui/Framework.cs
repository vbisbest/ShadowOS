using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.ui
{
    public class IndexLengthPairData
    {
        public IndexLengthPairData()
        {
        }

        public IndexLengthPairData(int index, int length)
        {
            Index = index;
            Length = length;
        }

        public int Index { get; set; }

        public int Length { get; set; }
    }
}
