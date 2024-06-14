using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Linq;
using Spectre.Console;

namespace TextWorld
{
    class Game
    {
        public void Start()
        {
            [DllImport("kernel32.dll", SetLastError = true)] //I have little to no idea what this is, it makes the text colorful Credit: Alexei Shcherbakov
            static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool GetConsoleMode(IntPtr handle, out int mode);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetStdHandle(int handle);

            var handle = GetStdHandle(-11);
            int mode;
            GetConsoleMode(handle, out mode);
            SetConsoleMode(handle, mode | 0x4);

            Random rand = new();

            bool running = true;

            while (running)
            {
                RunMainMenu();

                int screenWidth = GV.screenWidth;
                int screenHeight = GV.screenHeight;

                int worldSize = GV.newWorldSize;

                switch (GV.newWorldGamemode)
                {
                    case "World Viewer":
                        int camX = rand.Next(0, (int)LimitF(worldSize - screenWidth * 0.2f));
                        int camY = rand.Next(0, (int)LimitF(worldSize - screenWidth * 0.2f));

                        while (true)
                        {
                            ScreenTop('a');

                            RenderWorld(GV.currentWorld, worldSize, camX, camX + (int)(screenWidth * 0.5f), camY, camY + (int)(screenWidth * 0.5f), 's'); //Renders the world map

                            ScreenLeft(GV.screenWidth / 5);
                            ScreenLeft(GV.screenWidth / 2 - 10);
                            ResetTextColor(false);
                            Console.Write("World Viewer Mode");

                            ScreenLeft(GV.screenWidth / 4);
                            ResetTextColor(false);
                            Console.Write("Arrow Keys - Move");
                            for (int i = 0; i < GV.screenWidth / 4; i++)
                            {
                                Console.Write(" ");
                            }
                            Console.Write("Enter - Teleport to a random place");

                            ScreenLeft(GV.screenWidth / 2 - 20);
                            ResetTextColor(false);
                            Console.Write("Esc - Quit        Saving functions coming soon!");

                            ConsoleKey keyPressed;
                            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                            keyPressed = keyInfo.Key;

                            if (keyPressed == ConsoleKey.UpArrow)
                            {
                                camX -= 5;
                            }
                            else if (keyPressed == ConsoleKey.DownArrow)
                            {
                                camX += 5;
                            }
                            else if (keyPressed == ConsoleKey.LeftArrow)
                            {
                                camY -= 5;
                            }
                            else if (keyPressed == ConsoleKey.RightArrow)
                            {
                                camY += 5;
                            }
                            else if (keyPressed == ConsoleKey.Enter)
                            {
                                camX = rand.Next(0, worldSize);
                                camY = rand.Next(0, worldSize);
                            }
                            else if (keyPressed == ConsoleKey.Escape)
                            {
                                break;
                            }
                        }
                        break;
                    case "Survival":

                        if (GV.playerX == 789)
                        {
                            GV.playerX = worldSize / 2; //Tries to find spawn point, if cant, sets it to this instead.
                            GV.playerY = worldSize / 2;
                            
                            bool spawnFound = false;

                            for (int x = (int)(worldSize * 0.25f); x < (int)(worldSize * 0.75f); x++)
                            {

                                for (int y = (int)(worldSize * 0.25f); y < (int)(worldSize * 0.75f); y++)
                                {
                                    if (Convert.ToInt32(GV.currentWorldDepth[x, y][0].ToString()) < GV.worldSeaLevel)
                                    {
                                        if (GV.currentWorld[x, y][0].ToString() == "s")
                                        {
                                            GV.playerX = x;
                                            GV.playerY = y;

                                            spawnFound = true;

                                            break;
                                        }
                                    }
                                }

                                if (spawnFound) { break; }
                            }

                            if (!spawnFound)
                            {
                                for (int x = worldSize; x < worldSize; x++)
                                {
                                    for (int y = worldSize; y < worldSize; y++)
                                    {
                                        if (!GV.tilesBlockPlayer.Any(GV.currentWorld[x, y][0].ToString().Contains))
                                        {
                                            GV.playerX = x;
                                            GV.playerY = y;

                                            spawnFound = true;

                                            break;
                                        }
                                    }
                            
                                    if (spawnFound) { break; }
                                }
                            }
                        }

                        RefreshSurvival();

                        while (running)
                        {
                            RunSurvivalTick();
                        }

                        break;
                }

                running = true;
            }

            void RefreshSurvival() 
            {
                ScreenTop('y');

                ScreenLeft(GV.screenWidth / 2 - 10); //Rendering hud
                ScreenLeft(GV.screenWidth / 2 - 10);
                ResetTextColor(false);
                Console.Write(GV.newWorldName + ", ");
                if (GV.worldTime < 140)
                {
                    SetTextColor(190);
                    Console.Write($"Day {GV.worldDay}");
                }
                else
                {
                    SetTextColor(12);
                    Console.Write($"Night {GV.worldDay}");
                }

                ScreenLeft(GV.screenWidth / 3 - 10);
                ScreenLeft(GV.screenWidth / 3 - 10);
                ResetTextColor(false);
                Console.Write("Money: ");
                SetTextColor(3);
                Console.Write(GV.playerMoney);

                ResetTextColor(false);
                for (int i = 0; i < GV.screenWidth / 3; i++)
                {
                    Console.Write(" ");
                }
                Console.Write("Energy: ");
                SetTextColor(50);
                Console.Write(GV.playerEnergy);

                ScreenLeft(GV.screenWidth / 2 - 8);

                ResetTextColor(false);
                Console.Write("Health: ");
                SetTextColor(1);
                Console.Write(GV.playerHealth);

                RenderWorld(GV.currentWorld, GV.newWorldSize, GV.playerX - 9, GV.playerX + 10, GV.playerY - 9, GV.playerY + 10, 's'); //Render world

                ScreenLeft(GV.screenWidth / 4 - 10); //Rendering bottom hud

                ScreenLeft(GV.screenWidth / 4 + 6);
                Console.Write("I - Inventory");
                for (int i = 0; i < GV.screenWidth / 3 - 20; i++)
                {
                    Console.Write(" ");
                }
                Console.Write("E - Interact");

                ScreenLeft(GV.screenWidth / 4 + 7);
                Console.Write("R - Building");
                for (int i = 0; i < GV.screenWidth / 3 - 20; i++)
                {
                    Console.Write(" ");
                }
                Console.Write("S - Save");
            }

            void RunSurvivalTick()
            {
                ConsoleKey keyPressed; //Register key presses
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                bool refreshScreen = false;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    refreshScreen = true;

                    string nextTile = ReturnWorldTile(GV.playerX - 1, GV.playerY, GV.currentWorld, 1);
                    if (!GV.tilesBlockPlayer.Any(p => p == nextTile))
                    {
                        GV.currentWorldEntities[GV.playerX, GV.playerY] = "[]";

                        GV.playerX -= 1;
                        GV.playerEnergy -= 0.5f;

                        GV.currentWorldEntities[GV.playerX, GV.playerY] = "H";
                        AdvanceTime();
                    }
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    refreshScreen = true;

                    string nextTile = ReturnWorldTile(GV.playerX + 1, GV.playerY, GV.currentWorld, 1);
                    if (!GV.tilesBlockPlayer.Any(p => p == nextTile))
                    {
                        GV.currentWorldEntities[GV.playerX, GV.playerY] = "[]";

                        GV.playerX += 1;
                        GV.playerEnergy -= 0.5f;

                        GV.currentWorldEntities[GV.playerX, GV.playerY] = "H";
                        AdvanceTime();
                    }
                }
                else if (keyPressed == ConsoleKey.LeftArrow)
                {
                    refreshScreen = true;

                    string nextTile = ReturnWorldTile(GV.playerX, GV.playerY - 1, GV.currentWorld, 1);
                    if (!GV.tilesBlockPlayer.Any(p => p == nextTile))
                    {
                        GV.currentWorldEntities[GV.playerX, GV.playerY] = "[]";

                        GV.playerY -= 1;
                        GV.playerEnergy -= 0.5f;

                        GV.currentWorldEntities[GV.playerX, GV.playerY] = "H";
                        AdvanceTime();
                    }
                }
                else if (keyPressed == ConsoleKey.RightArrow)
                {
                    refreshScreen = true;

                    string nextTile = ReturnWorldTile(GV.playerX, GV.playerY + 1, GV.currentWorld, 1);
                    if (!GV.tilesBlockPlayer.Any(p => p == nextTile))
                    {
                        GV.currentWorldEntities[GV.playerX, GV.playerY] = "[]";

                        GV.playerY += 1;
                        GV.playerEnergy -= 0.5f;

                        GV.currentWorldEntities[GV.playerX, GV.playerY] = "H";
                        AdvanceTime();
                    }
                }
                else if (keyPressed == ConsoleKey.I)
                {
                    ShowInventory();
                }
                else if (keyPressed == ConsoleKey.Z)
                {
                    AddToInv("Wood", 7);
                    AddToInv("Stone", 0);
                    AddToInv("ERROR", 8);
                    AddToInv("ERROR", -7);
                    AddToInv("Buckthorn", -1);
                }
                else if (keyPressed == ConsoleKey.X)
                {
                    AddToInv(rand.Next(10000, 99999).ToString(), (Int16)rand.Next(1, 99));
                }
                else if (keyPressed == ConsoleKey.Escape)
                {
                    running = false;
                }

                if (refreshScreen)
                {
                    RefreshSurvival();
                }
            }

