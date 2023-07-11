using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace EU2012.Common
{

    public class Tools
    {

        public static string ReplaceBhDataSource(string inTemplate)
        {
            MatchCollection mc = Regex.Matches(inTemplate, @"<billhuntersource><url>(?<surl>.+)</url><charset>(?<scharset>.+)<\/charset><\/billhuntersource>");
            foreach (Match ExamReq in mc)
            {
                string url_value = ExamReq.Groups["surl"].Value.Trim();
                string charset_value = ExamReq.Groups["scharset"].Value.Trim();

                string oldsource = ExamReq.ToString();
                string newdata = EU2012.Common.HttpUtil.Gethttp(url_value, System.Text.Encoding.GetEncoding(charset_value));

                inTemplate = inTemplate.Replace(oldsource, newdata);
            }
            return inTemplate;
        }

        public static string ReplaceBhColumn(string inStr, DataRow dr)
        {
            MatchCollection mc = Regex.Matches(inStr, @"<bhcolumn>(?<selfcolumn>[^<]+|.+)<\/bhcolumn>");
            foreach (Match mReq in mc)
            {
                string self_value = mReq.Groups["selfcolumn"].Value.Trim();
                try
                {
                    inStr = inStr.Replace(@"<bhcolumn>" + self_value + "</bhcolumn>", dr[self_value].ToString());
                }
                catch { }
            }

            mc = Regex.Matches(inStr, @"&lt;bhcolumn&gt;(?<selfcolumn>[^(&)]+|.+)&lt;\/bhcolumn&gt;");
            foreach (Match mReq in mc)
            {
                string self_value = mReq.Groups["selfcolumn"].Value.Trim();
                try
                {
                    inStr = inStr.Replace(@"&lt;bhcolumn&gt;" + self_value + "&lt;/bhcolumn&gt;", dr[self_value].ToString());
                }
                catch { }
            }
            return inStr;
        }

        public static string ReplaceBhColumnWithUrlEncode(string inStr, DataRow dr)
        {
            MatchCollection mc = Regex.Matches(inStr, @"<bhcolumn>(?<selfcolumn>[^<]+|.+)<\/bhcolumn>");
            foreach (Match mReq in mc)
            {
                string self_value = mReq.Groups["selfcolumn"].Value.Trim();
                try
                {
                    inStr = inStr.Replace(@"<bhcolumn>" + self_value + "</bhcolumn>", System.Web.HttpUtility.UrlEncode(dr[self_value].ToString(), System.Text.Encoding.UTF8));
                }
                catch { }
            }

            mc = Regex.Matches(inStr, @"&lt;bhcolumn&gt;(?<selfcolumn>[^(&)]+|.+)&lt;\/bhcolumn&gt;");
            foreach (Match mReq in mc)
            {
                string self_value = mReq.Groups["selfcolumn"].Value.Trim();
                try
                {
                    inStr = inStr.Replace(@"&lt;bhcolumn&gt;" + self_value + "&lt;/bhcolumn&gt;", System.Web.HttpUtility.UrlEncode(dr[self_value].ToString(), System.Text.Encoding.UTF8));
                }
                catch { }
            }
            return inStr;
        }

        public static string GetSplitTableId(DateTime queryDate, string categoryCode)
        {
            return queryDate.Month.ToString().PadLeft(2, '0');
        }

        public static int ResultTableCount = 12;
        public static int ExamIdCount = 5;
        public static string GetSplitExam(int exam_id)
        {
            return ((exam_id % 5) + 1).ToString();
        }

        public Tools()
        {

        }

        public static string GetSystemInfo()
        {

            string Info = "";

            ManagementObjectCollection myMOC;
            try
            {

                myMOC =
                    (new ManagementObjectSearcher(new SelectQuery("SELECT Name,  LoadPercentage  FROM Win32_Processor "))).Get();
                foreach (ManagementObject myMO in myMOC)
                    Info = Info + "CPU " + myMO.Properties["Name"].Value.ToString() + " Loading = " + myMO.Properties["LoadPercentage"].Value.ToString() + " % \n";

                myMOC = (new ManagementObjectSearcher(new SelectQuery("SELECT FreePhysicalMemory,FreeSpaceInPagingFiles,FreeVirtualMemory FROM Win32_OperatingSystem "))).Get();
                foreach (ManagementObject myMO in myMOC)
                {
                    Info = Info + "FreePhysicalMemory =  " + myMO.Properties["FreePhysicalMemory"].Value.ToString() + " KByte\n";
                    Info = Info + "FreeSpaceInPagingFiles =  " + myMO.Properties["FreeSpaceInPagingFiles"].Value.ToString() + " KByte\n";
                    Info = Info + "FreeVirtualMemory =  " + myMO.Properties["FreeVirtualMemory"].Value.ToString() + " KByte\n";

                }

                myMOC = (new ManagementObjectSearcher(new SelectQuery("SELECT FreeSpace, deviceID  FROM Win32_LogicalDisk "))).Get();
                foreach (ManagementObject myMO in myMOC)
                    Info = Info + "Drive " + myMO.Properties["deviceID"].Value.ToString() + " has " + myMO.Properties["FreeSpace"].Value.ToString() + " bytes free.\n";

            }
            catch (Exception rr)
            {
                Info = Info + "\n Error Get System info.";
            }
            return Info;
        }

        public static void DirectoryCopy(String src, String dest)
        {

            DirectoryInfo di = new DirectoryInfo(src);

            foreach (FileSystemInfo fsi in di.GetFileSystemInfos())
            {

                String destName = Path.Combine(dest, fsi.Name);

                if (fsi is FileInfo)

                    File.Copy(fsi.FullName, destName);

                else
                {

                    Directory.CreateDirectory(destName);

                    DirectoryCopy(fsi.FullName, destName);

                }

            }

        }

        public static List<string> doCsv(string myString)
        {
            bool inQuotes = false;
            char delim = ',';
            List<string> strings = new List<string>();
            StringBuilder sb = new StringBuilder();
            char startQuotes = 'x';
            foreach (char c in myString)
            {
                if (c == '\'' || c == '"')
                {
                    if (sb.Length == 0)
                        startQuotes = c;
                    if (startQuotes == c)
                    {
                        if (!inQuotes)
                            inQuotes = true;
                        else
                            inQuotes = false;
                    }
                }
                if (c == delim)
                {
                    if (!inQuotes)
                    {
                        strings.Add(doQuotes(sb));
                        sb.Remove(0, sb.Length);
                        startQuotes = 'x';
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            strings.Add(doQuotes(sb));
            return strings;
        }
        static string doQuotes(StringBuilder sb)
        {
            if (sb.Length <= 1)
                return sb.ToString();

            if (sb[0] == '"' && sb[sb.Length - 1] == '"')
                return sb.Remove(sb.Length - 1, 1).Remove(0, 1).Replace("\"\"", "\"").ToString();
            else if (sb[0] == (char)39 && sb[sb.Length - 1] == (char)39)
                return sb.Remove(sb.Length - 1, 1).Remove(0, 1).Replace("''", "'").ToString();
            else
                return sb.ToString();
        }

        public static string doCsvOutputQuotes(string strIn)
        {
            if ((strIn.IndexOf("\"") >= 0) || (strIn.IndexOf(",") >= 0))
                return "\"" + strIn.Replace("\"", "\"\"") + "\"";
            else
                return strIn;
        }

    }
}