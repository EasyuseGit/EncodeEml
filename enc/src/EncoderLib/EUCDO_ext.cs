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

        private static MIME_Entity EncryptData(MIME_Entity meNeedEncry)
        {
            MIME_Entity entity_encrypt = new MIME_Entity();

            MIME_b_ApplicationPkcs7Mime mbp7m = new MIME_b_ApplicationPkcs7Mime();
            entity_encrypt.ContentTransferEncoding = MIME_TransferEncodings.Base64;
            entity_encrypt.Body = mbp7m;

            entity_encrypt.ContentType.Parameters["smime-type"] = "signed-data";
            entity_encrypt.ContentType.Parameters["name"] = "smime.p7m";

            MIME_Encoding_EncodedWord x = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.Q, Encoding.UTF8);
            string alltxt = meNeedEncry.ToString(x, Encoding.UTF8);

            byte[] signature = DoEncrypt(alltxt);
            Stream msstream = new MemoryStream(signature);
            mbp7m.SetData(msstream, "base64");

            return entity_encrypt;
        }

        private static MIME_Entity EnvelopData(MIME_Entity meNeedSign)
        {

            MIME_Entity entity_singed = new MIME_Entity();

            MIME_b_ApplicationPkcs7Mime mbp7m = new MIME_b_ApplicationPkcs7Mime();
            entity_singed.Body = mbp7m;

            MIME_Encoding_EncodedWord x = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.Q, Encoding.UTF8);
            string alltxt = meNeedSign.ToString(x, Encoding.UTF8);

            byte[] signature = Enveloped(alltxt);
            Stream msstream = new MemoryStream(signature);
            mbp7m.SetData(msstream, "base64");

            return entity_singed;
        }

        private static byte[] Enveloped(string Content)
        {
            string pfxPath = @"pfxfile";
            string pfxPd = "malomalo";
            X509Certificate2 certificate = new X509Certificate2(pfxPath, pfxPd);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(Content);

            ContentInfo content = new ContentInfo(data);
            SignedCms signedCms = new SignedCms(content, false);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            signedCms.ComputeSignature(signer);
            byte[] signedbytes = signedCms.Encode();

            return signedbytes;
        }

        public static byte[] DoEncrypt(string message)
        {

            string pfxPath = @"pfxfile";
            string pfxPd = "malomalo";
            X509Certificate2 certificate = new X509Certificate2(pfxPath, pfxPd);

            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            EnvelopedCms envelopedCms = new EnvelopedCms(new ContentInfo(messageBytes));

            CmsRecipient recipients = new CmsRecipient(SubjectIdentifierType.IssuerAndSerialNumber, certificate);

            envelopedCms.Encrypt(recipients);

            return envelopedCms.Encode();
        }

    }
}