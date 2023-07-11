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
using System.IO;
using EvoPdf;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;

namespace EncoderLib
{
    public class EncodeMail
    {

        public static void ThreadProc(Object stateInfo)
        {

            string result_id = "";
            string split_table_id = "";
            string savename = "";
            try
            {
                EncodeStateObj ecobj = (EncodeStateObj)stateInfo;
                EU2012.Common.Trace.DebugWrite("ThreadProc", "開始組信 result_id = " + ecobj.UserDr["result_id"].ToString());

                string project_id = ecobj.Mailbody.Tables["app_bh_project"].Rows[0]["project_id"].ToString();
                result_id = ecobj.UserDr["result_id"].ToString();
                split_table_id = ecobj.Mailbody.Tables["app_bh_project"].Rows[0]["project_split_table_id"].ToString();
                string storage_file = EU2012.Common.BHConfig.BhPath + "EmlStorage\\" + project_id + "\\" + result_id + ".eml";
                savename = EU2012.Common.BHConfig.BhPath + "eml\\" + System.Guid.NewGuid().ToString() + ".eml";

                if (System.IO.File.Exists(storage_file))
                {
                    EU2012.Common.Trace.DebugWrite("ThreadProc", "直接從 EmlStore 取出檔案 " + storage_file);
                    System.IO.File.Copy(storage_file, savename, true);
                }
                else
                {
                    EUCDO ecdo = MakeCDO(ecobj);
                    EU2012.Common.Trace.DebugWrite("ThreadProc", "本次開始存檔" + savename + "(result_id=" + result_id + ")");
                    string thread_index = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(project_id + "_" + result_id));

                    GetPfxPwd(ecdo, ecobj.Mailbody.Tables["app_bh_project"].Rows[0]);
                    ecdo.SaveFile(savename, thread_index);

                    System.IO.Directory.CreateDirectory(EU2012.Common.BHConfig.BhPath + "EmlStorage\\" + project_id);
                    System.IO.File.Copy(savename, storage_file, true);

                }
                EU2012.Common.Trace.DebugWrite("ThreadProc", "本次結束存檔" + savename + "(result_id=" + result_id + ")");
                
            }
            catch (Exception me)
            {
                EU2012.Common.Trace.DebugWrite("組信有誤", me.ToString());
                try
                {
                    if (savename != "")
                    {
                        if (System.IO.File.Exists(savename))
                            System.IO.File.Delete(savename);
                    }
                }
                catch (Exception ee)
                {
                    EU2012.Common.Trace.DebugWrite("刪信有誤", ee.ToString());
                }
            }
            finally
            {
            }

        }

