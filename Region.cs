using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace RainFrogs
{
    class Region
    {
        public string tag;
        int[] dimensions;
        int[] cornerCoords;
        byte[,] tileGrid;
        public byte metaNum;

        public Region(string tag, int[] dimensions, int[] cornerCoords, byte metaNum)
        {
            this.tag = tag;
            this.dimensions = dimensions;
            this.cornerCoords = cornerCoords;
            this.tileGrid = new byte[dimensions[0], dimensions[1]];
            this.metaNum = metaNum;

            for (int i = 0; i < dimensions[0]; i++) 
            {
                for (int j = 0; j < dimensions[1]; j++) 
                {
                    tileGrid[i, j] = 0;
                }
            }
        }
        public void turnOnTile(int[] coords)
        {
            tileGrid[coords[0], coords[1]] = 1;
        }
    }
    class Chunk
    {
        Tile[,] tiles = new Tile[30,18];
        public int[] currentCorner;
        int daysPassed;
        int[] dimensions = new int[] { 30, 18 };
        public int[] coords;
        public bool generated = false;
        public bool loaded = false;
        public List<Scenery> scenery = new List<Scenery>();
        public List<Crop> crops = new List<Crop>();
        public List<FarmTile> farmTiles = new List<FarmTile>();
        public List<DroppedItem> droppedItems = new List<DroppedItem>();
        public List<Scenery> hangingOff = new List<Scenery>();
        Queue<string> sceneryQueue = new Queue<string>();

        public string dateLastUpdated;

        string state;

        public Chunk(int[] corner, string state)
        {
            //TEMPORARY STRING
            this.state = state;
            this.currentCorner = corner;
            this.coords = new int[] { corner[0] / 48 / 30, corner[1] / 48 / 18 };
        }

        public Tile getTile(int[] coords) { if (this.generated) { return tiles[(coords[0] - currentCorner[0]) / 48, (coords[1] - currentCorner[1]) / 48]; } else return null; }

        public void update(Playing playing) 
        {

        }
        public void newDay(Playing playing)
        {
            this.dateLastUpdated = playing.getDate();
            foreach (Scenery prop in scenery) { prop.updateDaysPassed(1); }
        }

        public void Draw(SpriteBatch spriteBatch, int[] screenCorner, Texture2D tileSheet, int tileAlpha)
        {
            for (int i = 0; i < dimensions[0]; i++)
            {
                for (int j = 0; j < dimensions[1]; j++)
                {
                    int tileNum = tiles[i, j].getNum();
                    int metaNum = tiles[i, j].metaNum;
                    spriteBatch.Draw(tileSheet, new Vector2(tiles[i, j].getCoords()[0] + currentCorner[0] - screenCorner[0], tiles[i, j].getCoords()[1] + currentCorner[1] - screenCorner[1]), new Rectangle(16 * tileNum, 16 * metaNum, 16, 16), new Color(255, 255, 255, tileAlpha), 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                    
                    if (Playing.drawOccupied && tiles[i, j].occupied)
                    {
                        spriteBatch.Draw(tileSheet, new Vector2(tiles[i, j].getCoords()[0] + currentCorner[0] - screenCorner[0], tiles[i, j].getCoords()[1] + currentCorner[1] - screenCorner[1]), new Rectangle(16 * tileNum, 16 * metaNum, 16, 16), new Color(0, 0, 0, tileAlpha), 0f, new Vector2(0, 0), new Vector2(3, 3), SpriteEffects.None, 0f);
                        spriteBatch.DrawString(Playing.output, string.Format("{0}", tiles[i, j].occupiedBy.Count), new Vector2(tiles[i, j].getCoords()[0] + currentCorner[0] - screenCorner[0], tiles[i, j].getCoords()[1] + currentCorner[1] - screenCorner[1]), Color.Red);
                    }
                }
            } 
            spriteBatch.DrawString(Playing.output, string.Format("{0},{1}", this.coords[0], this.coords[1]), new Vector2(this.currentCorner[0] - screenCorner[0], this.currentCorner[1] - screenCorner[1]), Playing.getColor("Red"));
            
        }
        public bool contains(Rectangle location) 
        {
            return new Rectangle(this.currentCorner[0],this.currentCorner[1],30*48,18*48).Contains(location);
        }
        public Rectangle getRectangle() { return new Rectangle(this.currentCorner[0], this.currentCorner[1], 30 * 48, 30 * 18); }
        public bool intersects(Rectangle location) { return new Rectangle(this.currentCorner[0], this.currentCorner[1], 30 * 48, 18 * 48).Intersects(location); }
        public Tile[,] getTileArray() { return this.tiles; }
        public void populate(Playing playing)
        {
            XmlDocument xmlReader = new XmlDocument();
            xmlReader.LoadXml(this.state);

            XmlNode chunk = xmlReader.ChildNodes[0];

            string[] tileArray = chunk.ChildNodes[0].InnerText.Split('\n');
            for (int i = 0; i < tiles.GetLength(1); i++) 
            {
                string[] line = tileArray[i+1].Split(',','\r');
                for (int j = 0; j < tiles.GetLength(0); j++) 
                {
                    byte tileNum = Convert.ToByte(line[j].Trim(' ','\r'));
                    tiles[j, i] = new Tile(tileNum, Constructor.tileTypes[tileNum], j * 48, i * 48, Constructor.rand, Constructor.regions[tileNum]);
                }
            }

            daysPassed = Clock.getDayDifference(playing.getDate(), chunk.ChildNodes[1].InnerText);
            this.dateLastUpdated = playing.getDate();
            //Debug.Print("{0},{1} : {2}",this.coords[0], this.coords[1], daysPassed);

            for (int i = 2; i < chunk.ChildNodes.Count; i++)
            {
                sceneryQueue.Enqueue(chunk.ChildNodes[i].Name +","+ chunk.ChildNodes[i].InnerText);
            }

            this.generated = true;
        }

        public bool loadScenery(Playing playing)
        {
            if (sceneryQueue.Count == 0) { daysPassed = 0; loaded = true; return true; }

            string[] stringArray = sceneryQueue.Dequeue().Split(',');
            switch (stringArray[0])
            {
                case "static":
                    {
                        this.addScenery(new Scenery(Scene.scenery[stringArray[1]], new int[] { Convert.ToInt32(stringArray[2]) / 48, Convert.ToInt32(stringArray[3]) / 48 }, playing, stringArray[4].Equals("True")));

                        //!!!Occupy tiles based on "Mostly inside" >80%
                        int tileCount = 0;
                        //!!!don't loop through all tiles, mathematically find closest tiles
                        foreach (Tile tile in tiles)
                        {
                            if (this.scenery[scenery.Count - 1].getHitBox().Intersects(new Rectangle(tile.getLocation().X + currentCorner[0], tile.getLocation().Y + currentCorner[1], tile.getLocation().Width, tile.getLocation().Height)))
                            {
                                tile.occupiedBy.Add(this.scenery[scenery.Count - 1]);
                                tile.occupied = true;
                                tileCount++;
                            }
                        }
                        if (Scene.scenery[stringArray[1]].hitbox.Width * Scene.scenery[stringArray[1]].hitbox.Height > (48*48)* tileCount)
                        { playing.scene.floatingObjects.Add(this.scenery[scenery.Count - 1]); hangingOff.Add(this.scenery[scenery.Count - 1]); }
                        break;
                    }
                case "item":
                    {
                        this.addDroppedItem(new DroppedItem(Scene.items[stringArray[1]], new int[] { Convert.ToInt32(stringArray[2]), Convert.ToInt32(stringArray[3]) }, playing));
                        break;
                    }
                case "grass":
                    {
                        this.addScenery(new Grass(new int[] { Convert.ToInt16(stringArray[2]) / 48, Convert.ToInt16(stringArray[3]) / 48 }, playing, false, Scene.scenery["TallGrassCut"], daysPassed + Convert.ToInt16(stringArray[1])));

                        foreach (Tile tile in tiles)
                        {
                            if (this.scenery[scenery.Count - 1].getHitBox().Intersects(new Rectangle(tile.getLocation().X + currentCorner[0], tile.getLocation().Y + currentCorner[1], tile.getLocation().Width, tile.getLocation().Height)))
                            {
                                tile.occupiedBy.Add(this.scenery[scenery.Count - 1]);
                                tile.occupied = true;
                            }
                        }
                        break;
                    }
                case "farmTile":
                    {
                        Tile tile = tiles[Convert.ToInt16(stringArray[1]) / 48,Convert.ToInt16(stringArray[2]) / 48];
                        this.farmTiles.Add(new FarmTile(tile, this, playing));
                        tile.setFarmTile(farmTiles[farmTiles.Count - 1]);
                        break;
                    }
            }
            return false;
        }

        public void destroy() 
        { 
            this.loaded = false;
            this.generated = false;
            this.state = this.ToString();
            scenery.Clear();
            crops.Clear();
            farmTiles.Clear();
            hangingOff.Clear();
        }
        public string getState() { return this.state; }
        public void addScenery(Scenery scenery) { this.scenery.Add(scenery); }
        public void addDroppedItem(DroppedItem item) { this.droppedItems.Add(item); }
        public void addFarmTile(FarmTile farmTile) { this.farmTiles.Add(farmTile); }
        public void addCrop(Crop crop) { this.crops.Add(crop); }
        public override string ToString() 
        {
            string export = string.Format("<chunk x=\"{0}\" y=\"{1}\">\n<tiles>\n",coords[0],coords[1]);
            for (int i = 0; i < tiles.GetLength(1); i++) 
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    export += tiles[j, i].getNum();
                    if (j != tiles.GetLength(0) - 1) { export += ",";}
                }
                export += "\n";
            }
            export += string.Format("</tiles>\n<time>{0}</time>\n", dateLastUpdated);
            
            foreach (Scenery prop in scenery) 
            { 
                //static object tag, coords, inside bool
                if (prop is Grass) { export += "<grass>" + prop.getDays() + "," + prop.coords[0] + "," + prop.coords[1] + "</grass>\n"; }
                else export += "<static>"+prop.getTag() + "," + prop.coords[0] + "," + prop.coords[1] + "," + prop.getInside().ToString()+"</static>\n";
            }
            foreach (DroppedItem item in droppedItems)
            {
                export += "<item>" + item.getTag() + "," + item.coords[0] + "," + item.coords[1] 
                    //attributes
                    + "</item>\n";
            }
            foreach (FarmTile farmTile in farmTiles)
            {
                export += "<farmTile>" + farmTile.tile.getCoords()[0] + "," + farmTile.tile.getCoords()[1] + "</farmTile>\n";
            }
            export += "</chunk>";
            //File.WriteAllText("chunk.dat", export);
            return export;
        }
        public static string chunkDirection(Chunk center, Chunk outside)
        {
            //only neighbors
            int x = (outside.currentCorner[0] - center.currentCorner[0]) / (center.dimensions[0]*48);
            int y = (outside.currentCorner[1] - center.currentCorner[1]) / (center.dimensions[1]*48);
            if (x == 0 && y == 0) { return "SAME"; }
            else if (x == -1 && y == 0) { return "W"; }
            else if (x == 1 && y == 0) { return "E"; }
            else if (x == -1 && y == 1) { return "SW"; }
            else if (x == 1 && y == 1) { return "SE"; }
            else if (x == 0 && y == 1) { return "S"; }
            else if (x == -1 && y == -1) { return "NW"; }
            else if (x == 1 && y == -1) { return "NE"; }
            else if (x == 0 && y == -1) { return "N"; }
            else return "WRONG";

        }
    }
}
