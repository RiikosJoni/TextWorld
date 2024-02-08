using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Linq;

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

            while (true)
            {
                RunMainMenu();

                int screenWidth = GlobalVariables.screenWidth;
                int screenHeight = GlobalVariables.screenHeight;

                int worldSize = GlobalVariables.newWorldSize;

                switch (GlobalVariables.newWorldGamemode)
                {
                    case "World Viewer":
                        int camX = rand.Next(0, (int)Math.Clamp((worldSize - screenWidth * 0.2f), 0, worldSize));
                        int camY = rand.Next(0, (int)Math.Clamp((worldSize - screenWidth * 0.2f), 0, worldSize));

                        while (true)
                        {
                            ScreenTop('a');

                            RenderWorld(GlobalVariables.currentWorld, worldSize, camX, camX + (int)(screenWidth * 0.5f), camY, camY + (int)(screenWidth * 0.5f), 'a'); //Renders the world map

                            ScreenLeft(GlobalVariables.screenWidth / 5);
                            ScreenLeft(GlobalVariables.screenWidth / 2 - 10);
                            ResetTextColor(false);
                            Console.Write("World Viewer Mode");

                            ScreenLeft(GlobalVariables.screenWidth / 4);
                            ResetTextColor(false);
                            Console.Write("Arrow Keys - Move");
                            for (int i = 0; i < GlobalVariables.screenWidth / 4; i++)
                            {
                                Console.Write(" ");
                            }
                            Console.Write("Enter - Teleport to a random place");

                            ScreenLeft(GlobalVariables.screenWidth / 2 - 20);
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
                                camX = rand.Next(0, (int)Math.Clamp((worldSize), 0, worldSize));
                                camY = rand.Next(0, (int)Math.Clamp((worldSize), 0, worldSize));
                            }
                            else if (keyPressed == ConsoleKey.Escape)
                            {
                                break;
                            }
                        }
                        break;
                    case "Survival":

                        if (GlobalVariables.playerX == 789)
                        {
                            GlobalVariables.playerX = worldSize / 2; //Tries to find spawn point, if cant, sets it to this instead.
                            GlobalVariables.playerY = worldSize / 2;

                            bool spawnFound = false;

                            for (int x = worldSize / 2; x < worldSize / 2; x++)
                            {

                                for (int y = worldSize / 2; y < worldSize / 2; y++)
                                {
                                    if (GlobalVariables.currentWorldDepth[x, y][0].ToString() == (GlobalVariables.worldSeaLevel - 1).ToString())
                                    {
                                        if (!GlobalVariables.tilesBlockPlayer.Any(GlobalVariables.currentWorld[x, y][0].ToString().Contains))
                                        {
                                            GlobalVariables.playerX = x;
                                            GlobalVariables.playerY = y;

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
                                        if (!GlobalVariables.tilesBlockPlayer.Any(GlobalVariables.currentWorld[x, y][0].ToString().Contains))
                                        {
                                            GlobalVariables.playerX = x;
                                            GlobalVariables.playerY = y;

                                            spawnFound = true;

                                            break;
                                        }
                                    }

                                    if (spawnFound) { break; }
                                }
                            }
                        }

                        while (true)
                        {
                            RunSurvivalTick();
                        }
                }
            }

            static void RunSurvivalTick()
            {
                if (GlobalVariables.worldTime > 199) //Advencing time
                {
                    GlobalVariables.worldDay += 1;
                    GlobalVariables.worldTime = 1;
                }
                else
                {
                    GlobalVariables.worldTime += 1;
                }

                ScreenTop('y');

                ScreenLeft(GlobalVariables.screenWidth / 2 - 10); //Rendering hud
                ScreenLeft(GlobalVariables.screenWidth / 2 - 10);
                ResetTextColor(false);
                Console.Write(GlobalVariables.newWorldName + ", ");
                if (GlobalVariables.worldTime < 140)
                {
                    SetTextColor(190);
                    Console.Write($"Day {GlobalVariables.worldDay}");
                }
                else
                {
                    SetTextColor(12);
                    Console.Write($"Night {GlobalVariables.worldDay}");
                }

                ScreenLeft(GlobalVariables.screenWidth / 3 - 10);
                ScreenLeft(GlobalVariables.screenWidth / 3 - 10);
                ResetTextColor(false);
                Console.Write("Money: ");
                SetTextColor(3);
                Console.Write(GlobalVariables.playerMoney);

                ResetTextColor(false);
                for (int i = 0; i < GlobalVariables.screenWidth / 3; i++)
                {
                    Console.Write(" ");
                }
                Console.Write("Energy: ");
                SetTextColor(50);
                Console.Write(GlobalVariables.playerEnergy);

                RenderWorld(GlobalVariables.currentWorld, GlobalVariables.newWorldSize, GlobalVariables.playerX - 9, GlobalVariables.playerX + 10, GlobalVariables.playerY - 9, GlobalVariables.playerY + 10, 's'); //Render world

                ConsoleKey keyPressed; //Register key presses
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    if (!GlobalVariables.tilesBlockPlayer.Any(GlobalVariables.currentWorld[Math.Clamp(GlobalVariables.playerX - 1, 0, GlobalVariables.newWorldSize), GlobalVariables.playerY][0].ToString().Contains))
                    {
                        GlobalVariables.playerX -= 1;
                        GlobalVariables.playerEnergy -= 0.5f;
                    }
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    if (!GlobalVariables.tilesBlockPlayer.Any(GlobalVariables.currentWorld[Math.Clamp(GlobalVariables.playerX + 1, 0, GlobalVariables.newWorldSize), GlobalVariables.playerY][0].ToString().Contains))
                    {
                        GlobalVariables.playerX += 1;
                        GlobalVariables.playerEnergy -= 0.5f;
                    }
                }
                else if (keyPressed == ConsoleKey.LeftArrow)
                {
                    if (!GlobalVariables.tilesBlockPlayer.Any(GlobalVariables.currentWorld[GlobalVariables.playerX, Math.Clamp(GlobalVariables.playerY - 1, 0, GlobalVariables.newWorldSize)][0].ToString().Contains))
                    { 
                        GlobalVariables.playerY -= 1;
                        GlobalVariables.playerEnergy -= 0.5f;
                    }
                }
                else if (keyPressed == ConsoleKey.RightArrow)
                {
                    if (!GlobalVariables.tilesBlockPlayer.Any(GlobalVariables.currentWorld[GlobalVariables.playerX, Math.Clamp(GlobalVariables.playerY + 1, 0, GlobalVariables.newWorldSize)][0].ToString().Contains))
                    {
                        GlobalVariables.playerY += 1;
                        GlobalVariables.playerEnergy -= 0.5f;
                    }
                }
                else if (keyPressed == ConsoleKey.Escape)
                {
                }
            }

            static void RenderWorld(string[,] world, int worldSize, int xStart, int xEnd, int yStart, int yEnd, char renderer)
            {
                string text = "[]";

                ScreenLeft((int)(GlobalVariables.screenWidth / 2 - (xEnd - xStart + 2)));

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
                                ScreenLeft((int)(GlobalVariables.screenWidth / 2 - (xEnd - xStart + 2)));
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
                                    switch (GlobalVariables.currentWorldStructures[limitedX, limitedY][0].ToString())
                                    {
                                        case "T": //Town
                                            SetTextColor(27);
                                            break;
                                        case "F": //Forest
                                            SetTextColor(12);
                                            break;
                                    }
                                    break;
                                default:
                                    SetTextBackgroundColor(0);
                                    break;
                            }

                            if (!GlobalVariables.renderDebug && GlobalVariables.currentWorldStructures[limitedX, limitedY] == "[]")
                            {
                                SetTextColor(GlobalVariables.currentTextBackgroundColor);
                                text = world[limitedX, limitedY];
                            }
                            else if (GlobalVariables.currentWorldStructures[limitedX, limitedY] == "[]")
                            {
                                SetTextColor(1);
                                text = world[limitedX, limitedY];
                            }
                            else if (GlobalVariables.currentWorldStructures[limitedX, limitedY][0].ToString() == "T") //Checks for towns
                            {
                                SetTextColor(8);
                                text = "/\\";
                            }
                            else if (GlobalVariables.currentWorldStructures[limitedX, limitedY][0].ToString() == "F") //Checks for towns
                            {
                                SetTextColor(58);
                                switch (GlobalVariables.currentWorldStructures[limitedX, limitedY][1].ToString())
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
                            else
                            {
                                SetTextColor(1);
                                text = "??";
                            }
                            tileNum++;

                            if (limitedX == GlobalVariables.playerX && limitedY == GlobalVariables.playerY)
                            {
                                SetTextColor(0);
                                text = "H ";
                            }

                            Console.Write(text);

                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }

        }

        static void SetTextColor(int colorIndex)
        {
            Console.Write("\x1b[38;5;" + colorIndex + "m");
            GlobalVariables.currentTextColor = colorIndex;
        }

        static void SetTextBackgroundColor(int colorIndex)
        {
            Console.Write("\x1b[48;5;" + colorIndex + "m");
            GlobalVariables.currentTextBackgroundColor = colorIndex;
        }

        public static void ResetTextColor(bool inverted)
        {
            if (!inverted)
            {
                Console.Write("\x1b[38;5;" + GlobalVariables.defaultTextColor + "m");
                Console.Write("\x1b[48;5;" + GlobalVariables.defaultTextBackgroundColor + "m");
            }
            else
            {
                Console.Write("\x1b[38;5;" + GlobalVariables.defaultTextBackgroundColor + "m");
                Console.Write("\x1b[48;5;" + GlobalVariables.defaultTextColor + "m");
            }
        }

        public static void ScreenTop(char topType)
        {
            ResetTextColor(false);

            Console.Clear();
            Console.Write("  /");
            for (int i = 0; i < GlobalVariables.screenWidth - 6; i++)
            {
                Console.Write("-");
            }
            Console.Write("\\  ");

            switch (topType)
            {
                case 'm':
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write(" _____             _    __    __              _      _ ");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write("/__   \\ ___ __  __| |_ / / /\\ \\ \\ ___   _ __ | |  __| |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write("  / /\\// _ \\\\ \\/ /| __|\\ \\/  \\/ // _ \\ | '__|| | / _` |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write(" / /  |  __/ >  < | |_  \\  /\\  /| (_) || |   | || (_| |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write(" \\/    \\___|/_/\\_\\ \\__|  \\/  \\/  \\___/ |_|   |_| \\__,_|\n");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(226);
                    Console.Write($"Version {GlobalVariables.version} Loaded!");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write("(Move in the menu with arrow keys.Select an option with Enter.)\n\n\n");
                    ResetTextColor(false);

                    break;
                case 'o':
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write(" _____             _    __    __              _      _ ");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(10);
                    Console.Write("/__   \\ ___ __  __| |_ / / /\\ \\ \\ ___   _ __ | |  __| |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write("  / /\\// _ \\\\ \\/ /| __|\\ \\/  \\/ // _ \\ | '__|| | / _` |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write(" / /  |  __/ >  < | |_  \\  /\\  /| (_) || |   | || (_| |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(58);
                    Console.Write(" \\/    \\___|/_/\\_\\ \\__|  \\/  \\/  \\___/ |_|   |_| \\__,_|");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 2);
                    ResetTextColor(false);

                    break;
                case 'c':
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("   ___               _         __    __           _     _ ");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("  / __\\ __ ___  __ _| |_ ___  / / /\\ \\ \\___  _ __| | __| |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write(" / / | '__/ _ \\/ _` | __/ _ \\ \\ \\/  \\/ / _ \\| '__| |/ _` |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("/ /__| | |  __/ (_| | ||  __/  \\  /\\  / (_) | |  | | (_| |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("\\____/_|  \\___|\\__,_|\\__\\___|   \\/  \\/ \\___/|_|  |_|\\__,_|");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write($"{GlobalVariables.newWorldName} Options:\n\n");
                    ResetTextColor(false);

                    break;
                case 'd':
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("   ___               _         __    __           _     _ ");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("  / __\\ __ ___  __ _| |_ ___  / / /\\ \\ \\___  _ __| | __| |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write(" / / | '__/ _ \\/ _` | __/ _ \\ \\ \\/  \\/ / _ \\| '__| |/ _` |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("/ /__| | |  __/ (_| | ||  __/  \\  /\\  / (_) | |  | | (_| |");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 4);
                    SetTextColor(245);
                    Console.Write("\\____/_|  \\___|\\__,_|\\__\\___|   \\/  \\/ \\___/|_|  |_|\\__,_|");
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
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(10);
            Console.WriteLine(@$"Thank you for playing TextWorld Version {GlobalVariables.version}!");
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(245);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        private void DisplayCredits()
        {
            ScreenTop('o');
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(10);
            Console.Write($"TextWorld Version {GlobalVariables.version} Credits:\n");
            ResetTextColor(false);

            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            Console.Write("-Most code made by Joni R.\n");
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
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
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);

            SetTextColor(245);
            Console.Write("Press any key to go back to the main menu...");
            ResetTextColor(false);
            Console.ReadKey(true);
            RunMainMenu();
        }

        private void DisplayWorldSelection()
        {
            string promptType = "worldSelect";
            string[] options = { "Create New World","Load World [NOT YET POSSIBLE]", "Back" };
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
            string[] options = { $"World Name: {GlobalVariables.newWorldName}", $"Gamemode: {GlobalVariables.newWorldGamemode}", $"Difficulty: {GlobalVariables.newWorldDifficulty}", $"World Type: {GlobalVariables.newWorldType}", $"World Size: {GlobalVariables.newWorldSize}", "CREATE", "Back" };
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
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(245);
            Console.Write("Type in the world name. Press Enter to confirm.\n");
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);

            GlobalVariables.newWorldName = Console.ReadLine();
            DisplayWorldCreation();
        }

        private void WorldTypeOptions()
        {
            string promptType = "worldType";
            string[] options = { "Default", "Warm", "Desert", "Cool", "Antarctica", "Flooded", "Dry", "Flat", "Volcanic", "Inverted", "Custom" };
            Menu worldTypeMenu = new(promptType, options);
            int selectedIndex = worldTypeMenu.Run();

            GlobalVariables.newWorldType = options[selectedIndex];

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
            string[] options = { $"Generate Random Biomes: {GlobalVariables.worldGenerateBiomes}", $"Generate Special Biomes: {GlobalVariables.worldGenerateSpecialBiomes}", $"Main Biome: {GlobalVariables.worldMainBiome}", $"Generate Features: {GlobalVariables.worldGenerateFeatures}", $"Generate Structures: {GlobalVariables.worldGenerateStructures}", $"Temperature: {GlobalVariables.worldTemperature}", $"Sea Level: {GlobalVariables.worldSeaLevel}", $"Island Amount: {GlobalVariables.worldIslandAmount}", "Back" };
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
            if (GlobalVariables.worldGenerateBiomes)
            {
                GlobalVariables.worldGenerateBiomes = false;
            }
            else
            {
                GlobalVariables.worldGenerateBiomes = true;
            }

                DisplayCustomWorldCreation();
        }

        private void CustomGenSpecialBiomes()
        {
            if (GlobalVariables.worldGenerateSpecialBiomes)
            {
                GlobalVariables.worldGenerateSpecialBiomes = false;
            }
            else
            {
                GlobalVariables.worldGenerateSpecialBiomes = true;
            }

            DisplayCustomWorldCreation();
        }

        private void CustomMainBiome()
        {
            string promptType = "worldType";
            string[] options = {"Grassy Plains", "Jungle", "Savanna", "Desert", "Mountains", "Tundra", "Snow", "Volcanic", "Candy", "Inverted", "None"};
            Menu mainBiomeMenu = new(promptType, options);
            int selectedIndex = mainBiomeMenu.Run();

            GlobalVariables.worldMainBiome = options[selectedIndex];

            DisplayCustomWorldCreation();
        }

        private void CustomGenFeatures()
        {
            if (GlobalVariables.worldGenerateFeatures)
            {
                GlobalVariables.worldGenerateFeatures = false;
            }
            else
            {
                GlobalVariables.worldGenerateFeatures = true;
            }

            DisplayCustomWorldCreation();
        }

        private void CustomGenStructures()
        {
            if (GlobalVariables.worldGenerateStructures)
            {
                GlobalVariables.worldGenerateStructures = false;
            }
            else
            {
                GlobalVariables.worldGenerateStructures = true;
            }

            DisplayCustomWorldCreation();
        }

        private void CustomTemperature()
        {
            bool isInt = false;

            Console.Clear();
            ScreenTop('d');
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            Game.ScreenLeft(GlobalVariables.screenWidth / 8);
            SetTextColor(245);
            Console.Write("(50 is the default, lower makes cool biomes more common and larger makes hot biomes more common)");
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(245);
            Console.Write("Type in the temperature. Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                string unparsedSize = Console.ReadLine();
                if (int.TryParse(unparsedSize, out GlobalVariables.worldTemperature) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('d');
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 8);
                    SetTextColor(245);
                    Console.Write("(50 is the default, lower makes cool biomes more common and larger makes hot biomes more common)");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
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
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            Game.ScreenLeft(GlobalVariables.screenWidth / 8);
            SetTextColor(245);
            Console.Write("(8 is the default, lower makes seas higher and larger makes seas lower. >9 for no seas.)");
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(245);
            Console.Write("Type in the sea level. Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                string unparsedSize = Console.ReadLine();
                if (int.TryParse(unparsedSize, out GlobalVariables.worldSeaLevel) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('d');
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 8);
                    SetTextColor(245);
                    Console.Write("(8 is the default, lower makes seas higher and larger makes seas lower. >9 for no seas.)");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
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
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(245);
            Console.Write("Type in how many islands you want. Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                string unparsedSize = Console.ReadLine();
                if (int.TryParse(unparsedSize, out GlobalVariables.worldIslandAmount) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('d');
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
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
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            Game.ScreenLeft(GlobalVariables.screenWidth / 8);
            SetTextColor(245);
            Console.Write("Type in the world size. (Use anything else than 1-5000 with caution!) Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                string unparsedSize = Console.ReadLine();
                if (int.TryParse(unparsedSize, out GlobalVariables.newWorldSize) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('d');
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 8);
                    SetTextColor(245);
                    Console.Write("Type in the world size. (Use anything else than 1-5000 with caution!) Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }

            DisplayWorldCreation();
        }

        private void WorldGamemodeOptions()
        {
            switch (GlobalVariables.newWorldGamemode)
            {
                case "World Viewer":
                    GlobalVariables.newWorldGamemode = "Survival";
                    break;
                case "Survival":
                    GlobalVariables.newWorldGamemode = "Debug Mode";
                    break;
                case "Debug Mode":
                    GlobalVariables.newWorldGamemode = "World Viewer";
                    break;
            }
            DisplayWorldCreation();
        }

        private void WorldDifficultyOptions()
        {
            switch (GlobalVariables.newWorldDifficulty)
            {
                case "Easy":
                    GlobalVariables.newWorldDifficulty = "Medium";
                    break;
                case "Medium":
                    GlobalVariables.newWorldDifficulty = "Hard";
                    break;
                case "Hard":
                    GlobalVariables.newWorldDifficulty = "One Life";
                    break;
                case "One Life":
                    GlobalVariables.newWorldDifficulty = "Easy";
                    break;
            }
            DisplayWorldCreation();
        }

        void CreateWorld()
        {
            switch (GlobalVariables.newWorldType) //Sets the settings
            {
                case "Default":
                    GlobalVariables.worldGenerateBiomes = true;
                    GlobalVariables.worldGenerateSpecialBiomes = true;
                    GlobalVariables.worldMainBiome = "Grassy Plains";

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldTemperature = 50;
                    GlobalVariables.worldSeaLevel = 8;

                    break;
                case "Warm":
                    GlobalVariables.worldGenerateBiomes = true;
                    GlobalVariables.worldGenerateSpecialBiomes = true;

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldTemperature = 80;
                    GlobalVariables.worldSeaLevel = 8;

                    break;
                case "Desert":
                    GlobalVariables.worldGenerateBiomes = false;
                    GlobalVariables.worldGenerateSpecialBiomes = false;
                    GlobalVariables.worldMainBiome = "Desert";

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldSeaLevel = 10;

                    break;
                case "Cold":
                    GlobalVariables.worldGenerateBiomes = true;
                    GlobalVariables.worldGenerateSpecialBiomes = true;

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldTemperature = 20;
                    GlobalVariables.worldSeaLevel = 8;

                    break;
                case "Antarctica":
                    GlobalVariables.worldGenerateBiomes = false;
                    GlobalVariables.worldGenerateSpecialBiomes = false;
                    GlobalVariables.worldMainBiome = "Snow";

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldSeaLevel = 8;

                    break;
                case "Flooded":
                    GlobalVariables.worldGenerateBiomes = true;
                    GlobalVariables.worldGenerateSpecialBiomes = true;

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldTemperature = 50;
                    GlobalVariables.worldSeaLevel = 4;

                    break;
                case "Dry":
                    GlobalVariables.worldGenerateBiomes = true;
                    GlobalVariables.worldGenerateSpecialBiomes = true;

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldTemperature = 50;
                    GlobalVariables.worldSeaLevel = 10;

                    break;
                case "Flat":
                    GlobalVariables.worldGenerateBiomes = true;
                    GlobalVariables.worldGenerateSpecialBiomes = true;

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldTemperature = 50;
                    GlobalVariables.worldSeaLevel = 100;

                    break;
                case "Volcanic":
                    GlobalVariables.worldGenerateBiomes = false;
                    GlobalVariables.worldGenerateSpecialBiomes = false;
                    GlobalVariables.worldMainBiome = "Volcanic";

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldSeaLevel = 10;

                    break;
                case "Inverted":
                    GlobalVariables.worldGenerateBiomes = false;
                    GlobalVariables.worldGenerateSpecialBiomes = false;
                    GlobalVariables.worldMainBiome = "Inverted";

                    GlobalVariables.worldGenerateFeatures = true;
                    GlobalVariables.worldGenerateStructures = true;

                    GlobalVariables.worldSeaLevel = 8;

                    break;
            }

            if (GlobalVariables.newWorldType != "Custom") //Sets the default island amount if it hasn't been set
            {
                GlobalVariables.worldIslandAmount = GlobalVariables.newWorldSize / 20 * GlobalVariables.newWorldSize / 20;
            }

            char mainBiome = '0';

            switch (GlobalVariables.worldMainBiome) //Sets the main biome
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

            GlobalVariables.currentWorldDepth = GenerateWorldDepth(GlobalVariables.newWorldSize, GlobalVariables.worldIslandAmount, GlobalVariables.worldTemperature, GlobalVariables.worldGenerateBiomes, GlobalVariables.worldGenerateSpecialBiomes, mainBiome); //Generates the depthmap
            GlobalVariables.currentWorld = GenerateWorld(GlobalVariables.newWorldSize, GlobalVariables.currentWorldDepth, GlobalVariables.worldSeaLevel, GlobalVariables.worldGenerateFeatures); //Generates the world map
            GlobalVariables.currentWorldStructures = GenerateWorldStructures(GlobalVariables.currentWorld, GlobalVariables.currentWorldDepth, GlobalVariables.newWorldSize, GlobalVariables.worldSeaLevel, GlobalVariables.worldGenerateFeatures);//Generates structures
            //Generates entities

            var rand = new Random();

            string[,] GenerateWorldDepth(int worldSize, int islandAmount, float temperature, bool generateBiomes, bool generateSpecialBiomes, char mainBiome)
            {

                string[,] world = new String[worldSize, worldSize]; //Creates the map

                ScreenTop('d');
                ScreenLeft(GlobalVariables.screenWidth / 3);
                ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                ScreenLeft(GlobalVariables.screenWidth / 3);
                ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                ScreenLeft(GlobalVariables.screenWidth / 3);
                ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                ScreenLeft(GlobalVariables.screenWidth / 3);
                ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                    ScreenLeft(GlobalVariables.screenWidth / 3);
                    ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                    ScreenLeft(GlobalVariables.screenWidth / 3);
                    ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                ScreenLeft(GlobalVariables.screenWidth / 3);
                ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                ScreenLeft(GlobalVariables.screenWidth / 3);
                ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
                Console.WriteLine("Creating structure map");

                int worldLimit = worldSize - 1;

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++) //Fills the map with empty spaace
                    {
                        worldStructures[x, y] = "[]";
                    }
                }

                if (GlobalVariables.worldGenerateStructures)
                {
                    ScreenTop('d');
                    ScreenLeft(GlobalVariables.screenWidth / 3);
                    ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                                        worldStructures[Math.Clamp(x + rand.Next(-2, 3), 0, worldLimit), Math.Clamp(y + rand.Next(-2, 3), 0, worldLimit)] = $"T{depthMap[Math.Clamp(x + rand.Next(-2, 3), 0, worldLimit), Math.Clamp(y + rand.Next(-2, 3), 0, worldLimit)][1].ToString()}";
                                    }
                                }
                            }
                        }
                    }

                    ScreenTop('d');
                    ScreenLeft(GlobalVariables.screenWidth / 3);
                    ScreenLeft((int)(GlobalVariables.screenWidth / 2.5f));
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
                                            if (rand.Next(1, 10) == 1)
                                            {
                                                worldStructures[x, y] = "F0";

                                                for (int e = 0; e < rand.Next(3, 12); e++)
                                                {
                                                    worldStructures[Math.Clamp(x + rand.Next(-2, 3), 0, worldLimit), Math.Clamp(y + rand.Next(-2, 3), 0, worldLimit)] = "F0";
                                                }
                                            }
                                            break;
                                        case "S":
                                            worldStructures[x, y] = "FS";

                                            for (int e = 0; e < rand.Next(3, 12); e++)
                                            {
                                                worldStructures[Math.Clamp(x + rand.Next(-3, 4), 0, worldLimit), Math.Clamp(y + rand.Next(-3, 4), 0, worldLimit)] = "FS";
                                            }
                                            break;
                                        case "T":
                                            worldStructures[x, y] = "FT";

                                            for (int e = 0; e < rand.Next(5, 17); e++)
                                            {
                                                worldStructures[Math.Clamp(x + rand.Next(-3, 4), 0, worldLimit), Math.Clamp(y + rand.Next(-3, 4), 0, worldLimit)] = "FT";
                                            }
                                            break;
                                        case "J":
                                            worldStructures[x, y] = "FJ";

                                            for (int e = 0; e < rand.Next(18, 40); e++)
                                            {
                                                worldStructures[Math.Clamp(x + rand.Next(-4, 5), 0, worldLimit), Math.Clamp(y + rand.Next(-4, 5), 0, worldLimit)] = "FJ";
                                            }
                                            break;
                                        case "3":
                                            worldStructures[x, y] = "F3";

                                            for (int e = 0; e < rand.Next(3, 12); e++)
                                            {
                                                worldStructures[Math.Clamp(x + rand.Next(-3, 4), 0, worldLimit), Math.Clamp(y + rand.Next(-3, 4), 0, worldLimit)] = "F3";
                                            }
                                            break;
                                        case "9":
                                            worldStructures[x, y] = "F9";

                                            for (int e = 0; e < rand.Next(3, 12); e++)
                                            {
                                                worldStructures[Math.Clamp(x + rand.Next(-2, 3), 0, worldLimit), Math.Clamp(y + rand.Next(-2, 3), 0, worldLimit)] = "F9";
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

            string[,] ExtendLand(int x, int y, int worldLimit, string[,] world, string chance1, string chance2)
            {
                var rand = new Random();

                for (int extendChance = 0; extendChance < 8; extendChance++)
                {
                    int xPosition = rand.Next(-1, 2);
                    int yPosition = rand.Next(-1, 2);

                    if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() == "9")
                    {
                        if (rand.Next(0, 3) > 0)
                        {
                            world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = chance1;
                        }
                        else
                        {
                            world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = chance2;
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
                                world[Math.Clamp((x + tiles), 0, worldLimit), Math.Clamp((y + radius - tiles), 0, worldLimit)] = tile;
                            }
                        }
                        else
                        {
                            if (altTile != "null")
                            {
                                world[Math.Clamp((x + tiles), 0, worldLimit), Math.Clamp((y + radius - tiles), 0, worldLimit)] = altTile;
                            }
                        }
                    }
                    else
                    {
                        if (rand.Next(0, 3) != 0)
                        {
                            if (tile != "null")
                            {
                                world[Math.Clamp((x + radius - (tiles - radius)), 0, worldLimit), Math.Clamp((y + radius - tiles), 0, worldLimit)] = tile;
                            }
                        }
                        else
                        {
                            if (altTile != "null")
                            {
                                world[Math.Clamp((x + radius - (tiles - radius)), 0, worldLimit), Math.Clamp((y + radius - tiles), 0, worldLimit)] = altTile;
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
                                world[Math.Clamp((x - tiles), 0, worldLimit), Math.Clamp((y + radius - tiles), 0, worldLimit)] = tile;
                            }
                        }
                        else
                        {
                            if (altTile != "null")
                            {
                                world[Math.Clamp((x - tiles), 0, worldLimit), Math.Clamp((y + radius - tiles), 0, worldLimit)] = altTile;
                            }
                        }
                    }
                    else
                    {
                        if (rand.Next(0, 3) != 0)
                        {
                            if (tile != "null")
                            {
                                world[Math.Clamp((x - radius + (tiles - radius)), 0, worldLimit), Math.Clamp((y + radius - tiles), 0, worldLimit)] = tile;
                            }
                        }
                        else
                        {
                            if (altTile != "null")
                            {
                                world[Math.Clamp((x - radius + (tiles - radius)), 0, worldLimit), Math.Clamp((y + radius - tiles), 0, worldLimit)] = altTile;
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

                            world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "ww";
                            for (int extendChance2 = 0; extendChance2 < 2; extendChance2++)
                            {
                                int xPosition2 = rand.Next(-1, 2);
                                int yPosition2 = rand.Next(-1, 2);

                                world[Math.Clamp((x + xPosition + xPosition2), 0, worldLimit), Math.Clamp((y + yPosition + yPosition2), 0, worldLimit)] = "GD";
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

                                    while (world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)][0].ToString() == "W" | world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)][0].ToString() == "w")
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
                                        if (world[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)][0].ToString() == "f" | world[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)][0].ToString() == "F")
                                        {
                                            world[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)] = "II";
                                            world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)] = "II";
                                        }
                                        else if (depthMap[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)][1].ToString() != "H")
                                        {
                                            world[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)] = "WR";
                                            world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)] = "WR";
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

                                            if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "G")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "gg";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "S")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "ss";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "F")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "ff";
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
                            case 'b':
                                for (int extendChance = 0; extendChance < rand.Next(5, 10); extendChance++)
                                {
                                    int xPosition = rand.Next(-1, 2) + xDirection;
                                    int yPosition = rand.Next(-1, 2) + yDirection;

                                    int genTries = 0;
                                    bool canGen = true;

                                    while (world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)][0].ToString() == "L" | world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)][0].ToString() == "l" | world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)][0].ToString() == "r" | world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)][0].ToString() == "R")
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
                                        if (world[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)][0].ToString() == "f" | world[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)][0].ToString() == "F")
                                        {
                                            world[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)] = "rr";
                                            world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)] = "rr";
                                        }
                                        else
                                        {
                                            world[Math.Clamp((newX + xPosition - xDirection), 0, worldLimit), Math.Clamp((newY + yPosition - yDirection), 0, worldLimit)] = "ll";
                                            world[Math.Clamp((newX + xPosition), 0, worldLimit), Math.Clamp((newY + yPosition), 0, worldLimit)] = "ll";
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

                                            if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "G")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "rV";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "S")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "rV";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "F")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "rV";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "g")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "rV";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "s")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "rV";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "f")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "rV";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "W")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "RV";
                                            }
                                            else if (world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)][0].ToString() == "w")
                                            {
                                                world[Math.Clamp((newX + xPosition2), 0, worldLimit), Math.Clamp((newY + yPosition2), 0, worldLimit)] = "rV";
                                            }
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

                            if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() == "g")
                            {
                                world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "GG";
                            }
                            else if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() == "s")
                            {
                                world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "SS";
                            }
                            else if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() == "f")
                            {
                                world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "GG";
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

                            world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "LH";
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

                            if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() != "W" && world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][1].ToString() == "C")
                            {
                                canGenerate = true;
                                break;
                            }
                            else if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() != "W" && world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][1].ToString() == "f")
                            {
                                canGenerate = true;
                                break;
                            }
                            else if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][0].ToString() != "W" && world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)][1].ToString() == "F")
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

                                world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "II";
                                for (int extendChance2 = 0; extendChance2 < 2; extendChance2++)
                                {
                                    int xPosition2 = rand.Next(-1, 2);
                                    int yPosition2 = rand.Next(-1, 2);

                                    world[Math.Clamp((x + xPosition + xPosition2), 0, worldLimit), Math.Clamp((y + yPosition + yPosition2), 0, worldLimit)] = "II";
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
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(10);
            Console.Write("Change Screen size with arrow keys. Press Enter to confirm.\n");
            ResetTextColor(false);

            SetTextColor(226);
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            Console.Write("Screen Size:\n");
            ResetTextColor(false);
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            Console.Write(GlobalVariables.screenWidth);

            ConsoleKey keyPressed;
            do
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    GlobalVariables.screenWidth += 5;
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    GlobalVariables.screenWidth -= 5;
                    if (GlobalVariables.screenWidth < 0)
                    {
                        GlobalVariables.screenWidth = 0;
                    }
                }

                Console.Clear();
                ScreenTop('o');
                Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                SetTextColor(10);
                Console.Write("Change Screen size with arrow keys. Press Enter to confirm.\n");
                ResetTextColor(false);

                SetTextColor(226);
                Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                Console.Write("Screen Size:\n");
                ResetTextColor(false);
                Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                Console.Write(GlobalVariables.screenWidth);

            } while (keyPressed != ConsoleKey.Enter);

            DisplayOptions();
        }

        private void ColorSchemeOptions()
        {
            string promptType = "clrScheme";
            string[] options = { "Default", "Classic", "Reverse", "Cool", "Warm", "Ocean", "Air", "Ground", "Inverse Ground", "Volcanic", "Retro", "Error", "Mint", "Bubblegum", "Pink", "Custom"};
            Menu clrMenu = new(promptType, options);
            int selectedIndex = clrMenu.Run();

            switch (selectedIndex)
            {
                case 0:
                    GlobalVariables.defaultTextColor = 253;
                    GlobalVariables.defaultTextBackgroundColor = 233;
                    break;
                case 1:
                    GlobalVariables.defaultTextColor = 15;
                    GlobalVariables.defaultTextBackgroundColor = 16;
                    break;
                case 2:
                    GlobalVariables.defaultTextColor = 233;
                    GlobalVariables.defaultTextBackgroundColor = 253;
                    break;
                case 3:
                    GlobalVariables.defaultTextColor = 50;
                    GlobalVariables.defaultTextBackgroundColor = 12;
                    break;
                case 4:
                    GlobalVariables.defaultTextColor = 214;
                    GlobalVariables.defaultTextBackgroundColor = 94;
                    break;
                case 5:
                    GlobalVariables.defaultTextColor = 27;
                    GlobalVariables.defaultTextBackgroundColor = 17;
                    break;
                case 6:
                    GlobalVariables.defaultTextColor = 253;
                    GlobalVariables.defaultTextBackgroundColor = 12;
                    break;
                case 7:
                    GlobalVariables.defaultTextColor = 10;
                    GlobalVariables.defaultTextBackgroundColor = 58;
                    break;
                case 8:
                    GlobalVariables.defaultTextColor = 213;
                    GlobalVariables.defaultTextBackgroundColor = 19;
                    break;
                case 9:
                    GlobalVariables.defaultTextColor = 208;
                    GlobalVariables.defaultTextBackgroundColor = 233;
                    break;
                case 10:
                    GlobalVariables.defaultTextColor = 40;
                    GlobalVariables.defaultTextBackgroundColor = 16;
                    break;
                case 11:
                    GlobalVariables.defaultTextColor = 196;
                    GlobalVariables.defaultTextBackgroundColor = 16;
                    break;
                case 12:
                    GlobalVariables.defaultTextColor = 75;
                    GlobalVariables.defaultTextBackgroundColor = 122;
                    break;
                case 13:
                    GlobalVariables.defaultTextColor = 48;
                    GlobalVariables.defaultTextBackgroundColor = 55;
                    break;
                case 14:
                    GlobalVariables.defaultTextColor = 219;
                    GlobalVariables.defaultTextBackgroundColor = 97;
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
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(10);
            Console.Write("Type in the text color index. (0 - 255) Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                string unparsedColor = Console.ReadLine();
                if (int.TryParse(unparsedColor, out GlobalVariables.defaultTextColor) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('o');
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    SetTextColor(10);
                    Console.Write("Type in the text color index. (0 - 255) Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }

            isInt = false;

            Console.Clear();
            ScreenTop('o');
            Game.ScreenLeft(GlobalVariables.screenWidth / 5);
            SetTextColor(10);
            Console.Write("Type in the text background color index. (0 - 255) Press Enter to confirm.\n");
            ResetTextColor(false);

            while (isInt == false)
            {
                string unparsedColor = Console.ReadLine();
                if (int.TryParse(unparsedColor, out GlobalVariables.defaultTextBackgroundColor) == true)
                {
                    isInt = true;
                }
                else
                {
                    Console.Clear();
                    ScreenTop('o');
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    SetTextColor(0);
                    Console.Write("Incorrect input.");
                    Game.ScreenLeft(GlobalVariables.screenWidth / 5);
                    SetTextColor(10);
                    Console.Write("Type in the text color index. (0 - 255) Press Enter to confirm.\n");
                    ResetTextColor(false);
                }
            }
        }

    }
}