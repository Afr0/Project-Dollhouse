﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TSO.Files.utils;

namespace TSO.Files.formats.iff.chunks
{
    /// <summary>
    /// This chunk type holds the name of a sound event that this object uses.
    /// </summary>
    public class FWAV : IffChunk
    {
        public string Name;

        /// <summary>
        /// Reads a FWAV chunk from a stream.
        /// </summary>
        /// <param name="iff">An Iff instance.</param>
        /// <param name="stream">A Stream object holding a FWAV chunk.</param>
        public override void Read(Iff iff, Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN))
            {
                Name = io.ReadNullTerminatedString();
            }
        }
    }
}
