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

namespace EncoderLib
{
    public class iTextSharpRichmedia
    {
        public static byte[] AddInSwf(byte[] source_in, SwfDesc swfin)
        {

            System.IO.FileInfo fino = new System.IO.FileInfo(swfin.SwfFile);
            if (fino.Exists)
            {
                byte[] bytes = null;
                iTextSharp.text.pdf.PdfReader pr = new iTextSharp.text.pdf.PdfReader(source_in);
                using (System.IO.MemoryStream mos = new System.IO.MemoryStream())
                {
                    iTextSharp.text.pdf.PdfStamper stamper = new iTextSharp.text.pdf.PdfStamper(pr, mos);

                    iTextSharp.text.pdf.PdfWriter writer = stamper.Writer;

                    iTextSharp.text.pdf.richmedia.RichMediaAnnotation richMedia = new iTextSharp.text.pdf.richmedia.RichMediaAnnotation(writer, new iTextSharp.text.Rectangle(swfin.llx, swfin.lly, swfin.urx, swfin.ury));
                    iTextSharp.text.pdf.PdfFileSpecification fs = iTextSharp.text.pdf.PdfFileSpecification.FileEmbedded(writer, fino.FullName, fino.Name, null);
                    iTextSharp.text.pdf.PdfIndirectReference asset = richMedia.AddAsset(fino.Name, fs);

                    iTextSharp.text.pdf.richmedia.RichMediaPresentation presentation = new iTextSharp.text.pdf.richmedia.RichMediaPresentation();

                    iTextSharp.text.pdf.richmedia.RichMediaConfiguration configuration = new iTextSharp.text.pdf.richmedia.RichMediaConfiguration(iTextSharp.text.pdf.PdfName.FLASH);
                    iTextSharp.text.pdf.richmedia.RichMediaInstance instance = new iTextSharp.text.pdf.richmedia.RichMediaInstance(iTextSharp.text.pdf.PdfName.FLASH);
                    iTextSharp.text.pdf.richmedia.RichMediaParams flashVars = new iTextSharp.text.pdf.richmedia.RichMediaParams();

                    flashVars.FlashVars = swfin.SwfParam;
                    instance.Params = flashVars;
                    instance.Asset = asset;
                    configuration.AddInstance(instance);

                    iTextSharp.text.pdf.PdfIndirectReference configurationRef = richMedia.AddConfiguration(configuration);
                    iTextSharp.text.pdf.richmedia.RichMediaActivation activation = new iTextSharp.text.pdf.richmedia.RichMediaActivation();
                    activation.Condition = iTextSharp.text.pdf.PdfName.PV;
                    activation.Configuration = configurationRef;
                    activation.Presentation = presentation;
                    richMedia.Activation = activation;

                    iTextSharp.text.pdf.richmedia.RichMediaDeactivation deActivation = new iTextSharp.text.pdf.richmedia.RichMediaDeactivation();
                    deActivation.Condition = iTextSharp.text.pdf.PdfName.XD;

                    richMedia.Deactivation = deActivation;

                    iTextSharp.text.pdf.PdfAnnotation richMediaAnnotation = richMedia.CreateAnnotation();
                    richMediaAnnotation.Flags = iTextSharp.text.pdf.PdfAnnotation.FLAGS_HIDDEN;

                    stamper.AddAnnotation(richMediaAnnotation, swfin.SwfPage);

                    stamper.Writer.CloseStream = false;
                    stamper.Close();
                    mos.Position = 0;
                    bytes = mos.ToArray();
                    pr.Close();
                }

                return bytes;
            }
            else
                return source_in;
        }
    }

    public struct SwfDesc
    {
        public string SwfFile;
        public float llx;
        public float lly;
        public float urx;
        public float ury;
        public int SwfPage;
        public string SwfParam;
    }
}