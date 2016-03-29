using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace RainFrogs
{

    class Unpaused : GameState
    {
        KeyboardState state;
        Playing playing;
        Player player;
        public ProcessInput processInput;
        GameState gameState;

        int inventoryTimer = 320;
        int consoleTimer = 320;
        int swapTimer = 320;
        bool ceilingOff = false;
        public bool inside = false;

        ActivityRegion currentRegion;
        bool insideRegion = false;

        public Unpaused(Playing playing, Scene scene, Player player)
        {
            this.playing = playing;
            this.player = player;
            this.processInput = this.freeInput;
            this.gameState = new Outside(playing, 255);
        }
        public int getTileAlpha() { return gameState.getTileAlpha(); }
        public string getTag()
        {
            return "unpaused";
        }

        public void update(GameTime gameTime)
        {
            if (inventoryTimer < 320) inventoryTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (consoleTimer < 320) consoleTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (swapTimer < 320) swapTimer += gameTime.ElapsedGameTime.Milliseconds;

            gameState.update(gameTime);

            if (!insideRegion)
            {
                foreach (ActivityRegion region in playing.activityRegions)
                {
                    //Debug.Print(Convert.ToString(region.inside(player.getHitBox())));
                    if (region.inside(player.getAbsoluteHitBox())) { this.currentRegion = region; this.insideRegion = true; break; }
                }
            }
            else { if (currentRegion.update(player.getAbsoluteHitBox())) { this.currentRegion = null; this.insideRegion = false; } }

            foreach (Building building in playing.buildings)
            {
                building.update(gameTime);
            }
            //if player not swinging, charging, throwing, fishing, etc.
            processInput();
            player.update(gameTime);

        }

        public delegate void ProcessInput();

        public void freeInput()
        {
            state = Keyboard.GetState();

            if (!player.swinging && !player.throwing && !player.pickingUp && !player.puttingDown && !player.spaceHeld && !player.QHeld && !player.EHeld)
            {
                if (state.IsKeyDown(Keys.W)) { player.moveUp(); }
                else if (state.IsKeyDown(Keys.S)) { player.moveDown(); }
                else if (state.IsKeyDown(Keys.D)) { player.moveRight(); }
                else if (state.IsKeyDown(Keys.A)) { player.moveLeft(); }

                #region hotkeys
                if (state.IsKeyDown(Keys.D1) && swapTimer >= 320) 
                {
                    if (player.hasInventoryItemAt(0, 0) && player.getInventoryItemAt(0, 0).type.Equals("Tool")) { player.swapTool(0, 0); }
                    else { player.swapItem(0, 0); }
                    swapTimer = 0;
                }
                else if (state.IsKeyDown(Keys.D2) && swapTimer >= 320)
                {
                    if (player.hasInventoryItemAt(1,0) && player.getInventoryItemAt(1, 0).type == "Tool") { player.swapTool(1, 0); }
                    else { player.swapItem(1, 0); }
                    swapTimer = 0;
                }
                else if (state.IsKeyDown(Keys.D3) && swapTimer >= 320)
                {
                    if (player.hasInventoryItemAt(2, 0) && player.getInventoryItemAt(2, 0).type == "Tool") { player.swapTool(2, 0); }
                    else { player.swapItem(2, 0); }
                    swapTimer = 0;
                }
                else if (state.IsKeyDown(Keys.D4) && swapTimer >= 320)
                {
                    if (player.hasInventoryItemAt(3, 0) && player.getInventoryItemAt(3, 0).type == "Tool") { player.swapTool(3, 0); }
                    else { player.swapItem(3, 0); }
                    swapTimer = 0;
                }
                else if (state.IsKeyDown(Keys.D5) && swapTimer >= 320)
                {
                    if (player.hasInventoryItemAt(4, 0) && player.getInventoryItemAt(4, 0).type == "Tool") { player.swapTool(4, 0); }
                    else { player.swapItem(4, 0); }
                    swapTimer = 0;
                }
                else if (state.IsKeyDown(Keys.D6) && swapTimer >= 320)
                {
                    if (player.hasInventoryItemAt(5, 0) && player.getInventoryItemAt(5, 0).type == "Tool") { player.swapTool(5, 0); }
                    else { player.swapItem(5, 0); }
                    swapTimer = 0;
                }
                #endregion

                if (state.IsKeyDown(Keys.R))
                {
                    if(player.hasHeldItem()) player.storeItem();
                }

                if (state.IsKeyDown(Keys.Q))
                {
                    //if (state.IsKeyDown(Keys.LeftAlt)) { player.dropQ(); player.throwing = true; }
                    if (!player.QSelect) { player.selectQ(); }
                    else if (player.canSwing()) { player.QHeld = true; }
                }

                /*if (state.IsKeyDown(Keys.E))
                {
                    //if (state.IsKeyDown(Keys.LeftAlt)) { player.dropSelected(); player.throwing = true; }
                    if (!player.ESelect) { player.selectE(); }
                    else if (player.canSwing()) { player.EHeld = true; }
                }*/

                if (state.IsKeyDown(Keys.Space))
                {
                    player.spaceHeld = true;
                    DroppedItem item;
                    Drawable interacted;
                    
                    //!!!MAKE TILE BASED, JERK
                    if (!player.holdingItem && playing.scene.getItemAt(new Rectangle(playing.currentCorner[0] + player.coords[0] + player.facing[0] * 40, playing.currentCorner[1] + player.coords[1] + (player.facing[1] * 30) + 70, 60, 30), out item)) 
                    {
                        player.pickupItem(item);
                    }
                    else if (player.holdingItem && !playing.scene.onlyVisibleCollides(player.getPlayerFront())) { player.dropItem(); }
                    else if (playing.scene.onlyVisibleCollides(player.getInteractRectangle(), out interacted)) { interacted.interact(); }
                }
            }
            else if (player.spaceHeld && state.IsKeyUp(Keys.Space)) { player.releaseSpace(); }
            else if (player.QHeld && state.IsKeyUp(Keys.Q)) { player.releaseQ(); }
            //else if (player.EHeld && state.IsKeyUp(Keys.E)) { player.releaseE(); }


            if (state.IsKeyDown(Keys.Z) && !ceilingOff)
            {
                foreach (Building building in playing.buildings) { building.roofVisible = false; ceilingOff = true; }
            }
            else if (state.IsKeyUp(Keys.Z) && ceilingOff)
            {
                foreach (Building building in playing.buildings) { building.roofVisible = true; ceilingOff = false; }
            }

            if (state.IsKeyDown(Keys.Escape) && consoleTimer >= 320) { playing.changeState("DevConsole"); consoleTimer = 0; }

            if (state.IsKeyDown(Keys.E) && inventoryTimer >= 320) { playing.changeState("InventoryState"); inventoryTimer = 0; }
        }

        public void enterBuilding(Building building) 
        {
            if (!this.gameState.getTag().Equals("Inside"))
            {
                this.inside = true;
                this.gameState.leaving();
                this.gameState = new Inside(playing, building);
                this.gameState.entering();
            }
        }
        public void leaveBuilding() 
        {
            if (!this.gameState.getTag().Equals("Outside"))
            {
                this.inside = false;
                this.gameState.leaving();
                this.gameState = new Outside(playing, 0);
                this.gameState.entering();
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {

        }

        public void entering()
        {
            this.inventoryTimer = 160;
            this.consoleTimer = 160;
        }

        public void leaving()
        {
        }
        public string getState() { return gameState.getState(); }

        public int getAlpha(Drawable prop)
        {
            return gameState.getAlpha(prop);
        }
    }
    class Inside : GameState
    {
        Playing playing;
        Building building;
        int alpha = 255;
        bool fadeOut = true;
        public Inside(Playing playing, Building building)
        {
            this.playing = playing;
            this.building = building;
        }
        public int getTileAlpha() { return alpha; }
        public string getTag()
        {
            return "Inside";
        }

        public void update(GameTime gameTime)
        {
            if (fadeOut)
            {
                alpha -= 20; if (alpha <= 0) { fadeOut = false; alpha = 0; };
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
        }

        public void draw(SpriteBatch spriteBatch)
        {

        }
        public void entering()
        {

            building.fadeRoofOut();
        }

        public void leaving()
        {
            building.fadeRoofIn();
        }


        public int getAlpha(Drawable prop)
        {
            if (prop.getInside()) { return 255; }
            return this.alpha;
        }


        public string getState()
        {
            return "Inside";
        }
    }
    class Outside : GameState
    {
        Playing playing;
        int alpha;
        bool fadeIn = true;
        bool sceneryFade = false;

        public Outside(Playing playing, int alpha) 
        {
            this.playing = playing;
            this.alpha = alpha;
            
        }
        public int getTileAlpha() { return alpha; } 
        public string getTag()
        {
            return "Outside";
        }

        public void update(GameTime gameTime)
        {
            if (fadeIn) { alpha += 50; if (alpha >= 255) { fadeIn = false; alpha = 255; }; }
            foreach (Scenery scenery in playing.scenery) { scenery.update(gameTime); }
            foreach (DroppedItem item in playing.droppedItems)
            {
                item.update(gameTime);
                //check if item bounding box
            }
            foreach (Particle particle in playing.particlesAbove) { particle.update(gameTime); }
            foreach (Particle particle in playing.particlesBelow) { particle.update(gameTime); }
            foreach (FarmTile farmTile in playing.farmTiles) { farmTile.update(gameTime); } //encapsulates crop update

            //!!!remove on release
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.X) && !sceneryFade)
            {
                this.alpha = 50;
                sceneryFade = true;
            }
            else if (state.IsKeyUp(Keys.X) && sceneryFade)
            {
                this.alpha = 255;
                sceneryFade = false;
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {
        }

        public void entering()
        {
        }

        public void leaving()
        {
        }


        public int getAlpha(Drawable prop)
        {
            return alpha;
        }


        public string getState()
        {
            return "Outside";
        }
    }
    class ChestState : GameState
    {
        Playing playing;
        Chest chest;
        Inventory inventory;
        Texture2D tileSheet;
        Texture2D itemSheet;

        KeyboardState state;
        MouseState mouse;
        Vector2 mousePosition;
        SpriteFont output;

        Item clickedItem;
        Slot clickedItemSlot;

        int escapeTimer = 0;
        
        int inventoryTranslation = -180;

        public ChestState(Playing playing, Chest chest, Inventory inventory, Texture2D tileSheet, Texture2D itemSheet, SpriteFont output)
        {
            //!!!chest state is a MESS - be a better programmer and fix it
            this.playing = playing;
            this.chest = chest;
            this.inventory = inventory;
            this.tileSheet = tileSheet;
            this.itemSheet = itemSheet;
            this.output = output;
        }

        public int getTileAlpha()
        {
            if (playing.player.getInside()) { return 0; }
            return 255;
        }

        public void processInput()
        {
            state = Keyboard.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (clickedItem == null)
                {
                    if (chest.mouseOver(out clickedItemSlot))
                    {
                        clickedItemSlot.itemFloating = true;
                        this.clickedItem = clickedItemSlot.getItem();
                    }
                    else
                    {
                        clickedItem = inventory.pickupItem(inventoryTranslation, 0);
                        clickedItemSlot = inventory.getSlotOver(inventoryTranslation, 0);
                    }
                }
            }
            else if (clickedItem != null)
            {
                Slot dropZone;
                if (chest.mouseOver(out dropZone)) 
                {
                    inventory.swapItems(clickedItemSlot, dropZone);
                }
                inventory.dropItem(clickedItem, clickedItemSlot, inventoryTranslation, 0);
                clickedItem = null;
            }
            if ((state.IsKeyDown(Keys.E) || state.IsKeyDown(Keys.Escape)) && escapeTimer >= 320) { playing.changeState("Unpaused"); escapeTimer = 0; }
        }

        public string getTag()
        {
            return "ChestState";
        }

        public void update(GameTime gameTime)
        {
            if (escapeTimer < 320) escapeTimer += gameTime.ElapsedGameTime.Milliseconds;

            mouse = Mouse.GetState();
            mousePosition.X = mouse.X;
            mousePosition.Y = mouse.Y;

            this.processInput();
        }

        public void draw(SpriteBatch spriteBatch)
        {
            //draw background
            spriteBatch.Draw(tileSheet, inventory.screenVector, new Rectangle(230,151,300,151), Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);


            for (int i = 0; i < inventory.getLength(0); i++)
            {
                for (int j = 0; j < inventory.getLength(1); j++)
                {
                    string item;
                    if (inventory.hasItem(out item, i, j))
                    {
                        spriteBatch.Draw(itemSheet, new Vector2(inventory.getVectorAt(i, j).X + inventoryTranslation,inventory.getVectorAt(i,j).Y), playing.scene.getItem(item).spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    }
                }
            }

            for (int i = 0; i < chest.dimensions[0]; i++) 
            {
                for (int j = 0; j < chest.dimensions[1]; j++)
                {
                    spriteBatch.Draw(tileSheet, chest.getSlotAt(i, j).location, new Rectangle(262, 160, 18, 18), Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    if (chest.getSlotAt(i, j).hasItem())
                    {
                        spriteBatch.Draw(itemSheet, chest.getSlotAt(i, j).location, chest.getSlotAt(i, j).getItem().spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    }
                }
            }


            if (clickedItem != null) spriteBatch.Draw(itemSheet, new Vector2(mouse.X, mouse.Y), clickedItem.spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);

            spriteBatch.DrawString(output, Convert.ToString(inventory.getMouseOver(inventoryTranslation, 0)[0] + "," + inventory.getMouseOver(inventoryTranslation, 0)[1]), new Vector2(15, 125), Playing.getColor("Black"));

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
        }


        public int getAlpha(Drawable prop)
        {
            return 255;
        }


        public string getState()
        {
            return "ChestState";
        }
    }
    class InventoryState : GameState
    {
        Inventory inventory;
        Texture2D tileSheet;
        Texture2D itemSheet;
        int escapeTimer = 0;
        KeyboardState state;
        Playing playing;
        MouseState mouse;
        Vector2 mousePosition;
        SpriteFont output;
        Tooltip tooltip;

        Item clickedItem;
        Slot clickedItemSlot;

        Item itemOver;

        public InventoryState(Playing playing, Inventory inventory, Texture2D tileSheet, Texture2D itemSheet, SpriteFont output)
        {
            this.playing = playing;
            this.inventory = inventory;
            this.tileSheet = tileSheet;
            this.itemSheet = itemSheet;
            this.output = output;
            tooltip = new Tooltip();
        }

        public string getTag()
        {
            return "InventoryState";
        }
        public void processInput()
        {
            state = Keyboard.GetState();

            if (mouse.LeftButton == ButtonState.Pressed) 
            {
                if (clickedItem == null) { clickedItem = inventory.pickupItem(); clickedItemSlot = inventory.getSlotOver(); }
            }
            else if (clickedItem != null)
            {
                inventory.dropItem(clickedItem, clickedItemSlot);
                clickedItem = null;
            }
            if ((state.IsKeyDown(Keys.E) || state.IsKeyDown(Keys.Escape)) && escapeTimer >= 320) { playing.changeState("Unpaused"); escapeTimer = 0; }
        }

        public void update(GameTime gameTime)
        {
            if (escapeTimer < 320) escapeTimer += gameTime.ElapsedGameTime.Milliseconds;

            mouse = Mouse.GetState();
            mousePosition.X = mouse.X;
            mousePosition.Y = mouse.Y;

            itemOver = null;
            if( inventory.getSlotOver() != null ) itemOver = inventory.getSlotOver().getItem();

            processInput();
        }

        public void draw(SpriteBatch spriteBatch)
        {
            //draw background
            spriteBatch.Draw(tileSheet, inventory.screenVector, inventory.spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);

            //draw Items and tools
            //for (int i = 0; i < playing.player.tools.Count; i++) { spriteBatch.Draw(itemSheet, new Vector2(inventory.hotkeyCorner.X, inventory.hotkeyCorner.Y + (i * 69)), playing.scene.getItem(playing.player.tools[i]).spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }
            //for(int i = 0; i < playing.player.items.Count; i++) { spriteBatch.Draw(itemSheet, new Vector2(inventory.hotkeyCorner.X,inventory.hotkeyCorner.Y + ((i + 3)* 69)), playing.scene.getItem(playing.player.items[i]).spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f); }

            for (int i = 0; i < inventory.getLength(0); i++)
            {
                for (int j = 0; j < inventory.getLength(1); j++)
                {
                    string item;
                    if (inventory.hasItem(out item, i, j))
                    {
                        spriteBatch.Draw(itemSheet, inventory.getVectorAt(i,j), playing.scene.getItem(item).spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    }
                }
            }

            if (playing.player.hasEquippedTool() && !inventory.equippedTool.itemFloating) spriteBatch.Draw(itemSheet, inventory.equippedTool.location, inventory.equippedTool.getItem().spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
            if (playing.player.hasAccessory() && !inventory.equippedAccessory.itemFloating) spriteBatch.Draw(itemSheet, inventory.equippedAccessory.location, inventory.equippedAccessory.getItem().spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
            if (playing.player.hasHeldItem() && !inventory.heldItem.itemFloating) spriteBatch.Draw(itemSheet, inventory.heldItem.location, inventory.heldItem.getItem().spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);

            //DRAWING TOOLTIP
            if (clickedItem == null && itemOver != null) 
            {
                string longest = null;
                if(itemOver.attributes.Count > 0) longest = itemOver.attributes.OrderByDescending(s => s.tag.Length).First().tag;
                if (longest == null || itemOver.tag.Length > longest.Length) longest = itemOver.tag;

                //draw top + tag
                spriteBatch.Draw(tileSheet, new Vector2(mouse.X, mouse.Y), tooltip.topLeftSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                for (int i = 3; i <= longest.Length; i += 3)
                {
                    spriteBatch.Draw(tileSheet, new Vector2(mouse.X + (13 * i), mouse.Y), tooltip.topMidSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                }
                spriteBatch.Draw(tileSheet, new Vector2(mouse.X + (39 * (longest.Length / 3 + 1)), mouse.Y), tooltip.topRightSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                spriteBatch.DrawString(Playing.output, itemOver.tag, new Vector2(mouse.X + 9, mouse.Y + 3), Playing.getColor("Black"));

                //draw mids
                int numAtts = 1;
                foreach (Attribute attribute in itemOver.attributes)
                {
                    //!!!switch for attributes

                    spriteBatch.Draw(tileSheet, new Vector2(mouse.X, mouse.Y + numAtts * 24 + 3), tooltip.midLeftSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    for (int i = 3; i <= longest.Length; i += 3)
                    {
                        spriteBatch.Draw(tileSheet, new Vector2(mouse.X + (13 * i), mouse.Y + numAtts * 24 + 3), tooltip.midMidSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    }
                    spriteBatch.Draw(tileSheet, new Vector2(mouse.X + (39 * (longest.Length / 3 + 1)), mouse.Y + numAtts * 24 + 3), tooltip.midRightSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    spriteBatch.DrawString(Playing.output, attribute.tag, new Vector2(mouse.X + 9, mouse.Y + 3 + numAtts * 24 + 3), attribute.color);
                    numAtts++;
                }

                //draw bottoms
                spriteBatch.Draw(tileSheet, new Vector2(mouse.X, mouse.Y + numAtts * 24 + 3), tooltip.botLeftSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                for (int i = 3; i <= longest.Length; i += 3)
                {
                    spriteBatch.Draw(tileSheet, new Vector2(mouse.X + (13 * i), mouse.Y + numAtts * 24 + 3), tooltip.botMidSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                }
                spriteBatch.Draw(tileSheet, new Vector2(mouse.X + (39 * (longest.Length / 3 + 1)), mouse.Y + numAtts * 24 + 3), tooltip.botRightSprite, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    
            }

            if (clickedItem != null) spriteBatch.Draw(itemSheet, new Vector2(mouse.X - 16,mouse.Y-16), clickedItem.spriteRectangle, Color.White, 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);

            spriteBatch.DrawString(output, Convert.ToString(inventory.getMouseOver()[0] + "," + inventory.getMouseOver()[1]), new Vector2(15, 125), Playing.getColor("Black"));

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

        public int getTileAlpha()
        {
            if (playing.player.getInside()) { return 0; }
            return 255;
        }


        public int getAlpha(Drawable prop)
        {
            if (playing.player.getInside() && prop.getInside()) { return 255; }
            else if (playing.player.getInside()) { return 0; }
            else return 255;
        }


        public string getState()
        {
            return "InventoryState";
        }
    }
    class DevConsole : GameState
    {
        private string tag = "Paused";
        Playing playing;
        KeyboardState state;
        private int escapeUpdate = 0;
        public ingameMenu menu;
        SpriteFont output;

        public DevConsole(Playing playing, ingameMenu menu)
        {
            this.playing = playing;
            this.menu = menu;
            this.tag = "Paused";
            output = playing.ingame.Content.Load<SpriteFont>("Output18pt");
        }

        public void update(GameTime gameTime)
        {
            escapeUpdate += gameTime.ElapsedGameTime.Milliseconds;
            state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape) && escapeUpdate >= 128) { escapeUpdate = 0; playing.changeState("Unpaused"); }
        }

        public void draw(SpriteBatch spriteBatch)
        {

            Texture2D dummyTexture = new Texture2D(playing.ingame.GraphicsDevice, 1, 1);
            Texture2D dummyTexture2 = new Texture2D(playing.ingame.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Playing.getColor("Gray") });
            spriteBatch.Draw(dummyTexture, new Rectangle(200, 50, 900, 600), Playing.getColor("Gray"));
            dummyTexture2.SetData(new Color[] { Playing.getColor("Black") });
            spriteBatch.Draw(dummyTexture2, new Rectangle(215, 65, 870, 570), Playing.getColor("Gray"));

            menu.update();
            spriteBatch.DrawString(output, menu.draw(), new Vector2(225, 75), Playing.getColor("White"));
        }

        public void entering()
        {
            escapeUpdate = 0;
            foreach (Drawable prop in playing.visible) { prop.alpha = 50; }
        }

        public void leaving()
        {
            foreach (Drawable prop in playing.visible) { prop.alpha = 255; }
            this.menu.clear();
        }

        public string getTag()
        {
            return tag;
        }
        public int getTileAlpha()
        {
            return 255;
        }


        public int getAlpha(Drawable prop)
        {
            return 255;
        }


        public string getState()
        {
            return "DevConsole";
        }
    }


    class ingameMenu
    {
        RainFrog ingame;
        KeyboardState keyboard;
        KeyboardState oldkeyboardState;
        SpriteFont output;
        Playing playing;

        string entry = "";
        string outputText = "";

        public ingameMenu(RainFrog ingame, SpriteFont output, Playing playing) { this.playing = playing; this.ingame = ingame; this.output = output; }


        public string draw()
        {
            return "> " + entry + "\n" + outputText;
        }
        public void clear() { entry = ""; outputText = ""; }

        public void update()
        {
            keyboard = Keyboard.GetState();
            char key;
            TryConvertKeyboardInput(keyboard, oldkeyboardState, out key);
            oldkeyboardState = keyboard;
            //Debug.Print(Convert.ToString(key));
            if (!key.Equals('\0') && entry.Length <= 56)
            {
                entry += Convert.ToString(key);
            }
            string[] outputSplit = outputText.Split('\n');
            if (outputSplit.Length > 570 / 32)
            {
                outputText = "\n" + string.Join("\n", outputSplit.Skip(outputSplit.Length - 570 / 32));
                //outputText = outputSplit.Skip(outputSplit.Length - 25).; 
            }
        }

        public void parseCommand(string command)
        {
            string[] key = command.ToUpper().Split(' ');
            switch (key[0])
            {
                case "HELP": { outputText += "\n<" + command + ">" + "\nCOMMANDS:\nNew           -> Start a new game\nSaved         -> List of saved games\nLoad [name]   -> Load a saved game\nDelete [name] -> Delete a saved game\nClear         -> Clear the console\nAbout         -> About the game\nQuit          -> Exit the game"; break; }
                case "CLEAR": { outputText = ""; break; }
                case "NEW": { outputText += "\n<" + command + ">" + "\nNEW"; break; }
                case "LOAD": { outputText += "\n<" + command + ">" + "\nOutput text if is broken"; break; }
                case "SAVE": { outputText += "\n<" + command + ">" + "\nList saved games here"; break; }
                case "ABOUT": { outputText += "\n<" + command + ">" + "\nThis is a game by Hank!  Thank you for playing!"; break; }
                case "QUIT": { ingame.Exit(); break; }
                case "EXIT": { ingame.Exit(); break; }
                case "EDITOR": { playing.changeState("Editor"); break; }
                case "MENU": { ingame.changeGameState("initialize"); break; }
                case "NOCLIP": { outputText += "\n<" + command + ">" + "\nNo Clip toggled."; playing.toggleNoClip(); break; }
                case "SPEED": { outputText += "\n<" + command + ">" + "\nPlayer speed increased from" + playing.player.speed + "to" + command.Split(' ')[1] + "."; playing.player.speed = Convert.ToInt16(command.Split(' ')[1]); break; }
                case "CURSOR": { outputText += "\n<" + command + ">" + "\nCursor enabled"; ingame.IsMouseVisible = true; break; }
                case "ACTIVITY": { outputText += "\n<" + command + ">" + "\nNow drawing Activity Regions."; playing.drawActivity = true; break; }
                case "GETCHUNK": { outputText += "\n<" + command + ">" + "\n" + playing.scene.currentChunk.ToString(); break; }
                case "EXPORT":
                    {
                        if (key[1] == "ALL") 
                        { 
                            outputText += "\n<" + command + ">" + "\n";
                            playing.scene.exportAllChunks();
                        }
                        break;
                    }
                case "OCCUPIED": { Playing.drawOccupied = !Playing.drawOccupied; break; }
                case "GRASS": { playing.scene.currentChunk.addScenery(new Grass(new int[] { playing.player.coords[0] + playing.player.coords[0], playing.player.coords[1] + playing.player.coords[1] }, playing, false, Scene.scenery["TallGrassCut"])); break; }
                case "MINIGAME":
                    {
                        if (key[1] == "BOOK")
                        {
                            playing.changeState("BookMinigame");
                        }
                        break;
                    }
                default: { outputText += "\n<" + command + ">" + "\nI do not recognize your command: '" + command.Split(' ')[0] + "' \n-- Please type 'help' for a list of accepted commands."; break; }
            }
        }

        public bool TryConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard, out char key)
        {
            Keys[] keys = keyboard.GetPressedKeys();

            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            for (int i = 0; i < keys.Length; i++)
            {
                if (!oldKeyboard.IsKeyDown(keys[i]))
                {
                    switch (keys[i])
                    {
                        //Alphabet keys
                        case Keys.A: if (shift) { key = 'A'; } else { key = 'a'; } return true;
                        case Keys.B: if (shift) { key = 'B'; } else { key = 'b'; } return true;
                        case Keys.C: if (shift) { key = 'C'; } else { key = 'c'; } return true;
                        case Keys.D: if (shift) { key = 'D'; } else { key = 'd'; } return true;
                        case Keys.E: if (shift) { key = 'E'; } else { key = 'e'; } return true;
                        case Keys.F: if (shift) { key = 'F'; } else { key = 'f'; } return true;
                        case Keys.G: if (shift) { key = 'G'; } else { key = 'g'; } return true;
                        case Keys.H: if (shift) { key = 'H'; } else { key = 'h'; } return true;
                        case Keys.I: if (shift) { key = 'I'; } else { key = 'i'; } return true;
                        case Keys.J: if (shift) { key = 'J'; } else { key = 'j'; } return true;
                        case Keys.K: if (shift) { key = 'K'; } else { key = 'k'; } return true;
                        case Keys.L: if (shift) { key = 'L'; } else { key = 'l'; } return true;
                        case Keys.M: if (shift) { key = 'M'; } else { key = 'm'; } return true;
                        case Keys.N: if (shift) { key = 'N'; } else { key = 'n'; } return true;
                        case Keys.O: if (shift) { key = 'O'; } else { key = 'o'; } return true;
                        case Keys.P: if (shift) { key = 'P'; } else { key = 'p'; } return true;
                        case Keys.Q: if (shift) { key = 'Q'; } else { key = 'q'; } return true;
                        case Keys.R: if (shift) { key = 'R'; } else { key = 'r'; } return true;
                        case Keys.S: if (shift) { key = 'S'; } else { key = 's'; } return true;
                        case Keys.T: if (shift) { key = 'T'; } else { key = 't'; } return true;
                        case Keys.U: if (shift) { key = 'U'; } else { key = 'u'; } return true;
                        case Keys.V: if (shift) { key = 'V'; } else { key = 'v'; } return true;
                        case Keys.W: if (shift) { key = 'W'; } else { key = 'w'; } return true;
                        case Keys.X: if (shift) { key = 'X'; } else { key = 'x'; } return true;
                        case Keys.Y: if (shift) { key = 'Y'; } else { key = 'y'; } return true;
                        case Keys.Z: if (shift) { key = 'Z'; } else { key = 'z'; } return true;

                        //Decimal keys
                        case Keys.D0: if (shift) { key = ')'; } else { key = '0'; } return true;
                        case Keys.D1: if (shift) { key = '!'; } else { key = '1'; } return true;
                        case Keys.D2: if (shift) { key = '@'; } else { key = '2'; } return true;
                        case Keys.D3: if (shift) { key = '#'; } else { key = '3'; } return true;
                        case Keys.D4: if (shift) { key = '$'; } else { key = '4'; } return true;
                        case Keys.D5: if (shift) { key = '%'; } else { key = '5'; } return true;
                        case Keys.D6: if (shift) { key = '^'; } else { key = '6'; } return true;
                        case Keys.D7: if (shift) { key = '&'; } else { key = '7'; } return true;
                        case Keys.D8: if (shift) { key = '*'; } else { key = '8'; } return true;
                        case Keys.D9: if (shift) { key = '('; } else { key = '9'; } return true;

                        //Decimal numpad keys
                        case Keys.NumPad0: key = '0'; return true;
                        case Keys.NumPad1: key = '1'; return true;
                        case Keys.NumPad2: key = '2'; return true;
                        case Keys.NumPad3: key = '3'; return true;
                        case Keys.NumPad4: key = '4'; return true;
                        case Keys.NumPad5: key = '5'; return true;
                        case Keys.NumPad6: key = '6'; return true;
                        case Keys.NumPad7: key = '7'; return true;
                        case Keys.NumPad8: key = '8'; return true;
                        case Keys.NumPad9: key = '9'; return true;

                        //Special keys
                        //case Keys.OemTilde: if (shift) { key = '~'; } else { key = '`'; } return true;
                        case Keys.OemSemicolon: if (shift) { key = ':'; } else { key = ';'; } return true;
                        case Keys.OemQuotes: if (shift) { key = '"'; } else { key = '\''; } return true;
                        case Keys.OemQuestion: if (shift) { key = '?'; } else { key = '/'; } return true;
                        case Keys.OemPlus: if (shift) { key = '+'; } else { key = '='; } return true;
                        case Keys.OemPipe: if (shift) { key = '|'; } else { key = '\\'; } return true;
                        case Keys.OemPeriod: if (shift) { key = '>'; } else { key = '.'; } return true;
                        case Keys.OemOpenBrackets: if (shift) { key = '{'; } else { key = '['; } return true;
                        case Keys.OemCloseBrackets: if (shift) { key = '}'; } else { key = ']'; } return true;
                        case Keys.OemMinus: if (shift) { key = '_'; } else { key = '-'; } return true;
                        case Keys.OemComma: if (shift) { key = '<'; } else { key = ','; } return true;
                        case Keys.Space: key = ' '; return true;
                        case Keys.Back: key = '\0'; if (entry.Length > 0) { entry = entry.Substring(0, entry.Length - 1); }; return true;
                        //change enter command to put result up and clear entry
                        case Keys.Enter: key = '\0'; this.parseCommand(entry); entry = ""; return true;
                    }
                }
            }

            key = (char)0;
            return false;
        }
    }
}
