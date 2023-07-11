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

namespace ConsoleEncoderService {
    class Program {
        static void Main(string[] args) {
            int queue_index = 0;
            if (args.Length < 1){
                Console.WriteLine("Need args[0]");
                return;
            }
            queue_index = int.Parse(args[0]);
            EncoderLib.BufferQueueListen bq;
            if (args.Length == 2)
            {
                bq = new EncoderLib.BufferQueueListen(queue_index, false);
            }
            else
            {
                bq = new EncoderLib.BufferQueueListen(queue_index);
            }
            Console.WriteLine("OK");
            Console.Read();
        }
    }
}
