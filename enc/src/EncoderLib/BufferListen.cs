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
using EU2012.Common;
using System.Messaging;

namespace EncoderLib {
   
    public class BufferListen {
        private System.IO.FileSystemWatcher _fileSystemWatcher;
        private int queue_index = 2;
        private int counter = 0;
        public BufferListen(int qix) {
            Trace.SetLogFile(BHConfig.LogPath, "Encoder");
            queue_index = qix;
            Trace.DebugWrite("Encoder", " BufferFileListen Start Service , quene_index = " + queue_index.ToString());
            try {
                for (int i = 0; i < queue_index; i++) {
                    Create_Admin("eu_bh2012_encoder_files" + i.ToString());
                }
            } catch (Exception me) {
                EU2012.Common.Trace.ErrorWrite("BufferListen", me.ToString());
            }
            try {
                this._fileSystemWatcher = new System.IO.FileSystemWatcher();
                ((System.ComponentModel.ISupportInitialize)(this._fileSystemWatcher)).BeginInit();
                this._fileSystemWatcher.EnableRaisingEvents = true;
                this._fileSystemWatcher.Filter = "*.ok";
                this._fileSystemWatcher.Path = EU2012.Common.BHConfig.BhPath + "Ftproot\\EncoderBuffer";
                this._fileSystemWatcher.Created += new System.IO.FileSystemEventHandler(_fileSystemWatcher_Created);
                ((System.ComponentModel.ISupportInitialize)(this._fileSystemWatcher)).EndInit();
            } catch (Exception me) {
                EU2012.Common.Trace.ErrorWrite("Encoder", me.ToString());
            }
        }

        public BufferListen() {
            Trace.SetLogFile(BHConfig.LogPath, "Encoder");
            Trace.DebugWrite("Encoder", "BufferFileListen Start Service");
            try {
                Create_Admin("eu_bh2012_encoder_files");
                this._fileSystemWatcher = new System.IO.FileSystemWatcher();
                ((System.ComponentModel.ISupportInitialize)(this._fileSystemWatcher)).BeginInit();
                this._fileSystemWatcher.EnableRaisingEvents = true;
                this._fileSystemWatcher.Filter = "*.ok";
                this._fileSystemWatcher.Path = EU2012.Common.BHConfig.BhPath + "Ftproot\\EncoderBuffer";
                this._fileSystemWatcher.Created += new System.IO.FileSystemEventHandler(_fileSystemWatcher_Created);
                ((System.ComponentModel.ISupportInitialize)(this._fileSystemWatcher)).EndInit();
            } catch (Exception me) {
                EU2012.Common.Trace.ErrorWrite("Encoder", me.ToString());
            }
        }

        private void Create_Admin(string QueueName) {
            MessageQueue mp = new System.Messaging.MessageQueue();
            if (MessageQueue.Exists(@".\Private$\" + QueueName)) {
                EU2012.Common.Trace.DebugWrite("Create_Admin", @".\Private$\" + QueueName + " 已存在");
                mp.Path = ".\\private$\\" + QueueName;
            } else {
                EU2012.Common.Trace.DebugWrite("Create_Admin", @".\Private$\" + QueueName + " 開始建立");
                mp = MessageQueue.Create(@".\Private$\" + QueueName);
                mp.Label = @"private$\" + QueueName;
            }
        }


        void _fileSystemWatcher_Created(object sender, System.IO.FileSystemEventArgs e) {
            try {
                EU2012.Common.Trace.DebugWrite("fileSystemWatcher", "接收到檔案 " + e.Name);
                counter++;
                counter = counter % queue_index;
                if (e.Name.EndsWith(".stop.ok")) {
                    EU2012.Common.MSMQ.EncoderFilesObj efobj = new EU2012.Common.MSMQ.EncoderFilesObj();
                    efobj.fileName = e.Name.Substring(0, e.Name.Length - 3);
                    if (BHConfig.GetCommonSet("encoder_console") != "")
                        for (int i = 0; i < queue_index; i++) {
                            EU2012.Common.MSMQ.BhQueue.SendEncoderFilesMessageHigh(efobj, i);
                        }
                    else
                        EU2012.Common.MSMQ.BhQueue.SendEncoderFilesMessageHigh(efobj);
                    EU2012.Common.Trace.DebugWrite("fileSystemWatcher", "傳送 中斷 " + e.Name + " 到 eu_bh2012_encoder_files Queue 中等待");
                } else {
                    EU2012.Common.MSMQ.EncoderFilesObj efobj = new EU2012.Common.MSMQ.EncoderFilesObj();
                    efobj.fileName = e.Name.Substring(0, e.Name.Length - 3);
                    if (BHConfig.GetCommonSet("encoder_console") != "")
                        EU2012.Common.MSMQ.BhQueue.SendEncoderFilesMessage(efobj, counter);
                    else
                        EU2012.Common.MSMQ.BhQueue.SendEncoderFilesMessage(efobj);
                    EU2012.Common.Trace.DebugWrite("fileSystemWatcher", "傳送 " + e.Name + " 到 eu_bh2012_encoder_files Queue 中等待");
                }
            } catch (Exception ee) {
            }
        }
    }
}
