using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using System.Linq;

int worldSize = 50;
int islandAmount = 5;
float temperature = 1;
int seaLevel = 8;

string worldType = "default";

Console.WriteLine("TextWorld V0.2 Loaded!");

string[,] worldDepth = GenerateWorldDepth(worldSize, islandAmount, temperature);
string[,] world = GenerateWorld(worldSize, islandAmount, worldType);
string[,] worldStructures;
string[,] worldEntities;

RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'a');
Console.Clear();
RenderWorld(world, worldSize, 0, 10, 0, 10, 'a');
Console.Clear();
RenderWorld(world, worldSize, 5, 15, 5, 15, 'a');
Console.Clear();
RenderWorld(world, worldSize, worldSize / 2, worldSize, worldSize / 2, worldSize, 'a');
Console.Clear();

string[,] GenerateWorldDepth(int worldSize, int islandAmount, float temperature){
    string[,] world = new String[worldSize, worldSize];
    var rand = new Random();

    int worldLimit = worldSize - 1;

    for (int x = 0; x < worldSize; x++)
    {
        for (int y = 0; y < worldSize; y++)
        {
            world[x, y] = "[]";
        }
    }

    for (int islands = 0; islands < islandAmount; islands++)
    {
        float randTemp = rand.Next(0, 100) * temperature;

        if (randTemp < 25) 
        {
            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = "0C";
        }
        else if (randTemp > 75)
        {
            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = "0H";
        }
        else
        {
            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = "00";
        }
    }

    RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

    for (int gen1 = 0; gen1 < 10; gen1++)
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                switch (world[x, y])
                {
                    case "00":
                        ExtendLand(x, y, worldLimit, world, "11", "00");
                        break;
                    case "0C":
                        ExtendLand(x, y, worldLimit, world, "1C", "0C");
                        break;
                    case "0H":
                        ExtendLand(x, y, worldLimit, world, "1H", "0H");
                        break;
                    case "11":
                        ExtendLand(x, y, worldLimit, world, "22", "33");
                        break;
                    case "1C":
                        ExtendLand(x, y, worldLimit, world, "2C", "3C");
                        break;
                    case "1H":
                        ExtendLand(x, y, worldLimit, world, "2H", "3H");
                        break;
                }
            }
        }
    }
    RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

    for (int gen2 = 0; gen2 < 10; gen2++)
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                switch (world[x, y])
                {
                    case "22":
                        ExtendLand(x, y, worldLimit, world, "33", "22");
                        break;
                    case "2C":
                        ExtendLand(x, y, worldLimit, world, "3C", "2C");
                        break;
                    case "2H":
                        ExtendLand(x, y, worldLimit, world, "3H", "2H");
                        break;
                    case "33":
                        ExtendLand(x, y, worldLimit, world, "44", "55");
                        break;
                    case "3C":
                        ExtendLand(x, y, worldLimit, world, "4C", "5C");
                        break;
                    case "3H":
                        ExtendLand(x, y, worldLimit, world, "4H", "5H");
                        break;
                }
            }
        }
    }
    RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

    for (int gen3 = 0; gen3 < 10; gen3++)
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                switch (world[x, y])
                {
                    case "44":
                        ExtendLand(x, y, worldLimit, world, "55", "44");
                        break;
                    case "4C":
                        ExtendLand(x, y, worldLimit, world, "5C", "4C");
                        break;
                    case "4H":
                        ExtendLand(x, y, worldLimit, world, "5H", "4H");
                        break;
                    case "55":
                        ExtendLand(x, y, worldLimit, world, "66", "77");
                        break;
                    case "5C":
                        ExtendLand(x, y, worldLimit, world, "6C", "7C");
                        break;
                    case "5H":
                        ExtendLand(x, y, worldLimit, world, "6H", "7H");
                        break;
                }
            }
        }
    }
    RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

    for (int gen4 = 0; gen4 < 10; gen4++)
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                switch (world[x, y])
                {
                    case "66":
                        ExtendLand(x, y, worldLimit, world, "77", "77");
                        break;
                    case "6C":
                        ExtendLand(x, y, worldLimit, world, "7C", "7C");
                        break;
                    case "6H":
                        ExtendLand(x, y, worldLimit, world, "7H", "7H");
                        break;
                    case "77":
                        ExtendLand(x, y, worldLimit, world, "88", "77");
                        break;
                    case "7C":
                        ExtendLand(x, y, worldLimit, world, "8C", "7C");
                        break;
                    case "7H":
                        ExtendLand(x, y, worldLimit, world, "8H", "7H");
                        break;
                }
            }
        }
    }
    RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

    for (int gen5 = 0; gen5 < 3; gen5++)
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                switch (world[x, y])
                {
                    case "88":
                        ExtendLand(x, y, worldLimit, world, "99", "88");
                        break;
                    case "8C":
                        ExtendLand(x, y, worldLimit, world, "9C", "8C");
                        break;
                    case "8H":
                        ExtendLand(x, y, worldLimit, world, "9H", "8H");
                        break;
                    case "[]":
                        world[x, y] = "99";
                        break;
                }
            }
        }
    }
    RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

    return world;
}

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
                            world = ExtendLand(x, y, worldLimit, world, "RR", "RR"); //Adds rocks
                        }
                        else if (rand.Next(0, worldSize * 20) == 0)
                        {
                            world = ExtendLand(x, y, worldLimit, world, "RR", "RR");
                        }

                        switch (world[x, y]) //Extends land
                        {
                            case "00":
                                world = ExtendLand(x, y, worldLimit, world, "11", "22");
                                break;
                            case "11":
                                world = ExtendLand(x, y, worldLimit, world, "11", "22");
                                break;
                            case "22":
                                world = ExtendLand(x, y, worldLimit, world, "33", "44");
                                break;
                            case "33":
                                world = ExtendLand(x, y, worldLimit, world, "44", "55");
                                break;
                            case "44":
                                world = ExtendLand(x, y, worldLimit, world, "44", "55");
                                break;
                            case "55":
                                world = ExtendLand(x, y, worldLimit, world, "66", "77");
                                break;
                            case "66":
                                world = ExtendLand(x, y, worldLimit, world, "77", "66");
                                break;
                            case "77":
                                world = ExtendLand(x, y, worldLimit, world, "77", "{}");
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

string[,] ExtendLand(int x, int y, int worldLimit, string[,] world, string chance1, string chance2)
{
    var rand = new Random();

    for (int extendChance = 0; extendChance < 8; extendChance++)
    {
        int xPosition = rand.Next(-1, 2);
        int yPosition = rand.Next(-1, 2);

        if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] == "[]")
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

static void RenderWorld(string[,] world, int worldSize, int xStart, int xEnd, int yStart, int yEnd, char renderer)
{
    int tileNum = 0;
    for (int x = 0; x < worldSize; x++)
    {
       for (int y = 0; y < worldSize; y++)
        {
            if (x >= xStart && x <= xEnd && y > yStart && y < yEnd)
            {
                if (Convert.ToDecimal(tileNum) / Convert.ToDecimal(xStart - xEnd) == Math.Round(Convert.ToDecimal(tileNum) / Convert.ToDecimal(xStart - xEnd)))
                {
                    Console.Write("\n");
                }

                bool showDebug = true;

                switch (renderer)
                {
                    case 'a':
                        switch (world[x, y][0].ToString())
                        {
                            case "W": //Deep water
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                                break;

                            case "w": //Shallow water
                                Console.BackgroundColor = ConsoleColor.DarkCyan;
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                break;
                            case "G": //High ground
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "g": //Low ground
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "F": //Snow
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "f": //Deep snow
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "S": //Sand
                                Console.BackgroundColor = ConsoleColor.Yellow;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "I": //Ice
                                Console.BackgroundColor = ConsoleColor.Cyan;
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                break;
                            case "R": //Stone
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            default:
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                        }
                        break;
                    case 'b':
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
                    default:
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }

                if (!showDebug)
                {
                    Console.ForegroundColor = Console.BackgroundColor;
                }

                Console.Write(world[x, y]);
                tileNum++;

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }

    Console.ReadLine();
    Console.Clear();
}