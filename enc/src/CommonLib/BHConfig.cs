using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EU2012.Common
{
    public class BHConfig
    {
        private static System.Data.DataRow mydr = null;
        private static string _connectionString = null;
        private static string _bh_path = null;
        private static string _log_path = null;
        private static string _web_path = null;
        private static string _plug_in_path = null;
        private static string _http_a = null;
        private static string _http_confirm = null;
        private static string _http_app = null;
        private static string _system_email_name = null;
        private static string _system_email_from = null;
        private static string[] _admin_email = null;
        private static string _bh_title = null;

        private static string _domainkey_domain = null;
        private static string _domainkey_selector = null;
        private static string _domainkey_pwd = null;

        private static string _ldap = null;
        private static string _sso_url = null;

        private static string _receiver_domain = null;

        private static string _access_db = null;
        private static string _iam_sender = null;
        private static string _iam_app = null;

        private static string _msmq_virual_machine = null;

        private static void ReadXml()
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            try
            {
                if (!System.IO.File.Exists(XmlFilePath))
                {
                    System.IO.Directory.CreateDirectory("C:\\Encoder\\log");
                    System.IO.Directory.CreateDirectory("C:\\Encoder\\Ftproot\\EncoderBuffer");
                    _connectionString = "";
                    _bh_path = @"C:\\Encoder\\";
                    _log_path = _bh_path + @"log\";
                    _web_path = _bh_path + @"web\";
                    _plug_in_path = _bh_path + @"plugin\";
                    _http_a = "";
                    _http_app = "";
                    _http_confirm = "";
                    _bh_title = "mailtitle";
                    _system_email_name = "system";
                    _system_email_from = "system";
                    _admin_email = new string[] { "adminemail" };
                    _msmq_virual_machine = ".";
                    _domainkey_domain = "";
                    _domainkey_selector = "";
                    _domainkey_pwd = "";
                    _receiver_domain = "returndomain";
                    _access_db = "0";
                    _iam_sender = "0";
                    _iam_app = "0";
                }
                else
                {
                    ds.ReadXml(XmlFilePath);
                    mydr = ds.Tables["common_set"].Rows[0];
                    _connectionString = ds.Tables["common_set"].Rows[0]["con_str"].ToString();
                    _bh_path = ds.Tables["common_set"].Rows[0]["bh_path"].ToString();
                    _log_path = _bh_path + @"log\";
                    _web_path = _bh_path + @"web\";
                    _plug_in_path = _bh_path + @"plugin\";
                    _http_a = ds.Tables["common_set"].Rows[0]["http_a"].ToString();
                    _http_app = ds.Tables["common_set"].Rows[0]["http_app"].ToString();
                    _http_confirm = ds.Tables["common_set"].Rows[0]["http_confirm"].ToString();
                    _bh_title = ds.Tables["common_set"].Rows[0]["bh_title"].ToString();
                    _system_email_name = ds.Tables["common_set"].Rows[0]["system_email_name"].ToString();
                    _system_email_from = ds.Tables["common_set"].Rows[0]["system_email_from"].ToString();
                    _admin_email = ds.Tables["common_set"].Rows[0]["admin_email"].ToString().Split(new char[] { ';' });

                    _msmq_virual_machine = ".";
                    try
                    {
                        _msmq_virual_machine = ds.Tables["common_set"].Rows[0]["msmq_virual_machine"].ToString();
                    }
                    catch { }

                    _domainkey_domain = ds.Tables["common_set"].Rows[0]["domainkey_domain"].ToString();
                    _domainkey_selector = ds.Tables["common_set"].Rows[0]["domainkey_selector"].ToString();
                    _domainkey_pwd = ds.Tables["common_set"].Rows[0]["domainkey_pwd"].ToString();

                    _receiver_domain = ds.Tables["common_set"].Rows[0]["receiver_domain"].ToString();

                    _access_db = ds.Tables["common_set"].Rows[0]["access_db"].ToString();
                    _iam_sender = ds.Tables["common_set"].Rows[0]["iam_sender"].ToString();
                    _iam_app = "1";
                    try
                    {
                        _iam_app = ds.Tables["common_set"].Rows[0]["iam_app"].ToString();
                    }
                    catch { };
                }
            }
            catch (Exception ee)
            {
                throw new Exception("開啟或讀取 c:\\BHSetting.xml 發生錯誤。" + ee.ToString());
            }
        }

        public static string GetCommonSet(string keyname)
        {
            if (mydr == null)
            {
                ReadXml();
            }
            if (mydr.Table.Columns.Contains(keyname))
                return mydr[keyname].ToString();
            else
                return "";
        }

        public static string XmlFilePath
        {
            get
            {
                return "C:\\BHSetting.xml";
            }
            set
            {
            }
        }

        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    ReadXml();
                }
                return _connectionString;
            }
            set
            {
            }
        }
        public static string BhPath
        {
            get
            {
                if (_bh_path == null)
                {
                    ReadXml();
                }
                return _bh_path;
            }
            set
            {
            }
        }
        public static string LogPath
        {
            get
            {
                if (_log_path == null)
                {
                    ReadXml();
                }
                return _log_path;
            }
            set
            {
            }
        }
        public static string WebPath
        {
            get
            {
                if (_web_path == null)
                {
                    ReadXml();
                }
                return _web_path;
            }
            set
            {
            }
        }
        public static string HttpA
        {
            get
            {
                if (_http_a == null)
                {
                    ReadXml();
                }
                return _http_a;
            }
            set
            {
            }
        }
        public static string HttpApp
        {
            get
            {
                if (_http_app == null)
                {
                    ReadXml();
                }
                return _http_app;
            }
            set
            {
            }
        }
        public static string HttpConfirm
        {
            get
            {
                if (_http_confirm == null)
                {
                    ReadXml();
                }
                return _http_confirm;
            }
            set
            {
            }
        }
        public static string BhTitle
        {
            get
            {
                if (_bh_title == null)
                {
                    ReadXml();
                }
                return _bh_title;
            }
            set
            {
            }
        }
        public static string SystemEmailName
        {
            get
            {
                if (_system_email_name == null)
                {
                    ReadXml();
                }
                return _system_email_name;
            }
            set
            {
            }
        }
        public static string SystemEmailFrom
        {
            get
            {
                if (_system_email_from == null)
                {
                    ReadXml();
                }
                return _system_email_from;
            }
            set
            {
            }
        }
        public static string[] AdminEmail
        {
            get
            {
                if (_admin_email == null)
                {
                    ReadXml();
                }
                return _admin_email;
            }
            set
            {
            }
        }
        public static string TempEml
        {
            get
            {
                if (_bh_path == null)
                {
                    ReadXml();
                }
                return _bh_path + @"TempEml\";
            }
            set
            {
            }
        }

        public static string PlugInPath
        {
            get
            {
                if (_plug_in_path == null)
                {
                    ReadXml();
                }
                return _plug_in_path;
            }
            set
            {
            }
        }

        public static string AccessDB
        {
            get
            {
                if (_access_db == null)
                {
                    ReadXml();
                }
                return _access_db;
            }
            set
            {
            }
        }
        public static string IamSender
        {
            get
            {
                if (_iam_sender == null)
                {
                    ReadXml();
                }
                return _iam_sender;
            }
            set
            {
            }
        }
        public static string IamApp
        {
            get
            {
                if (_iam_app == null)
                {
                    ReadXml();
                }
                return _iam_app;
            }
            set
            {
            }
        }

        public static string DomainkeyDomain
        {
            get
            {
                if (_domainkey_domain == null)
                {
                    ReadXml();
                }
                return _domainkey_domain;
            }
            set
            {
            }
        }
        public static string DomainkeySelector
        {
            get
            {
                if (_domainkey_selector == null)
                {
                    ReadXml();
                }
                return _domainkey_selector;
            }
            set
            {
            }
        }
        public static string DomainkeyPwd
        {
            get
            {
                if (_domainkey_pwd == null)
                {
                    ReadXml();
                }
                return _domainkey_pwd;
            }
            set
            {
            }
        }
        public static string ReceiverDomain
        {
            get
            {
                if (_receiver_domain == null)
                {
                    ReadXml();
                }
                return _receiver_domain;
            }
            set
            {
            }
        }
        public static string MsmqVirualMachine
        {
            get
            {
                if (_msmq_virual_machine == null)
                {
                    ReadXml();
                }
                return _msmq_virual_machine;
            }
            set
            {
            }
        }
    }
}