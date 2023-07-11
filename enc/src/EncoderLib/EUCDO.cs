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
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;

using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace EncoderLib
{
    public partial class EUCDO
    {
        #region ======== 公用基本參數 ====
        public string FromName;
        public string FromAddress;

        public string ToName;
        public string ToAddress;

        public string ReplyToName;
        public string ReplyToAddress;

        public string Subject;

        public Encoding CharSetEncoding;

        public string HtmlBody;
        public string TxtBody;

        public string PfxFilePath = "";
        public string PfxPassword = "";
        #endregion

        #region ========= 私域變數

        private bool _isEmbedded = false;

        private string _baseUrl = "";

        private List<MIME_Entity> _mimeAttachments = new List<MIME_Entity>();
        private List<MIME_Entity> _mimeEmbeddeds = new List<MIME_Entity>();
        #endregion

        public EUCDO(bool webap)
        {
            CharSetEncoding = Encoding.GetEncoding(950);
            if (!webap)
            {
                mut = new Mutex(false, "EuCDOSmimeMut");
            }
            PfxFilePath = "";
            PfxPassword = "";
        }
        public EUCDO()
        {
            mut = new Mutex(false, "EuCDOSmimeMut");

            CharSetEncoding = Encoding.GetEncoding(950);
            PfxFilePath = "";
            PfxPassword = "";
        }

        public void AddAttachment(string filepath)
        {
            
            _mimeAttachments.Add(MIME_Entity.CreateEntity_Attachment(filepath));
        }

        public void AddByteAttachment(System.IO.Stream str, string filename)
        {
            
            _mimeAttachments.Add(MIME_Entity.CreateEntity_Attachment(filename, str));

        }

        public void AppendTrace(string traceHtml)
        {
            HtmlBody += traceHtml;
        }

        public void SaveFile(string filename, string threadinedxvalue)
        {

            Mail_Message msg = new Mail_Message();
            msg.MimeVersion = "1.0";
            msg.MessageID = MIME_Utils.CreateMessageID();
            msg.Date = DateTime.Now;
            msg.From = new Mail_t_MailboxList();
            msg.From.Add(new Mail_t_Mailbox(FromName.Trim(), FromAddress.Trim()));
            msg.To = new Mail_t_AddressList();
            msg.To.Add(new Mail_t_Mailbox(ToName.Trim(), ToAddress.Trim()));
            if (ReplyToAddress != null && ReplyToAddress != "")
            {
                msg.ReplyTo = new Mail_t_AddressList();
                msg.ReplyTo.Add(new Mail_t_Mailbox("", ReplyToAddress.Trim()));
            }
            msg.Subject = Subject;

            msg.Header.Add(new MIME_h_Unstructured("X-AssuranceSys-BulkMail", "1"));
            msg.Header.Add(new MIME_h_Unstructured("Precedence", "bulk"));
            msg.Header.Add(new MIME_h_Unstructured("thread-index", threadinedxvalue));

            MIME_Entity mimeMain = null;

            mimeMain = BuildContent();

            if (_isEmbedded)
            {
                mimeMain = BuildEmbeddedContent(mimeMain);
            }

            if (_mimeAttachments.Count > 0)
            {
                mimeMain = BuildAttachmentsContent(mimeMain);
            }

            if (PfxFilePath != "" && PfxPassword != "")
            {
                mimeMain = SignData(mimeMain, PfxFilePath, PfxPassword);
            }
            msg.Body = mimeMain.Body;

            if (mimeMain.ContentTransferEncoding != null)
            {
                msg.ContentTransferEncoding = mimeMain.ContentTransferEncoding;
            }

            MIME_Encoding_EncodedWord x = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, CharSetEncoding);

            System.IO.FileInfo fino = new FileInfo(filename);
            System.IO.Directory.CreateDirectory(fino.Directory.FullName);
            msg.ToFile(filename, x, CharSetEncoding);

        }

        public byte[] ToByte()
        {

            Mail_Message msg = new Mail_Message();
            msg.MimeVersion = "1.0";
            msg.MessageID = MIME_Utils.CreateMessageID();
            msg.Date = DateTime.Now;
            msg.From = new Mail_t_MailboxList();
            msg.From.Add(new Mail_t_Mailbox(FromName.Trim(), FromAddress.Trim()));
            msg.To = new Mail_t_AddressList();
            msg.To.Add(new Mail_t_Mailbox(ToName.Trim(), ToAddress.Trim()));
            if (ReplyToAddress != null && ReplyToAddress != "")
            {
                msg.ReplyTo = new Mail_t_AddressList();
                msg.ReplyTo.Add(new Mail_t_Mailbox("", ReplyToAddress.Trim()));
            }
            msg.Subject = Subject;

            MIME_Entity mimeMain = null;

            mimeMain = BuildContent();

            if (_isEmbedded)
            {
                mimeMain = BuildEmbeddedContent(mimeMain);
            }

            if (_mimeAttachments.Count > 0)
            {
                mimeMain = BuildAttachmentsContent(mimeMain);
            }

            if (PfxFilePath != "" && PfxPassword != "")
            {
                mimeMain = SignData(mimeMain, PfxFilePath, PfxPassword);
            }
            msg.Body = mimeMain.Body;
            if (mimeMain.ContentTransferEncoding != null)
            {
                msg.ContentTransferEncoding = mimeMain.ContentTransferEncoding;
            }

            MIME_Encoding_EncodedWord x = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, CharSetEncoding);

            return msg.ToByte(x, CharSetEncoding);
        }

        private MIME_Entity BuildContent()
        {

            MIME_Entity entity_multipartAlternative = new MIME_Entity();
            MIME_h_ContentType contentType_multipartAlternative = new MIME_h_ContentType(MIME_MediaTypes.Multipart.alternative);
            contentType_multipartAlternative.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
            MIME_b_MultipartAlternative multipartAlternative = new MIME_b_MultipartAlternative(contentType_multipartAlternative);
            entity_multipartAlternative.Body = multipartAlternative;

            MIME_Entity entity_text_plain = new MIME_Entity();
            MIME_b_Text text_plain = new MIME_b_Text(MIME_MediaTypes.Text.plain);
            entity_text_plain.Body = text_plain;
            text_plain.SetText(MIME_TransferEncodings.QuotedPrintable, CharSetEncoding, TxtBody);
            multipartAlternative.BodyParts.Add(entity_text_plain);

            MIME_Entity entity_text_html = new MIME_Entity();
            MIME_b_Text text_html = new MIME_b_Text(MIME_MediaTypes.Text.html);
            entity_text_html.Body = text_html;
            text_html.SetText(MIME_TransferEncodings.QuotedPrintable, CharSetEncoding, HtmlBody);
            multipartAlternative.BodyParts.Add(entity_text_html);

            return entity_text_html;
        }

        private MIME_Entity BuildEmbeddedContent(MIME_Entity mimeHtml)
        {

            MIME_Entity entity_multipartRelated = new MIME_Entity();
            MIME_h_ContentType contentType_multipartRelated = new MIME_h_ContentType(MIME_MediaTypes.Multipart.related);
            contentType_multipartRelated.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
            MIME_b_MultipartRelated multipartRelated = new MIME_b_MultipartRelated(contentType_multipartRelated);

            entity_multipartRelated.Body = multipartRelated;

            multipartRelated.BodyParts.Add(mimeHtml);

            for (int i = 0; i < _mimeEmbeddeds.Count; i++)
            {
                multipartRelated.BodyParts.Add(_mimeEmbeddeds[i]);
            }

            return entity_multipartRelated;
        }

        private MIME_Entity BuildAttachmentsContent(MIME_Entity mimeMix)
        {
            MIME_Entity entity_mix = new MIME_Entity();

            MIME_h_ContentType contentType_multipartMixed = new MIME_h_ContentType(MIME_MediaTypes.Multipart.mixed);
            contentType_multipartMixed.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
            MIME_b_MultipartMixed multipartMixed = new MIME_b_MultipartMixed(contentType_multipartMixed);
            entity_mix.Body = multipartMixed;

            multipartMixed.BodyParts.Add(mimeMix);

            for (int i = 0; i < _mimeAttachments.Count; i++)
            {
                multipartMixed.BodyParts.Add(_mimeAttachments[i]);
            }

            return entity_mix;

        }

        private static MIME_Entity SignData(MIME_Entity meNeedSign, string pfxFilePath, string pfxPassword)
        {

            MIME_Entity entity_singed = new MIME_Entity();

            MIME_h_ContentType contentType_multipartSign = new MIME_h_ContentType(MIME_MediaTypes.Multipart.signed);
            contentType_multipartSign.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
            contentType_multipartSign.Parameters["protocol"] = "application/x-pkcs7-signature";
            contentType_multipartSign.Parameters["micalg"] = "SHA1";

            MIME_b_MultipartSigned multipartSign = new MIME_b_MultipartSigned(contentType_multipartSign);

            entity_singed.Body = multipartSign;
            entity_singed.ContentTransferEncoding = MIME_TransferEncodings.SevenBit;

            multipartSign.BodyParts.Add(meNeedSign);

            MIME_Encoding_EncodedWord x = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.Q, Encoding.UTF8);
            string alltxt = meNeedSign.ToString(x, Encoding.UTF8);
            MIME_Entity entity_smime = new MIME_Entity();
            MIME_b_Application smime = new MIME_b_Application(MIME_MediaTypes.Application.x_pkcs7_signature);

            MIME_h_ContentDisposition mhc = new MIME_h_ContentDisposition(MIME_DispositionTypes.Attachment);
            mhc.Parameters["filename"] = "smime.p7s";
            entity_smime.ContentDisposition = mhc;
            entity_smime.ContentTransferEncoding = MIME_TransferEncodings.Base64;

            entity_smime.Body = smime;
            entity_smime.ContentType.Parameters["name"] = "smime.p7s";

            byte[] signature = GetSignature(alltxt, pfxFilePath, pfxPassword);
            Stream msstream = new MemoryStream(signature);
            smime.SetData(msstream, "base64");

            multipartSign.BodyParts.Add(entity_smime);

            return entity_singed;

        }

        private static Mutex mut;
        public static byte[] GetSignature(string message, string pfxFilePath, string pfxPassword)
        {
            try
            {
                System.Web.Caching.Cache _Cache = System.Web.HttpRuntime.Cache;
                X509Certificate2 certificate;

                if (mut != null)
                    mut.WaitOne();
                try
                {
                    if (_Cache[pfxFilePath] == null)
                    {
                        certificate = new X509Certificate2(pfxFilePath, pfxPassword);
                        _Cache.Insert(pfxFilePath, certificate, null, DateTime.Now.AddSeconds(3600), System.Web.Caching.Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        certificate = (X509Certificate2)_Cache[pfxFilePath];
                    }
                }
                catch (Exception cex)
                {
                    throw cex;
                }
                finally
                {

                    try
                    {
                        mut.ReleaseMutex();
                    }
                    catch { };
                }

                byte[] messageBytes = Encoding.ASCII.GetBytes(message);

                SignedCms signedCms = new SignedCms(new ContentInfo(messageBytes), true);

                CmsSigner cmsSigner = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
                cmsSigner.IncludeOption = X509IncludeOption.WholeChain;

                Pkcs9SigningTime signingTime = new Pkcs9SigningTime();
                cmsSigner.SignedAttributes.Add(signingTime);

                signedCms.ComputeSignature(cmsSigner, false);

                return signedCms.Encode();
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }

        public static void ForceReleaseMut()
        {
            try
            {
                mut.ReleaseMutex();
            }
            catch { };
        }
    }
}