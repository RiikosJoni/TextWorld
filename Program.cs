using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using System.Linq;

int worldSize = 40; //World size in tiles (50 = 50 x 50)
int islandAmount = 5; //How many island seeds are planted

float temperature = 1f; //A number between 0 and 2. Smaller number makes ice biomes more common and larger numbers make deserts more common

int seaLevel = 8;

string worldType = "default";

Console.WriteLine("TextWorld V0.2 Loaded!");

string[,] worldDepth = GenerateWorldDepth(worldSize, islandAmount, temperature);
string[,] world = GenerateWorld(worldSize, worldDepth, seaLevel, worldType);
string[,] worldStructures;
string[,] worldEntities;

RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'a');

string[,] GenerateWorldDepth(int worldSize, int islandAmount, float temperature){
    string[,] world = new String[worldSize, worldSize];
    var rand = new Random();

    int worldLimit = worldSize - 1;

    for (int x = 0; x < worldSize; x++)
    {
        for (int y = 0; y < worldSize; y++)
        {
            world[x, y] = "99";
        }
    }

    for (int islands = 0; islands < islandAmount; islands++)
    {
        float randTemp = rand.Next(1, 6) * temperature;

        if (randTemp <= 1f) 
        {
            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = "0C";
        }
        else if (randTemp >= 5f)
        {
            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = "0H";
        }
        else
        {
            world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = "00";
        }
    }

    RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

    for (int gen1 = 0; gen1 < 5; gen1++)
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

    for (int gen2 = 0; gen2 < 5; gen2++)
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                switch (world[x, y])
                {
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

    for (int gen3 = 0; gen3 < 5; gen3++)
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

    for (int gen4 = 0; gen4 < 5; gen4++)
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
                    case "88":
                        ExtendLand(x, y, worldLimit, world, "88", "99");
                        break;
                    case "8C":
                        ExtendLand(x, y, worldLimit, world, "8C", "9C");
                        break;
                    case "8H":
                        ExtendLand(x, y, worldLimit, world, "8H", "9H");
                        break;
                }
            }
        }
    }
    RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

    return world;
}

string[,] GenerateWorld(int worldSize, string[,] depthMap, int seaLevel, string worldType)
{
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

    for (int x = 0; x < worldSize; x++)
    {
        for (int y = 0; y < worldSize; y++)
        {
            switch (depthMap[x, y][0].ToString())
            {
                case "9":
                    if (seaLevel <= 9) //Checks if it's under the sea level
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else
                    {
                        if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "ss";
                        }
                        else
                        {
                            world[x, y] = "rr";
                        }
                    }
                    break;
                case "8":
                    if (seaLevel <= 8) //Checks if it's under the sea level
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else
                    {
                        world[x, y] = "ss";
                    }
                    break;
                case "7":
                    if (seaLevel <= 7) //Checks if it's under the sea level
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else if (seaLevel <= 9) //Checks if it's over the sea
                    {
                        world[x, y] = "ss";
                    }
                    else //Checks if it's far enough from the sea to grow grass
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "ff";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "gg";
                        }
                    }
                    break;
                case "6":
                    if (seaLevel <= 6)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else if (seaLevel <= 8)
                    {
                        world[x, y] = "ss";
                    }
                    else
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "ff";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "gg";
                        }
                    }
                    break;
                case "5":
                    if (seaLevel <= 5)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else if (seaLevel <= 7)
                    {
                        world[x, y] = "ss";
                    }
                    else if (seaLevel <= 10)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "ff";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "gg";
                        }
                    }
                    else
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "FF";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "GG";
                        }
                    }
                    break;
                case "4":
                    if (seaLevel <= 4)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else if (seaLevel <= 6)
                    {
                        world[x, y] = "ss";
                    }
                    else if (seaLevel <= 9)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "ff";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "gg";
                        }
                    }
                    else
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "FF";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "GG";
                        }
                    }
                    break;
                case "3":
                    if (seaLevel <= 3)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else if (seaLevel <= 5)
                    {
                        world[x, y] = "ss";
                    }
                    else if (seaLevel <= 8)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "ff";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "gg";
                        }
                    }
                    else
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "FF";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "GG";
                        }
                    }
                    break;
                case "2":
                    if (seaLevel <= 2)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else if (seaLevel <= 4)
                    {
                        world[x, y] = "ss";
                    }
                    else if (seaLevel <= 7)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "ff";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "gg";
                        }
                    }
                    else
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "FF";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "GG";
                        }
                    }
                    break;
                case "1":
                    if (seaLevel <= 1)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else if (seaLevel <= 3)
                    {
                        world[x, y] = "ss";
                    }
                    else if (seaLevel <= 6)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "ff";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "gg";
                        }
                    }
                    else
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "FF";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "GG";
                        }
                    }
                    break;
                case "0":
                    if (seaLevel <= 0)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "II";
                        }
                        else
                        {
                            world[x, y] = "WW";
                        }
                    }
                    else if (seaLevel <= 2)
                    {
                        world[x, y] = "ss";
                    }
                    else if (seaLevel <= 5)
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "ff";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "gg";
                        }
                    }
                    else
                    {
                        if (depthMap[x, y][1].ToString() == "C")
                        {
                            world[x, y] = "FF";
                        }
                        else if (depthMap[x, y][1].ToString() == "H")
                        {
                            world[x, y] = "SS";
                        }
                        else
                        {
                            world[x, y] = "GG";
                        }
                    }
                    break;
            }
        }
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

        if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] == "99")
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
            if (x >= xStart && x < xEnd && y >= yStart && y <= yEnd)
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
                                Console.ForegroundColor = ConsoleColor.Blue;
                                break;
                            case "w": //Shallow water
                                Console.BackgroundColor = ConsoleColor.DarkCyan;
                                Console.ForegroundColor = ConsoleColor.Blue;
                                break;
                            case "I": //Ice
                                Console.BackgroundColor = ConsoleColor.Cyan;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "r": //Rocky terrain
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "R": //Rock
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "s": //Sand
                                Console.BackgroundColor = ConsoleColor.Yellow;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "S": //Sand Dune
                                Console.BackgroundColor = ConsoleColor.DarkYellow;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "f": //Snow
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "F": //Snow hill
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "g": //Ground
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case "G": //High ground
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
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