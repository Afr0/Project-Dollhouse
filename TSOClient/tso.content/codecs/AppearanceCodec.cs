﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Vitaboy;
using TSO.Content.framework;

namespace TSO.Content.codecs
{
    /// <summary>
    /// Codec for appearances (*.apr).
    /// </summary>
    public class AppearanceCodec : IContentCodec<Appearance>
    {
        #region IContentCodec<Appearance> Members

        public Appearance Decode(System.IO.Stream stream)
        {
            var result = new Appearance();
            result.Read(stream);
            return result;
        }

        #endregion
    }
}