        public static EUCDO MakeCDO(EncodeStateObj ecobj, bool isWeb = false)
        {
            EUCDO ecdo = new EUCDO(isWeb);
            ecdo.FromName = ecobj.Mailbody.Tables["app_bh_project"].Rows[0]["project_sender_name"].ToString();
            ecdo.FromAddress = ecobj.Mailbody.Tables["app_bh_project"].Rows[0]["project_sender_email"].ToString();
            string mailsubject = ecobj.Mailbody.Tables["app_bh_project"].Rows[0]["project_subject"].ToString();
            mailsubject = EU2012.Common.Tools.ReplaceBhColumn(mailsubject, ecobj.UserDr);
            ecdo.Subject = mailsubject;
            ecdo.CharSetEncoding = System.Text.Encoding.UTF8;
            if (ecobj.UserDr.Table.Columns.Contains("send_name"))
                ecdo.ToName = ecobj.UserDr["send_name"].ToString();
            else
                ecdo.ToName = ecobj.UserDr["user_name"].ToString();
            if (ecobj.UserDr.Table.Columns.Contains("send_email"))
                ecdo.ToAddress = ecobj.UserDr["send_email"].ToString();
            else
                ecdo.ToAddress = ecobj.UserDr["user_email"].ToString();
            bool forceNoPwd = false;
            if (ecobj.UserDr.Table.Columns.Contains("send_no_pwd"))
            {
                if (ecobj.UserDr["send_no_pwd"].ToString() == "1")
                    forceNoPwd = true;
            }
            else
            {
                if (ecobj.UserDr.Table.Columns.Contains("user_no_pwd"))
                {
                    if (ecobj.UserDr["user_no_pwd"].ToString() == "1")
                        forceNoPwd = true;
                }
            }
            ecdo.ReplyToAddress = ecobj.Mailbody.Tables["app_bh_project"].Rows[0]["project_reply_email"].ToString();

            int project_id = int.Parse(ecobj.Mailbody.Tables["app_bh_project"].Rows[0]["project_id"].ToString());
            int user_id = int.Parse(ecobj.UserDr["user_id"].ToString());
            string project_category_code = ecobj.Mailbody.Tables["app_bh_project"].Rows[0]["project_category_code"].ToString();
            foreach (DataRow dr in ecobj.Mailbody.Tables["app_bh_project_mailbody"].Rows)
            {
                string Custom_RemoteName = "";
                string bdcontent = dr["body_content"].ToString();

                if (dr["body_from"].ToString() == "2" || dr["body_from"].ToString() == "3")
                {
                    bdcontent = EU2012.Common.Tools.ReplaceBhColumn(bdcontent, ecobj.UserDr);
                }
                else
                    bdcontent = EU2012.Common.Tools.ReplaceBhColumnWithUrlEncode(bdcontent, ecobj.UserDr);
                switch (dr["mail_content_type"].ToString())
                {
                    case "html":
                        switch (dr["body_from"].ToString())
                        {
                            case "1":
                                if (!isWeb)
                                    EU2012.Common.Trace.DebugWrite("MakeCDO", "URL = " + bdcontent);
                                ecdo.HtmlBody = EU2012.Common.HttpUtil.Gethttp(bdcontent, System.Text.Encoding.GetEncoding(dr["body_url_charset"].ToString())) + TraceCode(project_id, user_id);
                                break;
                            case "2":
                                bdcontent = EU2012.Common.Tools.ReplaceBhDataSource(bdcontent);
                                ecdo.HtmlBody = bdcontent + TraceCode(project_id, user_id);
                                break;
                            case "3":
                                ecdo.HtmlBody = bdcontent + TraceCode(project_id, user_id);
                                break;
                            case "4":
                                if (!isWeb)
                                    EU2012.Common.Trace.DebugWrite("MakeCDO", "URL = " + bdcontent);
                                string mc = EU2012.Common.HttpUtil.Gethttp(bdcontent, System.Text.Encoding.GetEncoding(dr["body_url_charset"].ToString())) + TraceCode(project_id, user_id);

                                mc = EU2012.Common.Tools.ReplaceBhColumn(mc, ecobj.UserDr);
                                ecdo.HtmlBody = mc;
                                break;
                            case "7":
                                break;
                        }
                        break;
                    case "text":
                        ecdo.TxtBody = bdcontent;
                        break;
                    case "application/octet-stream":
                        byte[] bytes = null;
                        switch (dr["body_from"].ToString())
                        {
                            case "1":
                                if (!isWeb)
                                    EU2012.Common.Trace.DebugWrite("MakeCDO", "URL = " + bdcontent);
                                bytes = EU2012.Common.HttpUtil.GetByBytes(bdcontent, out Custom_RemoteName);
                                break;
                            case "2":
                                bdcontent = EU2012.Common.Tools.ReplaceBhDataSource(bdcontent);
                                bytes = System.Text.Encoding.UTF8.GetBytes(bdcontent);
                                break;
                            case "3":
                                bytes = System.Text.Encoding.UTF8.GetBytes(bdcontent);
                                break;
                            case "4":
                                if (!isWeb)
                                    EU2012.Common.Trace.DebugWrite("MakeCDO", "URL = " + bdcontent);
                                string mc = EU2012.Common.HttpUtil.Gethttp(bdcontent, System.Text.Encoding.GetEncoding(dr["body_url_charset"].ToString()), out Custom_RemoteName) + TraceCode(project_id, user_id);

                                mc = EU2012.Common.Tools.ReplaceBhColumn(mc, ecobj.UserDr);
                                bytes = System.Text.Encoding.UTF8.GetBytes(mc);
                                break;
                            case "5":
                                if (!isWeb)
                                    EU2012.Common.Trace.DebugWrite("MakeCDO", "URL = " + bdcontent);
                                string pdfmc = EU2012.Common.HttpUtil.Gethttp(bdcontent, System.Text.Encoding.GetEncoding(dr["body_url_charset"].ToString()), out Custom_RemoteName);
                                if (pdfmc.Trim() == "")
                                {
                                    bytes = new byte[] { };
                                    break;
                                }

                                pdfmc = EU2012.Common.Tools.ReplaceBhColumn(pdfmc, ecobj.UserDr);

                                Document document = new Document();
                                document.CompressionLevel = PdfCompressionLevel.Normal;
                                document.Margins = new Margins(10, 10, 0, 0);
                                PdfPage page = document.Pages.AddNewPage(PdfPageSize.A4, new Margins(10, 10, 10, 10), PdfPageOrientation.Portrait);

                                AddElementResult addResult;

                                if (1 == 1)
                                {
                                    HtmlToPdfElement htmlToPdfElement;
                                    htmlToPdfElement = new HtmlToPdfElement(0, 0, 0, 0, pdfmc, bdcontent);
                                    if (BHConfig.GetCommonSet(project_category_code + "_pdf_width") != "")
                                        htmlToPdfElement.HtmlViewerWidth = int.Parse(BHConfig.GetCommonSet(project_category_code + "_pdf_width"));
                                    htmlToPdfElement.FitWidth = true;
                                    htmlToPdfElement.EmbedFonts = true;
                                    htmlToPdfElement.LiveUrlsEnabled = true;
                                    htmlToPdfElement.AvoidImageBreak = true;
                                    htmlToPdfElement.AvoidTextBreak = true;
                                    addResult = page.AddElement(htmlToPdfElement);
                                }
                                else
                                {
                                    HtmlToImageElement htmlToImageElement;
                                    htmlToImageElement = new HtmlToImageElement(0, 0, 0, 0, pdfmc, bdcontent);
                                    htmlToImageElement.FitWidth = true;

                                    addResult = page.AddElement(htmlToImageElement);
                                }
                                bytes = document.Save();

                                foreach (DataRow drswf in ecobj.Mailbody.Tables["app_bh_project_swf"].Rows)
                                {
                                    EncoderLib.SwfDesc swfdesc = new EncoderLib.SwfDesc();
                                    swfdesc.llx = float.Parse(drswf["llx"].ToString());
                                    swfdesc.lly = float.Parse(drswf["lly"].ToString());
                                    swfdesc.urx = float.Parse(drswf["urx"].ToString());
                                    swfdesc.ury = float.Parse(drswf["ury"].ToString());
                                    swfdesc.SwfFile = BHConfig.BhPath + "Swf\\" + drswf["swf_real_name"].ToString();
                                    swfdesc.SwfPage = int.Parse(drswf["swf_page"].ToString());
                                    swfdesc.SwfParam = drswf["swf_param"].ToString();

                                    bytes = EncoderLib.iTextSharpRichmedia.AddInSwf(bytes, swfdesc);
                                }

                                if (dr["pdf_pwd_column"].ToString() != "" && ecobj.UserDr[dr["pdf_pwd_column"].ToString()].ToString() != "---" && !forceNoPwd)
                                {
                                    switch (dr["is_pdf"].ToString())
                                    {

                                        case "0":
                                            break;
                                        case "1":
                                            iTextSharp.text.pdf.PdfReader pr1 = new iTextSharp.text.pdf.PdfReader(bytes);
                                            using (System.IO.MemoryStream mos = new System.IO.MemoryStream())
                                            {
                                                iTextSharp.text.pdf.PdfStamper stamper = new iTextSharp.text.pdf.PdfStamper(pr1, mos);
                                                stamper.SetEncryption(System.Text.Encoding.ASCII.GetBytes(ecobj.UserDr[dr["pdf_pwd_column"].ToString()].ToString().Trim()), System.Text.Encoding.ASCII.GetBytes(ecobj.UserDr[dr["pdf_pwd_column"].ToString()].ToString().Trim()), iTextSharp.text.pdf.PdfWriter.ALLOW_PRINTING, iTextSharp.text.pdf.PdfWriter.STANDARD_ENCRYPTION_128);

                                                stamper.Writer.CloseStream = false;
                                                stamper.Close();
                                                mos.Position = 0;
                                                bytes = mos.ToArray();
                                            }
                                            break;
                                        case "2":
                                            iTextSharp.text.pdf.PdfReader pr2 = new iTextSharp.text.pdf.PdfReader(bytes);
                                            using (System.IO.MemoryStream mos = new System.IO.MemoryStream())
                                            {
                                                iTextSharp.text.pdf.PdfStamper stamper = new iTextSharp.text.pdf.PdfStamper(pr2, mos);
                                                byte[] pk = System.Convert.FromBase64String(ecobj.UserDr[dr["pdf_pwd_column"].ToString()].ToString());
                                                Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
                                                Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cp.ReadCertificate(pk) };
                                                stamper.SetEncryption(chain, new int[] { iTextSharp.text.pdf.PdfWriter.ALLOW_PRINTING }, iTextSharp.text.pdf.PdfWriter.ENCRYPTION_AES_128);
                                                stamper.Writer.CloseStream = false;
                                                stamper.Close();
                                                mos.Position = 0;
                                                bytes = mos.ToArray();
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                if (bytes.LongLength < 55000)
                                {

                                    EU2012.Common.Trace.DebugWrite("MakeCDO", "size URL = " + bdcontent);
                                    EU2012.Common.Trace.DebugWrite("MakeCDO", "size  pdfmc = " + pdfmc);
                                    throw (new Exception("PDF Size = " + bytes.LongLength.ToString() + ", 小於 55000 所以不組!!"));
                                }

                                break;
                            case "7":
                                break;
                        }

                        if (bytes.Length > 0)
                        {
                            if (dr["is_zip"].ToString() == "1")
                            {
                                string filename = EU2012.Common.Tools.ReplaceBhColumn(dr["file_name"].ToString(), ecobj.UserDr);
                                filename = filename.Replace("<|>custom_remotename<|>", Custom_RemoteName);
                                using (System.IO.MemoryStream mos = new System.IO.MemoryStream())
                                {
                                    using (ZipOutputStream s = new ZipOutputStream(mos))
                                    {
                                        s.IsStreamOwner = false;
                                        s.SetLevel(3);
                                        if (dr["zip_pwd_column"].ToString() != "" && !forceNoPwd)
                                        {
                                            s.Password = ecobj.UserDr[dr["zip_pwd_column"].ToString()].ToString().Trim();
                                        }
                                        ZipEntry entry = new ZipEntry(filename);
                                        entry.DateTime = DateTime.Now;
                                        entry.Size = bytes.Length;
                                        s.PutNextEntry(entry);
                                        s.Write(bytes, 0, bytes.Length);
                                        s.Flush();
                                        s.Close();
                                    }

                                    string zipfilename = filename;
                                    if (filename.IndexOf(".") > 0)
                                    {
                                        zipfilename = filename.Substring(0, filename.IndexOf(".")) + ".zip";
                                    }
                                    else
                                        zipfilename = filename + ".zip";
                                    mos.Position = 0;
                                    ecdo.AddByteAttachment(mos, zipfilename);
                                }


                            }
                            else
                            {
                                string filename = EU2012.Common.Tools.ReplaceBhColumn(dr["file_name"].ToString(), ecobj.UserDr);
                                filename = filename.Replace("<|>custom_remotename<|>", Custom_RemoteName);
                                System.IO.Stream stream = new System.IO.MemoryStream(bytes);
                                ecdo.AddByteAttachment(stream, filename);
                            }
                        }
                        break;
                }
            }

            return ecdo;

        }

        public static void GetPfxPwd(EUCDO myeucdo, DataRow drProject)
        {
            if (drProject["project_smime"].ToString() == "1")
            {
                System.Web.Caching.Cache _Cache = System.Web.HttpRuntime.Cache;
                if (_Cache[drProject["project_sender_email"].ToString()] == null)
                {
                    string pfxfile = BHConfig.BhPath + "PlugIn\\Pfx\\" + drProject["project_sender_email"].ToString() + ".pfx";
                    if (System.IO.File.Exists(pfxfile))
                    {
                        string pfxpwdfile = BHConfig.BhPath + "PlugIn\\Pfx\\" + drProject["project_sender_email"].ToString() + ".txt";
                        string readpassword = "";
                        using (StreamReader reader = new StreamReader(pfxpwdfile))
                        {
                            readpassword = reader.ReadToEnd();
                        }
                        _Cache.Insert(drProject["project_sender_email"].ToString(), readpassword, null, DateTime.Now.AddSeconds(3600), System.Web.Caching.Cache.NoSlidingExpiration); ;
                        myeucdo.PfxFilePath = pfxfile;
                        myeucdo.PfxPassword = readpassword;

                    }
                    else
                    {
                        _Cache.Insert(drProject["project_sender_email"].ToString(), "", null, DateTime.Now.AddSeconds(3600), System.Web.Caching.Cache.NoSlidingExpiration);

                    }
                }
                else
                {
                    myeucdo.PfxFilePath = BHConfig.BhPath + "PlugIn\\Pfx\\" + drProject["project_sender_email"].ToString() + ".pfx";
                    myeucdo.PfxPassword = _Cache[drProject["project_sender_email"].ToString()].ToString();
                }
            }
        }
        private static string TraceCode(int project_id, int user_id)
        {
            string encodevalue = "project_id=" + project_id.ToString() + "&user_id=" + user_id.ToString();
            string keyvalue = System.Web.HttpUtility.UrlEncode(encodevalue);
            return "<img src=\"" + BHConfig.HttpA + "/s.aspx?k=" + keyvalue + "\" width=1 height=1 />";
        }
    }
}