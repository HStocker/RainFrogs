using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using RainFrogs;

namespace RainFrogs
{
    class Playing : GameState
    {

        public Unpaused unpaused;
        DevConsole devConsole;
        public RainFrog ingame;
        Inventory inventory;
        UI ui;
        GameState state;
        public Scene scene;
        public static SpriteFont output;
        SpriteBatch spriteBatch;
        ingameMenu menu;

        public static Texture2D tileSheet;
        public static Texture2D UIMisc;
        public static Texture2D frogSheet;
        public static Texture2D itemSheet;
        public static Texture2D scenerySheet;
        public static Texture2D particleSheet;
        public static Texture2D cropSheet;
        public static Texture2D buildingSheet;

        public int[] currentCorner = { 2289, 1050 };
        public int tileWidth = 48;
        public int screenWidth = 1350;
        public int screenHeight = 768;
        //int[] screenDim = { 50, 20 };

        public Rectangle messageBoxSprite = new Rectangle(530, 0, 194, 40);
        public int messageBoxAlpha = 255;
        public bool messageBoxDisplayed = false;
        public int messageBoxTimer = 0;
        public string message;

        public Player player;
        public List<NPC> npcs = new List<NPC>();
        public List<DroppedItem> droppedItems = new List<DroppedItem>();
        public List<Particle> particlesAbove = new List<Particle>();
        public List<Particle> particlesBelow = new List<Particle>();
        public List<Projectile> projectiles = new List<Projectile>();
        public List<Scenery> scenery = new List<Scenery>();
        public List<Building> buildings = new List<Building>();
        public List<FarmTile> farmTiles = new List<FarmTile>();
        public List<Crop> crops = new List<Crop>();
        public List<ActivityRegion> activityRegions = new List<ActivityRegion>();

        public List<Drawable> visible = new List<Drawable>();

        public bool collisionMarkers = false;
        public bool pause = false;
        public bool noClip = false;
        public bool drawActivity = false;
        public static bool drawOccupied = false;

        public int itemSpacer = 15;
        public Random rand = new Random(Guid.NewGuid().GetHashCode());
        public string stateText = "Unpaused";

