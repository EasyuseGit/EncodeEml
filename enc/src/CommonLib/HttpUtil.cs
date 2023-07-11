using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace EU2012.Common
{
    public class HttpUtil
    {

        public static string Gethttp(string url, System.Text.Encoding urlEncoding)
        {
            string filename = "";
            return Gethttp(url, urlEncoding, out filename);
        }

        public static string Gethttp(string url, Encoding urlEncoding, out string filename)
        {
            filename = "";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Method = "GET";
            myReq.ContentType = "text/xml";
            myReq.Headers.Add("Accept-Language", "zh-tw");
            myReq.Accept = "*/*";
            myReq.AllowAutoRedirect = false;
            myReq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";
            try
            {
                HttpWebResponse myWebResponse = (HttpWebResponse)myReq.GetResponse();
                if (myReq.HaveResponse)
                {
                    System.IO.Stream respStream = myWebResponse.GetResponseStream();
                    System.IO.StreamReader respStreamReader = new System.IO.StreamReader(respStream, urlEncoding);
                    string strBuff = "";
                    strBuff = respStreamReader.ReadToEnd();
                    respStream.Close();
                    #region 取得檔名-從ASPX Header取得=========== by saleter
                    if (strBuff.Length > 0)
                    {
                        filename = myWebResponse.GetResponseHeader("X-Custom-RemoteName");
                        if (!string.IsNullOrEmpty(filename))
                        {
                            System.Text.Encoding enc = System.Text.Encoding.UTF8;
                            filename = enc.GetString(Convert.FromBase64String(filename));
                        }
                        else
                            filename = "";
                    }
                    #endregion =====================================
                    return strBuff;
                }
                else
                {
                    throw new ApplicationException("No response received from host.");
                }
            }
            catch (System.Net.WebException we)
            {
                throw (we);
            }
            catch (Exception me)
            {
                throw (me);
            }
        }

        public static byte[] GetByBytes(string url)
        {
            string filename = "";
            return GetByBytes(url, out filename);
        }

        public static byte[] GetByBytes(string url, out string filename)
        {
            filename = "";
            try
            {
                MyWebClient webClient = new MyWebClient();
                byte[] byteimg = webClient.DownloadData(url);
                #region 取得檔名-從ASPX Header取得=========== by saleter
                if (byteimg.Length > 0)
                {
                    filename = webClient.ResponseHeaders.Get("X-Custom-RemoteName");
                    if (!string.IsNullOrEmpty(filename))
                    {
                        System.Text.Encoding enc = System.Text.Encoding.UTF8;
                        filename = enc.GetString(Convert.FromBase64String(filename));
                    }
                    else
                        filename = "";
                }
                #endregion =====================================
                return byteimg;
            }
            catch (Exception me)
            {
                throw (me);
            }
        }

        public static string PostUTF8(string url, string datastring)
        {
            string filename = "";
            return PostUTF8(url, datastring, out filename);
        }

        public static string PostUTF8(string url, string datastring, out string filename)
        {
            filename = "";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Method = "POST";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(datastring);
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;
            Stream newStream = myReq.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            try
            {
                HttpWebResponse myWebResponse = (HttpWebResponse)myReq.GetResponse();
                if (myReq.HaveResponse)
                {
                    System.IO.Stream respStream = myWebResponse.GetResponseStream();
                    System.IO.StreamReader respStreamReader = new System.IO.StreamReader(respStream, System.Text.Encoding.UTF8);
                    string strBuff = "";
                    strBuff = respStreamReader.ReadToEnd();
                    respStream.Close();
                    #region 取得檔名-從ASPX Header取得=========== by saleter
                    if (strBuff.Length > 0)
                    {
                        filename = myWebResponse.GetResponseHeader("X-Custom-RemoteName");
                        if (!string.IsNullOrEmpty(filename))
                        {
                            System.Text.Encoding enc = System.Text.Encoding.UTF8;
                            filename = enc.GetString(Convert.FromBase64String(filename));
                        }
                        else
                            filename = "";
                    }
                    #endregion =====================================
                    return strBuff;
                }
                else
                {
                    throw new ApplicationException("No response received from host.");
                }
            }
            catch (System.Net.WebException we)
            {
                throw (we);
            }
            catch (Exception me)
            {
                throw (me);
            }
        }

        private class MyWebClient : WebClient
        {
            public MyWebClient()
            {

            }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 20 * 60 * 1000;
                return w;
            }
        }
    }
}