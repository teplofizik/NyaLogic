using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NyaLogic.Base
{
    public class Filter
    {
        public readonly string Name;

        public Filter(string Name)
        {
            this.Name = Name;
        }

        protected virtual byte[] Process(byte[] data)
        {
            return data;
        }

        public void Apply(Sequence sequence)
        {
            sequence.Update(Process(sequence.Data));
        }
    }
}
