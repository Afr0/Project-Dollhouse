﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SimsLib.IFF
{
    /// <summary>
    /// Represents an image in a drawgroup.
    /// An image is used to map together
    /// multiple drawgroup sprites.
    /// </summary>
    public class DrawGroupImg
    {
        private uint m_SpriteCount;
        private uint m_DirectionFlag;
        private uint m_Zoom;
        private List<DrawGroupSprite> m_Sprites = new List<DrawGroupSprite>();
        private Bitmap m_CompiledBitmap;

        public uint SpriteCount { get { return m_SpriteCount; } }
        public uint DirectionFlag { get { return m_DirectionFlag; } }
        public uint Zoom { get { return m_Zoom; } }
        public List<DrawGroupSprite> Sprites { get { return m_Sprites; } }
        public Bitmap CompiledBitmap { get { return m_CompiledBitmap; } }

        /// <summary>
        /// Creates a new drawgroup image chunk.
        /// </summary>
        /// <param name="SpriteCount">The number of sprites in this image.</param>
        /// <param name="DirectionFlag">Which direction are these sprites facing?</param>
        /// <param name="Zoom">The zoomlevel for this drawgroup image.</param>
        public DrawGroupImg(uint SpriteCount, uint DirectionFlag, uint Zoom)
        {
            m_SpriteCount = SpriteCount;
            m_DirectionFlag = DirectionFlag;
            m_Zoom = Zoom;
        }

        /// <summary>
        /// Compiles the list of sprites into a tile bitmap
        /// </summary>
        public void CompileSprites()
        {
            // TODO: Render transparency and z-buffer channels
            // TODO: Mirrored sprites are not aligned correctly

            m_CompiledBitmap = new Bitmap(136, 384);
            Graphics gfx = Graphics.FromImage(m_CompiledBitmap);

            foreach (DrawGroupSprite Sprite in Sprites)
            {
                int xOffset = m_CompiledBitmap.Width / 2 + Sprite.SpriteOffset.X;
                int yOffset = m_CompiledBitmap.Height / 2 + Sprite.SpriteOffset.Y;

                gfx.DrawImageUnscaled(Sprite.Bitmap, Sprite.Sprite.XLocation, Sprite.Sprite.YLocation);
            }
            gfx.Dispose();
        }
    }
}
