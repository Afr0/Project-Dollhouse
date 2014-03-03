﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Vitaboy;
using TSO.Content.framework;

namespace TSO.Content.codecs
{
    /// <summary>
    /// Codec for skeletons (*.skel).
    /// </summary>
    public class SkeletonCodec : IContentCodec<Skeleton> 
    {

        #region IContentCodec<Skeleton> Members

        public Skeleton Decode(System.IO.Stream stream)
        {
            var result = new Skeleton();
            result.Read(stream);
            return result;
        }

        #endregion
    }
}
