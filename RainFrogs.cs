using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace RainFrogs
{
    public class RainFrog : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        bool active = true;
        GameState state;
        Playing playing;
        Initializer init;

        public RainFrog()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1350; //26
            graphics.PreferredBackBufferHeight = 768; //16
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            init = new Initializer(this);
            state = init;
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            active = false;
            base.OnDeactivated(sender, args);
        }
        protected override void OnActivated(object sender, EventArgs args)
        {
            active = true;
            base.OnActivated(sender, args);
        }

        protected override void UnloadContent()
        {
        }

        public void changeGameState(string newState)
        {
            switch (newState)
            {
                case "playing":
                    {
                        playing = new Playing(spriteBatch, this);
                        state.leaving();
                        state = playing;
                        state.entering();
                        break;
                    }
                case "initialize":
                    {
                        init = new Initializer(this);
                        state.leaving();
                        state = init;
                        state.entering();
                        break;
                    }
            }

        }
        protected override void Update(GameTime gameTime)
        {
            if (active)
            {
                state.update(gameTime);
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Playing.getColor("Purple"));

            state.draw(spriteBatch);

            base.Draw(gameTime);
        }
    }
}