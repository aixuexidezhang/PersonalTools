using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Tools.CSharpExtension
{
    public static class ByteExtention
    {

        public static byte[] MergeEventIdAndLength(this byte[] data, ushort eventId, ushort length)
        {
            byte[] buffer = new byte[length + 4];

            buffer[0] = (byte)eventId;
            buffer[1] = (byte)(eventId >> 8);

            buffer[2] = (byte)length;
            buffer[3] = (byte)(length >> 8);

            Buffer.BlockCopy(data, 0, buffer, 4, data.Length);
            return buffer;
        }

        public static string ToFullString(this byte[] data)
        {
            string str = "";
            for (int i = 0; i < data.Length; i += 1)
            {
                str += data[i] + ",";
            }
            str.TrimEnd(',');
            return str;
        }
    }
}
