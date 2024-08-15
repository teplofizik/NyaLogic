using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NyaLogic.Base
{
    public class Measure
    {
        public virtual int SequenceCount => 0;

        public virtual Sequence? GetSequence(int Index) => null;
    }
}
