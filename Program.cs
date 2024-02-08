using System;
using static System.Console;

namespace TextWorld
{
    public class Program
    {
        static void Main(string[] args) 
        {
            Game textGame = new();
            textGame.Start();
        }
    }

    public static class GlobalVariables
    {
        public static string version = "0.5";

        public static int screenWidth = 120;
        public static int screenHeight = 20;

        public static int defaultTextColor = 253; //Default text color used for the edges of the screen and other menus Default is 15 & 0
        public static int defaultTextBackgroundColor = 233;

        public static int currentTextColor = 253;
        public static int currentTextBackgroundColor = 233;



        //Used for world gen
        public static string newWorldName = "New World";

        public static int newWorldSize = 1000;
        public static string newWorldType = "Default";

        public static string newWorldGamemode = "Survival";
        public static string newWorldDifficulty = "Medium";



        //used for custom worlds
        public static bool worldGenerateBiomes = true;
        public static bool worldGenerateSpecialBiomes = true;

        public static string worldMainBiome = "Grassy Plains";

        public static bool worldGenerateFeatures = true;
        public static bool worldGenerateStructures = true;

        public static int worldTemperature = 50;
        public static int worldSeaLevel = 8;

        public static int worldIslandAmount = newWorldSize / 20 * newWorldSize / 20;

        //used for world
        public static string[,] currentWorldDepth;
        public static string[,] currentWorld;
        public static string[,] currentWorldStructures;
        public static string[,] currentWorldEntities;

        public static string[] tilesBlockPlayer = {"W", "L", "l"};
        public static string[] tilesBlockBuildings = { "W", "w", "L", "l" };

        public static int worldDay = 1;
        public static int worldTime = 1;

        public static int playerX = 789;
        public static int playerY;

        public static int playerMoney = 0;
        public static float playerEnergy = 100;

        public static List<string> playerInventory;

        public static bool renderDebug = false;
    }
}