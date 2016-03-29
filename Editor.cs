using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace RainFrogs
{
    class Editor : GameState
    {
        Playing playing;
        List<StaticObject> objects = Scene.scenery.Values.ToList();
        int objectIndex = 0;
        int switchTimer = 0;

        int scrollDivisor = 120;
        int placeTimer = 0;
        int escapeTimer = 0;
        int undoTimer = 0;

        Stack<Tuple<Scenery, Chunk>> undoStack = new Stack<Tuple<Scenery, Chunk>>();
        Stack<Tuple<Scenery, Chunk>> redoStack = new Stack<Tuple<Scenery, Chunk>>();

        bool sceneFalseTileTrue = false;


        public Editor(Playing playing)
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
            return "Editor";
        }

        public string getState()
        {
            return "Editor";
        }

        public void update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            if (switchTimer < 320) switchTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (escapeTimer < 320) escapeTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (placeTimer < 600) placeTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (undoTimer < 600) undoTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (state.IsKeyDown(Keys.A)) { playing.currentCorner[0] -= 10; }
            else if (state.IsKeyDown(Keys.W)) { playing.currentCorner[1] -= 10; }
            else if (state.IsKeyDown(Keys.D)) { playing.currentCorner[0] += 10; }
            else if (state.IsKeyDown(Keys.S)) { playing.currentCorner[1] += 10; }

            if (switchTimer >= 320 && state.IsKeyDown(Keys.Tab)) { sceneFalseTileTrue = !sceneFalseTileTrue; objectIndex = 0; switchTimer = 0; }
            if (undoTimer >= 600 && state.IsKeyDown(Keys.Z) && undoStack.Count > 0) { undoTimer = 0; undoStack.Peek().Item2.scenery.Remove(undoStack.Peek().Item1); redoStack.Push(undoStack.Pop()); }
            else if (undoTimer >= 600 && state.IsKeyDown(Keys.Y) && redoStack.Count > 0) { undoTimer = 0; redoStack.Peek().Item2.addScenery(redoStack.Peek().Item1); undoStack.Push(redoStack.Pop()); }
            if (escapeTimer >= 320 && state.IsKeyDown(Keys.Escape)) { playing.changeState("DevConsole"); escapeTimer = 0; }


            if (!sceneFalseTileTrue)
            {
                //!!!if X is held, use absolute coords
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && placeTimer >= 600)
                {
                    if (state.IsKeyDown(Keys.X)) { playing.scene.getChunkAt(Mouse.GetState().X + playing.currentCorner[0], Mouse.GetState().Y + playing.currentCorner[1]).addScenery(new Scenery(objects[objectIndex], new Vector2((Mouse.GetState().X + playing.currentCorner[0]), (Mouse.GetState().Y + playing.currentCorner[1])), playing, false)); }
                    else
                    {
                        Chunk chunk = playing.scene.getChunkAt(Mouse.GetState().X + playing.currentCorner[0], Mouse.GetState().Y + playing.currentCorner[1]);
                        Scenery prop = new Scenery(objects[objectIndex], new int[] { (Mouse.GetState().X + playing.currentCorner[0]) / 48, (Mouse.GetState().Y + playing.currentCorner[1]) / 48 }, playing, false);
                        chunk.addScenery(prop);
                        undoStack.Push(new Tuple<Scenery, Chunk>(prop, chunk));
                        if (redoStack.Count > 0) { redoStack.Clear(); }
                    }
                    placeTimer = 0;
                }
                objectIndex = Math.Abs(Mouse.GetState().ScrollWheelValue / scrollDivisor) % objects.Count;
                //Debug.Print("{0}",Mouse.GetState().ScrollWheelValue);
                if (objectIndex <= 0) { objectIndex = 0; }
            }
            else
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && placeTimer >= 600)
                {
                    Chunk chunk = playing.scene.getChunkAt(Mouse.GetState().X + playing.currentCorner[0], Mouse.GetState().Y + playing.currentCorner[1]);
                    chunk.getTile(new int[] { Mouse.GetState().X + playing.currentCorner[0], Mouse.GetState().Y + playing.currentCorner[1] }).setNum(objectIndex);
                }
                
                objectIndex = Math.Abs(Mouse.GetState().ScrollWheelValue / scrollDivisor);
            }

            foreach (Scenery scenery in playing.scenery) { scenery.update(gameTime); }
            foreach (DroppedItem item in playing.droppedItems)
            {
                item.update(gameTime);
                //check if item bounding box
            }
            foreach (Particle particle in playing.particlesAbove) { particle.update(gameTime); }
            foreach (Particle particle in playing.particlesBelow) { particle.update(gameTime); }
            foreach (FarmTile farmTile in playing.farmTiles) { farmTile.update(gameTime); } //encapsulates crop update
        }

        public void draw(SpriteBatch spriteBatch)
        {
            MouseState mouse = Mouse.GetState();
            if (!sceneFalseTileTrue)
            {
                spriteBatch.Draw(Playing.scenerySheet, new Vector2((mouse.X/48)*48, (mouse.Y/48)*48), objects[objectIndex].sprite, Color.White, 0f, new Vector2(0f, 0f), new Vector2(3, 3), SpriteEffects.None, 0f);
            }
            else spriteBatch.Draw(Playing.tileSheet, mouse.Position.ToVector2(), new Rectangle((objectIndex % 8) * 16, (objectIndex / 8) * 16, 16, 16), Color.White, 0f, new Vector2(0f, 0f), new Vector2(3, 3), SpriteEffects.None, 0f);
        }

        public void entering()
        {
            playing.ingame.IsMouseVisible = true;
        }

        public void leaving()
        {
            playing.ingame.IsMouseVisible = false;
        }
    }
}