            void ShowInventory()
            {
                foreach (var (key, value) in GV.playerInventory) //remove useless items
                {
                    if (GV.playerInventory[key] <= 0)
                    {
                        GV.playerInventory.Remove(key);
                    }
                }

                bool inInv = true;
                short itemNum = 1;
                short selectedItem = 1;
                short page = 1;
                short maxPage = (Int16)Math.Ceiling(GV.playerInventory.Count() / 10.0d);

                if (maxPage == 0) { maxPage = 1; }

                while (inInv)
                {
                    itemNum = 1;

                    ScreenTop('i');

                    ScreenLeft(GV.screenWidth / 5);
                    Console.Write($"Weapon: {GV.playerWeapon}");

                    ScreenLeft(GV.screenWidth / 5);
                    Console.Write($"Armor: {GV.playerArmor}");
                    ScreenLeft(GV.screenWidth / 5);
                    Console.Write($"Equipment: {GV.playerEquipment}");
                    ScreenLeft(GV.screenWidth / 5);
                    ScreenLeft(GV.screenWidth / 5);
                    Console.Write("Items:");
                    ScreenLeft(GV.screenWidth / 5);
                    ScreenLeft(GV.screenWidth / 5);

                    foreach (var (key, value) in GV.playerInventory)
                    {
                        if ((Int16)Math.Ceiling(itemNum / 10.0d) == page)
                        {
                            if (itemNum == selectedItem) { ResetTextColor(true); }
                            Console.Write($"{key} - {value}");
                            if (itemNum == selectedItem) { Console.Write(" <"); }
                            ScreenLeft(GV.screenWidth / 5);
                        }

                        itemNum++;
                    }

                    ScreenLeft(GV.screenWidth / 5);
                    ScreenLeft(GV.screenWidth / 5);
                    ScreenLeft(GV.screenWidth / 5);
                    Console.Write($"Page {page}/{maxPage} - Use Arrow Keys to switch pages");
                    ScreenLeft(GV.screenWidth / 5);
                    Console.Write("ESC - Back");

                    ConsoleKey keyPressed; //Register key presses
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    keyPressed = keyInfo.Key;

                    if (keyPressed == ConsoleKey.Escape)
                    {
                        inInv = false;
                        RefreshSurvival();
                    }
                    else if (keyPressed == ConsoleKey.DownArrow)
                    {
                        selectedItem++;
                        if (selectedItem > (Int16)GV.playerInventory.Count())
                        {
                            selectedItem = 1;
                            page = 1;
                        }

                        if (selectedItem > page * 10)
                        {
                            page++;
                        }
                    }
                    else if (keyPressed == ConsoleKey.UpArrow)
                    {
                        selectedItem--;
                        if (selectedItem < 1)
                        {
                            selectedItem = (Int16)GV.playerInventory.Count();
                            page = maxPage;
                        }

                        if (selectedItem < (page - 1) * 10 + 1)
                        {
                            page--;
                        }
                    }
                    else if (keyPressed == ConsoleKey.LeftArrow)
                    {
                        page--;
                        if (page < 1)
                        {
                            page = maxPage;
                        }
                        selectedItem = (Int16)(page * 10 - 9);
                    }
                    else if (keyPressed == ConsoleKey.RightArrow)
                    {
                        page++;
                        if (page > maxPage)
                        {
                            page = 1;
                        }
                        selectedItem = (Int16)(page * 10 - 9);
                    }
                }
            }

            void AddToInv(string type, short amount)
            {
                if (GV.playerInventory.ContainsKey(type))
                {
                    GV.playerInventory[type] += amount;
                }
                else
                {
                    GV.playerInventory.Add(type, amount);
                }
            }

