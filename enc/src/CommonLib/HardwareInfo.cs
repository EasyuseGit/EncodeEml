using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

namespace EU2012.Common
{
    public class HardwareInfo
    {

        private string _xmlfilename = "";
        public HardwareInfo(string filename)
        {
            _xmlfilename = filename;

        }

        public void Flush(string valuesin)
        {

            System.IO.Directory.CreateDirectory(BHConfig.BhPath + "\\ftproot\\LoadTrace");
            string loadxml = BHConfig.BhPath + "\\ftproot\\LoadTrace\\" + _xmlfilename + ".dat";
            System.Collections.Hashtable hs = new System.Collections.Hashtable();
            if (System.IO.File.Exists(loadxml))
            {
                using (StreamReader sr = new StreamReader(loadxml))
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    hs = (System.Collections.Hashtable)bf.Deserialize(sr.BaseStream);
                }
            }
            else
            {

            }

            string nowkey = System.DateTime.Now.ToString("yyyy/MM/dd HH:") + (System.DateTime.Now.Minute).ToString().PadLeft(2, '0');
            if (hs.ContainsKey(nowkey))
            {
                hs[nowkey] = valuesin;
            }
            else
            {
                hs.Add(nowkey, valuesin);
            }

            object[] keys = new object[hs.Keys.Count];
            hs.Keys.CopyTo(keys, 0);
            foreach (object key in keys)
            {
                DateTime dtkey = DateTime.Parse(key.ToString());
                if (dtkey < System.DateTime.Now.AddMinutes(-60 * 24))
                    hs.Remove(key);
            }

            using (StreamWriter sw = new StreamWriter(loadxml, false))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(sw.BaseStream, hs);
            }
        }

        public void FlushDesc(string valuesin)
        {
            System.IO.Directory.CreateDirectory(BHConfig.BhPath + "\\ftproot\\LoadTrace");
            string loadxml = BHConfig.BhPath + "\\ftproot\\LoadTrace\\" + _xmlfilename + ".dat";

            using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(loadxml, false, System.Text.Encoding.UTF8))
            {
                streamWriter.WriteLine(valuesin);
                streamWriter.Flush();
            }
        }
    }

    public static class PerformanceInfo
    {
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        public static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }

        }

        public static Int64 GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }

        }
    }
}