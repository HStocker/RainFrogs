using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace RainFrogs
{
    class Player : Drawable
    {
        Playing playing;
        Inventory inventory;
        //MAX 3 of each

        public int speed = 4;

        int indexTimer = 0;
        int maxIndexTimer = 180;
        int moveTimer = 0;
        int swingTimer = 0;
        int throwTimer = 0;
        int toolUseTimer = 0;
        int pickupTimer = 0;
        int putdownTimer = 0;

        public int[] facing = new int[] { 0, 1 };
        public bool QSelect = true;
        public bool ESelect = false;
        public int[] nextMove;
        public int[] itemCoords;

        //debugging
        public Vector2 toolVector;
        public Vector2 pickupVector;
        public Rectangle toolRectangle;
        public Rectangle pickupRectangle;

        public Chest openChest;

        public bool active = true;

        public bool throwing = false;
        public bool swinging = false;
        public bool spaceHeld = false;
        public bool QHeld = false;
        public bool EHeld = false;
        public bool holdingItem = false;
        public bool pickingUp = false;
        public bool puttingDown = false;

        public int chargeTimer = 0;
        bool collided = false;

        public string collidedWith;
        public string lastCollided;

        public Player(int[] coords, Playing playing, Inventory inventory)
        {
            this.playing = playing;
            this.inventory = inventory;

            inventory.addItemAt(Scene.items["Hoe"], 0, 0);
            inventory.addItemAt(Scene.items["WateringCan"], 1, 0);
            //inventory.addItemAt("Scythe", 2, 0);
            inventory.equippedTool.setItem(Scene.items["Scythe"]);

            inventory.addItemAt(Scene.items["Apple"], 3, 0);
            inventory.addItemAt(Scene.items["Rock"], 4, 0);
            inventory.addItemAt(Scene.items["Wheat Seeds"], 2, 0);
            inventory.getItemAt(new int[] { 3, 0 }).attributes.Add(new Attribute("Fresh", Playing.getColor("Red"), new Rectangle(), false));

            this.coords = coords;
            this.nextMove = new int[] { coords[0], coords[1] };
            this.setSpriteRectangle(new Rectangle(0, 0, 19, 35));
            this.draw = playing.drawPlayer;
        }
        public void update(GameTime gameTime)
        {
            playing.visible.Add(this);
            indexTimer += gameTime.ElapsedGameTime.Milliseconds;
            toolUseTimer += gameTime.ElapsedGameTime.Milliseconds;

            //!!!REPLACE WITH ANIMATION TIMERS LATER
            if (swinging) { swingTimer += gameTime.ElapsedGameTime.Milliseconds; if (swingTimer > 180) { swinging = false; swingTimer = 0; } }
            if (throwing) { throwTimer += gameTime.ElapsedGameTime.Milliseconds; if (throwTimer > 360) { throwing = false; throwTimer = 0; } }
            if (spaceHeld || QHeld || EHeld) { chargeTimer += gameTime.ElapsedGameTime.Milliseconds; } else { chargeTimer = 0; }
            if (this.pickingUp) { pickupTimer += gameTime.ElapsedGameTime.Milliseconds; if (pickupTimer > 360) { pickingUp = false; this.holdingItem = true; pickupTimer = 0; } }
            if (this.puttingDown) { putdownTimer += gameTime.ElapsedGameTime.Milliseconds; if (putdownTimer > 360) { puttingDown = false; this.holdingItem = false; putdownTimer = 0; } }

            if (holdingItem) { this.itemCoords[0] = this.coords[0]; this.itemCoords[1] = this.coords[1] - 36; }

        }
        public void collisionUpdate(GameTime gameTime)
        {
            if (playing.noClip || !playing.scene.onlyVisibleCollides(new Rectangle(nextMove[0] + playing.currentCorner[0] + 3, nextMove[1] + playing.currentCorner[1] + 60, 53, 42), out collidedWith))
            { //move over to account for UI
                this.collided = false;
                if (nextMove[0] < 300) { playing.currentCorner[0] += nextMove[0] - coords[0]; coords[0] = 300; nextMove[0] = 300; }
                else if (nextMove[0] > 850) { playing.currentCorner[0] += nextMove[0] - coords[0]; coords[0] = 850; nextMove[0] = 850; }
                else if (nextMove[1] < 250) { playing.currentCorner[1] += nextMove[1] - coords[1]; coords[1] = 250; nextMove[1] = 250; }
                else if (nextMove[1] > 500) { playing.currentCorner[1] += nextMove[1] - coords[1]; coords[1] = 500; nextMove[1] = 500; }
                else this.coords = new int[] { nextMove[0], nextMove[1] };
            }
            else
            {
                nextMove = new int[] { coords[0], coords[1] };
                //paralysed when on blocks
                //this doesn't work  if (this.collided) { this.coords[0] -= 10; this.collided = false; }
                this.collided = true;
                lastCollided = collidedWith;
            }
        }
        public void releaseSpace()
        {
            spaceHeld = false;
            //if (this.holdingItem) { this.dropItem(); this.holdingItem = false; }

            //if (QSelect) { useTool(chargeTimer); }
            //else { useItem(chargeTimer); }
        }
        public bool canSwing() { if (indexTimer < maxIndexTimer) { return false; } else return true; }
        public void releaseQ() { QHeld = false; useTool(chargeTimer); chargeTimer = 0; }
        public void releaseE() { EHeld = false; useItem(chargeTimer); chargeTimer = 0; }

        #region movement methods
        public void moveLeft()
        {
            this.facing = new int[] { -1, 0 };
            this.spriteRectangle = new Rectangle(19, 0, 19, 35);
            this.flipped = true;
            this.nextMove[0] -= this.speed;
        }
        public void moveUp()
        {
            this.facing = new int[] { 0, -1 };
            this.spriteRectangle = new Rectangle(38, 0, 19, 35);
            this.flipped = false;
            this.nextMove[1] -= this.speed;
        }
        public void moveRight()
        {
            this.facing = new int[] { 1, 0 };
            this.spriteRectangle = new Rectangle(19, 0, 19, 35);
            this.flipped = false;
            this.nextMove[0] += this.speed;
        }
        public void moveDown()
        {
            this.facing = new int[] { 0, 1 };
            this.spriteRectangle = new Rectangle(0, 0, 19, 35);
            this.flipped = false;
            this.nextMove[1] += this.speed;
        }
        #endregion

        public void pickupItem(DroppedItem item)
        {
            inventory.heldItem.setItem(item.item);
            item.toBeDeleted = true;
            this.itemCoords = new int[] { item.coords[0] - playing.currentCorner[0], item.coords[1] - playing.currentCorner[1] };
            this.pickingUp = true;

        }

        public void storeItem()
        {
            //!!!add storage animation and sound + arrest controls
            this.holdingItem = false;
            inventory.addNextItem();
        }
        public void dropItem()
        {
            this.puttingDown = true;
            playing.droppedItems.Add(new DroppedItem(playing.scene.getItem(inventory.heldItem.getItem().tag), new int[] { this.getPlayerFront().X, this.getPlayerFront().Y }, playing));
            inventory.heldItem.setItem(null);
        }
        public void openBox(Chest chest) { this.openChest = chest; }
        public Chest getChest() { return openChest; }
        public void setChest(Chest chest) { this.openChest = chest; }

        public Rectangle getPlayerFront() { return new Rectangle(playing.currentCorner[0] + coords[0] + (facing[0] * 33) + 21, playing.currentCorner[1] + coords[1] + (facing[1] * 30) + 80, 6, 6); }
        public Rectangle getInteractRectangle() { return new Rectangle(playing.currentCorner[0] + coords[0] + (facing[0] * 36) + 6, playing.currentCorner[1] + coords[1] + (facing[1] * 30) + 60, 20, 20); }
        public bool hasInventoryItemAt(int x, int y) { return inventory.hasItem(x, y); }
        public Item getInventoryItemAt(int x, int y) { return inventory.getItemAt(x, y); }
        public void swapTool(int x, int y) { inventory.swapItems(inventory.equippedTool, inventory.getSlotAt(x, y)); }
        public void swapItem(int x, int y) { inventory.swapItems(inventory.heldItem, inventory.getSlotAt(x, y)); }
        public bool hasEquippedTool() { return inventory.equippedTool.getItem() != null; }
        public bool hasHeldItem() { return inventory.heldItem.getItem() != null; }
        public bool hasAccessory() { return inventory.equippedAccessory.getItem() != null; }
        public void selectQ() { if (inventory.hasHotkeyTools()) { this.QSelect = true; this.ESelect = false; } this.indexTimer = 0; }
        public void selectE() { if (inventory.hasHotkeyItems()) { this.ESelect = true; this.QSelect = false; } this.indexTimer = 0; }

        public void useTool(int timeHeld)
        {
            if (toolUseTimer > inventory.equippedTool.getItem().useTimer)
            {
                //Debug.Print(Convert.ToString(scene.getItem(tools[QIndex]).useTimer));
                swinging = true;
                toolUseTimer = 0;

                switch (inventory.equippedTool.getItem().tag)
                {
                    case "Pick": { break; }
                    case "Shovel": { break; }
                    case "FishingRod": { break; }
                    case "Hammer": { break; }
                    case "Hoe":
                        {
                            Chunk chunk = null;
                            toolVector = new Vector2(this.getPlayerFront().X - playing.currentCorner[0], this.getPlayerFront().Y - playing.currentCorner[1]);
                            toolRectangle = new Rectangle(0, 0, 6, 6);
                            Tile tile = playing.scene.getBestFit(this.getPlayerFront(), ref chunk);
                            if (tile.interact(inventory.equippedTool.getItem()))
                            {
                                FarmTile farmTile = new FarmTile(tile, chunk, playing);
                                chunk.addFarmTile(farmTile);
                                tile.setFarmTile(farmTile);
                            }
                            break;
                        }
                    case "WateringCan":
                        {
                            Chunk chunk = null;
                            toolVector = new Vector2(this.getPlayerFront().X - playing.currentCorner[0], this.getPlayerFront().Y - playing.currentCorner[1]);
                            toolRectangle = new Rectangle(0, 0, 6, 6);
                            Tile tile = playing.scene.getBestFit(this.getPlayerFront(), ref chunk);

                            if (tile.interact(inventory.equippedTool.getItem()))
                            {
                                tile.occupiedBy.Find(a => a.getTag() == "FarmTile").useItem(inventory.equippedTool.getItem());
                            }

                            break;
                        }

                    case "Wheat Seeds":
                        {
                            Chunk chunk = null;
                            Tile tile = playing.scene.getBestFit(this.getPlayerFront(), ref chunk);
                            if (tile.isOccupied()) 
                            { 
                                Crop crop = new Crop(playing.scene.getCrop("Wheat"), tile, playing);
                                tile.interact(inventory.equippedTool.getItem());
                                chunk.addCrop(crop);
                            }
                            

                            //playing.particlesAbove.Add(new Particle(new ParticleType(new Rectangle(10, 20, 40, 40), "wheat seeds", 0, 320, true, false), new int[] { coords[0] + playing.currentCorner[0], coords[1] + playing.currentCorner[1] }, facing, playing));
                            break;
                        }

                    case "Scythe":
                        {
                            Chunk chunk = null;
                            toolVector = new Vector2(this.getPlayerFront().X - playing.currentCorner[0], this.getPlayerFront().Y - playing.currentCorner[1]);
                            toolRectangle = new Rectangle(0, 0, 6, 6);
                            Tile tile = playing.scene.getBestFit(this.getPlayerFront(), ref chunk);

                            tile.interact(inventory.equippedTool.getItem());
                            
                            break;
                        }
                }
            }
        }
        public void useItem(int timeHeld)
        {
            swinging = true;
            toolUseTimer = 0;
            switch (inventory.heldItem.getItem().tag)
            {
                case "Apple": { break; }
                case "Rock": { break; }

            }
        }
        public Rectangle getAbsoluteHitBox() 
        {
            return new Rectangle(this.coords[0] + playing.currentCorner[0], this.coords[1] + playing.currentCorner[1] + 60, 53, 42);
        }
        public override Rectangle getHitBox()
        {
            return new Rectangle(0, 60, 53, 42);
        }
        public override float getDrawDepth()
        {
            return (this.coords[1] + (this.getSpriteRectangle().Height * 3)) / (768f);
        }

        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override bool getInside()
        {
            return playing.unpaused.inside;
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            throw new NotImplementedException();
        }
    }
}