            static void RenderWorld(string[,] world, int worldSize, int xStart, int xEnd, int yStart, int yEnd, char renderer)
            {
                string text = "[]";

                bool refreshColor = true;
                bool refreshBgColor = true;

                int entityColor = 0;
                bool useEntityColor = false;

                string worldTile;
                string worldStructureTile;

                ScreenLeft((int)(GV.screenWidth / 2 - (xEnd - xStart + 2)));

                int tileNum = 0;
                for (int x = xStart; x < xEnd; x++)
                {
                    for (int y = yStart; y < yEnd; y++)
                    {
                        int limitedX = Math.Clamp(x, 0, worldSize - 1);
                        int limitedY = Math.Clamp(y, 0, worldSize - 1);

                        if (limitedX >= xStart && limitedX < xEnd && limitedY >= yStart && limitedY <= yEnd)
                        {
                            if ((float)tileNum / (xStart - xEnd) == Math.Round((float)tileNum / (xStart - xEnd)))
                            {
                                ScreenLeft((int)(GV.screenWidth / 2 - (xEnd - xStart + 2)));
                            }

                            switch (renderer)
                            {
                                case 'a': //Old renderer, only renders world map
                                    switch (world[limitedX, limitedY][0].ToString())
                                    {
                                        case "W": //Deep water
                                            SetTextBackgroundColor(27);
                                            break;
                                        case "w": //Shallow water
                                            SetTextBackgroundColor(12);
                                            break;
                                        case "I": //Ice
                                            SetTextBackgroundColor(14);
                                            break;
                                        case "r": //Rocky terrain
                                            SetTextBackgroundColor(244);
                                            break;
                                        case "R": //Rock
                                            if (world[limitedX, limitedY][1].ToString() == "v")
                                            {
                                                SetTextBackgroundColor(88);
                                            }
                                            else
                                            {
                                                SetTextBackgroundColor(241);
                                            }
                                            break;
                                        case "s": //Sand
                                            if (world[limitedX, limitedY][1].ToString() == "V")
                                            {
                                                SetTextBackgroundColor(238);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "3")
                                            {
                                                SetTextBackgroundColor(5);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "9")
                                            {
                                                SetTextBackgroundColor(19);
                                            }
                                            else
                                            {
                                                SetTextBackgroundColor(11);
                                            }
                                            break;
                                        case "S": //Sand Dune
                                            SetTextBackgroundColor(186);
                                            break;
                                        case "f": //Snow
                                            SetTextBackgroundColor(15);
                                            break;
                                        case "F": //Snow hill
                                            SetTextBackgroundColor(7);
                                            break;
                                        case "g": //Grass
                                            if (world[limitedX, limitedY][1].ToString() == "H")
                                            {
                                                SetTextBackgroundColor(106);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "C")
                                            {
                                                SetTextBackgroundColor(29);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "J")
                                            {
                                                SetTextBackgroundColor(28);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "3")
                                            {
                                                SetTextBackgroundColor(82);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "9")
                                            {
                                                SetTextBackgroundColor(213);
                                            }
                                            else
                                            {
                                                SetTextBackgroundColor(10);
                                            }
                                            break;
                                        case "G": //High grass
                                            if (world[limitedX, limitedY][1].ToString() == "H")
                                            {
                                                SetTextBackgroundColor(70);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "C")
                                            {
                                                SetTextBackgroundColor(35);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "J")
                                            {
                                                SetTextBackgroundColor(22);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "3")
                                            {
                                                SetTextBackgroundColor(86);
                                            }
                                            else if (world[limitedX, limitedY][1].ToString() == "9")
                                            {
                                                SetTextBackgroundColor(219);
                                            }
                                            else
                                            {
                                                SetTextBackgroundColor(2);
                                            }
                                            break;
                                        case "d": //Ground
                                            SetTextBackgroundColor(100);
                                            break;
                                        case "D": //High ground
                                            SetTextBackgroundColor(58);
                                            break;
                                        case "L": //Lava
                                            if (world[limitedX, limitedY][1].ToString() == "H")
                                            {
                                                SetTextBackgroundColor(208);
                                            }
                                            else
                                            {
                                                SetTextBackgroundColor(202);
                                            }
                                            break;
                                        case "l": //Lava
                                            SetTextBackgroundColor(202);
                                            break;
                                        default:
                                            SetTextBackgroundColor(0);
                                            break;
                                    }
                                    break;
                                case 'b': //Ancient renderer. Uses old color system.
                                    switch (world[x, y][0].ToString())
                                    {
                                        case "9":
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = ConsoleColor.DarkGray;
                                            break;

                                        case "8":
                                            Console.BackgroundColor = ConsoleColor.DarkMagenta;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        case "7":
                                            Console.BackgroundColor = ConsoleColor.Magenta;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        case "6":
                                            Console.BackgroundColor = ConsoleColor.DarkRed;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        case "5":
                                            Console.BackgroundColor = ConsoleColor.Red;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        case "4":
                                            Console.BackgroundColor = ConsoleColor.DarkGray;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        case "3":
                                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        case "2":
                                            Console.BackgroundColor = ConsoleColor.Yellow;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        case "1":
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        case "0":
                                            Console.BackgroundColor = ConsoleColor.White;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            break;
                                        default:
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            break;
                                    }
                                    break;
                                case 's': //Survival default renderer
                                    refreshColor = true;
                                    refreshBgColor = true;

                                    useEntityColor = false;

                                    worldTile = GV.currentWorld[limitedX, limitedY][0].ToString();
                                    worldStructureTile = GV.currentWorldStructures[limitedX, limitedY][0].ToString();

                                    switch (worldStructureTile)
                                    {
                                        case "T": //Town
                                            SetTextColor(0);
                                            refreshColor = false;
                                            break;
                                        case "F": //Forest
                                            SetTextColor(58);
                                            refreshColor = false;
                                            break;
                                    }

                                    if (refreshBgColor) //Set to false if you want to use a custom bg color for a structure
                                    {
                                        switch (worldTile)
                                        {
                                            case "W": //Deep water
                                                SetTextBackgroundColor(27);
                                                break;
                                            case "w": //Shallow water
                                                SetTextBackgroundColor(12);
                                                break;
                                            case "I": //Ice
                                                SetTextBackgroundColor(14);
                                                break;
                                            case "r": //Rocky terrain
                                                SetTextBackgroundColor(244);
                                                break;
                                            case "R": //Rock
                                                if (world[limitedX, limitedY][1].ToString() == "v")
                                                {
                                                    SetTextBackgroundColor(88);
                                                }
                                                else
                                                {
                                                    SetTextBackgroundColor(241);
                                                }
                                                break;
                                            case "s": //Sand
                                                if (world[limitedX, limitedY][1].ToString() == "V")
                                                {
                                                    SetTextBackgroundColor(238);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "3")
                                                {
                                                    SetTextBackgroundColor(5);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "9")
                                                {
                                                    SetTextBackgroundColor(19);
                                                }
                                                else
                                                {
                                                    SetTextBackgroundColor(11);
                                                }
                                                break;
                                            case "S": //Sand Dune
                                                SetTextBackgroundColor(186);
                                                break;
                                            case "f": //Snow
                                                SetTextBackgroundColor(15);
                                                break;
                                            case "F": //Snow hill
                                                SetTextBackgroundColor(7);
                                                break;
                                            case "g": //Grass
                                                if (world[limitedX, limitedY][1].ToString() == "H")
                                                {
                                                    SetTextBackgroundColor(106);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "C")
                                                {
                                                    SetTextBackgroundColor(29);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "J")
                                                {
                                                    SetTextBackgroundColor(28);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "3")
                                                {
                                                    SetTextBackgroundColor(82);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "9")
                                                {
                                                    SetTextBackgroundColor(213);
                                                }
                                                else
                                                {
                                                    SetTextBackgroundColor(10);
                                                }
                                                break;
                                            case "G": //High grass
                                                if (world[limitedX, limitedY][1].ToString() == "H")
                                                {
                                                    SetTextBackgroundColor(70);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "C")
                                                {
                                                    SetTextBackgroundColor(35);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "J")
                                                {
                                                    SetTextBackgroundColor(22);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "3")
                                                {
                                                    SetTextBackgroundColor(86);
                                                }
                                                else if (world[limitedX, limitedY][1].ToString() == "9")
                                                {
                                                    SetTextBackgroundColor(219);
                                                }
                                                else
                                                {
                                                    SetTextBackgroundColor(2);
                                                }
                                                break;
                                            case "d": //Ground
                                                SetTextBackgroundColor(100);
                                                break;
                                            case "D": //High ground
                                                SetTextBackgroundColor(58);
                                                break;
                                            case "L": //Lava
                                                if (world[limitedX, limitedY][1].ToString() == "H")
                                                {
                                                    SetTextBackgroundColor(208);
                                                }
                                                else
                                                {
                                                    SetTextBackgroundColor(202);
                                                }
                                                break;
                                            case "l": //Lava
                                                SetTextBackgroundColor(202);
                                                break;
                                            default:
                                                SetTextBackgroundColor(0);
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    SetTextBackgroundColor(0);
                                    break;
                            }

                            if (!GV.renderDebug && refreshColor)
                            {
                                SetTextColor(GV.currentTextBackgroundColor);
                                text = world[limitedX, limitedY];
                            }
                            else if (refreshColor)
                            {
                                SetTextColor(1);
                                text = world[limitedX, limitedY];
                            }

                            if (GV.currentWorldStructures[limitedX, limitedY][0].ToString() == "T") //Checks for towns
                            {
                                text = "/\\";
                            }
                            else if (GV.currentWorldStructures[limitedX, limitedY][0].ToString() == "F") //Checks for forests
                            {
                                switch (GV.currentWorldStructures[limitedX, limitedY][1].ToString())
                                {
                                    case "0":
                                        text = "PP";
                                        break;
                                    case "T":
                                        text = "AA";
                                        break;
                                    case "J":
                                        text = "TT";
                                        break;
                                    case "S":
                                        text = "TT";
                                        break;
                                    case "3":
                                        text = "pp";
                                        break;
                                    case "9":
                                        text = "LL";
                                        break;
                                }
                            }

                            if (GV.currentWorldEntities[limitedX, limitedY] != "[]")
                            {
                                text = GV.currentWorldEntities[limitedX, limitedY][0].ToString() + text[1].ToString();
                                useEntityColor = true;

                                switch (GV.currentWorldEntities[limitedX, limitedY][0].ToString())
                                {
                                    case "H":
                                        entityColor = 0;
                                        break;
                                    default:
                                        entityColor = 1;
                                        break;
                                }
                            }

                            tileNum++;

                            if (!useEntityColor)
                            {
                                Console.Write(text);
                            }
                            else
                            {
                                int currentIndex = GV.currentTextColor;
                                SetTextColor(entityColor);

                                Console.Write(text[0]);

                                SetTextColor(currentIndex);
                                Console.Write(text[1]);
                            }

                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }
        }


        //Here go all methods used for menus!!!

        private void DisplayOptions()
        {
            string promptType = "options";
            string[] options = { "Screen Size", "Color Scheme", "Back" };
            Menu optMenu = new(promptType, options);
            int selectedIndex = optMenu.Run();

            switch (selectedIndex)
            {
                case 0:
                    ScreenSizeOptions();
                    break;
                case 1:
                    ColorSchemeOptions();
                    break;
                case 2:
                    RunMainMenu();
                    break;
            }
        }

        private void ScreenSizeOptions()
        {
            Console.Clear();
            ScreenTop('o');
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(10);
            Console.Write("Change Screen size with arrow keys. Press Enter to confirm.\n");
            ResetTextColor(false);

            SetTextColor(226);
            Game.ScreenLeft(GV.screenWidth / 5);
            Console.Write("Screen Size:\n");
            ResetTextColor(false);
            Game.ScreenLeft(GV.screenWidth / 5);
            Console.Write(GV.screenWidth);

            ConsoleKey keyPressed;
            do
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    GV.screenWidth += 5;
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    GV.screenWidth -= 5;
                    if (GV.screenWidth < 0)
                    {
                        GV.screenWidth = 0;
                    }
                }

                Console.Clear();
                ScreenTop('o');
                Game.ScreenLeft(GV.screenWidth / 5);
                SetTextColor(10);
                Console.Write("Change Screen size with arrow keys. Press Enter to confirm.\n");
                ResetTextColor(false);

                SetTextColor(226);
                Game.ScreenLeft(GV.screenWidth / 5);
                Console.Write("Screen Size:\n");
                ResetTextColor(false);
                Game.ScreenLeft(GV.screenWidth / 5);
                Console.Write(GV.screenWidth);

            } while (keyPressed != ConsoleKey.Enter);

            DisplayOptions();
        }

        private void ColorSchemeOptions()
        {
            string promptType = "clrScheme";
            string[] options = { "Default", "Classic", "Reverse", "Cool", "Warm", "Ocean", "Air", "Ground", "Inverse Ground", "Volcanic", "Retro", "Error", "Mint", "Bubblegum", "Pink", "Custom" };
            Menu clrMenu = new(promptType, options);
            int selectedIndex = clrMenu.Run();

            switch (selectedIndex)
            {
                case 0:
                    GV.defaultTextColor = 253;
                    GV.defaultTextBackgroundColor = 233;
                    break;
                case 1:
                    GV.defaultTextColor = 15;
                    GV.defaultTextBackgroundColor = 16;
                    break;
                case 2:
                    GV.defaultTextColor = 233;
                    GV.defaultTextBackgroundColor = 253;
                    break;
                case 3:
                    GV.defaultTextColor = 50;
                    GV.defaultTextBackgroundColor = 12;
                    break;
                case 4:
                    GV.defaultTextColor = 214;
                    GV.defaultTextBackgroundColor = 94;
                    break;
                case 5:
                    GV.defaultTextColor = 27;
                    GV.defaultTextBackgroundColor = 17;
                    break;
                case 6:
                    GV.defaultTextColor = 253;
                    GV.defaultTextBackgroundColor = 12;
                    break;
                case 7:
                    GV.defaultTextColor = 10;
                    GV.defaultTextBackgroundColor = 58;
                    break;
                case 8:
                    GV.defaultTextColor = 213;
                    GV.defaultTextBackgroundColor = 19;
                    break;
                case 9:
                    GV.defaultTextColor = 208;
                    GV.defaultTextBackgroundColor = 233;
                    break;
                case 10:
                    GV.defaultTextColor = 40;
                    GV.defaultTextBackgroundColor = 16;
                    break;
                case 11:
                    GV.defaultTextColor = 196;
                    GV.defaultTextBackgroundColor = 16;
                    break;
                case 12:
                    GV.defaultTextColor = 75;
                    GV.defaultTextBackgroundColor = 122;
                    break;
                case 13:
                    GV.defaultTextColor = 48;
                    GV.defaultTextBackgroundColor = 55;
                    break;
                case 14:
                    GV.defaultTextColor = 219;
                    GV.defaultTextBackgroundColor = 97;
                    break;
                case 15:
                    CustomColorCreator();
                    break;
            }
            DisplayOptions();
        }

        private void CustomColorCreator()
        {
            bool isInt = false;

            Console.Clear();
            ScreenTop('o');
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(10);
            Console.Write("Type in the text color index. (0 - 255) Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                string unparsedColor = Console.ReadLine();
                if (int.TryParse(unparsedColor, out GV.defaultTextColor) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('o');
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(10);
                    Console.Write("Type in the text color index. (0 - 255) Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }

            isInt = false;

            Console.Clear();
            ScreenTop('o');
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(10);
            Console.Write("Type in the text background color index. (0 - 255) Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                string unparsedColor = Console.ReadLine();
                if (int.TryParse(unparsedColor, out GV.defaultTextBackgroundColor) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('o');
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(10);
                    Console.Write("Type in the text color index. (0 - 255) Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }
        }

        private void RunMainMenu()
        {
            string promptType = "main";
            string[] options = { "Create World", "Options", "Credits", "Exit" };
            Menu mainMenu = new(promptType, options);
            int selectedIndex = mainMenu.Run();

            switch (selectedIndex)
            {
                case 0:
                    DisplayWorldSelection();
                    break;
                case 1:
                    DisplayOptions();
                    break;
                case 2:
                    DisplayCredits();
                    break;
                case 3:
                    ExitGame();
                    break;
            }
        }

        private void ExitGame()
        {
            ScreenTop('o');
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(10);
            Console.WriteLine(@$"Thank you for playing TextWorld Version {GV.version}!");
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(245);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        private void DisplayCredits()
        {
            ScreenTop('o');
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(10);
            Console.Write($"TextWorld Version {GV.version} Credits:\n");
            ResetTextColor(false);

            Game.ScreenLeft(GV.screenWidth / 5);
            Console.Write("-Most code made by Joni R.\n");
            Game.ScreenLeft(GV.screenWidth / 5);
            Console.Write("-The code to make the text ");

            SetTextColor(27);
            Console.Write("c");
            SetTextColor(48);
            Console.Write("o");
            SetTextColor(40);
            Console.Write("l");
            SetTextColor(76);
            Console.Write("o");
            SetTextColor(148);
            Console.Write("r");
            SetTextColor(214);
            Console.Write("f");
            SetTextColor(196);
            Console.Write("u");
            SetTextColor(199);
            Console.Write("l");
            SetTextColor(93);
            Console.Write(":");

            ResetTextColor(false);

            Console.Write(" Alexei Shcherbakov\n");
            Game.ScreenLeft(GV.screenWidth / 5);

            SetTextColor(245);
            Console.Write("Press any key to go back to the main menu...");
            ResetTextColor(false);
            Console.ReadKey(true);
            RunMainMenu();
        }

        private void DisplayWorldSelection()
        {
            string promptType = "worldSelect";
            string[] options = { "Create New World", "Load World [NOT YET POSSIBLE]", "Back" };
            Menu worldSelectMenu = new(promptType, options);
            int selectedIndex = worldSelectMenu.Run();

            switch (selectedIndex)
            {
                case 0:
                    DisplayWorldCreation();
                    break;
                case 1:
                    DisplayWorldSelection();
                    break;
                case 2:
                    RunMainMenu();
                    break;
            }
        }

        private void DisplayWorldCreation()
        {
            string promptType = "createWorld";
            string[] options = { $"World Name: {GV.newWorldName}", $"Gamemode: {GV.newWorldGamemode}", $"Difficulty: {GV.newWorldDifficulty}", $"World Type: {GV.newWorldType}", $"World Size: {GV.newWorldSize}", "CREATE", "Back" };
            Menu worldSelectMenu = new(promptType, options);
            int selectedIndex = worldSelectMenu.Run();

            switch (selectedIndex)
            {
                case 0:
                    WorldNameOptions();
                    break;
                case 1:
                    WorldGamemodeOptions();
                    break;
                case 2:
                    WorldDifficultyOptions();
                    break;
                case 3:
                    WorldTypeOptions();
                    break;
                case 4:
                    WorldSizeOptions();
                    break;
                case 5:
                    CreateWorld();
                    break;
                case 6:
                    DisplayWorldSelection();
                    break;
            }
        }

        private void WorldNameOptions()
        {
            Console.Clear();
            ScreenTop('d');
            Game.ScreenLeft(GV.screenWidth / 5);
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(245);
            Console.Write("Type in the world name. Press Enter to confirm.\n");
            Game.ScreenLeft(GV.screenWidth / 5);

            GV.newWorldName = Console.ReadLine();
            DisplayWorldCreation();
        }

        private void WorldTypeOptions()
        {
            string promptType = "worldType";
            string[] options = { "Default", "Warm", "Desert", "Cool", "Antarctica", "Flooded", "Dry", "Flat", "Volcanic", "Inverted", "Custom" };
            Menu worldTypeMenu = new(promptType, options);
            int selectedIndex = worldTypeMenu.Run();

            GV.newWorldType = options[selectedIndex];

            if (selectedIndex == 10)
            {
                DisplayCustomWorldCreation();
            }
            else
            {
                DisplayWorldCreation();
            }
        }

        private void DisplayCustomWorldCreation()
        {
            string promptType = "worldType";
            string[] options = { $"Generate Random Biomes: {GV.worldGenerateBiomes}", $"Generate Special Biomes: {GV.worldGenerateSpecialBiomes}", $"Main Biome: {GV.worldMainBiome}", $"Generate Features: {GV.worldGenerateFeatures}", $"Generate Structures: {GV.worldGenerateStructures}", $"Temperature: {GV.worldTemperature}", $"Sea Level: {GV.worldSeaLevel}", $"Island Amount: {GV.worldIslandAmount}", "Back" };
            Menu worldTypeMenu = new(promptType, options);
            int selectedIndex = worldTypeMenu.Run();

            switch (selectedIndex)
            {
                case 0:
                    CustomGenBiomes();
                    break;
                case 1:
                    CustomGenSpecialBiomes();
                    break;
                case 2:
                    CustomMainBiome();
                    break;
                case 3:
                    CustomGenFeatures();
                    break;
                case 4:
                    CustomGenStructures();
                    break;
                case 5:
                    CustomTemperature();
                    break;
                case 6:
                    CustomSeaLevel();
                    break;
                case 7:
                    CustomIslandAmount();
                    break;
            }

            DisplayWorldCreation();
        }

        private void CustomGenBiomes()
        {
            if (GV.worldGenerateBiomes)
            {
                GV.worldGenerateBiomes = false;
            }
            else
            {
                GV.worldGenerateBiomes = true;
            }

            DisplayCustomWorldCreation();
        }

        private void CustomGenSpecialBiomes()
        {
            if (GV.worldGenerateSpecialBiomes)
            {
                GV.worldGenerateSpecialBiomes = false;
            }
            else
            {
                GV.worldGenerateSpecialBiomes = true;
            }

            DisplayCustomWorldCreation();
        }

        private void CustomMainBiome()
        {
            string promptType = "worldType";
            string[] options = { "Grassy Plains", "Jungle", "Savanna", "Desert", "Mountains", "Tundra", "Snow", "Volcanic", "Candy", "Inverted", "None" };
            Menu mainBiomeMenu = new(promptType, options);
            int selectedIndex = mainBiomeMenu.Run();

            GV.worldMainBiome = options[selectedIndex];

            DisplayCustomWorldCreation();
        }

        private void CustomGenFeatures()
        {
            if (GV.worldGenerateFeatures)
            {
                GV.worldGenerateFeatures = false;
            }
            else
            {
                GV.worldGenerateFeatures = true;
            }

            DisplayCustomWorldCreation();
        }

        private void CustomGenStructures()
        {
            if (GV.worldGenerateStructures)
            {
                GV.worldGenerateStructures = false;
            }
            else
            {
                GV.worldGenerateStructures = true;
            }

            DisplayCustomWorldCreation();
        }

        private void CustomTemperature()
        {
            bool isInt = false;

            Console.Clear();
            ScreenTop('d');
            Game.ScreenLeft(GV.screenWidth / 5);
            Game.ScreenLeft(GV.screenWidth / 8);
            SetTextColor(245);
            Console.Write("(50 is the default, lower makes cool biomes more common and larger makes hot biomes more common)");
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(245);
            Console.Write("Type in the temperature. Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                Game.ScreenLeft(GV.screenWidth / 5);
                string unparsedSize = Console.ReadLine();
                if (int.TryParse(unparsedSize, out GV.worldTemperature) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('d');
                    Game.ScreenLeft(GV.screenWidth / 5);
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GV.screenWidth / 8);
                    SetTextColor(245);
                    Console.Write("(50 is the default, lower makes cool biomes more common and larger makes hot biomes more common)");
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(245);
                    Console.Write("Type in the temperature. Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }

            DisplayCustomWorldCreation();
        }

        private void CustomSeaLevel()
        {
            bool isInt = false;

            Console.Clear();
            ScreenTop('d');
            Game.ScreenLeft(GV.screenWidth / 5);
            Game.ScreenLeft(GV.screenWidth / 8);
            SetTextColor(245);
            Console.Write("(8 is the default, lower makes seas higher and larger makes seas lower. >9 for no seas.)");
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(245);
            Console.Write("Type in the sea level. Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                Game.ScreenLeft(GV.screenWidth / 5);
                string unparsedSize = Console.ReadLine();
                if (int.TryParse(unparsedSize, out GV.worldSeaLevel) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('d');
                    Game.ScreenLeft(GV.screenWidth / 5);
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GV.screenWidth / 8);
                    SetTextColor(245);
                    Console.Write("(8 is the default, lower makes seas higher and larger makes seas lower. >9 for no seas.)");
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(245);
                    Console.Write("Type in the sea level. Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }

            DisplayCustomWorldCreation();
        }

        private void CustomIslandAmount()
        {
            bool isInt = false;

            Console.Clear();
            ScreenTop('d');
            Game.ScreenLeft(GV.screenWidth / 5);
            Game.ScreenLeft(GV.screenWidth / 5);
            SetTextColor(245);
            Console.Write("Type in how many islands you want. Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                Game.ScreenLeft(GV.screenWidth / 5);
                string unparsedSize = Console.ReadLine();
                if (int.TryParse(unparsedSize, out GV.worldIslandAmount) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('d');
                    Game.ScreenLeft(GV.screenWidth / 5);
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(245);
                    Console.Write("Type in how many islands you want. Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }

            DisplayCustomWorldCreation();
        }

        private void WorldSizeOptions()
        {
            bool isInt = false;

            Console.Clear();
            ScreenTop('d');
            Game.ScreenLeft(GV.screenWidth / 5);
            Game.ScreenLeft(GV.screenWidth / 8);
            SetTextColor(245);
            Console.Write("Type in the world size. (Use anything else than 1-5000 with caution!) Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                Game.ScreenLeft(GV.screenWidth / 5);
                string unparsedSize = Console.ReadLine();
                if (int.TryParse(unparsedSize, out GV.newWorldSize) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('d');
                    Game.ScreenLeft(GV.screenWidth / 5);
                    Game.ScreenLeft(GV.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GV.screenWidth / 8);
                    SetTextColor(245);
                    Console.Write("Type in the world size. (Use anything else than 1-5000 with caution!) Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }

            DisplayWorldCreation();
        }

        private void WorldGamemodeOptions()
        {
            switch (GV.newWorldGamemode)
            {
                case "World Viewer":
                    GV.newWorldGamemode = "Survival";
                    break;
                case "Survival":
                    GV.newWorldGamemode = "Debug Mode";
                    break;
                case "Debug Mode":
                    GV.newWorldGamemode = "World Viewer";
                    break;
            }
            DisplayWorldCreation();
        }

        private void WorldDifficultyOptions()
        {
            switch (GV.newWorldDifficulty)
            {
                case "Easy":
                    GV.newWorldDifficulty = "Medium";
                    break;
                case "Medium":
                    GV.newWorldDifficulty = "Hard";
                    break;
                case "Hard":
                    GV.newWorldDifficulty = "One Life";
                    break;
                case "One Life":
                    GV.newWorldDifficulty = "Easy";
                    break;
            }
            DisplayWorldCreation();
        }

        //Here go methods for general hud things

        static void SetTextColor(int colorIndex)
        {
            Console.Write("\x1b[38;5;" + colorIndex + "m");
            GV.currentTextColor = colorIndex;
        }

        static void SetTextBackgroundColor(int colorIndex)
        {
            Console.Write("\x1b[48;5;" + colorIndex + "m");
            GV.currentTextBackgroundColor = colorIndex;
        }

        public static void ResetTextColor(bool inverted)
        {
            if (!inverted)
            {
                Console.Write("\x1b[38;5;" + GV.defaultTextColor + "m");
                Console.Write("\x1b[48;5;" + GV.defaultTextBackgroundColor + "m");
            }
            else
            {
                Console.Write("\x1b[38;5;" + GV.defaultTextBackgroundColor + "m");
                Console.Write("\x1b[48;5;" + GV.defaultTextColor + "m");
            }
        }

        public static void ScreenTop(char topType)
        {
            ResetTextColor(false);

            Console.Clear();
            Console.Write("  /");
            for (int i = 0; i < GV.screenWidth - 6; i++)
            {
                Console.Write("-");
            }
            Console.Write("\\  ");

            switch (topType)
            {
                case 'm':
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write(" _____             _    __    __              _      _ ");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write("/__   \\ ___ __  __| |_ / / /\\ \\ \\ ___   _ __ | |  __| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write("  / /\\// _ \\\\ \\/ /| __|\\ \\/  \\/ // _ \\ | '__|| | / _` |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write(" / /  |  __/ >  < | |_  \\  /\\  /| (_) || |   | || (_| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write(" \\/    \\___|/_/\\_\\ \\__|  \\/  \\/  \\___/ |_|   |_| \\__,_|\n");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(226);
                    Console.Write($"Version {GV.version} Loaded!");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write("(Move in the menu with arrow keys.Select an option with Enter.)\n\n\n");
                    ResetTextColor(false);

                    break;
                case 'o':
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write(" _____             _    __    __              _      _ ");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write("/__   \\ ___ __  __| |_ / / /\\ \\ \\ ___   _ __ | |  __| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write("  / /\\// _ \\\\ \\/ /| __|\\ \\/  \\/ // _ \\ | '__|| | / _` |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write(" / /  |  __/ >  < | |_  \\  /\\  /| (_) || |   | || (_| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write(" \\/    \\___|/_/\\_\\ \\__|  \\/  \\/  \\___/ |_|   |_| \\__,_|");
                    Game.ScreenLeft(GV.screenWidth / 2);
                    ResetTextColor(false);

                    break;
                case 'c':
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("   ___               _         __    __           _     _ ");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("  / __\\ __ ___  __ _| |_ ___  / / /\\ \\ \\___  _ __| | __| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write(" / / | '__/ _ \\/ _` | __/ _ \\ \\ \\/  \\/ / _ \\| '__| |/ _` |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("/ /__| | |  __/ (_| | ||  __/  \\  /\\  / (_) | |  | | (_| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("\\____/_|  \\___|\\__,_|\\__\\___|   \\/  \\/ \\___/|_|  |_|\\__,_|");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write($"{GV.newWorldName} Options:\n\n");
                    ResetTextColor(false);

                    break;
                case 'd':
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("   ___               _         __    __           _     _ ");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("  / __\\ __ ___  __ _| |_ ___  / / /\\ \\ \\___  _ __| | __| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write(" / / | '__/ _ \\/ _` | __/ _ \\ \\ \\/  \\/ / _ \\| '__| |/ _` |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("/ /__| | |  __/ (_| | ||  __/  \\  /\\  / (_) | |  | | (_| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("\\____/_|  \\___|\\__,_|\\__\\___|   \\/  \\/ \\___/|_|  |_|\\__,_|");
                    ResetTextColor(false);

                    break;
                case 'i':
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("  _____                      _                   ");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("  \\_   \\_ ____   _____ _ __ | |_ ___  _ __ _   _ ");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("   / /\\/ '_ \\ \\ / / _ \\ '_ \\| __/ _ \\| '__| | | |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("/\\/ /_ | | | \\ V /  __/ | | | || (_) | |  | |_| |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("\\____/ |_| |_|\\_/ \\___|_| |_|\\__\\___/|_|   \\__, |");
                    Game.ScreenLeft(GV.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("                                           |___/ ");

                    ResetTextColor(false);

                    break;
            }
        }

        public static void ScreenLeft(int margin)
        {
            ResetTextColor(false);

            Console.Write("\n");
            for (int i = 0; i < margin; i++)
            {
                Console.Write(" ");
            }
        }
        public static void ScreenLeft(int location, string str)
        {
            ResetTextColor(false);

            Console.Write("\n");
            for (int i = 0; i < location - (str.Length / 2) - 4; i++)
            {
                Console.Write(" ");
            }
            Console.Write(str);
        }


        //Here are some miscellanous methods

        void CreateWorld()
        {
            switch (GV.newWorldType) //Sets the settings
            {
                case "Default":
                    GV.worldGenerateBiomes = true;
                    GV.worldGenerateSpecialBiomes = true;
                    GV.worldMainBiome = "Grassy Plains";

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldTemperature = 50;
                    GV.worldSeaLevel = 8;

                    break;
                case "Warm":
                    GV.worldGenerateBiomes = true;
                    GV.worldGenerateSpecialBiomes = true;

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldTemperature = 80;
                    GV.worldSeaLevel = 8;

                    break;
                case "Desert":
                    GV.worldGenerateBiomes = false;
                    GV.worldGenerateSpecialBiomes = false;
                    GV.worldMainBiome = "Desert";

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldSeaLevel = 10;

                    break;
                case "Cold":
                    GV.worldGenerateBiomes = true;
                    GV.worldGenerateSpecialBiomes = true;

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldTemperature = 20;
                    GV.worldSeaLevel = 8;

                    break;
                case "Antarctica":
                    GV.worldGenerateBiomes = false;
                    GV.worldGenerateSpecialBiomes = false;
                    GV.worldMainBiome = "Snow";

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldSeaLevel = 8;

                    break;
                case "Flooded":
                    GV.worldGenerateBiomes = true;
                    GV.worldGenerateSpecialBiomes = true;

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldTemperature = 50;
                    GV.worldSeaLevel = 4;

                    break;
                case "Dry":
                    GV.worldGenerateBiomes = true;
                    GV.worldGenerateSpecialBiomes = true;

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldTemperature = 50;
                    GV.worldSeaLevel = 10;

                    break;
                case "Flat":
                    GV.worldGenerateBiomes = true;
                    GV.worldGenerateSpecialBiomes = true;

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldTemperature = 50;
                    GV.worldSeaLevel = 100;

                    break;
                case "Volcanic":
                    GV.worldGenerateBiomes = false;
                    GV.worldGenerateSpecialBiomes = false;
                    GV.worldMainBiome = "Volcanic";

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldSeaLevel = 10;

                    break;
                case "Inverted":
                    GV.worldGenerateBiomes = false;
                    GV.worldGenerateSpecialBiomes = false;
                    GV.worldMainBiome = "Inverted";

                    GV.worldGenerateFeatures = true;
                    GV.worldGenerateStructures = true;

                    GV.worldSeaLevel = 8;

                    break;
            }

            if (GV.newWorldType != "Custom") //Sets the default island amount if it hasn't been set
            {
                GV.worldIslandAmount = GV.newWorldSize / 20 * GV.newWorldSize / 20;
            }

            char mainBiome = '0';

            switch (GV.worldMainBiome) //Sets the main biome
            {
                case "Grassy Plains":
                    mainBiome = '0';
                    break;
                case "Jungle":
                    mainBiome = 'J';
                    break;
                case "Savanna":
                    mainBiome = 'S';
                    break;
                case "Desert":
                    mainBiome = 'D';
                    break;
                case "Mountain":
                    mainBiome = 'M';
                    break;
                case "Taiga":
                    mainBiome = 'T';
                    break;
                case "Snow":
                    mainBiome = 'C';
                    break;
                case "Volcanic":
                    mainBiome = 'V';
                    break;
                case "Candy":
                    mainBiome = '3';
                    break;
                case "Inverted":
                    mainBiome = '9';
                    break;
                case "None":
                    mainBiome = 'X';
                    break;
            }

            GV.currentWorldDepth = GenerateWorldDepth(GV.newWorldSize, GV.worldIslandAmount, GV.worldTemperature, GV.worldGenerateBiomes, GV.worldGenerateSpecialBiomes, mainBiome); //Generates the depthmap
            GV.currentWorld = GenerateWorld(GV.newWorldSize, GV.currentWorldDepth, GV.worldSeaLevel, GV.worldGenerateFeatures); //Generates the world map
            GV.currentWorldStructures = GenerateWorldStructures(GV.currentWorld, GV.currentWorldDepth, GV.newWorldSize, GV.worldSeaLevel, GV.worldGenerateFeatures);//Generates structures
            GV.currentWorldEntities = GenerateWorldEntities(GV.currentWorld, GV.newWorldSize);

            var rand = new Random();

            string[,] GenerateWorldDepth(int worldSize, int islandAmount, float temperature, bool generateBiomes, bool generateSpecialBiomes, char mainBiome)
            {

                string[,] world = new String[worldSize, worldSize]; //Creates the map

                ScreenTop('d');
                ScreenLeft(GV.screenWidth / 3);
                ScreenLeft((int)(GV.screenWidth / 2.5f));
                Console.WriteLine("Creating depthmap");

                var rand = new Random();
                int worldLimit = worldSize - 1;

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++)
                    {
                        world[x, y] = "99"; //Fills the map with "empty space"
                    }
                }

                ScreenTop('d');
                ScreenLeft(GV.screenWidth / 3);
                ScreenLeft((int)(GV.screenWidth / 2.5f));
                Console.WriteLine("Creating island seeds");

                for (int islands = 0; islands < islandAmount; islands++) //Generates islands
                {
                    if (temperature == 50f)
                    {
                        temperature = rand.Next(40, 60);
                        Console.Write("Global temperature modifier is: " + (temperature - 50) + "!");
                    }

                    float randTemp = rand.Next(1, 200) * (temperature / 100); //Creates a random temperature

                    int islandHeight = 0;

                    if (rand.Next(0, 2) != 0) //Sets the island starting height
                    {
                        islandHeight = rand.Next(0, 5);
                    }

                    if (generateBiomes) //Detemines the biome
                    {
                        if (generateSpecialBiomes && rand.Next(0, 21) == 0)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}V";
                        }
                        else if (generateSpecialBiomes && rand.Next(0, 81) == 1)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}3";
                        }
                        else if (generateSpecialBiomes && rand.Next(0, 81) == 2)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}9";
                        }
                        else if (randTemp <= 7f)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}C";
                        }
                        else if (randTemp <= 15f)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}M";
                        }
                        else if (randTemp <= 25f)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}T";
                        }
                        else if (randTemp >= 93f)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}D";
                        }
                        else if (randTemp >= 85f)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}S";
                        }
                        else if (randTemp >= 75f)
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}J";
                        }
                        else
                        {
                            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}0";
                        }
                    }
                    else
                    {
                        world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}{mainBiome}";
                    }
                }

                //RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

                ScreenTop('d');
                ScreenLeft(GV.screenWidth / 3);
                ScreenLeft((int)(GV.screenWidth / 2.5f));
                Console.WriteLine("Extending islands");

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++)
                    {
                        switch (world[x, y][0].ToString())
                        {
                            case "0":
                                for (int extendChance = 0; extendChance < 3; extendChance++)
                                {
                                    int xPosition = rand.Next(-1, 2);
                                    int yPosition = rand.Next(-1, 2);

                                    if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() == "9")
                                    {
                                        world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = $"0{world[x, y][1].ToString()}";
                                    }
                                }
                                break;
                        }
                    }
                }

                //RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

                for (int gen2 = 0; gen2 < 5; gen2++)
                {
                    for (int x = 0; x < worldSize; x++)
                    {
                        for (int y = 0; y < worldSize; y++)
                        {
                            switch (world[x, y][0].ToString())
                            {
                                case "0":
                                    ExtendLand(x, y, worldLimit, world, $"1{world[x, y][1].ToString()}", $"0{world[x, y][1].ToString()}");
                                    break;
                                case "1":
                                    ExtendLand(x, y, worldLimit, world, $"2{world[x, y][1].ToString()}", $"3{world[x, y][1].ToString()}");
                                    break;
                                case "2":
                                    ExtendLand(x, y, worldLimit, world, $"3{world[x, y][1].ToString()}", $"2{world[x, y][1].ToString()}");
                                    break;
                                case "3":
                                    ExtendLand(x, y, worldLimit, world, $"4{world[x, y][1].ToString()}", $"5{world[x, y][1].ToString()}");
                                    break;
                            }
                        }
                    }
                }
                //RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

                for (int gen3 = 0; gen3 < 5; gen3++)
                {
                    for (int x = 0; x < worldSize; x++)
                    {
                        for (int y = 0; y < worldSize; y++)
                        {
                            switch (world[x, y][0].ToString())
                            {
                                case "4":
                                    ExtendLand(x, y, worldLimit, world, $"5{world[x, y][1].ToString()}", $"4{world[x, y][1].ToString()}");
                                    break;
                                case "5":
                                    ExtendLand(x, y, worldLimit, world, $"6{world[x, y][1].ToString()}", $"7{world[x, y][1].ToString()}");
                                    break;
                            }
                        }
                    }
                }
                //RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

                for (int gen4 = 0; gen4 < 5; gen4++)
                {
                    for (int x = 0; x < worldSize; x++)
                    {
                        for (int y = 0; y < worldSize; y++)
                        {
                            switch (world[x, y][0].ToString())
                            {
                                case "6":
                                    ExtendLand(x, y, worldLimit, world, $"7{world[x, y][1].ToString()}", $"7{world[x, y][1].ToString()}");
                                    break;
                                case "7":
                                    ExtendLand(x, y, worldLimit, world, $"8{world[x, y][1].ToString()}", $"7{world[x, y][1].ToString()}");
                                    break;
                                case "8":
                                    ExtendLand(x, y, worldLimit, world, $"8{world[x, y][1].ToString()}", $"9{world[x, y][1].ToString()}");
                                    break;
                            }
                        }
                    }
                }
                //RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

                return world;
            }

