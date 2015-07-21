﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TSO.Vitaboy;
using tso.world.model;

namespace tso.world.components
{
    public class AvatarComponent : WorldComponent
    {
        public Avatar Avatar;

        public override Vector3 GetSLOTPosition(int slot)
        {
            var handpos = Avatar.Skeleton.GetBone("R_FINGER0").AbsolutePosition / 3.0f;
            return Vector3.Transform(new Vector3(handpos.X, handpos.Z, handpos.Y), Matrix.CreateRotationZ((float)(RadianDirection+Math.PI))) + this.Position - new Vector3(0.5f, 0.5f, 0f); //todo, rotate relative to avatar
        }

        public double RadianDirection;
        public Vector2 LastScreenPos; //todo: move this and slots into an abstract class that contains avatars and objects
        public int LastZoomLevel;

        private Direction _Direction;
        public override Direction Direction
        {
            get
            {
                return _Direction;
            }
            set
            {
                _Direction = value;
                switch (value)
                {
                    case Direction.NORTH:
                        RadianDirection = 0;
                        break;
                    case Direction.EAST:
                        RadianDirection = Math.PI*0.5;
                        break;
                    case Direction.SOUTH:
                        RadianDirection = Math.PI;
                        break;
                    case Direction.WEST:
                        RadianDirection = Math.PI*1.5;
                        break;
                }
            }
        }

        public override Vector3 Position
        {
            get
            {
                if (Container == null) return _Position;
                else return Container.GetSLOTPosition(ContainerSlot) + new Vector3(0.5f, 0.5f, -1.4f); //apply offset to snap character into slot
            }
            set
            {
                _Position = value;
                OnPositionChanged();
                _WorldDirty = true;
            }
        }

        public override float PreferredDrawOrder
        {
            get { return 5000.0f;  }
        }

        public override void Initialize(GraphicsDevice device, WorldState world)
        {
            base.Initialize(device, world);
            Avatar.StoreOnGPU(device);
        }

        public override void Draw(GraphicsDevice device, WorldState world)
        {
            LastScreenPos = world.WorldSpace.GetScreenFromTile(Position) + world.WorldSpace.GetScreenOffset();
            LastZoomLevel = (int)world.Zoom;
            /*if (Container != null)
            {
                Direction = Container.Direction;
                _WorldDirty = true;
            }*/
            if (Avatar != null){
                world._3D.DrawMesh(Matrix.CreateRotationY((float)(Math.PI-RadianDirection))*this.World, Avatar); //negated so avatars spin clockwise
            }
        }
    }
}
