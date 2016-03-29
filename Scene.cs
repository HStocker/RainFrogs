using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace RainFrogs
{
    class Scene
    {
        int[] dimensions;
        Constructor constructor;
        public Playing playing;
        //public List<Event> events = new List<Event>();
        //TextBox textBox;
        //int[,] triggerArray;
        //public string currentArea;
        int level = 1;
        static public Dictionary<string, string> interactions = new Dictionary<string, string>();
        static public Dictionary<string, Item> items = new Dictionary<string, Item>();
        static public Dictionary<string, StaticObject> scenery = new Dictionary<string, StaticObject>();
        static public Dictionary<string, CropType> crops = new Dictionary<string, CropType>();
        static public Dictionary<string, BuildingType> buildingTypes = new Dictionary<string, BuildingType>();
        XmlDocument xmlReader = new XmlDocument();

        public Chunk currentChunk;
        public List<Chunk> liveChunks;
        public List<Chunk> loadingChunks = new List<Chunk>();
        public List<Scenery> floatingObjects = new List<Scenery>();
        public bool loading = true;

        public Region currentRegion;
        public Dictionary<string, Region> regions = new Dictionary<string, Region>();

        public Scene(Playing playing)
        {
            this.playing = playing;

            xmlReader.Load("Content/Statics.xml");
            this.parseItems();
            this.parseScenery();
            this.parseCrops();
            this.parseBuildings();

        }

        public void build()
        {
            this.constructor = new Constructor(string.Format("Level{0}.csv", level), new int[] { playing.currentCorner[0] + playing.player.coords[0], playing.currentCorner[1] + playing.player.coords[1] }, playing);
            this.currentChunk = constructor.getCurrentChunk(new int[] { playing.currentCorner[0] + playing.player.coords[0], playing.currentCorner[1] + playing.player.coords[1] });
            liveChunks = constructor.getLiveChunks();
            dimensions = constructor.getDimensions();
            xmlReader.Load("Content/RegionData.xml");
            this.parseRegions();
            this.currentRegion = regions[this.getTile((playing.player.coords[0] + playing.currentCorner[0]), (playing.player.coords[1] + playing.currentCorner[1])).region];

        }

        //!!!COLLIDE BASED ON ABSOLUTE COORDINATES
        public bool onlyVisibleCollides(Rectangle location)
        {
            foreach (Drawable prop in playing.visible)
            {
                if (prop.getDrawDepth() == 1f) { break; }
                else if (prop.solid && location.Intersects(prop.getHitBox())) { return true; }
            }
            /*
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (
                        constructor.getTileArray()[(location.X / 48) + i, (location.Y / 48) + j].getNum() == 1
                        && constructor.getTileArray()[(location.X / 48) + i, (location.Y / 48) + j].getLocation().Intersects(location)) 
                    {  return true; }
                }
            } */
            //loop through nine tiles around player hitbox
            return false;
        }
        public bool onlyVisibleCollides(Rectangle location, out string collided)
        {
            foreach (Drawable prop in playing.visible)
            {
                if (prop.getDrawDepth() == 1f) { break; }
                else if (prop.solid && location.Intersects(prop.getHitBox())) { collided = prop.getTag(); return true; }
            }
            /*
            for (int i = 0; i < 3; i++) 
            {
                for (int j = 0; j < 3; j++) 
                {
                    if (
                        constructor.getTileArray()[(location.X / 48) + i, (location.Y / 48) + j].getNum() == 1
                        && constructor.getTileArray()[(location.X / 48) + i, (location.Y / 48) + j].getLocation().Intersects(location)) { collided = "Water"; return true; }
                }
            }*/
            //loop through nine tiles around player hitbox
            collided = null;
            return false;
        }
        public bool onlyVisibleCollides(Rectangle location, out Drawable collided)
        {
            foreach (Drawable prop in playing.visible)
            {
                if (prop.getDrawDepth() == 1f) { break; }
                else if (prop.solid && location.Intersects(prop.getHitBox())) { collided = prop; return true; }
            }
            /*
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (constructor.getTileArray()[(location.X / 48) + i, (location.Y / 48) + j].getNum() == 1
                        && constructor.getTileArray()[(location.X / 48) + i, (location.Y / 48) + j].getLocation().Intersects(location)) 
                    { collided = null; return true; }
                }
            }*/
            //loop through nine tiles around player hitbox
            collided = null;
            return false;
        }

        public bool getItemAt(Rectangle location, out DroppedItem item)
        {
            //!!!CONSIDER ORDER OF UPDATING - MUST BE USED AFTER DROPPED ITEMS HAVE BEEN INSERTED TO THE VISIBLE LIST
            foreach (Drawable prop in playing.visible)
            {
                if (items.ContainsKey(prop.getTag()) && new Rectangle(prop.coords[0], prop.coords[1], 54, 54).Intersects(location))
                {
                    item = (DroppedItem)prop;
                    return true;
                }
            }
            item = null;
            return false;
        }
        public void update(GameTime gameTime, int[] playerCoords)
        {
            Chunk playerChunk = constructor.getChunkAt(new int[] { playerCoords[0] + playing.currentCorner[0], playerCoords[1] + playing.currentCorner[1] + playing.player.getHitBox().Y });

            if (!loading && !playerChunk.Equals(currentChunk))
            {
                loading = true;
                List<Chunk> neighborChunks = constructor.getNeighboringChunks(playerChunk);
                neighborChunks.Add(playerChunk);

                int count = 0;
                int max = liveChunks.Count;
                for (int i = 0; i < max; i++)
                {
                    if (!neighborChunks.Contains(liveChunks[count])) { liveChunks[count].destroy(); liveChunks.RemoveAt(count); }
                    else { neighborChunks.Remove(liveChunks[count]);  count++; }
                }
                foreach (Chunk chunk in liveChunks) { foreach (Scenery prop in chunk.hangingOff) { if (!containedInLoadedChunks(prop.getHitBox())) { floatingObjects.Add(prop); } } }
                loadingChunks = neighborChunks;
                currentChunk = playerChunk;
            }
            if (loading)
            {
                if (loadingChunks.Count == 0) 
                { 
                    loading = false;

                        //Debug.Print("{0}",liveChunks.Count);
                    
                        for (int j = floatingObjects.Count - 1; j >= 0; j--)
                        {
                            for (int i = 0; i < liveChunks.Count; i++)
                            {
                                Rectangle overlapping = Rectangle.Intersect(floatingObjects[j].getHitBox(), liveChunks[i].getRectangle());
                                //Debug.Print("{0},{1}", floatingObjects[j].getHitBox(), liveChunks[i].getRectangle());
                                int xOverlap = (overlapping.Width + 47) / 48;
                                int yOverlap = (overlapping.Height + 47) / 48;

                                //Debug.Print("{0},{1},{2}, hitbox: {3},{4}", overlapping, xOverlap, yOverlap, (floatingObjects[j].getHitBox().Width + 47) / 48, (floatingObjects[j].getHitBox().Height + 47) / 48);
                                if (xOverlap > 0 && yOverlap > 0)
                                {
                                    for (int x = 0; x < (floatingObjects[j].getHitBox().Width + 47) / 48; x++)
                                    {
                                        Tile tile = liveChunks[i].getTile(new int[] { overlapping.X + 48*x, overlapping.Y});
                                        for (int y = 0; y < (floatingObjects[j].getHitBox().Height + 47) / 48; y++)
                                        {
                                            if (floatingObjects[j].getHitBox().Intersects(new Rectangle(tile.getLocation().X + liveChunks[i].currentCorner[0], tile.getLocation().Y + liveChunks[i].currentCorner[1], tile.getLocation().Width, tile.getLocation().Height)))
                                            {
                                                //Debug.Print("occupied");
                                                tile.occupiedBy.Add(floatingObjects[j]);
                                                tile.occupied = true;
                                            }
                                            tile = liveChunks[i].getTile(new int[] { tile.getLocation().X + liveChunks[i].currentCorner[0], tile.getLocation().Y + 48 + liveChunks[i].currentCorner[1] });
                                        }
                                    }
                                }
                            }
                            //remove floating objects when out of scope
                            if (containedInLoadedChunks(floatingObjects[j].getHitBox())) { floatingObjects.Remove(floatingObjects[j]); }
                            foreach (Scenery prop in floatingObjects) { Debug.Print("still floating: "+prop.getTag()); }
                    }
                }
                else for (int i = loadingChunks.Count - 1; i >= 0; i--)
                    {
                        if (!loadingChunks[i].generated)
                        {
                            constructor.generateNewChunk(loadingChunks[i], playing);
                            //Debug.Print("{0}", floatingObjects.Count);
                            //floating object, tuple(object, area left)
                        }
                        if (loadingChunks[i].loadScenery(playing)) { liveChunks.Add(loadingChunks[i]); loadingChunks.RemoveAt(i); }
                    }
            }
            //!!!MUST USE getTile() AFTER CHECK TO SEE IF INSIDE CHUNK
            Tile currentTile = this.getTile((playing.player.coords[0] + playing.currentCorner[0]), (playing.player.coords[1] + playing.currentCorner[1] + playing.player.getHitBox().Y));

            if (!currentTile.region.Equals(currentRegion.tag))
            {
                //!!!use regions to stop player from entering water
                currentRegion = regions[currentTile.region];
            }
        }
        public bool containedInLoadedChunks(Rectangle prop)
        {
            Chunk topLeft = currentChunk;
            Chunk bottomRight = currentChunk;
            foreach (Chunk chunk in liveChunks) { 
                if (chunk.currentCorner[0] < topLeft.currentCorner[0] && chunk.currentCorner[1] < topLeft.currentCorner[1]) { topLeft = chunk; }
                if (chunk.currentCorner[0] > bottomRight.currentCorner[0] && chunk.currentCorner[1] > bottomRight.currentCorner[1]) { bottomRight = chunk; }
            }
            //Debug.Print("{0},{1}", new Rectangle(topLeft.currentCorner[0], topLeft.currentCorner[1], (bottomRight.currentCorner[0] - topLeft.currentCorner[0]) + 30 * 48, (bottomRight.currentCorner[1] - topLeft.currentCorner[1]) + 18 * 48), prop);
            return new Rectangle(topLeft.currentCorner[0], topLeft.currentCorner[1], (bottomRight.currentCorner[0] - topLeft.currentCorner[0]) + 30 * 48, (bottomRight.currentCorner[1]-topLeft.currentCorner[1]) + 18 * 48).Contains(prop);
        }
        public int[] getDimensions() { return dimensions; }
        public bool includesTile(int[] coordinates) { return constructor.includesTile(coordinates); }
        public Tile getTile(int[] coordinates)
        {
            if (currentChunk.contains(new Rectangle(coordinates[0], coordinates[1], 10, 10)))
            {
                return currentChunk.getTile(coordinates);
            }
            else { return liveChunks.Find(a => a.contains(new Rectangle(coordinates[0], coordinates[1], 10, 10))).getTile(coordinates); }
        }
        public Tile getTile(int x, int y) { return currentChunk.getTile(new int[] { x, y }); }

        void parseRegions()
        {
            XmlNode regionXML = xmlReader.ChildNodes[1];
            byte count = 0;
            foreach (XmlNode node in regionXML.ChildNodes)
            {
                this.regions[node.Attributes[0].Value] = new Region(node.Attributes[0].Value, new int[] { constructor.getDimensions()[0], constructor.getDimensions()[1] }, new int[] { 0, 0 }, count);
                count++;
            }
            for (int i = 0; i < constructor.regionValArray.GetLength(0); i++)
            {
                for (int j = 0; j < constructor.regionValArray.GetLength(1); j++)
                {
                    regions[Constructor.regions[constructor.regionValArray[i, j]]].turnOnTile(new int[] { i, j });
                }
            }
        }

        public void addScenery(Scenery scenery)
        {//ABSOLUTE COORDINATES
            Chunk chunk = constructor.getChunkAt(new int[] { scenery.coords[0], scenery.coords[1] });
            chunk.addScenery(scenery);
        }
        public void addDroppedItem(DroppedItem item)
        {//ABSOLUTE COORDINATES
            Chunk chunk = constructor.getChunkAt(new int[] { item.coords[0], item.coords[1] });
            chunk.addDroppedItem(item);
        }

        public void exportAllChunks()
        {
            constructor.exportAllChunks();
        }

        void parseItems()
        {
            string[] ioArray = File.ReadAllText(string.Format("Content//ItemSheet.csv", level)).Split('\n');
            for (int i = 0; i < ioArray.Length; i++)
            {
                string[] line = ioArray[i].Split(',');
                items[line[0]] = new Item(line[0], new Rectangle(Convert.ToInt16(line[1]), Convert.ToInt16(line[2]), 18, 18), Convert.ToInt16(line[4]), line[5].Substring(0, 4));
            }
        }
        void parseBuildings()
        {
            XmlNode Buildings = xmlReader.ChildNodes[1].ChildNodes[0];
            foreach (XmlNode node in Buildings.ChildNodes)
            {
                Rectangle floorSprite = new Rectangle(Convert.ToInt16(node.ChildNodes[0].Attributes[0].Value), Convert.ToInt16(node.ChildNodes[0].Attributes[1].Value), Convert.ToInt16(node.ChildNodes[0].Attributes[2].Value), Convert.ToInt16(node.ChildNodes[0].Attributes[3].Value));
                int[] floorOffset = new int[] { Convert.ToInt16(node.ChildNodes[0].Attributes[4].Value), Convert.ToInt16(node.ChildNodes[0].Attributes[5].Value) };
                Rectangle roofSprite = new Rectangle(Convert.ToInt16(node.ChildNodes[1].Attributes[0].Value), Convert.ToInt16(node.ChildNodes[1].Attributes[1].Value), Convert.ToInt16(node.ChildNodes[1].Attributes[2].Value), Convert.ToInt16(node.ChildNodes[1].Attributes[3].Value));

                BuildingType building = new BuildingType(node.Attributes[0].Value, floorSprite, floorOffset, roofSprite, Convert.ToInt16(node.ChildNodes[3].Attributes[0].Value) * 3, Convert.ToInt16(node.ChildNodes[3].Attributes[1].Value) * 3);
                foreach (XmlNode wall in node.ChildNodes[2])
                {
                    Rectangle sprite = new Rectangle(Convert.ToInt16(wall.ChildNodes[0].Attributes[0].Value), Convert.ToInt16(wall.ChildNodes[0].Attributes[1].Value), Convert.ToInt16(wall.ChildNodes[0].Attributes[2].Value), Convert.ToInt16(wall.ChildNodes[0].Attributes[3].Value));
                    int[] location = new int[] { Convert.ToInt16(wall.ChildNodes[1].Attributes[0].Value) * 3, Convert.ToInt16(wall.ChildNodes[1].Attributes[1].Value) * 3 };
                    Rectangle hitBox = new Rectangle(Convert.ToInt16(wall.ChildNodes[2].Attributes[0].Value) * 3, Convert.ToInt16(wall.ChildNodes[2].Attributes[1].Value) * 3, Convert.ToInt16(wall.ChildNodes[2].Attributes[2].Value) * 3, Convert.ToInt16(wall.ChildNodes[2].Attributes[3].Value) * 3);
                    building.addWall(sprite, location, hitBox);
                }
                foreach (XmlNode statics in node.ChildNodes[4])
                {
                    //Debug.Print(statics.Name);
                    switch (statics.Name)
                    {
                        case ("static"):
                            {
                                int[] location = new int[] { Convert.ToInt16(statics.ChildNodes[0].Attributes[0].Value), Convert.ToInt16(statics.ChildNodes[0].Attributes[1].Value) };
                                building.addStatic(location, scenery[statics.Attributes[0].Value]);
                                break;
                            }
                        case ("chest"):
                            {
                                int[] location = new int[] { Convert.ToInt16(statics.ChildNodes[0].Attributes[0].Value), Convert.ToInt16(statics.ChildNodes[0].Attributes[1].Value) };
                                building.addChest(location, scenery[statics.Attributes[0].Value], new int[] { Convert.ToInt16(statics.ChildNodes[1].Attributes[0].Value), Convert.ToInt16(statics.ChildNodes[1].Attributes[1].Value) });
                                break;
                            }
                        case ("bed"):
                            {
                                int[] location = new int[] { Convert.ToInt16(statics.ChildNodes[0].Attributes[0].Value), Convert.ToInt16(statics.ChildNodes[0].Attributes[1].Value) };
                                building.addBed(scenery[statics.Attributes[0].Value], location);
                                break;
                            }
                    }
                }
                Scene.buildingTypes[node.Attributes[0].Value] = building;
            }

        }
        void parseScenery()
        {
            XmlNode Scenery = xmlReader.ChildNodes[1].ChildNodes[1];
            foreach (XmlNode node in Scenery.ChildNodes)
            {
                XmlNode sprite = node.ChildNodes[0];
                Rectangle spriteRectangle = new Rectangle(Convert.ToInt16(sprite.Attributes[0].Value), Convert.ToInt16(sprite.Attributes[1].Value), Convert.ToInt16(sprite.Attributes[2].Value), Convert.ToInt16(sprite.Attributes[3].Value));

                XmlNode hitbox = node.ChildNodes[1];
                Rectangle hitboxRectangle = new Rectangle(Convert.ToInt16(hitbox.Attributes[0].Value), Convert.ToInt16(hitbox.Attributes[1].Value), Convert.ToInt16(hitbox.Attributes[2].Value), Convert.ToInt16(hitbox.Attributes[3].Value));

                StaticObject obj = new StaticObject(node.Attributes[0].Value, node.Attributes[2].Value.Equals("true"), spriteRectangle, hitboxRectangle, node.Attributes[3].Value.Equals("true"), node.Attributes[4].Value.Equals("true"), node.Attributes[5].Value.Equals("true"), node.ChildNodes[2].InnerText);

                if (node.Attributes[1].Value.Equals("true"))
                {
                    XmlNode particleNode = node.ChildNodes[3];
                    Rectangle spawnRegion = new Rectangle(Convert.ToInt16(particleNode.Attributes[0].Value), Convert.ToInt16(particleNode.Attributes[1].Value), Convert.ToInt16(particleNode.Attributes[2].Value), Convert.ToInt16(particleNode.Attributes[3].Value));
                    ParticleType particle = new ParticleType(spawnRegion, particleNode.Attributes[4].Value, Convert.ToInt16(particleNode.Attributes[5].Value), Convert.ToInt16(particleNode.Attributes[6].Value), particleNode.Attributes[7].Value.Equals("true"), particleNode.Attributes[8].Value.Equals("true"));
                    obj.setParticle(particle);
                }
                scenery[node.Attributes[0].Value] = obj;
            }
        }
        void parseCrops()
        {
            XmlNode Crops = xmlReader.ChildNodes[1].ChildNodes[3];
            foreach (XmlNode node in Crops.ChildNodes)
            {
                //!!!ADD SEED RECTANGLE
                CropType crop = new CropType(node.Attributes[0].Value, node.Attributes[1].Value, getItem(node.Attributes[2].Value), node.Attributes[3].Value.Equals("true"), Convert.ToInt32(node.Attributes[4].Value), new Rectangle(0,0,16,16));
                foreach (XmlNode sprite in node.ChildNodes)
                {
                    crop.addSprite(new Rectangle(Convert.ToInt16(sprite.Attributes[0].Value), Convert.ToInt16(sprite.Attributes[1].Value), Convert.ToInt16(sprite.Attributes[2].Value), Convert.ToInt16(sprite.Attributes[3].Value)));
                }
                crops[node.Attributes[0].Value] = crop;
            }

        }
        public BuildingType getBuilding(string tag)
        {
            return buildingTypes[tag];
        }
        public CropType getCrop(string tag)
        {
            return crops[tag];
        }
        public Item getItem(string tag)
        {
            return items[tag];
        }
        public StaticObject getScenery(string tag)
        {
            return scenery[tag];
        }
        public Chunk getChunkAt(int x, int y)
        {
            return constructor.getChunkAt(new int[] { x, y });
        }
        public Tile getBestFit(Rectangle location, ref Chunk tileChunk)
        {   //GET TILES AROUND RECTANGLE - DO NOT LOOP THROUGH ALL
            int maxOverlap = 0;
            Tile best = null;
            /*List<Tile[,]> tilesArray = new List<Tile[,]>();
            if (!currentChunk.contains(location))
            {
                foreach (Chunk chunk in liveChunks) { if (chunk.intersects(location)) { tilesArray.Add(chunk.getTileArray()); break; } }
            }
            else { tilesArray.Add(currentChunk.getTileArray()); }
           
            if (tilesArray.Count == 0) { throw new ArgumentException("location is outside of loaded chunks"); }
            */
            Debug.Print("Location: {0},{1},{2},{3}", location.X, location.Y, location.Width, location.Height);

            foreach (Chunk chunk in liveChunks)
            {
                if (chunk.intersects(location))
                {
                    Tile[,] tiles = chunk.getTileArray();
                    foreach (Tile tile in tiles)
                    {
                        ///!!!loop through the nine tiles it intersects with, not every tile in the chunk
                        int left = Math.Max(tile.getCoords()[0] + chunk.currentCorner[0], location.X);
                        int right = Math.Min(tile.getCoords()[0] + 48 + chunk.currentCorner[0], location.X + location.Width);
                        int bottom = Math.Max(tile.getCoords()[1] + chunk.currentCorner[1], location.Y);
                        int top = Math.Min(tile.getCoords()[1] + 48 + chunk.currentCorner[1], location.Y + location.Height);

                        if ((right - left > 0) && (top - bottom > 0) && (right - left) * (top - bottom) > maxOverlap) { maxOverlap = (right - left) * (top - bottom); best = tile; tileChunk = chunk; }
                        //Debug.Print("Tile: {0} Rect: {1},{2},48,48", tile.getNum(), tile.getCoords()[0] + chunk.currentCorner[0], tile.getCoords()[1] + chunk.currentCorner[1]);
                        //Debug.Print("{0},{1},{2},{3}, {4} max: {5}", left, right, bottom, top, (right - left) * (top - bottom), maxOverlap);
                    }
                }

                if (chunk.contains(location)) { break; }
            }
            Debug.Print("Best: {0},{1},{2},{3}", best.getNum(), best.getCoords()[0], best.getCoords()[1], best.occupied);
            return best;
        }
        public List<Tile> getContainedTiles(Rectangle location)
        {
            List<Tile> tiles = new List<Tile>();
            foreach (Chunk chunk in liveChunks)
            {
                Tile[,] tileArray = chunk.getTileArray();
                for (int i = 0; i < tileArray.GetLength(0); i++)
                {
                    for (int j = 0; j < tileArray.GetLength(1); j++)
                    {
                        if (location.Contains(tileArray[i, j].getLocation())) { tiles.Add(tileArray[i, j]); }
                    }
                }
            }
            return tiles;
        }
        public List<Tile> getMostlyContainedTiles(Rectangle location, float percent)
        {
            List<Tile> tiles = new List<Tile>();
            foreach (Chunk chunk in liveChunks)
            {
                Tile[,] tileArray = chunk.getTileArray();
                for (int i = 0; i < tileArray.GetLength(0); i++)
                {
                    for (int j = 0; j < tileArray.GetLength(1); j++)
                    {
                        if (getIntersectArea(location, tileArray[i, j].getLocation()) / (float)Tile.area > percent) { tiles.Add(tileArray[i, j]); }
                    }
                }
            }
            return tiles;
        }
        public int getIntersectArea(Rectangle A, Rectangle B)
        {
            if (A.X > B.X + B.Width || A.X + A.Width < B.X || A.Y > B.Y + B.Height || A.Y + A.Height < B.Y) { return 0; }

            int left = Math.Max(A.X, B.X);
            int right = Math.Min(A.X + A.Width, B.X + B.Width);
            int bottom = Math.Max(A.Y, B.Y);
            int top = Math.Min(A.Y + A.Height, B.Y + B.Height);

            return (right - left) * (top - bottom);
        }
    }

    class Constructor
    {
        int[] playerCoords;
        Chunk[,] chunkArray;
        public byte[,] regionValArray;
        string currentLevel;

        public static string[] tileTypes = { "tile", "tile", "tile", "tile", "tile", "tile", "tile", "tile", "tile", "tile" };
        public static string[] regions = { "water", "grass", "rocks", "beach", "plain", "forest", "deep forest", "point", "swamp" };
        public static Random rand = new Random(Guid.NewGuid().GetHashCode());

        public Constructor(string currentLevel, int[] playerCoords, Playing playing)
        {
            this.playerCoords = playerCoords;
            this.currentLevel = currentLevel;
            this.build(playing);
        }

        public void build(Playing playing)
        {
            string[] regionArray = File.ReadAllText("Content//Regions.csv").Split('\n');
            regionValArray = new byte[(regionArray[0].Length + 1) / 2, regionArray.GetLength(0)];
            for (int i = 0; i < regionValArray.GetLength(1); i++)
            {
                string[] regionSplit = regionArray[i].Split(',');
                for (int j = 0; j < regionValArray.GetLength(0); j++)
                {
                    //!!!Hard code when released
                    regionValArray[j, i] = Convert.ToByte(regionSplit[j]);
                }
            }

            XmlDocument xmlReader = new XmlDocument();
            xmlReader.Load("Content//ChunkData.xml");

            chunkArray = new Chunk[(regionValArray.GetLength(0)) / 30, (regionValArray.GetLength(1)) / 18];
            //Debug.Print("chunk dims {0},{1}", chunkArray.GetLength(0), chunkArray.GetLength(1));
            for (int i = 0; i < chunkArray.GetLength(1); i++)
            {
                for (int j = 0; j < chunkArray.GetLength(0); j++)
                {
                    int x = Convert.ToInt16(xmlReader.ChildNodes[0].ChildNodes[i * 10 + j].Attributes[0].Value);
                    int y = Convert.ToInt16(xmlReader.ChildNodes[0].ChildNodes[i * 10 + j].Attributes[1].Value);
                    string tiles = xmlReader.ChildNodes[0].ChildNodes[i * 10 + j].ChildNodes[0].InnerText;
                    chunkArray[x, y] = new Chunk(new int[] { x * 30 * 48, y * 18 * 48 }, xmlReader.ChildNodes[0].ChildNodes[i * 10 + j].OuterXml);
                    //Debug.Print(xmlReader.ChildNodes[1].ChildNodes[i*10 + j]);
                }
            }
            foreach (Chunk chunk in this.getLiveChunks())
            {
                generateNewChunk(chunk, playing);
            }
            List<Chunk> loadingChunks = this.getLiveChunks().ToArray().ToList();
            while (loadingChunks.Count > 0) for (int i = 0; i < loadingChunks.Count; i++)
                {
                    if (loadingChunks[i].loadScenery(playing)) { loadingChunks.RemoveAt(i); }
                }

            //!!!Generate empty chunks in the right size;
            /* PUT IN EMPTY CHUNKS:
             * -xml string (scenery, buildings, whatever)
             * -should all be indexed in scene
             * -corner/coords
             * 
             * METHODS:
             * -populate
             *  -load drawables
             * -destroy
             *  -reform xml string
             * -get neighbors
             * 
             * ISSUES:
             * region specific droppedItems
             * -encapsulate in region class
             * delayed updates?
             * 
            */

        }

        public void exportAllChunks()
        {
            StreamWriter streamWriter = File.CreateText("ChunkData.dat");
            streamWriter.WriteLine("<Chunks>");
            for (int i = 0; i < chunkArray.GetLength(0); i++)
            {
                for (int j = 0; j < chunkArray.GetLength(1); j++)
                {
                    if (chunkArray[i, j].loaded) { streamWriter.WriteLine(chunkArray[i, j].ToString()); }
                    else streamWriter.WriteLine(chunkArray[i, j].getState());
                }
            }
            streamWriter.WriteLine("</Chunks>");
            streamWriter.Flush();
        }

        public List<Chunk> getLiveChunks()
        {
            List<Chunk> liveChunks = new List<Chunk>();

            int[] playerInChunk = new int[] { (playerCoords[0]) / 48 / 30, (playerCoords[1]) / 48 / 18 };
            //Debug.Print("playerchunk {0},{1}", playerInChunk[0], playerInChunk[1]);
            for (int i = playerInChunk[0] - 1; i <= playerInChunk[0] + 1; i++)
            {
                for (int j = playerInChunk[1] - 1; j <= playerInChunk[1] + 1; j++)
                {
                    //Debug.Print("{0},{1}", i, j);
                    liveChunks.Add(chunkArray[i, j]);
                }
            }

            return liveChunks;
        }
        public List<Chunk> getNeighboringChunks(Chunk chunk)
        {
            List<Chunk> neighborChunks = new List<Chunk>();

            for (int i = chunk.coords[0] - 1; i <= chunk.coords[0] + 1; i++)
            {
                for (int j = chunk.coords[1] - 1; j <= chunk.coords[1] + 1; j++)
                {
                    if (!(i == chunk.coords[0] && j == chunk.coords[1]) && i >= 0 && j >= 0 && i < chunkArray.GetLength(0) && j < chunkArray.GetLength(1))
                    {
                        neighborChunks.Add(chunkArray[i, j]);
                    }
                }
            }

            return neighborChunks;

        }
        public Chunk getChunkAt(int[] coords) { return chunkArray[coords[0] / 30 / 48, coords[1] / 18 / 48]; }
        public void generateNewChunk(Chunk chunk, Playing playing)
        {
            int[] chunkCoords = new int[] { chunk.coords[0] * 30, chunk.coords[1] * 18 };
            chunk.populate(playing);
        }

        public int[] getDimensions()
        {
            return new int[] { this.regionValArray.GetLength(0), this.regionValArray.GetLength(1) };
        }

        public bool includesTile(int x, int y)
        {
            //Debug.Print("x:"+Convert.ToString(x)+"\ny:"+Convert.ToString(y)+"\ndim0:"+Convert.ToString(tileArray.GetLength(0))+"\ndim1:"+Convert.ToString(tileArray.GetLength(1)));
            if (x < 0 || x >= this.regionValArray.GetLength(0)) { return false; }
            if (y < 0 || y >= this.regionValArray.GetLength(1)) { return false; }
            return true;
        }
        public bool includesTile(int[] coords)
        {
            //Debug.Print("x:"+Convert.ToString(x)+"\ny:"+Convert.ToString(y)+"\ndim0:"+Convert.ToString(tileArray.GetLength(0))+"\ndim1:"+Convert.ToString(tileArray.GetLength(1)));
            if (coords[0] < 0 || coords[0] >= this.regionValArray.GetLength(0)) { return false; }
            if (coords[1] < 0 || coords[1] >= this.regionValArray.GetLength(1)) { return false; }
            return true;
        }
        //public Tile getTile(int x, int y) { return tileArray[x, y]; }
        //public Tile getTile(int[] coordinates) { return tileArray[coordinates[0], coordinates[1]]; }
        public Chunk getCurrentChunk(int[] coords)
        {
            //Debug.Print("{0},{1}", (coords[0] - (coords[0] % 30)) / 48, (coords[1] - (coords[1] % 18)) / 48);
            //Debug.Print("{0},{1}", chunkArray.GetLength(0), chunkArray.GetLength(1));
            return chunkArray[(coords[0] - (coords[0] % 48)) / 30 / 48, (coords[1] - (coords[1] % 48)) / 18 / 48];
        }
    }



    class Tile
    {
        string tileType;
        int tileNum;
        public bool solid;
        private int[] coords;
        string tag;
        public string region;
        Rectangle locationRectangle;
        public static int area = 2304;
        public byte metaNum;

        public bool occupied = false;
        public List<Drawable> occupiedBy = new List<Drawable>();
        

        public Tile(int tileNum, string tileType, int x, int y, Random rand, string region)
        {
            this.tileNum = tileNum;
            this.tileType = tileType;
            this.coords = new int[] { x, y };
            this.locationRectangle = new Rectangle(x, y, 48, 48);
            this.region = region;

            byte[] dist = new byte[] { 0, 0, 0, 0, 1, 1, 1, 2, 3, 4, 5 };
            this.metaNum = dist[(byte)rand.Next(0, dist.Length)];
        }
        public Rectangle getLocation() { return locationRectangle; }
        public void attachStatic(Scenery staticObject)
        {
            this.occupiedBy.Add(staticObject);
            this.occupied = true;
            //Debug.Print(Convert.ToString(this.coords[0]/48 + "," + this.coords[1]/48));
        }
        public void attachDroppedItem(DroppedItem droppedItem)
        {
            if (this.occupied) { throw new System.ArgumentException(string.Format("Tile {0},{1} already has an item", coords[0], coords[1])); }
            this.occupiedBy.Add(droppedItem);
            this.occupied = true;
            //Debug.Print(Convert.ToString(this.coords[0] / 48 + "," + this.coords[1] / 48));
        }

        public bool interact(Item item)
        {
            switch (item.tag)
            {
                case "Hoe": { if (this.tileNum == 2 && !occupied) { return true; } break; }
                case "WateringCan": { if (this.occupied && this.occupiedBy.Exists(a => a.getTag() == "FarmTile")) { return true; } break; }
                case "Shovel": { break; }
                default: { if(this.occupied)this.occupiedBy[0].useItem(item); break; }
            }
            return false;
        }
        /*public DroppedItem pickupItem()
        {
            Debug.Print("picked up");
            this.occupied = false;
            //this.droppedItem.toBeDeleted = true;
            DroppedItem temp = this.droppedItem;
            this.droppedItem = null;
            return temp;
        }*/

        public int[] getCoords() { return coords; }
        public int getNum() { return this.tileNum; }
        public void setNum(int num) { this.tileNum = num; }
        public string getType() { return this.tileType; }
        public void setTag(string tag) { this.tag = tag; }
        public string getTag() { return this.tag; }
        public void setFarmTile(FarmTile farmTile) { this.occupiedBy.Add(farmTile); occupied = true; }
        public bool isOccupied() { return occupied; }
        public int distance(Tile inTile) { return Math.Abs(this.coords[0] - inTile.coords[0]) + Math.Abs(this.coords[1] - inTile.coords[1]); }
    }
}
