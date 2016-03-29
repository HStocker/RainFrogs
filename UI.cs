using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace RainFrogs
{
    class UI
    {
        Player player;
        public Rectangle spriteRectangle;
        List<Vector2> quickSlotsQ = new List<Vector2>();
        List<Vector2> quickSlotsE = new List<Vector2>();
        public Clock clock;

        public UI(Player player)
        {
            this.player = player;
            this.spriteRectangle = new Rectangle(125, 0, 105, 900);
            this.clock = new Clock();

            quickSlotsQ.Add(new Vector2(1179, 105));
            quickSlotsQ.Add(new Vector2(1179, 165));
            quickSlotsQ.Add(new Vector2(1179, 225));

            quickSlotsE.Add(new Vector2(1260, 105));
            quickSlotsE.Add(new Vector2(1260, 165));
            quickSlotsE.Add(new Vector2(1260, 225));
        }
        public Vector2 getSlotQ(int i, int x, int y)
        { return new Vector2(quickSlotsQ[i].X + x, quickSlotsQ[i].Y + y); }
        public Vector2 getSlotE(int i, int x, int y) { return new Vector2(quickSlotsE[i].X + x, quickSlotsE[i].Y + y); }
        public Vector2 getToolSlot() { return new Vector2(1161, 516); }
        public Vector2 getItemSlot() { return new Vector2(1161, 642); }
        public Vector2 getAccessorySlot() { return new Vector2(1161, 579); }

        public void updateTime(GameTime gameTime, Playing playing)
        {
            clock.update(gameTime, playing);
        }

    }

    class Inventory
    {
        Playing playing;
        public Rectangle spriteRectangle = new Rectangle(230, 0, 300, 151);
        public Vector2 screenVector = new Vector2(120, 120);

        public Slot equippedTool = new Slot(new int[] { -1, -1 }, new Vector2(186, 198), "equippedTool");
        public Slot equippedAccessory = new Slot(new int[] { -1, -2 }, new Vector2(186, 315), "equippedAccessory");
        public Slot heldItem = new Slot(new int[] { -1, -3 }, new Vector2(186, 432), "heldItem");

        public Vector2 hotkeyCorner = new Vector2(327, 147);
        //69,9
        //327, 147

        Slot[,] inventoryArray = new Slot[6, 7];
        public Inventory(Playing playing)
        {
            this.playing = playing;
            for (int i = 0; i < inventoryArray.GetLength(0); i++)
            {
                for (int j = 0; j < inventoryArray.GetLength(1); j++)
                {
                    inventoryArray[i, j] = new Slot(new int[] { i, j }, new Vector2(hotkeyCorner.X + (j * 69), hotkeyCorner.Y + ((i) * 69)), "Main");
                }
            }

        }
        public int getLength(int i)
        {
            return inventoryArray.GetLength(i);
        }
        public Slot getSlotAt(int x, int y)
        {
            return inventoryArray[x, y];
        }
        public Vector2 getVectorAt(int x, int y)
        {
            return inventoryArray[x, y].location;
        }
        public void addItemAt(Item item, int x, int y)
        {
            inventoryArray[x, y].setItem(item);
        }

        public void addNextItem()
        {
            for (int i = 0; i < inventoryArray.GetLength(0); i++)
            {
                for (int j = 1; j < inventoryArray.GetLength(1); j++)
                {
                    if (!inventoryArray[i, j].hasItem()) { this.swapItems(this.heldItem, inventoryArray[i, j]); break; }
                }
            }
        }
        public Item setItemAt(Item item, int x, int y)
        {
            Item temp = inventoryArray[x, y].getItem();
            inventoryArray[x, y].setItem(item);
            return temp;
        }
        public bool hasItem(int x, int y)
        {
            return inventoryArray[x, y].hasItem();
        }
        public bool hasItem(int[] coords)
        {
            return inventoryArray[coords[0],coords[1]].hasItem();
        }
        public bool hasItem(out string itemOut, int x, int y)
        {
            if (inventoryArray[x, y].itemFloating) { itemOut = ""; return false; }
            return inventoryArray[x, y].hasItem(out itemOut);
        }
        public bool hasHotkeyTools()
        {
            string unused;
            if (inventoryArray[0, 0].hasItem(out unused) || inventoryArray[1, 0].hasItem(out unused) || inventoryArray[2, 0].hasItem(out unused)) return true;
            else { return false; }
        }
        public bool hasHotkeyItems()
        {
            string unused;
            if (inventoryArray[3, 0].hasItem(out unused) || inventoryArray[4, 0].hasItem(out unused) || inventoryArray[5, 0].hasItem(out unused)) return true;
            else { return false; }
        }

        public Item getItemAt(int x, int y) { return inventoryArray[x, y].getItem(); }
        public Item getItemAt(int[] coords) { return inventoryArray[coords[0], coords[1]].getItem(); }
        public int[] getMouseOver()
        {
            Slot slot = null;
            MouseState mouse = Mouse.GetState();

            byte x = (byte)((mouse.X - hotkeyCorner.X) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y) / 69);


            if (!(mouse.X - hotkeyCorner.X < 0 || mouse.Y - hotkeyCorner.Y < 0) && x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                slot = inventoryArray[(mouse.Y - (int)hotkeyCorner.Y) / 69, (mouse.X - (int)hotkeyCorner.X) / 69];
            }
            else if (new Rectangle((int)equippedTool.location.X, (int)equippedTool.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = equippedTool;
            }
            else if (new Rectangle((int)heldItem.location.X, (int)heldItem.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = heldItem;
            }
            else if (new Rectangle((int)equippedAccessory.location.X, (int)equippedAccessory.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = equippedAccessory;
            }
            else { return new int[] { -1, -1 }; }

            return new int[] { slot.coords[0], slot.coords[1] };

        }
        public Slot getSlotOver()
        {

            Slot slot = null;
            MouseState mouse = Mouse.GetState();

            byte x = (byte)((mouse.X - hotkeyCorner.X) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y) / 69);
            if (!(mouse.X - hotkeyCorner.X < 0 || mouse.Y - hotkeyCorner.Y < 0) && x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                slot = inventoryArray[(mouse.Y - (int)hotkeyCorner.Y) / 69, (mouse.X - (int)hotkeyCorner.X) / 69];
            }
            else if (new Rectangle((int)equippedTool.location.X, (int)equippedTool.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = equippedTool;
            }
            else if (new Rectangle((int)heldItem.location.X, (int)heldItem.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = heldItem;
            }
            else if (new Rectangle((int)equippedAccessory.location.X, (int)equippedAccessory.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = equippedAccessory;
            }
            else { return null; }

            return slot;
        }
        public Item pickupItem()
        {
            Slot slot = null;
            MouseState mouse = Mouse.GetState();
            byte x = (byte)((mouse.X - hotkeyCorner.X) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y) / 69);
            if (!(mouse.X - hotkeyCorner.X < 0 || mouse.Y - hotkeyCorner.Y < 0) && x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                slot = inventoryArray[(mouse.Y - (int)hotkeyCorner.Y) / 69, (mouse.X - (int)hotkeyCorner.X) / 69];
            }
            else if (new Rectangle((int)equippedTool.location.X, (int)equippedTool.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = equippedTool;
            }
            else if (new Rectangle((int)heldItem.location.X, (int)heldItem.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = heldItem;
            }
            else if (new Rectangle((int)equippedAccessory.location.X, (int)equippedAccessory.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                slot = equippedAccessory;
            }
            else { return null; }
            slot.itemFloating = true;
            return slot.getItem();
        }
        public void swapItems(Slot A, Slot B)
        {
            Item temp = A.getItem();
            A.setItem(B.getItem());
            B.setItem(temp);
        }
        public void dropItem(Item item, Slot origin)
        {
            MouseState mouse = Mouse.GetState();
            byte x = (byte)((mouse.X - hotkeyCorner.X) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y) / 69);

            if (!(mouse.X - hotkeyCorner.X < 0 || mouse.Y - hotkeyCorner.Y < 0) && x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                swapItems(inventoryArray[(mouse.Y - (int)hotkeyCorner.Y) / 69, (mouse.X - (int)hotkeyCorner.X) / 69], origin);
                //!!!Fix swap drag from equip slot to main slot

            }
            else if (new Rectangle((int)equippedTool.location.X, (int)equippedTool.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)) && item.type == "Tool")
            {
                swapItems(equippedTool, origin);
            }
            else if (new Rectangle((int)equippedAccessory.location.X, (int)equippedAccessory.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)) && item.type == "Axry")
            {
                swapItems(equippedAccessory, origin);
            }
            else if (new Rectangle((int)heldItem.location.X, (int)heldItem.location.Y, 54, 54).Contains(new Point(mouse.X, mouse.Y)))
            {
                swapItems(heldItem, origin);
            }
            origin.itemFloating = false;
        }
        public Item getItemOver()
        {
            Slot slot = null;
            MouseState mouse = Mouse.GetState();
            if (mouse.X - hotkeyCorner.X < 0 || mouse.Y - hotkeyCorner.Y < 0) { return null; }
            byte x = (byte)((mouse.X - hotkeyCorner.X) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y) / 69);
            if (x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                slot = inventoryArray[(mouse.Y - (int)hotkeyCorner.Y) / 69, (mouse.X - (int)hotkeyCorner.X) / 69];
            }
            else { return null; }
            slot.itemFloating = true;
            return slot.getItem();
        }

        #region translated for chests

        public int[] getMouseOver(int Xtranslation, int Ytranslation)
        {
            Slot slot = null;
            MouseState mouse = Mouse.GetState();
            byte x = (byte)((mouse.X - hotkeyCorner.X - Xtranslation) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y - Ytranslation) / 69);
            if (x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                slot = inventoryArray[y, x];
            }
            else { return new int[] { -1, -1 }; }

            return new int[] { slot.coords[0], slot.coords[1] };
        }
        public Item getItemOver(int Xtranslation, int Ytranslation)
        {
            Slot slot = null;
            MouseState mouse = Mouse.GetState();
            byte x = (byte)((mouse.X - hotkeyCorner.X - Xtranslation) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y - Ytranslation) / 69);
            if (x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                slot = inventoryArray[y, x];
            }
            else { return null; }
            slot.itemFloating = true;
            return slot.getItem();
        }

        public Slot getSlotOver(int Xtranslation, int Ytranslation)
        {

            Slot slot = null;
            MouseState mouse = Mouse.GetState();
            byte x = (byte)((mouse.X - hotkeyCorner.X - Xtranslation) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y - Ytranslation) / 69);
            if (x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                slot = inventoryArray[y, x];
            }
            else { return null; }

            return slot;
        }
        public Item pickupItem(int Xtranslation, int Ytranslation)
        {
            Slot slot = null;
            MouseState mouse = Mouse.GetState();
            byte x = (byte)((mouse.X - hotkeyCorner.X - Xtranslation) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y - Ytranslation) / 69);
            if (x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                slot = inventoryArray[y, x];
            }
            else { return null; }
            slot.itemFloating = true;
            return slot.getItem();
        }
        public void dropItem(Item item, Slot origin, int Xtranslation, int Ytranslation)
        {
            MouseState mouse = Mouse.GetState();

            byte x = (byte)((mouse.X - hotkeyCorner.X - Xtranslation) / 69);
            byte y = (byte)((mouse.Y - hotkeyCorner.Y - Ytranslation) / 69);
            if (x >= 0 && x < inventoryArray.GetLength(1) && y >= 0 && y < inventoryArray.GetLength(0))
            {
                swapItems(inventoryArray[y, x], origin);
                //!!!Fix swap drag from equip slot to main slot

            }

            origin.itemFloating = false;
        }
        #endregion
    }
    class Slot
    {
        public int[] coords;
        Item item;
        public bool itemFloating = false;
        public Vector2 location;
        public string tag;

        public Slot(int[] coords, Vector2 location, string tag)
        {
            this.coords = coords;
            this.location = location;
            this.tag = tag;
        }
        public void setItem(Item item)
        {
            this.item = item;
        }
        public bool hasItem() { return !this.itemFloating && this.item != null; }
        public bool hasItem(out string itemOut)
        {
            if (this.item == null) { itemOut = null; }
            else itemOut = this.item.tag;
            return this.item != null;
        }
        public Item getItem() { return this.item; }

    }

    class Tooltip
    {
        public Rectangle topLeftSprite = new Rectangle(0, 113, 13, 9);
        public Rectangle topMidSprite = new Rectangle(13, 113, 13, 9);
        public Rectangle topRightSprite = new Rectangle(26, 113, 13, 9);

        public Rectangle midLeftSprite = new Rectangle(0, 121, 13, 8);
        public Rectangle midMidSprite = new Rectangle(13, 121, 13, 8);
        public Rectangle midRightSprite = new Rectangle(26, 121, 13, 8);

        public Rectangle botLeftSprite = new Rectangle(0, 129, 13, 9);
        public Rectangle botMidSprite = new Rectangle(13, 129, 13, 9);
        public Rectangle botRightSprite = new Rectangle(26, 129, 13, 9);

        public Tooltip()
        {

        }

    }

    class Clock
    {
        int minute = 0;
        int hour = 0;
        public float minuteRotation = 0;
        public float hourRotation = 0;
        int time = 0;
        static List<string> months = new List<string> { "Soggy", "Sunny", "Windy", "Foggy", "Damp", "Cold" };
        static List<int> dayPerMonth = new List<int> { 15, 16, 30, 24, 20, 15 };
        int dayStep = 0;
        int monthStep = 0;

        //public int timeStep = 1;
        public int timeStep = 10000;

        public Clock() { }
        public void update(GameTime gameTime, Playing playing)
        {
            this.time += gameTime.ElapsedGameTime.Milliseconds;
            //Debug.Print(Convert.ToString(time));
            if (this.time >= timeStep) { this.minute += 1; this.time = 0; }

            //reaches 10,000 exactly
            //20 ticks an hour
            //      at one tick add to time counter
            //      tick forwards day/month
            //      !!!POSSIBLE GOAL - GET ENOUGH FOOD TO SURVIVE THE WINTER
            //      INFINITE MODE/STORY MODE/FOREVER FARM MODE  
            //200 seconds per hour
            //20 hour ticks, 4000 seconds per day
            //66 minutes and 40 seconds
            //!!!remove 6 minutes 40 seconds for 1 hour per day
            if (this.minute > 4)
            {
                this.minuteRotation += (float)Math.PI / 2.0f;
                this.minute = 0;

                if (this.minuteRotation >= (float)Math.PI * 2)
                {
                    this.hour += 1;
                    this.minuteRotation = 0;
                    if (this.hour > 4)
                    {
                        this.hourRotation += (float)Math.PI / 2.0f;
                        this.hour = 0;
                        if (this.hourRotation >= (float)Math.PI * 2)
                        {
                            this.hourRotation = 0;

                            this.dayStep++;
                            if (dayStep >= dayPerMonth[monthStep])
                            {
                                dayStep = 0;
                                playing.newDay();

                                monthStep++;
                                if (monthStep >= months.Count)
                                {
                                    monthStep = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
        public void reset()
        {
            minute = 0;
            hour = 0;
            minuteRotation = 0;
            hourRotation = 0;
            time = 0;
            this.dayStep++;
            if (dayStep >= dayPerMonth[monthStep])
            {
                dayStep = 0;
                monthStep++;
                if (monthStep >= months.Count)
                {
                    monthStep = 0;
                }
            }
        }
        public Rectangle minuteRectangle() { return new Rectangle(18 * minute, 76, 18, 18); }
        public Rectangle hourRectangle() { return new Rectangle(18 * hour, 94, 18, 18); }

        public string getMonth() { return months[monthStep]; }
        public string getDay() { return (dayStep + 1).ToString("D2"); }
        public string getDate() 
        {
            //[month]:[day]
            //!!!Add years
            return this.getMonth() + ":" + this.getDay();
        }
        public static int getDayDifference(string currentDate, string oldDate)
        {
            int days = 0;
            string[] current = currentDate.Split(':');
            string[] old = oldDate.Split(':');

            if (current[0].Equals(old[0])) { return Convert.ToInt16(current[1]) - Convert.ToInt16(old[1]); }
            else 
            {
                //calculate old month days left
                days += Clock.dayPerMonth[Clock.months.IndexOf(old[0])] - Convert.ToInt16(old[1]);
                int monthCount = Clock.months.IndexOf(old[0])+1;
                while (Clock.months[monthCount] != current[0])
                {
                    //calculate days in inbetween months
                    days += Clock.dayPerMonth[monthCount];
                    monthCount++;
                } 
                
                //calculate current month days past
                return days + Convert.ToInt16(current[1]);
            }
        }
        //get time of day function
    }
}
