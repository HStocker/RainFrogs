using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RainFrogs
{
    class BookMinigame : GameState
    {
        Rectangle spriteRectangle = new Rectangle(530, 40, 246, 153);
        Vector2 locationVector = new Vector2(258, 129);
        Rectangle gameLocation = new Rectangle(258, 129, 738, 459);
        Playing playing;
        MouseState mouse;
        Vector2 mousePosition;
        Vector2 oldMousePosition;
        bool movePanel = false;
        bool mouseHeld = false;

        Book[] topShelf;
        Book[] middleShelf;
        Book[] BottomShelf;

        class Book
        {
            //use book tag for text
            //text parser
        }


        public BookMinigame(Playing playing) 
        {
            this.playing = playing;
        }

        public int getTileAlpha()
        {
            return 255;
        }

        public int getAlpha(Drawable prop)
        {
            return 255;
        }

        public string getTag()
        {
            return "BookMinigame";
        }

        public string getState()
        {
            return "BookMinigame";
        }

        public void update(GameTime gameTime)
        {
            mouse = Mouse.GetState();
            mousePosition.X = mouse.X;
            mousePosition.Y = mouse.Y;
            if (!mouseHeld && new Rectangle((int)locationVector.X, (int)locationVector.Y, 738, 459).Contains(mousePosition) && !new Rectangle((int)locationVector.X + 12, (int)locationVector.Y + 12, 717, 438).Contains(mousePosition) && mouse.LeftButton == ButtonState.Pressed)
            {
                movePanel = true;
                locationVector.X += mousePosition.X - oldMousePosition.X;
                locationVector.Y += mousePosition.Y - oldMousePosition.Y;
            }
            else if (movePanel && mouseHeld)
            {
                locationVector.X += mousePosition.X - oldMousePosition.X;
                locationVector.Y += mousePosition.Y - oldMousePosition.Y;
            }
            else { movePanel = false; }
            oldMousePosition.X = mouse.X;
            oldMousePosition.Y = mouse.Y;

            mouseHeld = mouse.LeftButton == ButtonState.Pressed;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Playing.UIMisc, locationVector, spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
        }

        public void entering()
        {
            mouse = Mouse.GetState();
            mousePosition.X = mouse.X;
            mousePosition.Y = mouse.Y;
            playing.ingame.IsMouseVisible = true;
        }

        public void leaving()
        {
            playing.ingame.IsMouseVisible = false;
        }
    }
}