            string[,] GenerateWorld(int worldSize, string[,] depthMap, int seaLevel, bool generateFeatures)
            {
                string[,] world = new String[worldSize, worldSize]; //Generates empty map
                var rand = new Random();

                ScreenTop('d');
                ScreenLeft(GV.screenWidth / 3);
                ScreenLeft((int)(GV.screenWidth / 2.5f));
                Console.WriteLine("Creating the map");

                int worldLimit = worldSize - 1;

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++) //Fills the map with empty spaace
                    {
                        world[x, y] = "[]";
                    }
                }

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++) //Creates world based on heightmap
                    {
                        string sandTile = "ss";
                        string lowGroundTile = "dd";
                        string highGroundTile = "DD";
                        string waterTile = "WW";
                        string deepWaterTile = "WD";

                        switch (depthMap[x, y][1].ToString())
                        { //Determine tiles
                            case "C":
                                lowGroundTile = "ff";
                                highGroundTile = "FF";

                                break;
                            case "T":
                                lowGroundTile = "gC";
                                highGroundTile = "GC";

                                break;
                            case "M":
                                sandTile = "rr";
                                lowGroundTile = "rM";
                                highGroundTile = "RM";

                                break;
                            case "0":
                                lowGroundTile = "gg";
                                highGroundTile = "GG";

                                break;
                            case "J":
                                sandTile = "ss";
                                lowGroundTile = "gJ";
                                highGroundTile = "GJ";

                                break;
                            case "S":
                                lowGroundTile = "gH";
                                highGroundTile = "GH";

                                break;
                            case "D":
                                lowGroundTile = "ss";
                                highGroundTile = "SS";

                                break;
                            case "V":
                                sandTile = "sV";
                                lowGroundTile = "rV";
                                highGroundTile = "RV";

                                break;
                            case "3":
                                sandTile = "s3";
                                lowGroundTile = "g3";
                                highGroundTile = "G3";

                                break;
                            case "9":
                                sandTile = "s9";
                                lowGroundTile = "g9";
                                highGroundTile = "G9";

                                break;
                        }

                        if (Convert.ToInt16(depthMap[x, y][0].ToString()) == seaLevel)
                        {
                            world[x, y] = waterTile;
                        }
                        else if (Convert.ToInt16(depthMap[x, y][0].ToString()) > seaLevel)
                        {
                            world[x, y] = deepWaterTile;
                        }
                        else if (Convert.ToInt16(depthMap[x, y][0].ToString()) > seaLevel - 3)
                        {
                            world[x, y] = sandTile;
                        }
                        else if (Convert.ToInt16(depthMap[x, y][0].ToString()) > seaLevel - 6)
                        {
                            world[x, y] = lowGroundTile;
                        }
                        else
                        {
                            world[x, y] = highGroundTile;
                        }
                    }
                }

                if (generateFeatures == true) //Generates features
                {
                    ScreenTop('d');
                    ScreenLeft(GV.screenWidth / 3);
                    ScreenLeft((int)(GV.screenWidth / 2.5f));
                    Console.WriteLine("Creating water features");

                    for (int x = 0; x < worldSize; x++)
                    {
                        for (int y = 0; y < worldSize; y++)
                        {
                            if (depthMap[x, y][1].ToString() == "D" && rand.Next(0, 300) == 0)
                            {
                                if (Convert.ToInt16(depthMap[x, y][0].ToString()) < seaLevel)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "oasis", 'a');
                                }
                            }
                            if (depthMap[x, y][1].ToString() == "S" && rand.Next(0, 500) == 0)
                            {
                                if (Convert.ToInt16(depthMap[x, y][0].ToString()) < seaLevel)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "oasis", 'a');
                                }
                            }
                            if (world[x, y][0].ToString() != "W" && rand.Next(0, 2000) == 0)
                            {
                                GenerateFeature(x, y, worldSize, world, depthMap, "river", 'a');
                            }
                            if (world[x, y][0].ToString() != "W" && depthMap[x, y][1].ToString() == "V" && rand.Next(0, 500) == 0)
                            {
                                GenerateFeature(x, y, worldSize, world, depthMap, "river", 'b');
                            }
                        }
                    }

                    ScreenTop('d');
                    ScreenLeft(GV.screenWidth / 3);
                    ScreenLeft((int)(GV.screenWidth / 2.5f));
                    Console.WriteLine("Creating other features");

                    for (int x = 0; x < worldSize; x++)
                    {
                        for (int y = 0; y < worldSize; y++)
                        {
                            if (depthMap[x, y][1].ToString() != "C" && rand.Next(0, 8000) == 0)
                            {
                                if (Convert.ToInt16(depthMap[x, y][0].ToString()) < seaLevel)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "volcano", 'a');
                                }
                            }
                            if (depthMap[x, y][1].ToString() == "V" && rand.Next(0, 500) == 0)
                            {
                                if (Convert.ToInt16(depthMap[x, y][0].ToString()) < seaLevel)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "volcano", 'a');
                                }
                            }
                            if (depthMap[x, y][1].ToString() == "C" && rand.Next(0, 3) == 0)
                            {
                                if (Convert.ToInt16(depthMap[x, y][0].ToString()) >= seaLevel)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "ice", 'a');
                                }
                            }
                            if (depthMap[x, y][1].ToString() == "T" && rand.Next(0, 15) == 0)
                            {
                                if (Convert.ToInt16(depthMap[x, y][0].ToString()) >= seaLevel)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "ice", 'a');
                                }
                            }
                            if (depthMap[x, y][1].ToString() == "T" && rand.Next(0, 200) == 0)
                            {
                                if (Convert.ToInt16(depthMap[x, y][0].ToString()) < seaLevel)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "patch", 'c');
                                }
                            }
                            if (depthMap[x, y][1].ToString() == "M")
                            {
                                if (Convert.ToInt16(depthMap[x, y][0].ToString()) < seaLevel - 7)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "patch", 'm');
                                }
                            }
                            if (depthMap[x, y][1].ToString() != "9")
                            {
                                if (depthMap[x, y][0].ToString() != "W" && rand.Next(0, 1000) == 0)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "rock", 'a');
                                }
                                else if (depthMap[x, y][0].ToString() == "W" && rand.Next(0, 2000) == 0)
                                {
                                    GenerateFeature(x, y, worldSize, world, depthMap, "rock", 'a');
                                }
                            }
                        }
                    }
                }

                //Post-generation; Making the shores look nicer and stuff
                ScreenTop('d');
                ScreenLeft(GV.screenWidth / 3);
                ScreenLeft((int)(GV.screenWidth / 2.5f));
                Console.WriteLine("Making the world look nicer");

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++)
                    {
                        if (world[x, y][1].ToString() == "W")
                        {
                            for (int genTry = 0; genTry < 3; genTry++)
                            {
                                int xPosition = rand.Next(-1, 2);
                                int yPosition = rand.Next(-1, 2);

                                if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() != "W" && world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() != "I")
                                {
                                    world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "ww";
                                }
                            }
                        }
                    }
                }

                return world;
            }

            string[,] GenerateWorldStructures(string[,] world, string[,] depthMap, int worldSize, int seaLevel, bool generateFeatures)
            {
                string[,] worldStructures = new string[worldSize, worldSize]; //Generates empty map
                var rand = new Random();

                ScreenTop('d');
                ScreenLeft(GV.screenWidth / 3);
                ScreenLeft((int)(GV.screenWidth / 2.5f));
                Console.WriteLine("Creating structure map");

                int worldLimit = worldSize - 1;

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++) //Fills the map with empty spaace
                    {
                        worldStructures[x, y] = "[]";
                    }
                }

                if (GV.worldGenerateStructures)
                {
                    ScreenTop('d');
                    ScreenLeft(GV.screenWidth / 3);
                    ScreenLeft((int)(GV.screenWidth / 2.5f));
                    Console.WriteLine("Creating towns");

                    for (int x = 0; x < worldSize; x++) //Generates towns
                    {
                        for (int y = 0; y < worldSize; y++)
                        {
                            if (world[x, y][0].ToString() == "g" | world[x, y][0].ToString() == "G" | world[x, y][0].ToString() == "d" | world[x, y][0].ToString() == "D")
                            {
                                if (rand.Next(1, 900) == 1)
                                {
                                    //Generates Town
                                    worldStructures[x, y] = $"T{depthMap[x, y][1].ToString()}";

                                    for (int e = 0; e < rand.Next(3, 12); e++)
                                    {
                                        worldStructures[Limit(x + rand.Next(-2, 3)), Limit(y + rand.Next(-2, 3))] = $"T{depthMap[Limit(x + rand.Next(-2, 3)), Limit(y + rand.Next(-2, 3))][1].ToString()}";
                                    }
                                }
                            }
                        }
                    }

                    ScreenTop('d');
                    ScreenLeft(GV.screenWidth / 3);
                    ScreenLeft((int)(GV.screenWidth / 2.5f));
                    Console.WriteLine("Creating forests");

                    for (int x = 0; x < worldSize; x++) //Generates forests
                    {
                        for (int y = 0; y < worldSize; y++)
                        {
                            if (world[x, y][0].ToString() == "G")
                            {
                                if (rand.Next(1, 20) == 1)
                                {
                                    //Generates Forests
                                    switch(depthMap[x, y][1].ToString())
                                    {
                                        case "0":
                                            worldStructures[x, y] = "F0";

                                            for (int e = 0; e < rand.Next(3, 12); e++)
                                            {
                                                int xOffset = rand.Next(-2, 3);
                                                int yOffset = rand.Next(-2, 3);

                                                if (!GV.tilesAllowTrees.Any(GV.currentWorld[x, y][0].ToString().Contains))
                                                {
                                                    worldStructures[Limit(x + xOffset), Limit(y + yOffset)] = "F0";
                                                }
                                            }
                                            break;
                                        case "S":
                                            worldStructures[x, y] = "FS";

                                            for (int e = 0; e < rand.Next(3, 12); e++)
                                            {
                                                int xOffset = rand.Next(-3, 4);
                                                int yOffset = rand.Next(-3, 4);

                                                if (!GV.tilesAllowTrees.Any(GV.currentWorld[x, y][0].ToString().Contains))
                                                {
                                                    worldStructures[Limit(x + xOffset), Limit(y + yOffset)] = "FS";
                                                }
                                            }
                                            break;
                                        case "T":
                                            worldStructures[x, y] = "FT";

                                            for (int e = 0; e < rand.Next(5, 17); e++)
                                            {
                                                int xOffset = rand.Next(-3, 4);
                                                int yOffset = rand.Next(-3, 4);

                                                if (!GV.tilesAllowTrees.Any(GV.currentWorld[x, y][0].ToString().Contains))
                                                {
                                                    worldStructures[Limit(x + xOffset), Limit(y + yOffset)] = "FT";
                                                }
                                            }
                                            break;
                                        case "J":
                                            worldStructures[x, y] = "FJ";

                                            for (int e = 0; e < rand.Next(18, 40); e++)
                                            {
                                                int xOffset = rand.Next(-4, 5);
                                                int yOffset = rand.Next(-4, 5);

                                                if (!GV.tilesAllowTrees.Any(GV.currentWorld[x, y][0].ToString().Contains))
                                                {
                                                    worldStructures[Limit(x + xOffset), Limit(y + yOffset)] = "FJ";
                                                }
                                            }
                                            break;
                                        case "3":
                                            worldStructures[x, y] = "F3";

                                            for (int e = 0; e < rand.Next(5, 15); e++)
                                            {
                                                int xOffset = rand.Next(-2, 3);
                                                int yOffset = rand.Next(-4, 5);

                                                if (!GV.tilesAllowTrees.Any(GV.currentWorld[x, y][0].ToString().Contains)) 
                                                {
                                                    worldStructures[Limit(x + xOffset), Limit(y + yOffset)] = "F3";
                                                }
                                            }
                                            break;
                                        case "9":
                                            worldStructures[x, y] = "F9";

                                            for (int e = 0; e < rand.Next(3, 12); e++)
                                            {
                                                int xOffset = rand.Next(-2, 3);
                                                int yOffset = rand.Next(-2, 3);

                                                if (!GV.tilesAllowTrees.Any(GV.currentWorld[x, y][0].ToString().Contains))
                                                {
                                                    worldStructures[Limit(x + xOffset), Limit(y + yOffset)] = "F9";
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                return worldStructures;
            }

            string[,] GenerateWorldEntities(string[,] world, int worldSize)
            {
                string[,] worldEntities = new string[worldSize, worldSize]; //Generates empty map
                var rand = new Random();

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++) //Fills the map with empty spaace
                    {
                        worldEntities[x, y] = "[]";
                    }
                }

                return worldEntities;
            }

            string[,] ExtendLand(int x, int y, int worldLimit, string[,] world, string chance1, string chance2)
            {
                var rand = new Random();

                for (int extendChance = 0; extendChance < 8; extendChance++)
                {
                    int xPosition = rand.Next(-1, 2);
                    int yPosition = rand.Next(-1, 2);

                    if (ReturnWorldTile(x + xPosition, y + yPosition, world, 1) == "9")
                    {
                        if (rand.Next(0, 3) > 0)
                        {
                            world[Limit(x + xPosition), Limit(y + yPosition)] = chance1;
                        }
                        else
                        {
                            world[Limit(x + xPosition), Limit(y + yPosition)] = chance2;
                        }
                    }
                }

                return world;
            }
            string[,] CreateCircle(int x, int y, int radius, bool isFilled, string tile, string altTile, string[,] world, int worldSize)
            {
                var rand = new Random();
                int worldLimit = worldSize - 1;

                if (isFilled)
                {
                    for (int xPos = 0; xPos < worldSize; xPos++)
                    {
                        for (int yPos = 0; yPos < worldSize; yPos++)
                        {
                            if (xPos > x - (radius) && xPos < x + (radius))
                            {
                                if (yPos > y - (radius) && yPos < y + (radius))
                                {
                                    if (rand.Next(0, 3) == 0)
                                    {
                                        if (tile != "null")
                                        {
                                            world[xPos, yPos] = tile;
                                        }
                                    }
                                    else
                                    {
                                        if (altTile != "null")
                                        {
                                            world[xPos, yPos] = altTile;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                for (int tiles = 0; tiles < radius * 2 + 1; tiles++)
                {
                    if (tiles < radius)
                    {
                        if (rand.Next(0, 3) != 0)
                        {
                            if (tile != "null")
                            {
                                world[Limit(x + tiles), Limit(y + radius - tiles)] = tile;
                            }
                        }
                        else
                        {
                            if (altTile != "null")
                            {
                                world[Limit(x + tiles), Limit(y + radius - tiles)] = altTile;
                            }
                        }
                    }
                    else
                    {
                        if (rand.Next(0, 3) != 0)
                        {
                            if (tile != "null")
                            {
                                world[Limit(x + radius - (tiles - radius)), Limit(y + radius - tiles)] = tile;
                            }
                        }
                        else
                        {
                            if (altTile != "null")
                            {
                                world[Limit(x + radius - (tiles - radius)), Limit(y + radius - tiles)] = altTile;
                            }
                        }
                    }
                }

                for (int tiles = 0; tiles < (radius - 1) * 2 + 2; tiles++)
                {
                    if (tiles < radius)
                    {
                        if (rand.Next(0, 3) != 0)
                        {
                            if (tile != "null")
                            {
                                world[Limit(x - tiles), Limit(y + radius - tiles)] = tile;
                            }
                        }
                        else
                        {
                            if (altTile != "null")
                            {
                                world[Limit(x - tiles), Limit(y + radius - tiles)] = altTile;
                            }
                        }
                    }
                    else
                    {
                        if (rand.Next(0, 3) != 0)
                        {
                            if (tile != "null")
                            {
                                world[Limit(x - radius + (tiles - radius)), Limit(y + radius - tiles)] = tile;
                            }
                        }
                        else
                        {
                            if (altTile != "null")
                            {
                                world[Limit(x - radius + (tiles - radius)), Limit(y + radius - tiles)] = altTile;
                            }
                        }
                    }
                }

                return world;
            }

            string[,] GenerateFeature(int x, int y, int worldSize, string[,] world, string[,] depthMap, string type, char type2)
            {
                var rand = new Random();

                int worldLimit = worldSize - 1;

                switch (type)
                {
                    case "oasis":
                        world[x, y] = "ww";

                        for (int extendChance = 0; extendChance < 5; extendChance++)
                        {
                            int xPosition = rand.Next(-1, 2);
                            int yPosition = rand.Next(-1, 2);

                            world[Limit(x + xPosition), Limit(y + yPosition)] = "ww";
                            for (int extendChance2 = 0; extendChance2 < 2; extendChance2++)
                            {
                                int xPosition2 = rand.Next(-1, 2);
                                int yPosition2 = rand.Next(-1, 2);

                                world[Limit(x + xPosition + xPosition2), Limit(y + yPosition + yPosition2)] = "GD";
                            }
                        }
                        break;
                    case "river":
                        int newX = x;
                        int newY = y;

                        int xDirection = rand.Next(-1, 2);
                        int yDirection = rand.Next(-1, 2);

                        switch (type2)
                        {
                            case 'a':
                                for (int extendChance = 0; extendChance < rand.Next(10, 50); extendChance++)
                                {
                                    int xPosition = rand.Next(-1, 2) + xDirection;
                                    int yPosition = rand.Next(-1, 2) + yDirection;

                                    int genTries = 0;
                                    bool canGen = true;

                                    while (ReturnWorldTile(newX + xPosition, newY + yPosition, world, 1) == "W" | ReturnWorldTile(newX + xPosition, newY + yPosition, world, 1) == "w")
                                    {
                                        if (genTries > 100) //Makes sure so the program doesnt get stuck
                                        {
                                            canGen = false;
                                            break;
                                        }
                                        genTries++;

                                        xPosition = rand.Next(-1, 2) + xDirection;
                                        yPosition = rand.Next(-1, 2) + yDirection;
                                    }

                                    if (canGen)
                                    {
                                        if (ReturnWorldTile(newX + xPosition - xDirection, newY + yPosition - yDirection, world, 1) == "f" | ReturnWorldTile(newX + xPosition - xDirection, newY + yPosition - yDirection, world, 1) == "F")
                                        {
                                            world[Limit(newX + xPosition - xDirection), Limit(newY + yPosition - yDirection)] = "II";
                                            world[Limit(newX + xPosition), Limit(newY + yPosition)] = "II";
                                        }
                                        else if (ReturnWorldTile(newX + xPosition - xDirection, newY + yPosition - yDirection, depthMap, 2) != "H")
                                        {
                                            world[Limit(newX + xPosition - xDirection), Limit(newY + yPosition - yDirection)] = "WR";
                                            world[Limit(newX + xPosition), Limit(newY + yPosition)] = "WR";
                                        }
                                    }

                                    newX += xPosition;
                                    newY += yPosition;

                                    if (canGen)
                                    {
                                        for (int extendChance2 = 0; extendChance2 < 4; extendChance2++)
                                        {
                                            int xPosition2 = rand.Next(-1, 2);
                                            int yPosition2 = rand.Next(-1, 2);

                                            switch (ReturnWorldTile(newX + xPosition2, newY + yPosition2, world, 1))
                                            {
                                                case "G":
                                                    world[Limit(newX + xPosition2), Limit(newY + yPosition2)] = "gg";
                                                    break;
                                                case "S":
                                                    world[Limit(newX + xPosition2), Limit(newY + yPosition2)] = "ss";
                                                    break;
                                                case "F":
                                                    world[Limit(newX + xPosition2), Limit(newY + yPosition2)] = "ff";
                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (rand.Next(0, 4) == 0)
                                {
                                    for (int newRivers = 0; newRivers < rand.Next(1, 3); newRivers++)
                                    {
                                        GenerateFeature(x, y, worldSize, world, depthMap, "river", 'a');
                                    }
                                }

                                break;
                            case 'b': //Lava rivers
                                for (int extendChance = 0; extendChance < rand.Next(5, 10); extendChance++)
                                {
                                    int xPosition = rand.Next(-1, 2) + xDirection;
                                    int yPosition = rand.Next(-1, 2) + yDirection;

                                    int genTries = 0;
                                    bool canGen = true;

                                    while (world[Limit(newX + xPosition), Limit(newY + yPosition)][0].ToString() == "L" | world[Limit(newX + xPosition), Limit(newY + yPosition)][0].ToString() == "l" | world[Limit(newX + xPosition), Limit(newY + yPosition)][0].ToString() == "r" | world[Limit(newX + xPosition), Limit(newY + yPosition)][0].ToString() == "R")
                                    {
                                        if (genTries > 5) //Makes sure so the program doesnt get stuck
                                        {
                                            canGen = false;
                                            break;
                                        }
                                        genTries++;

                                        xPosition = rand.Next(-1, 2) + xDirection;
                                        yPosition = rand.Next(-1, 2) + yDirection;
                                    }

                                    if (canGen)
                                    {
                                        if (world[Limit(newX + xPosition - xDirection), Limit(newY + yPosition - yDirection)][0].ToString() == "f" | world[Limit(newX + xPosition - xDirection), Limit(newY + yPosition - yDirection)][0].ToString() == "F")
                                        {
                                            world[Limit(newX + xPosition - xDirection), Limit(newY + yPosition - yDirection)] = "rr";
                                            world[Limit(newX + xPosition - xDirection), Limit(newY + yPosition - yDirection)] = "rr";
                                        }
                                        else
                                        {
                                            world[Limit(newX + xPosition - xDirection), Limit(newY + yPosition - yDirection)] = "ll";
                                            world[Limit(newX + xPosition - xDirection), Limit(newY + yPosition - yDirection)] = "ll";
                                        }
                                    }

                                    newX += xPosition;
                                    newY += yPosition;

                                    if (canGen)
                                    {
                                        for (int extendChance2 = 0; extendChance2 < 6; extendChance2++)
                                        {
                                            int xPosition2 = rand.Next(-1, 2);
                                            int yPosition2 = rand.Next(-1, 2);

                                            world[Limit(newX + xPosition2), Limit(newY + yPosition2)] = "rr";

                                        }
                                    }
                                }

                                break;
                        }

                        break;
                    case "volcano":
                        world[x, y] = "LL";
                        int size = rand.Next(2, 7);

                        if (rand.Next(0, 100) == 0)
                        {
                            size = 15;
                        }

                        for (int extendChance = 0; extendChance < 60; extendChance++)
                        {
                            int xPosition = rand.Next(-size - 2, size + 2);
                            int yPosition = rand.Next(-size - 2, size + 2);

                            if (world[Limit(x + xPosition), Limit(y + yPosition)][0].ToString() == "g")
                            {
                                world[Limit(x + xPosition), Limit(y + yPosition)] = "GG";
                            }
                            else if (world[Limit(x + xPosition), Limit(y + yPosition)][0].ToString() == "s")
                            {
                                world[Limit(x + xPosition), Limit(y + yPosition)] = "SS";
                            }
                            else if (world[Limit(x + xPosition), Limit(y + yPosition)][0].ToString() == "f")
                            {
                                world[Limit(x + xPosition), Limit(y + yPosition)] = "GG";
                            }
                        }

                        CreateCircle(x, y, size + 2, false, "null", "rr", world, worldSize);
                        CreateCircle(x, y, size + 1, false, "rV", "null", world, worldSize);
                        CreateCircle(x, y, size, false, "RV", "rV", world, worldSize);
                        CreateCircle(x, y, size - 1, false, "RV", "Rv", world, worldSize);
                        CreateCircle(x, y, size - 2, false, "Rv", "Rv", world, worldSize);
                        CreateCircle(x, y, size - 3, false, "Rv", "Rv", world, worldSize);
                        CreateCircle(x, y, size - 4, false, "Rv", "LL", world, worldSize);

                        for (int lavaCircle = 0; lavaCircle < size - 3; lavaCircle++)
                        {
                            CreateCircle(x, y, size - 3 - lavaCircle, false, "LL", "LH", world, worldSize);
                        }

                        for (int extendChance = 0; extendChance < 6; extendChance++)
                        {
                            int xPosition = rand.Next(-1, 2);
                            int yPosition = rand.Next(-1, 2);

                            world[Limit(x + xPosition), Limit(y + yPosition)] = "LH";
                        }

                        if (rand.Next(0, 2) == 0)
                        {
                            GenerateFeature(x, y, worldSize, world, depthMap, "river", 'b');
                        }

                        break;
                    case "patch":
                        switch (type2)
                        {
                            case 'c':
                                for (int extendChance = 0; extendChance < 3; extendChance++)
                                {
                                    int xPosition = rand.Next(-1, 2);
                                    int yPosition = rand.Next(-1, 2);

                                    CreateCircle(x + xPosition, y + yPosition, rand.Next(0, 3), true, "ff", "FF", world, worldSize);
                                }
                                break;
                            case 'm':
                                CreateCircle(x, y, rand.Next(0, 2), true, "ff", "ff", world, worldSize);
                                break;
                        }
                        break;
                    case "ice":

                        bool canGenerate = false;
                        for (int landTries = 0; landTries < 100; landTries++)
                        {
                            int xPosition = rand.Next(-3, 4);
                            int yPosition = rand.Next(-3, 4);

                            if (world[Limit(x + xPosition), Limit(y + yPosition)][0].ToString() != "W" && world[Limit(x + xPosition), Limit(y + yPosition)][1].ToString() == "C")
                            {
                                canGenerate = true;
                                break;
                            }
                            else if (world[Limit(x + xPosition), Limit(y + yPosition)][0].ToString() != "W" && world[Limit(x + xPosition), Limit(y + yPosition)][1].ToString() == "f")
                            {
                                canGenerate = true;
                                break;
                            }
                            else if (world[Limit(x + xPosition), Limit(y + yPosition)][0].ToString() != "W" && world[Limit(x + xPosition), Limit(y + yPosition)][1].ToString() == "F")
                            {
                                canGenerate = true;
                                break;
                            }
                        }

                        if (canGenerate)
                        {
                            world[x, y] = "II";

                            for (int extendChance = 0; extendChance < 5; extendChance++)
                            {
                                int xPosition = rand.Next(-1, 2);
                                int yPosition = rand.Next(-1, 2);

                                world[Limit(x + xPosition), Limit(y + yPosition)] = "II";
                                for (int extendChance2 = 0; extendChance2 < 2; extendChance2++)
                                {
                                    int xPosition2 = rand.Next(-1, 2);
                                    int yPosition2 = rand.Next(-1, 2);

                                    world[Limit(x + xPosition + xPosition2), Limit(y + yPosition + yPosition2)] = "II";
                                }
                            }
                        }
                        break;
                    case "rock":
                        world[x, y] = "ww";

                        CreateCircle(x, y, rand.Next(1, 3), true, "rr", "RR", world, worldSize);
                        CreateCircle(x, y, rand.Next(0, 2), true, "RR", "rr", world, worldSize);
                        break;
                }

                return world;
            }
        }

        //And here are the more "Technical" methods. Idk what to call them but they're here anyway

        private string ReturnWorldTile(int x, int y, string[,] world, int type)
        {
            if (type != 0)
            {
                return world[Math.Clamp(x, 0, GV.newWorldSize - 1), Math.Clamp(y, 0, GV.newWorldSize - 1)][type - 1].ToString();
            }
            else
            {
                return world[Math.Clamp(x, 0, GV.newWorldSize - 1), Math.Clamp(y, 0, GV.newWorldSize - 1)].ToString();
            }
        }

        private void AdvanceTime()
        {
            if (GV.worldTime > 199) //Advencing time
            {
                GV.worldDay += 1;
                GV.worldTime = 1;
            }
            else
            {
                GV.worldTime += 1;
            }
        }

        private int Limit(int num)
        {
            return Math.Clamp(num, 0, GV.newWorldSize - 1);
        }

        private float LimitF(float num)
        {
            return Math.Clamp(num, 0, GV.newWorldSize - 1);
        }
    }
}