﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activities.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using namvik.Contact;
using namvik.GameObject;
using namvik.GameObject.Item;
using namvik.Tile;

namespace namvik
{
    public class Game1 : Game
    {
        public static GraphicsDevice Device;
        public static Camera2D Camera;
        public static GraphicsDeviceManager Graphics;
        public static Vector2 CenterOfScreen;
        private readonly GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;
        private Map _map;
        private Camera2D _camera;
        private Character _character;
        private FpsPrinter _fpsPrinter;
        private ContactManager _contactManager;

        public static SpriteFont DefaultFont;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Graphics = _graphics;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            Device = GraphicsDevice;
            

            var windowWidth = 1000;
            var windowHeight = 1000;
            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.ApplyChanges();

            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);


            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, windowWidth, windowHeight);
            _camera = new Camera2D(viewportAdapter);
            Camera = _camera;
            
            _map = new Map();
            _map.Initialize(Content, _spriteBatch);

            _character = new Character();

            _camera.LookAt(_character.Position);

            _fpsPrinter = new FpsPrinter();

            DefaultFont = Content.Load<SpriteFont>("font/defaultFont");


            KeyboardManager.OnKeyPress(Keys.F2, (key) =>
            {
                var characterSize = new Vector2(_character.Texture.Width, _character.Texture.Height);
                var monkeyPosition = _character.Position + characterSize + new Vector2(100, -100);
                var monkey = Monkey.SpawnMonkey(Content, monkeyPosition);
            });

            KeyboardManager.OnKeyPress(Keys.F3, (key) =>
            {
                var armor = new Armor(_character);
                _character.AddItem(armor);
            });

            KeyboardManager.OnKeyPress(Keys.F4, (key) =>
            {
                _character.OnHit();
            });

            var spawnSpots = _map.GetSpawnSpots();
            spawnSpots.ForEach(spawnSpot =>
            {
                var monkey = Monkey.SpawnMonkey(Content, spawnSpot);
            });

            _contactManager = new ContactManager();
            Map.World.SetContactListener(_contactManager);
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to Draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            TextureLoader.LoadTexture(typeof(Character), Content, "sprite/character");
            TextureLoader.LoadTexture(typeof(Monkey), Content, "sprite/monkey");
            TextureLoader.LoadTexture(typeof(Armor), Content, "sprite/armor");

            ContentLoader.LoadContents(Content);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);

            if (!(_character is null))
            {
                CenterOfScreen = _character.Position;
            }
            

            BaseGameObject.GameObjects.ForEach(gameObject =>
            {
                if (gameObject.IsDead)
                {
                    gameObject.Destroy();
                }
            });
            BaseGameObject.GameObjects.RemoveAll(gameObject => gameObject.IsDead);

            KeyboardManager.Update();

            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var physicsUpdateFrequency = 3;
            for (var i = 0; i < physicsUpdateFrequency; i += 1)
            {
                _map.Update(dt / (float)physicsUpdateFrequency);
            }

            BaseGameObject.GameObjects.ForEach(gameObject =>
            {
                gameObject.Update(dt);
            });

            FollowCameraTo(_character, dt);

            _fpsPrinter.Update(dt);
        }

        private void FollowCameraTo(Character target, float dt)
        {
            var interpolationFactor = Math.Min(10f * dt, 1);
            var size = _camera.BoundingRectangle.Size;
            var cameraCenter = _camera.Position + new Vector2(size.Width, size.Height) / 2;
            var nextCameraPosition = target.Position * interpolationFactor + cameraCenter * (1 - interpolationFactor);
            _camera.LookAt(nextCameraPosition);
        }

        protected override void Draw(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            BaseGameObject.GameObjects.ForEach(gameObject =>
            {
                gameObject.PreDraw(dt, Device, _camera);
            });

            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.Immediate, transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

            _map.Draw(_camera, _spriteBatch);

            BaseGameObject.GameObjects.ForEach(gameObject =>
            {
                gameObject.Draw(dt, _spriteBatch);
            });

            _spriteBatch.End();

            _spriteBatch.Begin();
            _fpsPrinter.Draw(dt, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
