using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NyaLogic.Base
{
    public class Sequence
    { 
        /// <summary>
        /// Sequence name
        /// </summary>
        public string Name { protected set; get; }

        public byte[] Data { protected set; get; }

        public Sequence(string Name, byte[] Data)
        {
            this.Name = Name;
            this.Data = Data;
        }

        public void Update(byte[] bytes)
        {
            if (Data.Length != bytes.Length)
                throw new InvalidDataException("Sequence update is possible with data of same size");

            Data = bytes;
        }
    }
}
