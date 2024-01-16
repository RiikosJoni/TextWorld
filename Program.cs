using System.Runtime.CompilerServices;
using System.Threading.Channels;

int worldSize = 50;
int islandAmount = 50;

string worldType = "default";

Console.WriteLine("TextWorld V0.1 Loaded!");


//Idea: XY
//X - Type of ground
//Y - Biome

string[,] world = GenerateWorld(worldSize, islandAmount, worldType);

int tileNum = 0;
foreach (string s in world)
{
    if (Convert.ToDecimal(tileNum) / Convert.ToDecimal(worldSize) == Math.Round(Convert.ToDecimal(tileNum) / Convert.ToDecimal(worldSize)))
    {
        Console.Write("\n");
    }

    bool showDebug = false;

    switch (s)
    {
        case "WW": //Deep water
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            break;

        case "ww": //Shallow water
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            break;
        case "GG": //High ground
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.Red;
            break;
        case "gg": //Low ground
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Red;
            break;
        case "FF": //Snow
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Red;
            break;
        case "ff": //Deep snow
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Red;
            break;
        case "SS": //Sand
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;
            break;
        case "II": //Ice
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Cyan;
            break;
        case "RR": //Stone
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.Red;
            break;
        default:
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            break;
    }

    if (!showDebug)
    {
        Console.ForegroundColor = Console.BackgroundColor;
    }

    Console.Write(s);
    tileNum++;

    Console.BackgroundColor = ConsoleColor.Black;
    Console.ForegroundColor = ConsoleColor.White;
}

Console.ReadLine();

string[,] GenerateWorld(int worldSize, int islands, string worldType)
{
    string[,] world = new String[worldSize, worldSize];
    var rand = new Random();

    int worldLimit = worldSize - 1;

    for (int x = 0; x < worldSize; x++)
    {
        for (int y = 0; y < worldSize; y++)
        {
            if (rand.Next(0, worldSize * worldSize / islands) == 0)
            {
                world[x, y] = "00";
            }
            else
            {
                world[x, y] = "[]";
            }
        }
    }

    switch (worldType)
    {
        case "default":
            for (int rep = 0; rep < 4; rep++)
            {
                for (int x = 0; x < worldSize; x++) //Loops code for all tiles
                {
                    for (int y = 0; y < worldSize; y++)
                    {
                        if (rand.Next(0, worldSize * 5) == 0 && world[x, y] != "[]")
                        {
                            world = ExtendLand(x, y, worldLimit, world, "RR", "RR", "RR"); //Adds rocks
                        }
                        else if (rand.Next(0, worldSize * 20) == 0)
                        {
                            world = ExtendLand(x, y, worldLimit, world, "RR", "RR", "RR");
                        }

                        switch (world[x, y]) //Extends land
                        {
                            case "00":
                                world = ExtendLand(x, y, worldLimit, world, "11", "22", "33");
                                break;
                            case "11":
                                world = ExtendLand(x, y, worldLimit, world, "11", "22", "33");
                                break;
                            case "22":
                                world = ExtendLand(x, y, worldLimit, world, "33", "44", "55");
                                break;
                            case "33":
                                world = ExtendLand(x, y, worldLimit, world, "44", "55", "66");
                                break;
                            case "44":
                                world = ExtendLand(x, y, worldLimit, world, "55", "66", "77");
                                break;
                            case "55":
                                world = ExtendLand(x, y, worldLimit, world, "66", "77", "77");
                                break;
                            case "66":
                                world = ExtendLand(x, y, worldLimit, world, "77", "66", "66");
                                break;
                            case "77":
                                world = ExtendLand(x, y, worldLimit, world, "77", "77", "{}");
                                break;
                            case "[]":
                                if (world[Math.Clamp(x - 1, 0, worldLimit), y] != "[]" && world[Math.Clamp(x - 1, 0, worldLimit), y] != "{}")
                                {
                                    if (world[Math.Clamp(x + 1, 0, worldLimit), y] != "[]" && world[Math.Clamp(x + 1, 0, worldLimit), y] != "{}")
                                    {
                                        if (world[x, Math.Clamp(y - 1, 0, worldLimit)] != "[]" && world[x, Math.Clamp(y - 1, 0, worldLimit)] != "{}")
                                        {
                                            if (world[x, Math.Clamp(y + 1, 0, worldLimit)] != "[]" && world[x, Math.Clamp(y + 1, 0, worldLimit)] != "{}")
                                            {
                                                world[x, y] = "SS";
                                            }
                                        }
                                    }
                                }
                                break;
                            case "{}":
                                if (world[Math.Clamp(x - 1, 0, worldLimit), y] != "[]" && world[Math.Clamp(x - 1, 0, worldLimit), y] != "{}")
                                {
                                    if (world[Math.Clamp(x + 1, 0, worldLimit), y] != "[]" && world[Math.Clamp(x + 1, 0, worldLimit), y] != "{}")
                                    {
                                        if (world[x, Math.Clamp(y - 1, 0, worldLimit)] != "[]" && world[x, Math.Clamp(y - 1, 0, worldLimit)] != "{}")
                                        {
                                            if (world[x, Math.Clamp(y + 1, 0, worldLimit)] != "[]" && world[x, Math.Clamp(y + 1, 0, worldLimit)] != "{}")
                                            {
                                                world[x, y] = "SS";
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            for (int x = 0; x < worldSize; x++) //Loops for every tile
            {
                for (int y = 0; y < worldSize; y++)
                {
                    switch (world[x, y]) //Transforms world after generation
                    {
                        case "[]":
                            int xPosition = rand.Next(-1, 2);
                            int yPosition = rand.Next(-1, 2);

                            if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] != "[]" && world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] != "{}" && rand.Next(0, 2) == 0)
                            {
                                world[x, y] = "{}";
                            }

                            world[x, y] = "WW";
                            break;
                        case "{}":
                            world[x, y] = "ww";
                            break;
                        case "00":
                            world[x, y] = "GG";
                            break;
                        case "11":
                            world[x, y] = "GG";
                            break;
                        case "22":
                            world[x, y] = "GG";
                            break;
                        case "33":
                            world[x, y] = "gg";
                            break;
                        case "44":
                            world[x, y] = "gg";
                            break;
                        case "55":
                            world[x, y] = "gg";
                            break;
                        case "66":
                            world[x, y] = "SS";
                            break;
                        case "77":
                            world[x, y] = "SS";
                            break;
                    }
                }
            }

            break;
    }

    return world;
}

string[,] ExtendLand(int x, int y, int worldLimit, string[,] world, string chance1, string chance2, string chance3)
{
    var rand = new Random();

    for (int extendChance = 0; extendChance < rand.Next(2, 4); extendChance++)
    {
        int xPosition = rand.Next(-1, 2);
        int yPosition = rand.Next(-1, 2);

        if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] == "[]")
        {
            if (rand.Next(0, 3) > 0)
            {
                world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = chance1;
            }
            else if (rand.Next(0, 2) > 0)
            {
                world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = chance2;
            }
            else
            {
                world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = chance3;
            }
        }
    }

    return world;
}