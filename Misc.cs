using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace RainFrogs
{
    abstract class ActivityRegion
    {
        public Rectangle location;
        public Playing playing;

        public bool inside(Rectangle entityLocation) 
        {
            return entityLocation.Intersects(location);
        }

        public string directionLeft(Rectangle entityLocation)
        {
            if ( playing.player.facing[1] == -1)
            {
                return "N";
            }
            if ( playing.player.facing[1] == 1)
            {
                return "S";
            }
            if ( playing.player.facing[0] == 1)
            {
                return "E";
            }
            if (playing.player.facing[0] == -1)
            {
                return "W";
            }
            return null;
        }
        public abstract bool update(Rectangle entityLocation);
        public Rectangle getHitBox() 
        {
            return this.location;
        }
        public abstract void trigger(string direction);
    }
    class Doorway : ActivityRegion
    {
        Building building;
        string buildingDir;
        string exteriorDir;

        public Doorway(Rectangle location, string buildingDir, string exteriorDir, Building building, Playing playing)
        {
            this.location = location;
            this.buildingDir = buildingDir;
            this.exteriorDir = exteriorDir;
            this.building = building;
            this.playing = playing;
        }
        public override bool update(Rectangle entityLocation) 
        {
            if (!this.inside(entityLocation)) { this.trigger(this.directionLeft(entityLocation)); return true; }
            return false;
        }
        public override void trigger(string direction) 
        {
            if (direction.Equals(buildingDir)) { playing.showMessage(building.getTag()); playing.unpaused.enterBuilding(building); }
            else { playing.unpaused.leaveBuilding(); }
        }
    }
    class Bed : Scenery
    {
        public Bed(StaticObject obj, int[] coords, Playing playing, bool inside) : base(obj, coords, playing, inside) 
        {
            this.coords = new int[] {coords[0], coords[1]};
        }

        public override void interact()
        {
            playing.newDay();
            //!!!handle other new day related junk here (maybe?)
        }
    }
    class Grass : Scenery
    {
        int offset = 0;

        public Grass(int[] coords, Playing playing, bool inside, StaticObject obj, int days = 0)
            : base(obj, coords, playing, inside) 
        {
            base.obj = Scene.scenery["TallGrass1"];
            base.days = days;
            //region
        }
        public void Grow()
        {
            //days = MathHelper.Clamp(0,days++,10);
            if (days >= 10) { base.obj = Scene.scenery["TallGrass2"]; base.setSpriteRectangle(obj.sprite); offset = 30; }
            else if (days >= 2) { base.obj = Scene.scenery["TallGrass1"]; base.setSpriteRectangle(obj.sprite); offset = 12; }
            else { base.obj = Scene.scenery["TallGrassCut"]; base.setSpriteRectangle(obj.sprite); offset = 0; }
        }
        public override void updateDaysPassed(int dayNum)
        {
            this.days += dayNum;
            this.Grow();
        }
        public void Cut()
        {
            if (days >= 2)
            {
                playing.particlesBelow.Add(new Particle(new ParticleType(new Rectangle(), "grass", 0, 420, false, true),new int[] {this.coords[0] + spriteRectangle.Width/2,this.coords[1] + spriteRectangle.Height/2},new int[] {Constructor.rand.Next(-2,2),Constructor.rand.Next(-2,2)},playing));
                playing.particlesBelow.Add(new Particle(new ParticleType(new Rectangle(), "grass", 0, 420, false, true), new int[] { this.coords[0] + spriteRectangle.Width / 2, this.coords[1] + spriteRectangle.Height / 2 }, new int[] { Constructor.rand.Next(-2, 2), Constructor.rand.Next(-2, 2) }, playing));
                playing.particlesBelow.Add(new Particle(new ParticleType(new Rectangle(), "grass", 0, 420, false, true), new int[] { this.coords[0] + spriteRectangle.Width / 2, this.coords[1] + spriteRectangle.Height / 2 }, new int[] { Constructor.rand.Next(-2, 2), Constructor.rand.Next(-2, 2) }, playing));
                playing.particlesBelow.Add(new Particle(new ParticleType(new Rectangle(), "grass", 0, 420, false, true), new int[] { this.coords[0] + spriteRectangle.Width / 2, this.coords[1] + spriteRectangle.Height / 2 }, new int[] { Constructor.rand.Next(-2, 2), Constructor.rand.Next(-2, 2) }, playing));
                playing.particlesBelow.Add(new Particle(new ParticleType(new Rectangle(), "grass", 0, 420, false, true), new int[] { this.coords[0] + spriteRectangle.Width / 2, this.coords[1] + spriteRectangle.Height / 2 }, new int[] { Constructor.rand.Next(-2, 2), Constructor.rand.Next(-2, 2) }, playing));
                playing.particlesBelow.Add(new Particle(new ParticleType(new Rectangle(), "grass", 0, 420, false, true), new int[] { this.coords[0] + spriteRectangle.Width / 2, this.coords[1] + spriteRectangle.Height / 2 }, new int[] { Constructor.rand.Next(-2, 2), Constructor.rand.Next(-2, 2) }, playing));
                playing.particlesBelow.Add(new Particle(new ParticleType(new Rectangle(), "grass", 0, 420, false, true), new int[] { this.coords[0] + spriteRectangle.Width / 2, this.coords[1] + spriteRectangle.Height / 2 }, new int[] { Constructor.rand.Next(-2, 2), Constructor.rand.Next(-2, 2) }, playing));
                playing.particlesBelow.Add(new Particle(new ParticleType(new Rectangle(), "grass", 0, 420, false, true), new int[] { this.coords[0] + spriteRectangle.Width / 2, this.coords[1] + spriteRectangle.Height / 2 }, new int[] { Constructor.rand.Next(-2, 2), Constructor.rand.Next(-2, 2) }, playing));
                this.toBeDeleted = true;
                playing.scene.addDroppedItem(new DroppedItem(new Seed(playing.scene.getCrop("Wheat")), new int[] { this.coords[0], this.coords[1] }, playing));
            }
        }


        public override void interact()
        {
            throw new NotImplementedException();
        }

        public override void useItem(Item item)
        {
            if (item.tag.Equals("Scythe")) { this.Cut(); }
        }
        public override Vector2 getCoords(int[] currentcorner)
        {
            return new Vector2(this.coords[0] - currentcorner[0], this.coords[1] - currentcorner[1] - offset);
        }
        public override Rectangle getHitBox()
        {
            return new Rectangle(this.coords[0] + obj.hitbox.X, this.coords[1] + obj.hitbox.Y - offset, obj.hitbox.Width, obj.hitbox.Height);
        }
    }

    class Seed : Item 
    {
        CropType cropType;

        public Seed(CropType cropType)
            : base(cropType.tag + " Seeds", Scene.items[string.Format("{0} Seeds",cropType.tag)].spriteRectangle, 420, "Tool") 
        {
            this.cropType = cropType;
            string colorString;
            Color color = Playing.getRandomColor(out colorString);
            if (colorString != null) attributes.Add(new Attribute(colorString, color, new Rectangle(), false));
            color = Playing.getRandomColor(out colorString);
            if (colorString != null) attributes.Add(new Attribute(colorString, color, new Rectangle(), false));
            color = Playing.getRandomColor(out colorString);
            if (colorString != null) attributes.Add(new Attribute(colorString, color, new Rectangle(), false));
        }
        public string getTag() { return base.tag; }
    }
    public class Attribute 
    {
        public string tag;
        public Color color;
        public Rectangle sprite;
        bool over; // if false, under
        //bool animated;
        //spriteEffect shader;
        public Attribute(string tag, Color color, Rectangle sprite, bool over)
        {
            this.tag = tag;
            this.color = color;
            this.sprite = sprite;
            this.over = over;
        }
    }
    /* ATTRIBUTE CLASSES
     * -color
     * -sprite overlay
     * -sprite underlay
     * -animation
     * -shader
     * 
     * return string
     */
}
