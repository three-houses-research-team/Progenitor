using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progenitor.IO.Common
{
    public static class BitFlags
    {
        public static bool GetFlag(byte inbyte, int bitindex)
        {
            bitindex &= 7;
            return (inbyte >> bitindex & 1) != 0;
        }

        public static byte SetFlag(byte inbyte, int bitindex, bool value)
        {
            bitindex &= 7;
            inbyte &= (byte)~(1 << bitindex);
            inbyte |= (byte)((value ? 1 : 0) << bitindex);
            return inbyte;
        }
    }
}
