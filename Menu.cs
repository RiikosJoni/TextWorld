using System;
using System.Data;

namespace TextWorld
{
    class Menu
    {
        int SelectedOption;
        string[] Options;
        string PromptType;

        public Menu(string promptType, string[] options)
        {
            PromptType = promptType;
            Options = options;
            SelectedOption = 0;
        }

        private void DisplayOptions()
        {
            Console.Clear();

            string prefix = ">";

            switch (PromptType)
            {
                case "main":
                    Game.ScreenTop('m');
                    break;
                case "createWorld":
                    Game.ScreenTop('c');
                    break;
                case "worldType":
                    Game.ScreenTop('d');
                    Console.Write("\n");
                    break;
                default:
                    Game.ScreenTop('o');
                    break;
            }

            for (int i = 0; i < Options.Length; i++) 
            {
                Game.ScreenLeft(GV.screenWidth / 12);
                string currentOption = Options[i];
                if (i == SelectedOption)
                {
                    Game.ResetTextColor(true);

                    Console.Write($"{prefix} < {currentOption} >");
                    if (PromptType == "createWorld")
                    {
                        if (i == 0 | i == 2 | i == 4)
                        {
                            Game.ScreenLeft(1);
                        }
                    }
                    Game.ResetTextColor(false);
                }
                else
                {
                    Console.Write($"  < {currentOption} >");
                    if (PromptType == "createWorld")
                    {
                        if (i == 0 | i == 2 | i == 4)
                        {
                            Game.ScreenLeft(1);
                        }
                    }
                }
            }
        }

        public int Run()
        {
            DisplayOptions();

            ConsoleKey keyPressed;
            do
            {
                bool refreshOptions = false;

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    SelectedOption--;
                    refreshOptions = true;
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    SelectedOption++;
                    refreshOptions = true;
                }

                if (SelectedOption < 0)
                {
                    SelectedOption = Options.Length - 1;
                }
                else if (SelectedOption > Options.Length - 1)
                {
                    SelectedOption = 0;
                }

                if (refreshOptions)
                {
                    DisplayOptions();
                }
            } while (keyPressed != ConsoleKey.Enter);

            return SelectedOption;
        }
    }
}