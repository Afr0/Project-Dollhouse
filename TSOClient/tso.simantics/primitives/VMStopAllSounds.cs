﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Simantics.engine;
using TSO.Files.utils;

namespace TSO.Simantics.primitives
{
    public class VMStopAllSounds : VMPrimitiveHandler
    {
        public override VMPrimitiveExitCode Execute(VMStackFrame context)
        {
            var operand = context.GetCurrentOperand<VMStopAllSoundsOperand>();

            var threads = context.Caller.SoundThreads;
            for (int i = 0; i < threads.Count; i++)
            {
                threads[i].Thread.RemoveOwner(context.Caller.ObjectID);
            }
            threads.Clear();

            return VMPrimitiveExitCode.GOTO_TRUE;
        }
    }

    public class VMStopAllSoundsOperand : VMPrimitiveOperand
    {

        #region VMPrimitiveOperand Members
        public void Read(byte[] bytes)
        {
            using (var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN))
            {
            }
        }
        #endregion
    }
}
