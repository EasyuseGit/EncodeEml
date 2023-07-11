using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace EU2012.Common {
    public class ServerConfig {

        public static List<ServerIp> ReceiverServer() {
            List<ServerIp> ls = new List<ServerIp>();

            try {
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(EU2012.Common.BHConfig.XmlFilePath);

                DataRow[] drs = ds.Tables["server_ip"].Select("server_type = 'receiver'");
                for (int i = 0; i < drs.Length; i++) {
                    ServerIp sip = new ServerIp();
                    sip.Ip = drs[i]["ip"].ToString();
                    sip.Port = int.Parse(drs[i]["port"].ToString());
                    ls.Add(sip);
                }
                
            } catch(Exception me) { };

            return ls;
        }

        public static List<ServerIp> SenderServer() {
            List<ServerIp> ls = new List<ServerIp>();

            try {
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(EU2012.Common.BHConfig.XmlFilePath);

                DataRow[] drs = ds.Tables["server_ip"].Select("server_type = 'sender'");
                for (int i = 0; i < drs.Length; i++) {
                    ServerIp sip = new ServerIp();
                    sip.Ip = drs[i]["ip"].ToString();
                    sip.Port = int.Parse(drs[i]["port"].ToString());
                    ls.Add(sip);
                }

            } catch (Exception me) { };

            return ls;
        }

        public static List<ServerIp> EncoderServer() {
            List<ServerIp> ls = new List<ServerIp>();

            try {
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(EU2012.Common.BHConfig.XmlFilePath);

                DataRow[] drs = ds.Tables["server_ip"].Select("server_type = 'encoder'");
                for (int i = 0; i < drs.Length; i++) {
                    ServerIp sip = new ServerIp();
                    sip.Ip = drs[i]["ip"].ToString();
                    sip.Port = int.Parse(drs[i]["port"].ToString());
                    ls.Add(sip);
                }

            } catch (Exception me) { };

            return ls;
        }

        public static List<ServerIp> AppServer() {
            List<ServerIp> ls = new List<ServerIp>();

            try {
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(EU2012.Common.BHConfig.XmlFilePath);

                DataRow[] drs = ds.Tables["server_ip"].Select("server_type = 'app'");
                for (int i = 0; i < drs.Length; i++) {
                    ServerIp sip = new ServerIp();
                    sip.Ip = drs[i]["ip"].ToString();
                    sip.Port = int.Parse(drs[i]["port"].ToString());
                    ls.Add(sip);
                }

            } catch (Exception me) { };

            return ls;
        }

        public static List<ServerIp> OtherServer() {
            List<ServerIp> ls = new List<ServerIp>();

            try {
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(EU2012.Common.BHConfig.XmlFilePath);

                DataRow[] drs = ds.Tables["server_ip"].Select("server_type = 'other'");
                for (int i = 0; i < drs.Length; i++) {
                    ServerIp sip = new ServerIp();
                    sip.Ip = drs[i]["ip"].ToString();
                    sip.Port = int.Parse(drs[i]["port"].ToString());
                    ls.Add(sip);
                }

            } catch (Exception me) { };

            return ls;
        }
    }


    public class ServerIp {
        public string Ip;
        public int Port;
    }
}