        public Playing(SpriteBatch spriteBatch, RainFrog ingame)
        {
            this.ingame = ingame;
            output = ingame.Content.Load<SpriteFont>("Output18pt");
            this.spriteBatch = spriteBatch;
            menu = new ingameMenu(ingame, output, this);
            inventory = new Inventory(this);
            scene = new Scene(this);
            player = new Player(new int[] { 696, 400 }, this, inventory);
            ui = new UI(player);
            scene.build();

            tileSheet = ingame.Content.Load<Texture2D>("TileSheet");
            UIMisc = ingame.Content.Load<Texture2D>("UIMisc");
            frogSheet = ingame.Content.Load<Texture2D>("FrogSpritesheet");
            itemSheet = ingame.Content.Load<Texture2D>("Items");
            scenerySheet = ingame.Content.Load<Texture2D>("Scenery");
            particleSheet = ingame.Content.Load<Texture2D>("particles");
            cropSheet = ingame.Content.Load<Texture2D>("Crops");
            buildingSheet = ingame.Content.Load<Texture2D>("Buildings");

            buildings.Add(new Building(scene.getBuilding("Merchant"), new int[] { 58, 25 }, this));
            activityRegions.Add(new Doorway(new Rectangle(2970, 1470, 50, 2), "N", "S", buildings[0], this));
            //!!!put doors activity regions inside buildings=

            unpaused = new Unpaused(this, scene, player);
            devConsole = new DevConsole(this, menu);

            state = unpaused;

        }
        public void update(GameTime gameTime)
        {
            this.scenery.Clear();
            this.droppedItems.Clear();
            this.farmTiles.Clear();
            this.crops.Clear();

            this.retrieveChunkData();

            particlesAbove.RemoveAll(a => a.toBeDeleted);
            particlesBelow.RemoveAll(a => a.toBeDeleted);
            droppedItems.RemoveAll(a => a.toBeDeleted);
            crops.RemoveAll(a => a.toBeDeleted);
            scenery.RemoveAll(a => a.toBeDeleted);

            if (this.messageBoxDisplayed)
            {
                this.messageBoxTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (messageBoxTimer > MathHelper.Clamp(message.Length * 100, 0, 2000))
                {
                    this.messageBoxAlpha -= 20;
                    if (this.messageBoxAlpha <= 0)
                    {
                        this.messageBoxDisplayed = false;
                        this.messageBoxTimer = 0;
                        this.messageBoxAlpha = 255;
                        this.message = null;
                    }
                }
            }

            if (!pause) visible.Clear();
            state.update(gameTime);
            scene.update(gameTime, player.coords);
            visible.Sort();
            player.collisionUpdate(gameTime);

            foreach (DroppedItem item in droppedItems) { if (item.inair) { item.travel(); } }

            if (!pause) ui.updateTime(gameTime, this);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

            drawTiles();
            foreach (Building building in buildings)
            {
                spriteBatch.Draw(buildingSheet, new Vector2(building.coords[0] + (building.type.floorOffset[0] * 3) - currentCorner[0], building.coords[1] + (building.type.floorOffset[1] * 3) - currentCorner[1]), building.type.floorSprite, Color.White, 0f, new Vector2(0,0), new Vector2(3, 3), SpriteEffects.None, 0f);
            }
            foreach (Particle particle in particlesBelow)
            {
                spriteBatch.Draw(particleSheet, new Vector2(particle.coords[0] + particle.pixMod[0] - currentCorner[0], particle.coords[1] + particle.pixMod[1] - currentCorner[1]), particle.getSpriteRectangle(), new Color(255, 255, 255, (byte)MathHelper.Clamp(particle.alpha, 0, 255)), particle.rotation, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
            }
            foreach (Drawable item in visible) { item.draw(item); }
            //spriteBatch.End();
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

            foreach (Particle particle in particlesAbove)
            {
                //Debug.Print(Convert.ToString(particle.alpha));
                spriteBatch.Draw(particleSheet, new Vector2(particle.coords[0] + particle.pixMod[0] - currentCorner[0], particle.coords[1] + particle.pixMod[1] - currentCorner[1]), particle.getSpriteRectangle(), new Color(255, 255, 255, (byte)MathHelper.Clamp(particle.alpha, 0, 255)), particle.rotation, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
            }
            //draw player
            foreach (Building building in buildings)
            {
                spriteBatch.Draw(buildingSheet, new Vector2(building.coords[0] - currentCorner[0], building.coords[1] - currentCorner[1]), building.type.roofSprite, new Color(255, 255, 255, (byte)MathHelper.Clamp(building.roofAlpha, 0, 255)), 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
            }

            if (this.messageBoxDisplayed)
            {
                spriteBatch.Draw(UIMisc, new Vector2(282, 640), this.messageBoxSprite, new Color(255, 255, 255, this.messageBoxAlpha), 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                spriteBatch.DrawString(output, this.message, new Vector2(300, 650), Playing.getColor("Black"));
            }

            drawUI();

            //draw state information after
            //draw calls specific to play states
            //such as dialog UI and pause screen
            state.draw(spriteBatch);

            //DEBUG draw calls
            spriteBatch.DrawString(output, Convert.ToString("Position: " + (player.coords[0] + currentCorner[0]) + "," + (player.coords[1] + currentCorner[1])), new Vector2(15, 15), Playing.getColor("Black"));
            spriteBatch.DrawString(output, Convert.ToString("Entities: " + (droppedItems.Count + farmTiles.Count + particlesAbove.Count + particlesBelow.Count + crops.Count) + " Statics: " + scenery.Count + " Floating: " + scene.floatingObjects.Count), new Vector2(15, 35), Playing.getColor("Black"));
            if (player.spaceHeld)
            {
                //spriteBatch.Draw(buildingSheet, new Rectangle(player.getInteractRectangle().X - currentCorner[0], player.getInteractRectangle().Y - currentCorner[1], player.getInteractRectangle().Width, player.getInteractRectangle().Height), new Rectangle(0, 0, player.getInteractRectangle().Width, player.getInteractRectangle().Height), Color.Black);
                spriteBatch.DrawString(output, Convert.ToString("Space held: " + player.chargeTimer), new Vector2(15, 55), Playing.getColor("Black"));
            }
            if (player.QHeld) { spriteBatch.DrawString(output, Convert.ToString("Q held: " + player.chargeTimer), new Vector2(15, 75), Playing.getColor("Black")); }
            if (player.EHeld) { spriteBatch.DrawString(output, Convert.ToString("E held: " + player.chargeTimer), new Vector2(15, 95), Playing.getColor("Black")); }
            if (player.collidedWith != null) { spriteBatch.DrawString(output, player.collidedWith, new Vector2(15, 135), Playing.getColor("Black")); }
            if (ingame.IsMouseVisible) { spriteBatch.DrawString(output, Convert.ToString(Mouse.GetState().X + "," + Mouse.GetState().Y), new Vector2(15, 155), Playing.getColor("Black")); }
            if (ingame.IsMouseVisible) { spriteBatch.DrawString(output, Convert.ToString(Mouse.GetState().X + currentCorner[0] + "," + (Mouse.GetState().Y + currentCorner[1])), new Vector2(15, 175), Playing.getColor("Black")); }
            if (drawActivity) { foreach (ActivityRegion region in this.activityRegions) { spriteBatch.Draw(tileSheet, new Rectangle(region.location.X - currentCorner[0], region.location.Y - currentCorner[1], region.location.Width, region.location.Height), new Rectangle(0, 0, region.location.Width, region.location.Height), new Color(0, 0, 0, 255)); } }
            spriteBatch.DrawString(output, Convert.ToString("Region: " + scene.currentRegion.tag + scene.currentRegion.metaNum), new Vector2(15, 235), Playing.getColor("Red"));
            spriteBatch.DrawString(output, Convert.ToString("Chunk: " + ((player.coords[0] + currentCorner[0]) / 48 / 30) + "," + ((player.coords[1] + currentCorner[1]) / 48 / 18)), new Vector2(15, 255), Playing.getColor("Red"));
            for (int i = 0; i < scene.liveChunks.Count; i++)
            { spriteBatch.DrawString(output, Convert.ToString(scene.liveChunks[i].coords[0] + "," + scene.liveChunks[i].coords[1] + " " + Chunk.chunkDirection(scene.currentChunk, scene.liveChunks[i])), new Vector2(15, 275 + i * 30), Playing.getColor("Red")); }
            spriteBatch.End();
        }
        public void drawPlayer(Drawable prop)
        {
            Player player = (Player)prop;
            if (player.flipped) { spriteBatch.Draw(frogSheet, new Vector2(player.coords[0], player.coords[1]), player.getSpriteRectangle(), Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.FlipHorizontally, 0f); }
            else { spriteBatch.Draw(frogSheet, new Vector2(player.coords[0], player.coords[1]), player.getSpriteRectangle(), Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }
            if (player.swinging) { spriteBatch.Draw(tileSheet, player.toolVector, player.toolRectangle, Color.Black, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }

            //!!!for the item traveling from ground to player held - not necessary
            //if (player.holdingItem || player.pickingUp) { spriteBatch.Draw(itemSheet, new Vector2(player.itemCoords[0], player.itemCoords[1]), scene.getItem(inventory.heldItem.getItem()).spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }

            if (player.spaceHeld) { spriteBatch.Draw(tileSheet, player.pickupVector, player.pickupRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }
        }
        public void drawWall(Drawable prop) { spriteBatch.Draw(buildingSheet, prop.getCoords(currentCorner), prop.getSpriteRectangle(), new Color(255, 255, 255, prop.alpha), prop.rotation, new Vector2(0, 0), new Vector2(prop.XSize, prop.YSize), SpriteEffects.None, 0f); }

        public void drawDroppedItem(Drawable item)
        {
            //draw attributes on bottom
            spriteBatch.Draw(itemSheet, item.getCoords(currentCorner), scene.getItem(item.getTag()).spriteRectangle, 
                new Color(255, 255, 255, this.state.getAlpha(item)), item.rotation, new Vector2(8, 8), 
                new Vector2(item.XSize, item.YSize), SpriteEffects.None, 0f);
            //draw attributes on top
        }

        public void drawScenery(Drawable prop) { spriteBatch.Draw(scenerySheet, prop.getCoords(currentCorner), prop.getSpriteRectangle(), 
            new Color(255, 255, 255, this.state.getAlpha(prop)), prop.rotation, new Vector2(0, 0), new Vector2(prop.XSize, prop.YSize), 
            SpriteEffects.None, 0f); }

        public void drawFarmTile(Drawable prop) { spriteBatch.Draw(UIMisc, prop.getCoords(currentCorner), prop.getSpriteRectangle(), 
            new Color(255, 255, 255, this.state.getAlpha(prop)), prop.rotation, new Vector2(0, 0), new Vector2(prop.XSize, prop.YSize), 
            SpriteEffects.None, 0f); }

        public void drawCrop(Drawable prop) { spriteBatch.Draw(cropSheet, prop.getCoords(currentCorner), prop.getSpriteRectangle(), 
            new Color(255, 255, 255, this.state.getAlpha(prop)), prop.rotation, new Vector2(0, 0), new Vector2(prop.XSize, prop.YSize), 
            SpriteEffects.None, 0f); }

        public void drawChest(Drawable prop) { spriteBatch.Draw(scenerySheet, prop.getCoords(currentCorner), prop.getSpriteRectangle(), 
            new Color(255, 255, 255, this.state.getAlpha(prop)), prop.rotation, new Vector2(0, 0), new Vector2(prop.XSize, prop.YSize), 
            SpriteEffects.None, 0f); }

        public void drawUI()
        {
            spriteBatch.Draw(UIMisc, new Vector2(1035, 0), ui.spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);

            for (int i = 0; i < 3; i++)
            {
                string tool;
                if (inventory.hasItem(out tool, i, 0))
                { spriteBatch.Draw(itemSheet, ui.getSlotQ(i, 0, 0), scene.getItem(tool).spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }

                string item;
                if (inventory.hasItem(out item, i + 3, 0))
                { spriteBatch.Draw(itemSheet, ui.getSlotE(i, 0, 0), scene.getItem(item).spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }
            }

            //draw equipped tool and held item
            if (player.hasEquippedTool()) { spriteBatch.Draw(itemSheet, ui.getToolSlot(), inventory.equippedTool.getItem().spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }
            if (player.hasHeldItem())
            {
                spriteBatch.Draw(itemSheet, ui.getItemSlot(), inventory.heldItem.getItem().spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                spriteBatch.Draw(itemSheet, new Vector2(player.coords[0], player.coords[1]), inventory.heldItem.getItem().spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
            }

            //draw clock
            spriteBatch.Draw(UIMisc, new Vector2(1074, 36), ui.clock.minuteRectangle(), Color.White, ui.clock.minuteRotation, new Vector2(9f, 9f), new Vector2(3, 3), SpriteEffects.None, 0f);
            spriteBatch.Draw(UIMisc, new Vector2(1074, 36), ui.clock.hourRectangle(), Color.White, ui.clock.hourRotation, new Vector2(9f, 9f), new Vector2(3, 3), SpriteEffects.None, 0f);

            spriteBatch.DrawString(output, ui.clock.getDay(), new Vector2(1121, 16), Playing.getColor("Black"));
            spriteBatch.DrawString(output, ": " + ui.clock.getMonth(), new Vector2(1164, 16), Playing.getColor("Black"));

            //call UI and draw day and month using spriteFont
            //ui.getMonth()  ui.getDay()
            //spriteBatch.DrawString(monthOutput, new Vector(X,Y), ui.getDay() + " : " + ui.getMonth(), etc.);

        }

        public void showMessage(string message)
        {
            if (!this.messageBoxDisplayed)
            {
                this.message = message;
                this.messageBoxDisplayed = true;
            }
        }

        public void retrieveChunkData()
        {
            foreach (Chunk chunk in scene.liveChunks)
            {
                scenery.AddRange(chunk.scenery);
                droppedItems.AddRange(chunk.droppedItems);
                farmTiles.AddRange(chunk.farmTiles);
                crops.AddRange(chunk.crops);
            }
        }

        public void newDay()
        {
            ui.clock.reset();
            this.showMessage(ui.clock.getDay() + " of " + ui.clock.getMonth());
            foreach (Chunk chunk in scene.liveChunks) { chunk.newDay(this); }
        }
        public string getDate() { return ui.clock.getDate(); }

        public void drawTiles()
        {
            foreach (Chunk chunk in scene.liveChunks)
            {
                chunk.Draw(spriteBatch, this.currentCorner, tileSheet, state.getTileAlpha());
            }
        }

        public bool onScreen(Drawable drawable)
        {
            if (drawable.coords[0] < currentCorner[0] - (drawable.getSpriteRectangle().Width * 3) || drawable.coords[0] > currentCorner[0] + (drawable.getSpriteRectangle().Width * 3) + screenWidth) { return false; }
            if (drawable.coords[1] < currentCorner[1] - (drawable.getSpriteRectangle().Height * 3) || drawable.coords[1] > currentCorner[1] + (drawable.getSpriteRectangle().Height * 3) + screenHeight) { return false; }
            return true;
        }

        public static Color getRandomColor(out string colorString)
        {
            List<string> colors = new List<string>() { null, "Yellow", "Brown", "Gray", "Green", "LightBlue", "Salmon", "LightGreen", "Black", "DarkBlue", "White", "Red", "Blue", "LightGray", "Sienna", "DarkGray", "Purple" };
            int[] weights = { 0, 120, 130, 140, 150, 160, 170, 175, 180, 185, 190, 193, 195, 197, 198, 199, 200 };
            int weight = Constructor.rand.Next(0, 201);
            colorString = colors.FindLast(a => weights[colors.IndexOf(a)] <= weight);
            Debug.Print("{0},{1}", weight,colorString);
            return Playing.getColor(colorString);
        }

        public static Color getColor(string color)
        {
            switch (color)
            {

                case "Black": return new Color(20, 12, 28);
                case "DarkBlue": return new Color(48, 52, 109);
                case "Green": return new Color(52, 101, 36);
                case "LightGreen": return new Color(109, 170, 44);
                case "White": return new Color(222, 218, 214);
                case "Red": return new Color(208, 70, 72);
                case "Blue": return new Color(89, 125, 206);
                case "Yellow": return new Color(218, 212, 94);
                case "Brown": return new Color(133, 76, 48);
                case "Salmon": return new Color(210, 170, 153);
                case "LightGray": return new Color(133, 149, 151);
                case "Gray": return new Color(117, 113, 97);
                case "Sienna": return new Color(210, 125, 44);
                case "LightBlue": return new Color(109, 194, 202);
                case "DarkGray": return new Color(78, 74, 78);
                case "Purple": return new Color(68, 36, 52);
                default: return new Color(0,0,0);
            }
        }
        public void toggleNoClip()
        {
            this.noClip = !noClip;
        }

        public string getTag() { return "playing"; }

        public void changeState(string inState)
        {
            this.state.leaving();
            switch (inState)
            {
                case "DevConsole": { this.state = devConsole; this.stateText = "DevConsole"; this.pause = true; break; }
                case "Unpaused": { this.state = unpaused; this.stateText = "Unpaused"; this.pause = false; break; }
                case "InventoryState": { this.state = new InventoryState(this, inventory, UIMisc, itemSheet, output); this.stateText = "InventoryState"; this.pause = true; break; }
                case "ChestState": { this.state = new ChestState(this, player.getChest(), inventory, UIMisc, itemSheet, output); this.pause = true; break; }
                case "BookMinigame": { this.state = new BookMinigame(this); this.pause = true; break; }
                case "Editor": { this.state = new Editor(this); this.pause = false; break; }
            }
            this.state.entering();
        }
        public string getState()
        {
            return state.getState();
        }

        public void entering()
        {
            //load logic
        }

        public void leaving()
        {
            //save logic
        }

        public int getTileAlpha()
        {
            throw new NotImplementedException();
        }


        public int getAlpha(Drawable prop)
        {
            throw new NotImplementedException();
        }
    }
    interface GameState
    {
        int getTileAlpha();
        int getAlpha(Drawable prop);
        string getTag();
        string getState();
        void update(GameTime gameTime);
        void draw(SpriteBatch spriteBatch);
        void entering();
        void leaving();
    }
}
///
/// RAINFROGS DESIGN TODO
///     implement scythe
///     use scythe to gather seeds
///     plant seeds
///     use water can to grow seeds
///     gather crops
///     receive crops + seeds
///     put crops in storage/sell
///     acquire dosh
///
///     consider assigning a tile to static objects
///     multiple tile for tile adjustment purposes
///     ie. rocks assigned to (3,3) which cannot be hoe'd and instead
///     the tile sends the static object a hit(Tool tool) call which
///     decides how it reacts to being hit
///     
///     Put time on a scale of an hour per day.
///     
///     test rotating item circle thing:
///     tilesheet : 129 X 60 - 35/35
///     
///     THINK CASTAWAY ON THE MOON
///     you are traveling through the wilderness and on the first day of the soggy month
///     you come across a cabin in the woods - Scrolling text pauses for input
///     "Diary of ____________ [ENTER]"
///     once name is entered, scroll up and have more text appear
///     "The cold has passed and the soggy month has begun.  It is a great doubt I find myself taking shade under"
///     "I feel my past sitting in my stomach like swallowed stones.  It has been a year and a half since ________"
///     "The pitiful dredges of my rations won't last to the end of the month."