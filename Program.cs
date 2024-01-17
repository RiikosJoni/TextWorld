using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;

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


int defaultTextColor = 15;
int defaultTextBackgroundColor = 0;

int worldSize = 100; //World size in tiles (50 = 50 x 50)
int islandAmount = 20; //How many island seeds are planted

float temperature = 1f; //A number between 0 and 2. Smaller number makes ice biomes more common and larger numbers make deserts more common

int seaLevel = 8; //Changes how high the sea level is. Larger numbers mean lower sea level. >9 means no sea, <1 means no land. 8 is the default

bool generateFeatures = true;
string worldType = "default";

Console.WriteLine("TextWorld V0.2 Loaded!");

Console.ReadLine();

string[,] worldDepth = GenerateWorldDepth(worldSize, islandAmount, temperature); //Generates the depthmap
string[,] world = GenerateWorld(worldSize, worldDepth, seaLevel, generateFeatures, worldType); //Generates the world map
string[,] worldStructures; //Generates structures
string[,] worldEntities; //Generates entities

RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'a'); //Renders the world map

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

    for (int x = 0; x < worldSize; x++)
    {
        for (int y = 0; y < worldSize; y++)
        {
            switch (world[x, y])
            {
                case "00":
                    for (int extendChance = 0; extendChance < 3; extendChance++)
                    {
                        int xPosition = rand.Next(-1, 2);
                        int yPosition = rand.Next(-1, 2);

                        if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] == "99")
                        {
                            world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "00";
                        }
                    }
                    break;
                case "0C":
                    for (int extendChance = 0; extendChance < 3; extendChance++)
                    {
                        int xPosition = rand.Next(-1, 2);
                        int yPosition = rand.Next(-1, 2);

                        if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] == "99")
                        {
                            world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "0C";
                        }
                    }
                    break;
                case "0H":
                    for (int extendChance = 0; extendChance < 3; extendChance++)
                    {
                        int xPosition = rand.Next(-1, 2);
                        int yPosition = rand.Next(-1, 2);

                        if (world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] == "99")
                        {
                            world[Math.Clamp((x + xPosition), 0, worldLimit), Math.Clamp((y + yPosition), 0, worldLimit)] = "0H";
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
    //RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

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
    //RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'b');

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

string[,] GenerateWorld(int worldSize, string[,] depthMap, int seaLevel, bool generateFeatures, string worldType)
{
    string[,] world = new String[worldSize, worldSize]; //Generates empty map
    var rand = new Random();

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
            if (Convert.ToInt16(depthMap[x, y][0].ToString()) == seaLevel)
            {
                if (depthMap[x, y][1].ToString() == "C") //Turns into ice if on the the biome
                {
                    world[x, y] = "II";
                }
                else
                {
                    world[x, y] = "WW";
                }
            }
            else if (Convert.ToInt16(depthMap[x, y][0].ToString()) > seaLevel)
            {
                if (depthMap[x, y][1].ToString() == "C") //Turns into ice if on the the biome
                {
                    world[x, y] = "II";
                }
                else
                {
                    world[x, y] = "WD";
                }
            }
            else if (Convert.ToInt16(depthMap[x, y][0].ToString()) > seaLevel - 3)
            {
                //if (depthMap[x, y][1].ToString() == "C") //Creates rocky beach
                //{
                    //world[x, y] = "rr";
                //}
                //else
                //{
                    world[x, y] = "ss";
                //}
            }
            else if (Convert.ToInt16(depthMap[x, y][0].ToString()) > seaLevel - 6)
            {
                if (depthMap[x, y][1].ToString() == "C")
                {
                    world[x, y] = "ff";
                }
                else if (depthMap[x, y][1].ToString() == "H")
                {
                    world[x, y] = "ss";
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
        }
    }

    if (generateFeatures == true) //Generates features
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                if (depthMap[x, y][1].ToString() == "H" && rand.Next(0, 400) == 0)
                {
                    if (Convert.ToInt16(depthMap[x, y][0].ToString()) < seaLevel)
                    {
                        GenerateFeature(x, y, worldSize, world, "oasis");
                    }
                }
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

string[,] GenerateFeature(int x, int y, int worldSize, string[,] world, string type)
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

                    world[Math.Clamp((x + xPosition + xPosition2), 0, worldLimit), Math.Clamp((y + yPosition + yPosition2), 0, worldLimit)] = "GH";
                }
            }
            break;
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
                                SetTextBackgroundColor(27);
                                SetTextColor(27);
                                break;
                            case "w": //Shallow water
                                SetTextBackgroundColor(12);
                                SetTextColor(12);
                                break;
                            case "I": //Ice
                                SetTextBackgroundColor(14);
                                SetTextColor(1);
                                break;
                            case "r": //Rocky terrain
                                SetTextBackgroundColor(242);
                                SetTextColor(1);
                                break;
                            case "R": //Rock
                                SetTextBackgroundColor(8);
                                SetTextColor(1);
                                break;
                            case "s": //Sand
                                SetTextBackgroundColor(11);
                                SetTextColor(1);
                                break;
                            case "S": //Sand Dune
                                SetTextBackgroundColor(186);
                                SetTextColor(1);
                                break;
                            case "f": //Snow
                                SetTextBackgroundColor(15);
                                SetTextColor(1);
                                break;
                            case "F": //Snow hill
                                SetTextBackgroundColor(7);
                                SetTextColor(1);
                                break;
                            case "g": //Ground
                                SetTextBackgroundColor(10);
                                SetTextColor(1);
                                break;
                            case "G": //High ground
                                if (world[x, y][1].ToString() == "H")
                                {
                                    SetTextBackgroundColor(70);
                                }
                                else
                                {
                                    SetTextBackgroundColor(2);
                                }
                                SetTextColor(1);
                                break;
                            case "L": //Lava
                                SetTextBackgroundColor(9);
                                SetTextColor(9);
                                break;
                            default:
                                SetTextBackgroundColor(0);
                                SetTextColor(1);
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
                        SetTextBackgroundColor(0);
                        SetTextColor(1);
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

static void SetTextColor(int colorIndex)
{
    Console.Write("\x1b[38;5;" + colorIndex + "m");
}

static void SetTextBackgroundColor(int colorIndex)
{
    Console.Write("\x1b[48;5;" + colorIndex + "m");
}

static void ResetTextColor(int defaultColor, int defaultBackgroundColor)
{
    Console.Write("\x1b[38;5;" + defaultColor + "m");
    Console.Write("\x1b[48;5;" + defaultBackgroundColor + "m");
}