using NyaLogic.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NyaLogic.Filters
{
    public class Denoise : Filter
    {
        private int Window;

        public Denoise(int Window) : base("Denoise")
        {
            this.Window = Window;
        }

        private int GetSetBitCount(byte[] bytes)
        {
            int Res = 0;
            for(int i = 0; i < bytes.Length; i++)
            {
                var B = bytes[i];

                for(int j = 0; j < 8; j++)
                {
                    if ((B & (1 << j)) != 0)
                        Res++;
                }
            }

            return Res;
        }

        private byte[] GetFilteredGroup(byte[] bytes)
        {
            int Total = bytes.Length * 8;
            int Set = GetSetBitCount(bytes);

            byte Fill = Convert.ToByte((Set > Total / 2) ? 0xFF : 0x00);
            Array.Fill(bytes, Fill);

            return bytes;
        }

        protected override byte[] Process(byte[] data)
        {
            var Part = new byte[Window];

            for (int i = 0; i < data.Length; i++)
            {
                if((i % Window == 0) && (i > Window))
                {
                    // Обновим участок...
                    Array.Copy(data, i - Window, Part, 0, Window);

                    var F = GetFilteredGroup(Part);
                    Array.Copy(Part, 0, data, i - Window, Window);
                }
            }

            return data;
        }
    }
}
