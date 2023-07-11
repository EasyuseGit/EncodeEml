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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using EU2012.Common;

namespace EncoderWindowsService {
    public partial class EncoderWindowsService : ServiceBase {

        private EncoderLib.BufferListen bl;
        private EncoderLib.BufferQueueListen bq;

        private Process[] notePad;

        public EncoderWindowsService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            EU2012.Common.Trace.SetLogFile(BHConfig.LogPath, "Encoder");
            EU2012.Common.Trace.DebugWrite("Encoder", "EncoderWindowsService Start Service");

            if (BHConfig.DomainkeyDomain != "") {
                int Consolecount = int.Parse(BHConfig.GetCommonSet("encoder_console"));
                notePad = new Process[Consolecount];
                bl = new EncoderLib.BufferListen(Consolecount);
                for (int i = 0; i < Consolecount; i++) {
                    notePad[i] = new Process();
                    notePad[i].StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "\\ConsoleEncoderService.exe";
                    notePad[i].StartInfo.Arguments = i.ToString();
                    notePad[i].StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    notePad[i].Start();
                    if (i == 0)
                        System.Threading.Thread.Sleep(1000 * 5);
                }
                
            } else {
                bq = new EncoderLib.BufferQueueListen();
                bl = new EncoderLib.BufferListen();
            }
        }

        protected override void OnStop() {
            EU2012.Common.Trace.DebugWrite("Encoder", "Stop EncoderService");
            if (BHConfig.GetCommonSet("encoder_console") != "") {
                for (int i = 0; i < notePad.Length; i++) {
                    try {
                        EU2012.Common.Trace.DebugWrite("Encoder", "try Kill console exe , no = " + i.ToString());
                        notePad[i].Kill();
                    } catch {
                    }
                }
            }
        }

    }
}
