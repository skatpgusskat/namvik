﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using namvik.Tile;
using ContactPoint = namvik.Contact.ContactPoint;

namespace namvik
{
    class Monkey: GameObject
    {
        private float _velocityX;
        private readonly float _maximumVelocityX = 360f.ToMeter();
        private readonly float _accelerationX = 1800f.ToMeter();

        public static Monkey SpawnMonkey(ContentManager content, Vector2 position)
        {
            var monkey = new Monkey
            {
                Position = position,
            };
            monkey.Initialize(content);

            return monkey;
        }
        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            Texture = content.Load<Texture2D>("sprite/monkey");
            MakeBox2DBoxWithTexture();

            var polygonDef = new PolygonDef();

            var hx = (Texture.Width / 2f).ToMeter() - 2f.ToMeter();
            var hy = (3.5f).ToMeter();
            var center = new Vec2((Texture.Width / 2f).ToMeter(), 0);
            polygonDef.SetAsBox(hx, hy, center, angle: 0);

            polygonDef.Density = 1f;
            polygonDef.Friction = 0f;
            polygonDef.Restitution = 0f;
            polygonDef.IsSensor = true;
            polygonDef.Filter.GroupIndex = ContactGroupIndex.Monster;

            Body.CreateShape(polygonDef);
            PolygonDefs.Add(polygonDef);
        }

        public override void OnCollisionBefore(ContactPoint point)
        {
            base.OnCollisionBefore(point);

            if (
                point.OppositeShape.GetBody().GetUserData() is TileObject tileObject
                && tileObject.TileGroupName == TileGroupName.Collision
                && point.Normal == new Vec2( IsSeeLeft ? -1 : 1, 0)
            )
            {
                IsSeeLeft = !IsSeeLeft;
                _velocityX = 0;
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            _velocityX += dt * _accelerationX;
            if (_velocityX > _maximumVelocityX)
            {
                _velocityX = _maximumVelocityX;
            }


            Body.SetVelocityX((IsSeeLeft  ? (-1) : (1)) *_velocityX);
        }
    }
}
