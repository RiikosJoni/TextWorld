using System;
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

string version = "V0.4";

int defaultTextColor = 15; //Default text color used for the edges of the screen and other menus
int defaultTextBackgroundColor = 0;

int worldSize = 100; //World size in tiles (50 = 50 x 50)
int islandAmount = 20; //How many island seeds are planted

float temperature = 50f; //A number between 0 and 100. Smaller number makes ice biomes more common and larger numbers make deserts more common

bool generateBiomes = true; //Toggles the generation of random biomes. If false, uses mainBiome instead
bool generateSpecialBiomes = true; //Toggles generation of rare "special biomes" such as the volcano biome, candy biome and the reverse biome

char mainBiome = 'V'; //This will be the only biome that's generated if generateBiomes is set to false.

int seaLevel = 8; //Changes how high the sea level is. Larger numbers mean lower sea level. >9 means no sea, <1 means no land. 8 is the default

bool generateFeatures = true; //Toggles generation of rivers, volcanoes, rocks, etc.

string worldType = "default";

Console.WriteLine("TextWorld " + version + " Loaded!");

Console.ReadLine();

string[,] worldDepth = GenerateWorldDepth(worldSize, islandAmount, temperature, generateBiomes, generateSpecialBiomes, mainBiome, worldType); //Generates the depthmap
string[,] world = GenerateWorld(worldSize, worldDepth, seaLevel, generateFeatures, worldType); //Generates the world map
string[,] worldStructures; //Generates structures
string[,] worldEntities; //Generates entities

RenderWorld(world, worldSize, 0, worldSize, 0, worldSize, 'a'); //Renders the world map

string[,] GenerateWorldDepth(int worldSize, int islandAmount, float temperature, bool generateBiomes, bool generateSpecialBiomes, char mainBiome, string worldType){

    string[,] world = new String[worldSize, worldSize]; //Creates the map

    var rand = new Random();
    int worldLimit = worldSize - 1;

    for (int x = 0; x < worldSize; x++)
    {
        for (int y = 0; y < worldSize; y++)
        {
            world[x, y] = "99"; //Fills the map with "empty space"
        }
    }

    for (int islands = 0; islands < islandAmount; islands++) //Generates islands
    {
        if (temperature == 50f)
        {
            temperature = rand.Next(40, 60);
            Console.Write("Global temperature modifier is: " + (temperature - 50) + "!");
        }

        float randTemp = rand.Next(1, 100) * (temperature / 100); //Creates a random temperature

        Console.Write(randTemp);

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
            else if (generateSpecialBiomes && rand.Next(0, 51) == 1)
            {
                world[rand.Next(0, worldSize), rand.Next(0, worldSize)] = $"{islandHeight}3";
            }
            else if (generateSpecialBiomes && rand.Next(0, 51) == 2)
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
                        ExtendLand(x, y, worldLimit, world, $"1{world[x, y][1].ToString()}", $"0{ world[x, y][1].ToString()}");
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
            string sandTile = "ss";
            string lowGroundTile = "dd";
            string highGroundTile = "DD";
            string waterTile = "WW";
            string deepWaterTile = "WD";

            switch (depthMap[x, y][1].ToString()) { //Determine tiles
                case "C":
                    lowGroundTile = "ff";
                    highGroundTile = "FF";
                    waterTile = "II";

                    break;
                case "T":
                    lowGroundTile = "gC";
                    highGroundTile = "GC";
                    waterTile = "II";

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
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                if (depthMap[x, y][1].ToString() != "C" && rand.Next(0, 6000) == 0)
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
            }
        }
    }

    //Post-generation; Making the shores look nicer and stuff
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
        for ( int xPos = 0; xPos < worldSize; xPos++ ) 
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
                        }else
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

    for ( int tiles = 0; tiles < radius * 2 + 1; tiles++ ) 
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
                    for (int extendChance = 0; extendChance < rand.Next(worldSize / 10, worldSize / 2); extendChance++)
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
                    for (int extendChance = 0; extendChance < rand.Next(worldSize / 20, worldSize / 10); extendChance++)
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

            for ( int lavaCircle = 0; lavaCircle < size - 3; lavaCircle++)
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
    }

    return world;
}

static void RenderWorld(string[,] world, int worldSize, int xStart, int xEnd, int yStart, int yEnd, char renderer)
{
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
                    Console.Write("\n");
                }

                bool showDebug = true;

                switch (renderer)
                {
                    case 'a':
                        switch (world[limitedX, limitedY][0].ToString())
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
                                SetTextBackgroundColor(244);
                                SetTextColor(1);
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
                                SetTextColor(1);
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
                                SetTextColor(1);
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
                                SetTextColor(1);
                                break;
                            case "d": //Ground
                                SetTextBackgroundColor(100);
                                SetTextColor(1);
                                break;
                            case "D": //High ground
                                SetTextBackgroundColor(58);
                                SetTextColor(1);
                                break;
                            case "L": //Lava
                                if (world[limitedX, limitedY][1].ToString() == "H")
                                {
                                    SetTextBackgroundColor(208);
                                    SetTextColor(208);
                                }
                                else
                                {
                                    SetTextBackgroundColor(202);
                                    SetTextColor(202);
                                }
                                break;
                            case "l": //Lava
                                SetTextBackgroundColor(202);
                                SetTextColor(202);
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

                Console.Write(world[limitedX, limitedY]);
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