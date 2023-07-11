/*
   This file is part of the Encoder

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Affero General Public License as
   published by the Free Software Foundation, either version 3 of the
   License, or (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Affero General Public License for more details.

   You should have received a copy of the GNU Affero General Public License
   along with this program. If not, see <http://www.gnu.org/licenses/>

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using EU2012.Common;
using System.Threading;

namespace EncoderLib
{
    public class BufferQueueListen
    {
        private System.Messaging.MessageQueue mqEncoder;
        private static AutoResetEvent[] myEvent = new AutoResetEvent[]{
            new AutoResetEvent(false)
        };
        private static int allcounter = 0;
        private System.Collections.ArrayList stopAr = new System.Collections.ArrayList();
        public BufferQueueListen(int queue_index, bool blnDeleteOkFile)
        {
            startBufferQueueListen(queue_index, blnDeleteOkFile);
        }
        public BufferQueueListen(int queue_index)
        {
            startBufferQueueListen(queue_index, true);
        }
        private void startBufferQueueListen(int queue_index, bool blnDeleteOkFile)
        {
            Trace.SetLogFile(BHConfig.LogPath, "Encoder");
            Trace.DebugWrite("Encoder", "BufferQueueListen Start Service , queue_index = " + queue_index.ToString());
            try
            {

                if (queue_index == 0 && blnDeleteOkFile)
                {
                    EU2012.Common.Trace.DebugWrite("startBufferQueueListen", "進入 ok 檔移除作業");
                    System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(BHConfig.BhPath + "Ftproot\\EncoderBuffer");
                    System.IO.FileInfo[] arrFiles = info.GetFiles("*.ok");
                    for (int i = 0; i < arrFiles.Length; i++)
                    {
                        try
                        {
                            EU2012.Common.Trace.DebugWrite("BufferQueueListen", "啟動(移走)-處理檔案 : " + arrFiles[i].FullName);
                            string targetfolder = BHConfig.BhPath + "Ftproot\\EncoderBuffer\\" + System.DateTime.Today.ToString("yyyyMMdd");
                            System.IO.Directory.CreateDirectory(targetfolder);
                            System.IO.File.Move(arrFiles[i].FullName, targetfolder + "\\" + arrFiles[i].Name);
                        }
                        catch (Exception fe)
                        {
                            EU2012.Common.Trace.DebugWrite("ERROR", "處理(移走)" + fe.ToString());
                        }
                    }
                }
                EUCDO.ForceReleaseMut();

                if (Environment.ProcessorCount > 10)
                {
                    System.Threading.ThreadPool.SetMaxThreads(Environment.ProcessorCount + 2, 500);
                    EU2012.Common.Trace.DebugWrite("Encoder", "設定 MaxThreads = " + (Environment.ProcessorCount + 2).ToString());
                }
                else
                {
                    System.Threading.ThreadPool.SetMaxThreads(10, 500);
                }
                this.mqEncoder = new System.Messaging.MessageQueue();
                this.mqEncoder.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(EU2012.Common.MSMQ.EncoderFilesObj) });
                this.mqEncoder.Path = "FormatName:DIRECT=OS:" + BHConfig.MsmqVirualMachine + "\\private$\\eu_bh2012_encoder_files" + queue_index.ToString();
                this.mqEncoder.MessageReadPropertyFilter.Priority = true;
                this.mqEncoder.ReceiveCompleted += new System.Messaging.ReceiveCompletedEventHandler(mqEncoder_ReceiveCompleted);

                mqEncoder.BeginReceive();
            }
            catch (Exception me)
            {
                EU2012.Common.Trace.ErrorWrite("Encoder", me.ToString());
            }
        }
        public BufferQueueListen()
        {
            Trace.SetLogFile(BHConfig.LogPath, "Encoder");
            Trace.DebugWrite("Encoder", "BufferQueueListen Start Service");
            try
            {

                System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(BHConfig.BhPath + "Ftproot\\EncoderBuffer");
                System.IO.FileInfo[] arrFiles = info.GetFiles("*.ok");
                for (int i = 0; i < arrFiles.Length; i++)
                {
                    try
                    {
                        EU2012.Common.Trace.DebugWrite("BufferQueueListen", "啟動(移走)-處理檔案 : " + arrFiles[i].FullName);
                        string targetfolder = BHConfig.BhPath + "Ftproot\\EncoderBuffer\\" + System.DateTime.Today.ToString("yyyyMMdd");
                        System.IO.Directory.CreateDirectory(targetfolder);
                        System.IO.File.Move(arrFiles[i].FullName, targetfolder + "\\" + arrFiles[i].Name);
                    }
                    catch (Exception fe)
                    {
                        EU2012.Common.Trace.DebugWrite("ERROR", "處理(移走)" + fe.ToString());
                    }
                }

                EUCDO.ForceReleaseMut();

                if (Environment.ProcessorCount > 10)
                {
                    System.Threading.ThreadPool.SetMaxThreads(Environment.ProcessorCount + 2, 500);
                    EU2012.Common.Trace.DebugWrite("Encoder", "設定 MaxThreads = " + (Environment.ProcessorCount + 2).ToString());
                }
                else
                {
                    System.Threading.ThreadPool.SetMaxThreads(10, 500);
                }

                this.mqEncoder = new System.Messaging.MessageQueue();
                this.mqEncoder.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(EU2012.Common.MSMQ.EncoderFilesObj) });
                this.mqEncoder.Path = "FormatName:DIRECT=OS:" + BHConfig.MsmqVirualMachine + "\\private$\\eu_bh2012_encoder_files";
                this.mqEncoder.MessageReadPropertyFilter.Priority = true;
                this.mqEncoder.ReceiveCompleted += new System.Messaging.ReceiveCompletedEventHandler(mqEncoder_ReceiveCompleted);

                mqEncoder.BeginReceive();
            }
            catch (Exception me)
            {
                EU2012.Common.Trace.ErrorWrite("Encoder", me.ToString());
            }
        }

        void mqEncoder_ReceiveCompleted(object sender, System.Messaging.ReceiveCompletedEventArgs e)
        {
            string nowfilename = "";
            try
            {
                System.Messaging.Message m = mqEncoder.EndReceive(e.AsyncResult);
                EU2012.Common.MSMQ.EncoderFilesObj enfobj = (EU2012.Common.MSMQ.EncoderFilesObj)m.Body;
                nowfilename = enfobj.fileName;
                EU2012.Common.Trace.DebugWrite("Encoder", "Queue = " + enfobj.fileName);
                if (enfobj.fileName.EndsWith(".stop"))
                {
                    EU2012.Common.Trace.DebugWrite("Encoder", "拉收到中斷的 Queue = " + enfobj.fileName);
                    if (stopAr.Count > 20)
                        stopAr.RemoveAt(0);
                    stopAr.Add(enfobj.fileName.Replace(".stop", ""));
                    try
                    {
                        System.IO.File.Delete(BHConfig.BhPath + "Ftproot\\EncoderBuffer\\" + enfobj.fileName + ".ok");
                    }
                    catch { };

                    string stopfile = EU2012.Common.BHConfig.BhPath + "eml\\interrupt_" + enfobj.fileName.Replace(".stop", "") + ".stop";
                    using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(stopfile, false, System.Text.Encoding.GetEncoding("big5")))
                    {
                        streamWriter.WriteLine("project_result_id = " + enfobj.fileName.Replace(".stop", "") + " , stop on " + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    }
                }
                else
                {
                    string userfile = BHConfig.BhPath + "Ftproot\\EncoderBuffer\\" + enfobj.fileName + "\\user.xml";
                    string mailbodyfile = BHConfig.BhPath + "Ftproot\\EncoderBuffer\\" + enfobj.fileName + "\\mailbody.xml";
                    DataSet ds = new DataSet();
                    ds.ReadXml(userfile);
                    DataTable dtalluser = ds.Tables[0];

                    DataSet dsa = new DataSet();
                    dsa.ReadXml(mailbodyfile);

                    foreach (DataRow drswf in dsa.Tables["app_bh_project_swf"].Rows)
                    {
                        string tempsourceswf = BHConfig.BhPath + "Ftproot\\EncoderBuffer\\" + enfobj.fileName + "\\" + drswf["swf_real_name"].ToString();
                        string tempdestswf = BHConfig.BhPath + "Swf\\" + drswf["swf_real_name"].ToString();
                        if (!System.IO.File.Exists(tempdestswf))
                        {

                            System.IO.Directory.CreateDirectory(BHConfig.BhPath + "Swf");
                            if (System.IO.File.Exists(tempsourceswf))
                            {
                                System.IO.File.Copy(tempsourceswf, tempdestswf, true);
                            }
                        }
                    }
                    DataSet allds = new DataSet();

                    bool isStop = false;
                    #region ============ 檢查此份  project_result_id 有沒有中斷
                    if (dtalluser.Columns.Contains("project_result_id"))
                    {
                        if (dtalluser.Rows.Count > 0)
                        {
                            EU2012.Common.Trace.DebugWrite("檢查中斷", "目前的 project_result_id = " + dtalluser.Rows[0]["project_result_id"].ToString());
                            for (int i = 0; i < stopAr.Count; i++)
                            {
                                EU2012.Common.Trace.DebugWrite("中斷", "project_result_id = " + stopAr[i].ToString());
                                if (stopAr[i].ToString() == dtalluser.Rows[0]["project_result_id"].ToString())
                                {
                                    isStop = true;
                                }
                            }
                        }
                    }
                    #endregion
                    EU2012.Common.Trace.DebugWrite("mqEncoder", "此批筆數 " + dtalluser.Rows.ToString());
                    foreach (DataRow dr in dtalluser.Rows)
                    {
                        if (isStop)
                        {

                        }
                        else
                        {
                            #region ========= 正常組信 thread
                            allcounter++;
                            EncodeStateObj ecobj = new EncodeStateObj();
                            ecobj.UserDr = dr;
                            ecobj.Mailbody = dsa;
                            ecobj.Allds = allds;

                            System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(EncodeMail.ThreadProc), ecobj);

                            if (allcounter % 20 == 0)
                            {
                                allcounter = 0;
                                EU2012.Common.Trace.DebugWrite("Encoder", "傳入等待");
                                System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(Wakeup), new object());
                                WaitHandle.WaitAny(myEvent);

                                bool IsNeedWait = true;
                                while (IsNeedWait)
                                {
                                    string emlFolder = EU2012.Common.BHConfig.BhPath + "eml\\";
                                    if (System.IO.Directory.GetFiles(emlFolder).Length > 100000)
                                    {
                                        EU2012.Common.Trace.DebugWrite("Encoder", "太多信組出來了 , 先 等等");
                                        System.Threading.Thread.Sleep(1000 * 60);
                                    }
                                    else
                                        IsNeedWait = false;
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception me)
            {
                EU2012.Common.Trace.DebugWrite("Error", me.ToString());
            }
            finally
            {
                if (nowfilename != "")
                {
                    try
                    {
                        System.IO.File.Delete(BHConfig.BhPath + "Ftproot\\EncoderBuffer\\" + nowfilename + ".ok");
                        System.IO.Directory.Delete(BHConfig.BhPath + "Ftproot\\EncoderBuffer\\" + nowfilename + "\\", true);
                    }
                    catch (Exception me)
                    {
                    }
                }
                mqEncoder.BeginReceive();
            }
        }

        static void Wakeup(Object stateInfo)
        {
            EU2012.Common.Trace.DebugWrite("Encoder", "解開等待");
            myEvent[0].Set();
        }
    }
}