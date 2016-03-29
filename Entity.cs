using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace RainFrogs
{
    class DroppedItem : Drawable
    {
        public Item item;
        Playing playing;
        Tile destination;
        public bool inair = true;
        int[] direction;
        float gravity = .2f;
        float[] velocity;


        public DroppedItem(Item item, int[] coords, Playing playing)
        {
            this.setTag(item.tag);
            this.coords = coords;
            this.item = item;
            this.playing = playing;
            this.inair = false;
            this.draw = playing.drawDroppedItem;
            this.setSpriteRectangle(item.spriteRectangle);

            //!!!this.destination = playing.scene.liveChunks[0].getTile(new int[] { 0, 0 });
            this.direction = new int[] {0,0};
            this.velocity = new float[] {0f,0f};
        }

        public DroppedItem(Item item, int[] coords, Playing playing, Tile destination, int[] direction)
        {
            this.setTag(item.tag);
            this.coords = coords;
            this.item = item;
            this.playing = playing;
            this.draw = playing.drawDroppedItem;
            this.setSpriteRectangle(item.spriteRectangle);

            this.destination = destination;
            this.direction = direction;
            this.velocity = new float[] { 2 * this.direction[0], 1.5f * direction[1] };
        }
        public void update(GameTime gameTime)
        {
            if (playing.onScreen(this)) { playing.visible.Add(this); }
            //this.time += gameTime.ElapsedGameTime.Milliseconds;
            //Debug.Print("item: " + Convert.ToString((this.coords[1] + item.getSpriteRectangle().Height) / (900f)));
        }
        public override Rectangle getHitBox()
        {
            if (this.inair) { return new Rectangle(); }
            else return new Rectangle(this.coords[0], this.coords[1], 38, 38);
        }
        public void travel()
        {
            this.velocity[1] -= gravity;
            this.coords[0] += (int)velocity[0];
            this.coords[1] -= (int)velocity[1];

            if (this.coords[1] >= this.destination.getCoords()[1])
            {
                this.coords[1] = this.destination.getCoords()[1];
                this.inair = false;
            }

            //solve for root
            //determine if that will collide
            //if (playing.onScreen(this) && playing.scene.onlyVisibleCollides(new Rectangle(this.coords[0], this.coords[1], 38, 38), out unused)) { this.velocity[1] = 3; }
            //else if (!playing.onScreen(this) && playing.scene.collides(new Rectangle(this.coords[0], this.coords[1], 38, 38), out unused)) { this.velocity[1] = 3; }
        }

        void bob()
        {
            this.coords[1] += Convert.ToInt16(Math.Sin((time / 144)));
            this.rotation += Convert.ToInt16(Math.Sin((time / 216))) * -.02f;
            //this.rotation += .02f;
            //Debug.Print(Convert.ToString(Convert.ToInt16(Math.Sin((time / 144)))));
        }
        public override float getDrawDepth()
        {
            if (this.inair && direction[1] >= 0) return 1;
            return (this.coords[1] - playing.currentCorner[1] + (playing.scene.getItem(this.getTag()).spriteRectangle.Height * 3)) / (900f);
        }
        public new string getTag() { return item.tag; }

        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override bool getInside()
        {
            //!!!create building function to get if it is inside the building
            return false;
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class NPC : Drawable
    {
        public override float getDrawDepth()
        {
            return 0f;
        }
        public override Rectangle getHitBox()
        {
            throw new NotImplementedException();
        }

        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override bool getInside()
        {
            throw new NotImplementedException();
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class Projectile : Drawable
    {
        public override float getDrawDepth()
        {
            return 0f;
        }
        public override Rectangle getHitBox()
        {
            throw new NotImplementedException();
        }


        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override bool getInside()
        {
            throw new NotImplementedException();
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class Particle : Drawable
    {
        int[] direction;
        double[] velocity;
        double[] decay = new double[] { 0, 0 };
        public int[] pixMod = new int[] { 0, 0 };
        int lifespan;
        ParticleType particle;
        Playing playing;
        string collidedWith;

        Random rand = new Random(Guid.NewGuid().GetHashCode());

        public Particle(ParticleType particle, int[] start, int[] direction, Playing playing)
        {
            this.playing = playing;
            this.particle = particle;
            this.lifespan = particle.life;
            this.level = 1;
            int[] offset = new int[] { rand.Next(particle.spawnRegion.X, particle.spawnRegion.X + particle.spawnRegion.Width), rand.Next(particle.spawnRegion.Y, particle.spawnRegion.Y + particle.spawnRegion.Height) };
            velocity = new double[] { rand.NextDouble() + .5, rand.NextDouble() + .5 };

            switch (particle.type)
            {
                case "leaves":
                    {
                        Rectangle[] sprites = new Rectangle[] { new Rectangle(0, 0, 6, 6), new Rectangle(6, 0, 6, 6) };
                        this.setSpriteRectangle(sprites[rand.Next(0, sprites.Length)]);
                        this.direction = new int[] { 1, -1 };
                        this.velocity = new double[] { rand.NextDouble(), -.5 };
                        break;
                    }
                case "grass":
                    {
                        Rectangle[] sprites = new Rectangle[] { new Rectangle(0, 0, 6, 6), new Rectangle(6, 0, 6, 6) };
                        this.setSpriteRectangle(sprites[rand.Next(0, sprites.Length)]);
                        this.direction = direction;
                        this.velocity = new double[] { rand.NextDouble() - .5, rand.NextDouble() - .5 };
                        break;
                    }
                case "wheat seeds":
                    {
                        Rectangle[] sprites = new Rectangle[] { new Rectangle(0, 6, 6, 6), new Rectangle(6, 6, 6, 6), new Rectangle(12, 6, 6, 6), new Rectangle(18, 6, 6, 6), new Rectangle(24, 6, 6, 6) };
                        this.setSpriteRectangle(sprites[rand.Next(0, sprites.Length)]);
                        //possible ambiguity between direction and this.direction
                        //rename to inDirection

                        int[] angleOffset = new int[] { rand.Next(-2, 2) * direction[0], rand.Next(-2, 2) * direction[1] };
                        this.direction = new int[] { direction[0] + angleOffset[0], direction[1] + angleOffset[1] };
                        this.velocity = new double[] { rand.NextDouble() / angleOffset[0] + 1, rand.NextDouble() / angleOffset[1] + 1 };
                        this.decay = new double[] { rand.NextDouble() * direction[1], rand.NextDouble() * direction[1] };
                        //offset = new int[] { rand.Next(-40, 40) * direction[1], rand.Next(-50, 40) * direction[0] };
                        break;
                    }
                case "water":
                    {
                        this.direction = direction;
                        this.velocity = new double[] { rand.NextDouble() + 1, rand.NextDouble() + 1 };
                        //water -water droplets
                        //large sprites of larger droplets
                        //
                        break;
                    }
                case "puff":
                    {
                        this.direction = new int[] { rand.Next(-1, 1), 0 };
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            //coordinate assignment after switch for offset adjustment
            //for triggered particles
            //non triggered/persistent particles will be spawned out of a spawn region
            //that will be defined in the XML file
            //!!!consider different system
            this.coords = new int[] { start[0] + offset[0], start[1] + offset[1] };
        }
        public void update(GameTime gameTime)
        {
            this.time += gameTime.ElapsedGameTime.Milliseconds;
            if (this.time > lifespan) { this.toBeDeleted = true; }
            else
            {
                this.pixMod = new int[] { (int)(4 * direction[0] * velocity[0]) + pixMod[0], (int)(4 * direction[1] * velocity[1]) + pixMod[1] };
            }

            switch (particle.type)
            {
                case "leaves":
                    {
                        velocity[0] = Math.Sin(this.time / 80) * rand.NextDouble();
                        if (this.time > lifespan - 120)
                        {
                            alpha -= 20;
                        }
                        if (particle.below)
                        {
                            /*!!!this is a mess, what's up with it
                            if (playing.scene.collides(new Rectangle(coords[0] + pixMod[0], coords[1] + pixMod[1], getSpriteRectangle().Width * 3, getSpriteRectangle().Height * 3), out collidedWith))
                            {
                                alpha = 0;
                                this.toBeDeleted = true;
                            }*/
                        }
                        break;
                    }
                case "grass":
                    {

                        if (this.time > lifespan - 120)
                        {
                            alpha -= 20;
                        }
                        break;
                    }
                case "wheat seeds":
                    {
                        this.rotation += .2f;
                        //ROTATE
                        //DECAY
                        //OFFSET
                        if (this.time > lifespan - 120)
                        {
                            alpha -= 20;
                        }
                        break;
                    }
            }
        }
        public int[] getLocation() { return coords; }
        public override float getDrawDepth()
        {
            return 0f;
        }
        public override Rectangle getHitBox()
        {
            throw new NotImplementedException();
        }

        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override bool getInside()
        {
            throw new NotImplementedException();
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class Crop : Drawable
    {
        CropType type;
        Playing playing;
        int spriteIndex = 0;
        public bool fullyGrown = false;

        public Crop(CropType type, Tile tile, Playing playing)
        {
            this.type = type;
            this.setTag(type.tag);
            this.coords = new int[] { tile.getCoords()[0], tile.getCoords()[1] - 36 };
            this.setSpriteRectangle(type.spriteRectangles[0]);
            this.playing = playing;
            this.draw = playing.drawCrop;

        }

        public void update(GameTime gameTime, FarmTile farm)
        {
            if (playing.onScreen(this)) { playing.visible.Add(this); }
        }

        public override Rectangle getHitBox() { return new Rectangle(); }
        public override float getDrawDepth()
        {
            return (this.coords[1] - playing.currentCorner[1] + (this.getSpriteRectangle().Height * 3)) / (768f);
        }

        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override bool getInside()
        {
            throw new NotImplementedException();
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class FarmTile : Drawable
    {
        public Tile tile;
        public List<Rectangle> spriteRectangles = new List<Rectangle> { new Rectangle(0, 60, 16, 16), new Rectangle(16, 60, 16, 16) };
        Playing playing;
        public bool occupied = false;
        public bool watered = false;
        Crop crop;

        public FarmTile(Tile tile, Chunk chunk, Playing playing)
        {
            this.setTag("FarmTile");
            this.playing = playing;
            this.tile = tile;
            this.draw = playing.drawFarmTile;
            this.coords = new int[] {tile.getCoords()[0] + chunk.currentCorner[0], tile.getCoords()[1] + chunk.currentCorner[1]};
            this.setSpriteRectangle(spriteRectangles[0]);
        }
        public void update(GameTime gameTime)
        {
            //this.time += gameTime.ElapsedGameTime.Milliseconds;

            if (playing.onScreen(this)) { playing.visible.Add(this); }
            if (occupied) { this.crop.update(gameTime, this); }

            //dessication happens during sleep cycle    
            //if (dessicationTimer < 0) { if (occupied) { this.crop.toBeDeleted = true; } this.toBeDeleted = true; }

        }
        public DroppedItem reap()
        {
            //if not fully grown - give seeds
            //if fully grown - give seeds and crop
            this.crop.toBeDeleted = true;
            Debug.Print(crop.getTag());
            this.occupied = false;
            DroppedItem item = new DroppedItem(playing.scene.getItem(crop.getTag()), new int[] { this.coords[0], this.coords[1] }, playing, this.tile, new int[] { 0, 1 });
            this.tile.attachDroppedItem(item);
            this.crop = null;
            return item;
        }
        public void water() { this.watered = true; this.setSpriteRectangle(spriteRectangles[1]); }
        public void dry() { this.watered = false; this.setSpriteRectangle(spriteRectangles[0]); }
        public void occupy(Crop crop)
        {
            if (!occupied)
            {
                Debug.Print(crop.getTag());
                this.crop = crop; occupied = true; playing.crops.Add(crop);
            }
        }

        public override Rectangle getHitBox() { return new Rectangle(); }
        public override float getDrawDepth()
        {
            return 0f;
        }

        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override bool getInside()
        {
            return false;
        }

        public override void useItem(Item item)
        {
            switch (item.tag)
            {
                case "WateringCan": { this.water(); break; }
            }
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class Scenery : Drawable
    {
        public StaticObject obj;
        public Playing playing;
        int particleTimer = 0;
        public int days = 0;
        int offset;
        bool inside;

        public Scenery(StaticObject obj, Vector2 coords, Playing playing, bool inside)
        {
            this.obj = obj;
            this.setTag(obj.tag);
            this.playing = playing;
            this.coords = new int[] { (int)coords.X, (int)coords.Y };
            this.setSpriteRectangle(obj.sprite);
            this.draw = playing.drawScenery;
            this.solid = obj.solid;
            this.inside = inside;
            this.offset = Constructor.rand.Next(1000, 30000);
        }

        public Scenery(StaticObject obj, int[] coords, Playing playing, bool inside)
        {
            this.obj = obj;
            this.setTag(obj.tag);
            this.playing = playing;
            this.coords = new int[] { coords[0] * 48, coords[1] * 48 };
            this.setSpriteRectangle(obj.sprite);
            this.draw = playing.drawScenery;
            this.solid = obj.solid;
            this.description = obj.description;
            this.inside = inside;
            this.offset = Constructor.rand.Next(1000, 30000);

            /*List<Tile> tiles = playing.scene.getMostlyContainedTiles(this.getHitBox(), .3f);
            foreach (Tile tile in tiles)
            {
                tile.attachStatic(this);
                if (this.getHitBox().X + this.getHitBox().Width < tile.getCoords()[0] + 48)
                {
                    this.coords[0] += ((tile.getCoords()[0] + 48) - (this.getHitBox().X + this.getHitBox().Width)) / 2;
                }
            }*/
        }
        public virtual void updateDaysPassed(int days) { }
        public int getDays() { return this.days; }
        public void update(GameTime gameTime)
        {
            if (obj.particle != null)
            {
                particleTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (particleTimer > obj.particle.frequency + offset)
                {
                    //account for multiple particles at once

                    particleTimer = 0;
                    if (obj.particle.below)
                    {
                        playing.particlesBelow.Add(new Particle(obj.particle, coords, new int[] { 0, 0 }, playing));
                    }
                    else playing.particlesAbove.Add(new Particle(obj.particle, coords, new int[] { 0, 0 }, playing));
                }
            }
            if (playing.onScreen(this)) { playing.visible.Add(this); }
            //Debug.Print(this.getTag() + Convert.ToString(this.coords[0] + "," + this.coords[1]));
        }
        public override float getDrawDepth()
        {
            if (obj.above) { return 1f; }
            if (obj.below) { return 0f; }
            return (this.coords[1] - playing.currentCorner[1] + (obj.sprite.Height * 3)) / (768f);
        }
        public override Rectangle getHitBox()
        {
            //Debug.Print(this.getTag() + ":" + Convert.ToString(this.solid));
            //Debug.Print(Convert.ToString(this.coords[0] + "," + obj.hitbox.X + "," + this.coords[1] + "," + obj.hitbox.Y + "," + obj.hitbox.Width + "," + obj.hitbox.Height));
            //if (!this.solid) { return new Rectangle(); }
            return new Rectangle(this.coords[0] + obj.hitbox.X, this.coords[1] + obj.hitbox.Y, obj.hitbox.Width, obj.hitbox.Height);
        }

        public override void interact()
        {
            playing.showMessage(this.description);
        }
        public override bool getInside()
        {
            return this.inside;
        }

        public override void useItem(Item item)
        {
            /*switch
             * axe - shake
             * hoe - shake
             * water - ??
            */
        }
        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class Chest : Drawable
    {
        Playing playing;
        StaticObject obj;
        bool inside;
        public int[] dimensions;
        Slot[,] slotArray;

        public Chest(Playing playing, int[] coords, StaticObject obj, bool inside, int[] dimensions)
        {
            this.playing = playing;
            this.obj = obj;
            this.coords = new int[] { coords[0], coords[1] };
            this.inside = inside;
            this.draw = playing.drawChest;
            this.setTag(obj.tag);
            this.setSpriteRectangle(obj.sprite);
            this.dimensions = dimensions;
            slotArray = new Slot[dimensions[0],dimensions[1]];
            this.solid = true;

            for (int i = 0; i < dimensions[0]; i++)
            {
                for (int j = 0; j < dimensions[1]; j++)
                {
                    slotArray[i, j] = new Slot(new int[] { i, j }, new Vector2( 657 + (j*69),147 + (i*69)), "chestSlot");
                }
            }
            slotArray[0, 0].setItem(Scene.items["Shovel"]);
        }

        public void update(GameTime gameTime)
        {

            if (playing.onScreen(this)) { playing.visible.Add(this); }
        }
        public Slot getSlotAt(int x, int y) { return slotArray[x, y]; }

        public bool mouseOver(out Slot slot) 
        {
            slot = null;
            MouseState mouse = Mouse.GetState();
            if (mouse.X > 657 && mouse.Y > 126) 
            {
                try
                {
                    slot = slotArray[(mouse.Y - 126) / 69, (mouse.X - 657) / 69];
                    return true;
                }
                catch (IndexOutOfRangeException e)
                {
                    return false;
                }
            }
            return false;
        }

        public override float getDrawDepth()
        {
            return (this.coords[1] - playing.currentCorner[1] + (obj.sprite.Height * 3)) / (768f);
        }

        public override Rectangle getHitBox()
        {
            return new Rectangle(this.coords[0] + obj.hitbox.X, this.coords[1] + obj.hitbox.Y, obj.hitbox.Width, obj.hitbox.Height);
        }

        public override void interact()
        {
            playing.player.setChest(this);
            playing.changeState("ChestState");
        }
        public override bool getInside()
        {
            return this.inside;
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class Building : Drawable
    {
        public BuildingType type;
        public List<Wall> walls = new List<Wall>();
        public List<Scenery> scenery = new List<Scenery>();
        public List<Chest> chests = new List<Chest>();
        public List<Bed> beds = new List<Bed>();
        Playing playing;
        public bool roofVisible = true;
        public bool fadeIn = false;
        public bool fadeOut = false;
        public int roofAlpha = 255;
        public Rectangle bounds;

        public Building(BuildingType type, int[] coords, Playing playing)
        {
            //48 = tile width and height

            this.type = type;
            this.setTag(type.tag);
            this.coords = new int[] {coords[0] * 48, coords[1] * 48};
            this.playing = playing;
            this.setSpriteRectangle(new Rectangle(0, 0, type.width, type.height));
            this.bounds = new Rectangle(this.coords[0], this.coords[1], type.width, type.height);
            for (int i = 0; i < type.wallSprites.Count; i++)
            {
                walls.Add(new Wall(type.wallSprites[i], new int[] { type.wallLocations[i][0] + (this.coords[0]), type.wallLocations[i][1] + (this.coords[1]) }, new Rectangle(type.hitBoxes[i].X + (this.coords[0]), type.hitBoxes[i].Y + (this.coords[1]), type.hitBoxes[i].Width, type.hitBoxes[i].Height), playing));
            }
            for (int i = 0; i < type.statics.Count; i++)
            {
                this.scenery.Add(new Scenery(type.statics[i], new Vector2( (type.staticLocations[i][0] * 3) + (this.coords[0]), (type.staticLocations[i][1] * 3) + (this.coords[1])), playing, true));
            }
            for (int i = 0; i < type.chests.Count; i++)
            {
                this.chests.Add(new Chest(playing, new int[] { type.chestLocation[i][0] * 3 + this.coords[0], type.chestLocation[i][1] * 3 + this.coords[1] }, type.chests[i], true, type.chestDimensions[i]));
            }
            for (int i = 0; i < type.beds.Count; i++)
            {
                this.beds.Add(new Bed(type.beds[i], new int[] { type.bedLocations[i][0] * 3 + this.coords[0], type.bedLocations[i][1] * 3 + this.coords[1] }, playing, true));
            }
        }
        public void update(GameTime gameTime)
        {
            if (fadeIn && roofAlpha < 255) { roofAlpha += 10; if (roofAlpha >= 255) { fadeIn = false; roofAlpha = 255; } }
            else if (fadeOut && roofAlpha > 0) { roofAlpha -= 10; if (roofAlpha <= 0) { fadeOut = false; roofAlpha = 0; } }

            foreach (Wall wall in walls) { wall.update(gameTime); }
            foreach (Scenery prop in this.scenery) { prop.update(gameTime);  }
            foreach (Chest chest in this.chests) { chest.update(gameTime); }
            foreach (Bed bed in this.beds) { bed.update(gameTime); }
        }
        public void fadeRoofOut()
        {
            fadeOut = true;
            roofVisible = false;
        }
        public void fadeRoofIn()
        {
            fadeIn = true;
            roofVisible = true;
        }

        public override Rectangle getHitBox()
        {
            throw new NotImplementedException();
        }
        public override float getDrawDepth()
        {
            return 0f;
        }

        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override bool getInside()
        {
            return true;
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }
    class Wall : Drawable
    {
        public Rectangle hitBox;
        Playing playing;

        public Wall(Rectangle spriteRectangle, int[] coords, Rectangle hitBox, Playing playing)
        {
            this.solid = true;
            this.setSpriteRectangle(spriteRectangle);
            this.coords = coords;
            this.hitBox = hitBox;
            this.playing = playing;
            this.draw = playing.drawWall;
            this.setTag("Wall");
        }
        public override Rectangle getHitBox() { return this.hitBox; }
        public void update(GameTime gameTime)
        {
            if (playing.onScreen(this)) { playing.visible.Add(this); }
        }

        public override float getDrawDepth()
        {
            return (this.coords[1] - playing.currentCorner[1] + (this.getSpriteRectangle().Height * 3)) / (768f);
        }


        public override void interact()
        {
            //play knocking sound?
        }

        public override bool getInside()
        {
            return true;
        }

        public override void useItem(Item item)
        {
            throw new NotImplementedException();
        }

        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1]);
        }
    }

    #region Types and base classes
    class StaticObject
    {
        public string tag;
        public ParticleType particle;
        public bool destructible;
        public Rectangle hitbox;
        public Rectangle sprite;
        public bool above;
        public bool below;
        public bool solid;
        public string description;

        public StaticObject(string tag, bool destructible, Rectangle spriteRectangle, Rectangle hitbox, bool above, bool below, bool solid, string description)
        {
            this.tag = tag;
            this.description = description;
            this.destructible = destructible;
            this.sprite = spriteRectangle;
            this.hitbox = hitbox;
            this.above = above;
            this.below = below;
            this.solid = solid;
        }
        public void setParticle(ParticleType particle)
        {
            this.particle = particle;
        }
    }
    class Item
    {
        public int useTimer;
        public Rectangle spriteRectangle;
        public string tag;
        public string type;
        public List<Attribute> attributes = new List<Attribute>();

        public Item(string tag, Rectangle sprite, int useTimer, string type)
        {
            this.tag = tag;
            this.spriteRectangle = sprite;
            this.useTimer = useTimer;
            this.type = type;
        }
    }

    class CropType
    {
        public List<Rectangle> spriteRectangles = new List<Rectangle>();
        public Rectangle seedSprite;
        public string tag;
        public string particle;
        public Item tool;
        public bool solid;
        public int growth;

        public CropType(string tag, string particle, Item tool, bool solid, int growth, Rectangle seedSprite)
        {
            this.tag = tag;
            this.particle = particle;
            this.tool = tool;
            this.solid = solid;
            this.growth = growth;
            this.seedSprite = seedSprite;
        }
        public void addSprite(Rectangle sprite) { spriteRectangles.Add(sprite); }

    }
    class ParticleType
    {
        public Rectangle spawnRegion;
        public int frequency;
        public int life;
        public bool above;
        public bool below;
        public string type;

        public ParticleType(Rectangle spawnRegion, string type, int frequency, int life, bool above, bool below)
        {
            this.frequency = frequency;
            this.type = type;
            this.spawnRegion = spawnRegion;
            this.life = life;
            this.above = above;
            this.below = below;
        }
    }

    class BuildingType
    {
        public List<Rectangle> wallSprites = new List<Rectangle>();
        public List<int[]> wallLocations = new List<int[]>();
        public List<Rectangle> hitBoxes = new List<Rectangle>();

        public List<StaticObject> statics = new List<StaticObject>();
        public List<int[]> staticLocations = new List<int[]>();

        public List<StaticObject> beds = new List<StaticObject>();
        public List<int[]> bedLocations = new List<int[]>();

        public List<StaticObject> chests = new List<StaticObject>();
        public List<int[]> chestLocation = new List<int[]>();
        public List<int[]> chestDimensions = new List<int[]>();

        public Rectangle roofSprite;
        public Rectangle floorSprite;
        public int[] floorOffset;
        public int width;
        public int height;

        public string tag;

        public BuildingType(string tag, Rectangle floorSprite, int[] floorOffset, Rectangle roofSprite, int width, int height)
        {
            this.tag = tag;
            this.floorSprite = floorSprite;
            this.floorOffset = floorOffset;
            this.roofSprite = roofSprite;
            this.width = width;
            this.height = height;
        }
        public void addStatic(int[] coords, StaticObject obj)
        {
            this.staticLocations.Add(coords);
            this.statics.Add(obj);
        }
        public void addChest(int[] coords, StaticObject obj, int[] dimensions) 
        {
            this.chests.Add(obj);
            this.chestLocation.Add(coords);
            this.chestDimensions.Add(dimensions);
        }
        public void addBed(StaticObject bed, int[] location)
        {
            this.beds.Add(bed);
            this.bedLocations.Add(location);
        }
        public void addWall(Rectangle sprite, int[] location, Rectangle hitbox)
        {
            wallSprites.Add(sprite);
            wallLocations.Add(location);
            hitBoxes.Add(hitbox);
        }
    }

    abstract class Drawable : IComparable<Drawable>
    {
        private string tag;
        public string description;
        public double time = 0;
        public int[] coords;
        public int level;
        public float rotation;
        public Rectangle spriteRectangle;
        public Draw draw;
        public bool flipped = false;
        public bool solid = false; 
        public int alpha = 255;
        public int XSize = 3;
        public int YSize = 3;
        //use level for animated sprites

        public bool toBeDeleted = false;
        public bool escaped = false;

        public void setTag(string tag) { this.tag = tag; }
        public string getTag() { return this.tag; }
        public Rectangle getSpriteRectangle() { return spriteRectangle; }
        public void setSpriteRectangle(Rectangle spriteRectangle) { this.spriteRectangle = spriteRectangle; }
        public Rectangle locationRectangle { get { return locationRectangle; } set { locationRectangle = value; } }
        public abstract float getDrawDepth();
        public abstract Rectangle getHitBox();
        public abstract bool getInside();

        public abstract void interact();
        public delegate void Draw(Drawable drawable);
        public abstract void useItem(Item item);

        public bool collides(Drawable other)
        {
            return this.locationRectangle.Intersects(other.locationRectangle);
        }
        public bool collides(Rectangle other)
        {
            return this.locationRectangle.Intersects(other);
        }

        public int CompareTo(Drawable other)
        {
            return this.getDrawDepth().CompareTo(other.getDrawDepth());
        }
        public abstract Vector2 getCoords(int[] currentcorner);
    }
    #endregion

}
