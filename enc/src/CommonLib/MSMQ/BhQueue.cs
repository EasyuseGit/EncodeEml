using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EU2012.Common.MSMQ
{
    public class BhQueue
    {



        public static void SendEncoderFilesMessage(EncoderFilesObj efObject)
        {
            System.Messaging.MessageQueue queue = new System.Messaging.MessageQueue("FormatName:DIRECT=OS:" + BHConfig.MsmqVirualMachine + "\\private$\\eu_bh2012_encoder_files");
            System.Messaging.Message message = new System.Messaging.Message(efObject);
            message.Recoverable = true;
            queue.Send(message);
        }
        public static void SendEncoderFilesMessageHigh(EncoderFilesObj efObject)
        {
            System.Messaging.MessageQueue queue = new System.Messaging.MessageQueue("FormatName:DIRECT=OS:" + BHConfig.MsmqVirualMachine + "\\private$\\eu_bh2012_encoder_files");
            System.Messaging.Message message = new System.Messaging.Message(efObject);
            message.Recoverable = true;
            message.Priority = System.Messaging.MessagePriority.High;
            queue.Send(message);
        }

        public static void SendEncoderFilesMessage(EncoderFilesObj efObject, int ta)
        {
            System.Messaging.MessageQueue queue = new System.Messaging.MessageQueue("FormatName:DIRECT=OS:" + BHConfig.MsmqVirualMachine + "\\private$\\eu_bh2012_encoder_files" + ta.ToString());
            System.Messaging.Message message = new System.Messaging.Message(efObject);
            message.Recoverable = true;
            queue.Send(message);
        }
        public static void SendEncoderFilesMessageHigh(EncoderFilesObj efObject, int ta)
        {
            System.Messaging.MessageQueue queue = new System.Messaging.MessageQueue("FormatName:DIRECT=OS:" + BHConfig.MsmqVirualMachine + "\\private$\\eu_bh2012_encoder_files" + ta.ToString());
            System.Messaging.Message message = new System.Messaging.Message(efObject);
            message.Recoverable = true;
            message.Priority = System.Messaging.MessagePriority.High;
            queue.Send(message);
        }


    }
}