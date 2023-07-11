using System;
using System.Diagnostics;
using System.Threading;

namespace EU2012.Common {
    public class Trace {
        public Trace() {
           

        }

        static private string LogFilePath = "";
        private static Mutex mut;
        private static DateTime errorDateTime = System.DateTime.Now.AddDays(-1);

        static public void SetLogFile(string File_Path, string ApName) {

            try {
                mut = new Mutex(false, ApName);
                LogFilePath = File_Path + ApName;
            } catch (Exception ee) {
                string none = ee.Source;
            }
        }

        static public void DebugWrite(string Class_Name, string Message) {
            
            
            try {
                mut.WaitOne(5*1000,true);

                SetTrackFile();

                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [" + Class_Name.PadRight(10) + "][DEBUG] " + Message);

                ClearTrack();



            } catch (Exception ee) {
                WriteEventLog(ee);
            } finally {
                try {
                    mut.ReleaseMutex();
                } catch { }
            }



        }

        static public void InfoWrite(string Class_Name, string Message) {
            
            mut.WaitOne(5 * 1000, true);
            try {
                SetTrackFile();

                System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [" + Class_Name.PadRight(10) + "][ INFO] " + Message);

                ClearTrack();
            } catch (Exception ee) {
                WriteEventLog(ee); 
            } finally {
                try {
                    mut.ReleaseMutex();
                } catch { }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Class_Name"></param>
        /// <param name="Message"></param>
        static public void WarnWrite(string Class_Name, string Message) {

            mut.WaitOne(5 * 1000, true);
            try {
                SetTrackFile();
                string strError = DateTime.Now.ToString("HH:mm:ss.fff") + " [" + Class_Name.PadRight(10) + "][ WARN] " + Message;
                System.Diagnostics.Trace.WriteLine(strError);

                ClearTrack();

            } catch (Exception ee) {
                WriteEventLog(ee);
            } finally {
                try {
                    mut.ReleaseMutex();
                } catch { }
            }
        }

        static public void ErrorWrite(string Class_Name, string Message) {
            string strError = "";
            mut.WaitOne(5*1000,true);
            try {
                strError = DateTime.Now.ToString("HH:mm:ss.fff") +
                           " [" + Class_Name.PadRight(10) + "][ERROR] " +
                           Message +
                           "\n" + EU2012.Common.Tools.GetSystemInfo();

                SetTrackFile();

                System.Diagnostics.Trace.WriteLine(strError);
                strError = strError.Replace("\n", "<br>");
                strError += "<br>來源應用程式:" + AppDomain.CurrentDomain.FriendlyName;
                ClearTrack();
            } catch (Exception ee) {
                WriteEventLog(ee);
            } finally {
                try {
                    mut.ReleaseMutex();
                } catch { }
            }

        }


        static public void WriteEventLog(Exception ee) {
            try {
                if (errorDateTime.AddMinutes(30) < System.DateTime.Now) {
                    errorDateTime = System.DateTime.Now;
                    EventLog.WriteEntry("Billhunter", "來源應用程式:" + AppDomain.CurrentDomain.FriendlyName + " " + ee.ToString(), EventLogEntryType.Error);
                }
            } catch{}
        }
        /// <summary>
        /// 
        /// </summary>
        static public void SetTrackFile() {
            try {
                string path = LogFilePath + "." + DateTime.Now.ToString("yyyy-MM-dd") + ".log";

                System.IO.StreamWriter write = new System.IO.StreamWriter(path, true, System.Text.Encoding.GetEncoding("big5"));
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(write, "log"));

                System.Diagnostics.Trace.AutoFlush = true;
                System.Diagnostics.Debug.AutoFlush = true;

            } catch (Exception ee) {
                string none = ee.Source;
            }
        }

        static public void ClearTrack() {
            try {
                System.Diagnostics.Trace.Listeners["log"].Flush();
                System.Diagnostics.Trace.Listeners["log"].Close();
                System.Diagnostics.Trace.Listeners.Remove("log");
            } catch (Exception ee) {
                string none = ee.Source;
            }
        }
    }
}
