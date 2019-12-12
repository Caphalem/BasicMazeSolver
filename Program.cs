using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MazeSolver
{
    public class Coordinates : IEquatable<Coordinates>
    {
        public int x, y;

        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Coordinates objAsCoordinates = obj as Coordinates;
            if (objAsCoordinates == null)
                return false;
            else
                return Equals(objAsCoordinates);
        }

        public bool Equals(Coordinates other)
        {
            if (other == null) 
                return false;

            return (this.x == other.x && this.y == other.y);
        }
    }

    public class Program
    {
        private const string _mazeFileLocation = "../../.././Maze.txt";
        private const string _mazeLogFile = "../../.././Log.txt";
        private const string _mazeWall = "1";
        private const string _mazeCorridor = "0";
        private const string _mazeCharacter = "2";
        private const string _mazeBreadcrumb = "x";
        private static string[][] maze; // needs to be an array because of rules
        private static Coordinates currentLocation;
        private static List<Coordinates> corridors;
        private static List<Coordinates> pathToExit = new List<Coordinates>();
        private static bool exploreMaze = true;
        private static StreamWriter mazeLogFile;

        public static void Main(string[] args)
        {
            try
            {
                PrintWelcome();

                mazeLogFile = GetLogFile();

                maze = GetMaze(_mazeFileLocation);
                PrintMaze();

                SetCustomCharacterLocation();

                corridors = GetCorridors();
                currentLocation = GetCurrentLocation();
                pathToExit.Add(currentLocation);

                bool exitedMaze = false;

                while (exploreMaze)
                {
                    if (IsAtExit())
                    {
                        exploreMaze = false;
                        exitedMaze = true;
                    }
                    else
                        TakeStep();

                    Thread.Sleep(500);
                }

                if (exitedMaze)
                    Console.WriteLine("Exited the maze!");
                else
                    Console.WriteLine("There is no exit!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (mazeLogFile != null)
                    mazeLogFile.Close();

                Console.ReadKey();
            }
        }

        private static string[][] GetMaze(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception("Maze file not found!");

            StreamReader mazeFile = new StreamReader(filePath);
            string line;
            List<string[]> mazeList = new List<string[]>();

            // Skip first line, don't really need it.
            mazeFile.ReadLine();

            while ((line = mazeFile.ReadLine()) != null)
            {
                mazeList.Add(line.Split(' '));
            }
            mazeFile.Close();

            if (mazeList.Count < 1)
                throw new Exception("Maze file has no maze!");

            return mazeList.ToArray();
        }

        private static void PrintMaze()
        {
            ConsoleAndLogWrite("===");
            foreach (string mazePart in maze[0])
            {
                ConsoleAndLogWrite("==");
            }
            ConsoleAndLogWrite("==");
            ConsoleAndLogWrite(Environment.NewLine);

            foreach (string[] mazeLine in maze)
            {
                ConsoleAndLogWrite("|| ");
                foreach (string mazePart in mazeLine)
                {
                    ConsoleAndLogWrite(mazePart + ' ');
                }
                ConsoleAndLogWrite("||");
                ConsoleAndLogWrite(Environment.NewLine);
            }
        }

        private static List<Coordinates> GetCorridors()
        {
            List<Coordinates> mazeCorridorsList = new List<Coordinates>();
            int x = 0, y = 0;

            foreach (string[] mazeLine in maze)
            {
                foreach (string mazePart in mazeLine)
                {
                    if (mazePart == _mazeCorridor)
                    {
                        mazeCorridorsList.Add(new Coordinates(x, y));
                    }

                    x++;
                }

                x = 0;
                y++;
            }

            return mazeCorridorsList;
        }

        private static Coordinates GetCurrentLocation()
        {
            int x = 0, y = 0;

            foreach (string[] mazeLine in maze)
            {
                foreach (string mazePart in mazeLine)
                {
                    if (mazePart == _mazeCharacter)
                    {
                        return new Coordinates(x, y);
                    }

                    x++;
                }

                x = 0;
                y++;
            }

            throw new Exception($"Character marked {_mazeCharacter} not found in maze!");
        }

        private static void MoveTo(Coordinates newLocation)
        {
            maze[currentLocation.y][currentLocation.x] = _mazeBreadcrumb;
            maze[newLocation.y][newLocation.x] = _mazeCharacter;
            currentLocation = newLocation;

            corridors.Remove(currentLocation);
            pathToExit.Add(currentLocation);
        }

        private static Coordinates GetLeftCoordinates()
        {
            return new Coordinates(currentLocation.x - 1, currentLocation.y);
        }

        private static Coordinates GetRightCoordinates()
        {
            return new Coordinates(currentLocation.x + 1, currentLocation.y);
        }

        private static Coordinates GetDownCoordinates()
        {
            return new Coordinates(currentLocation.x, currentLocation.y - 1);
        }

        private static Coordinates GetUpCoordinates()
        {
            return new Coordinates(currentLocation.x, currentLocation.y + 1);
        }

        private static bool IsAtExit()
        {
            if (currentLocation.x == maze[0].Length - 1 || currentLocation.x == 0 ||
                currentLocation.y == maze.Length - 1 || currentLocation.y == 0)
                return true;

            return false;
        }

        private static void TakeStep()
        {
            if (currentLocation.x - 1 >= 0 && corridors.Contains(GetLeftCoordinates()))
                MoveTo(GetLeftCoordinates());
            else if (currentLocation.x + 1 <= maze[0].Length && corridors.Contains(GetRightCoordinates()))
                MoveTo(GetRightCoordinates());
            else if (currentLocation.y - 1 >= 0 && corridors.Contains(GetDownCoordinates()))
                MoveTo(GetDownCoordinates());
            else if (currentLocation.y + 1 <= maze.Length && corridors.Contains(GetUpCoordinates()))
                MoveTo(GetUpCoordinates());
            else
                Backtrack();

            PrintMaze();
        }

        private static void Backtrack()
        {
            if (pathToExit.Count > 1)
            {
                pathToExit.RemoveAt(pathToExit.Count - 1);
                MoveTo(pathToExit[pathToExit.Count - 1]);
                pathToExit.RemoveAt(pathToExit.Count - 1);

                PrintMaze();
            }
            else
                exploreMaze = false;
        }

        private static void SetCustomCharacterLocation()
        {
            Console.WriteLine("Would like to set the starting location of the character? y/n");

            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.Write(Environment.NewLine);
                bool isEnteringInput = true;

                while (isEnteringInput)
                {
                    int x = 0, y = 0;
                    string input;
                    bool isEnteringCoordinate = true;

                    while (isEnteringCoordinate)
                    {
                        Console.WriteLine("Enter the X coordinate:");
                        input = Console.ReadLine();

                        if (Int32.TryParse(input, out x))
                        {
                            if (x < 0 || x > maze[0].Length - 1)
                                Console.WriteLine($"{x} is outside of the maze's X boundaries! Only numbers between 0 and {maze[0].Length - 1} are accepted.");
                            else
                                isEnteringCoordinate = false;
                        }
                        else
                            Console.WriteLine($"\"{input}\" is not a number!");
                    }

                    isEnteringCoordinate = true;

                    while (isEnteringCoordinate)
                    {
                        Console.WriteLine("Enter the Y coordinate:");
                        input = Console.ReadLine();

                        if (Int32.TryParse(input, out y))
                        {
                            if (y < 0 || y > maze.Length - 1)
                                Console.WriteLine($"{y} is outside of the maze's Y boundaries! Only numbers between 0 and {maze.Length - 1} are accepted.");
                            else
                                isEnteringCoordinate = false;
                        }
                        else
                            Console.WriteLine($"\"{input}\" is not a number!");
                    }

                    if (maze[y][x] == _mazeWall)
                        Console.WriteLine($"Entered coordinates (x:{x}, y:{y}) leads to a wall!");
                    else
                    {
                        isEnteringInput = false;
                        Coordinates tempLocation = GetCurrentLocation();
                        maze[tempLocation.y][tempLocation.x] = _mazeCorridor;
                        maze[y][x] = _mazeCharacter;
                    }
                }
            }
        }

        private static StreamWriter GetLogFile()
        {
            if (File.Exists(_mazeLogFile))
            {
                File.Delete(_mazeLogFile);
            }

            return new StreamWriter(_mazeLogFile);
        }

        private static void ConsoleAndLogWrite(string input)
        {
            Console.Write(input);
            mazeLogFile.Write(input);
        }

        private static void PrintWelcome()
        {
            Console.WriteLine(@"**********************************************************************************************");
            Console.WriteLine(@"**********************************************************************************************");
            Console.WriteLine(@"**   ██╗    ██╗███████╗██╗      ██████╗ ██████╗ ███╗   ███╗███████╗    ████████╗ ██████╗    **");
            Console.WriteLine(@"**   ██║    ██║██╔════╝██║     ██╔════╝██╔═══██╗████╗ ████║██╔════╝    ╚══██╔══╝██╔═══██╗   **");
            Console.WriteLine(@"**   ██║ █╗ ██║█████╗  ██║     ██║     ██║   ██║██╔████╔██║█████╗         ██║   ██║   ██║   **");
            Console.WriteLine(@"**   ██║███╗██║██╔══╝  ██║     ██║     ██║   ██║██║╚██╔╝██║██╔══╝         ██║   ██║   ██║   **");
            Console.WriteLine(@"**   ╚███╔███╔╝███████╗███████╗╚██████╗╚██████╔╝██║ ╚═╝ ██║███████╗       ██║   ╚██████╔╝   **");
            Console.WriteLine(@"**    ╚══╝╚══╝ ╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚══════╝       ╚═╝    ╚═════╝    **");
            Console.WriteLine(@"**        ██████╗  █████╗ ███████╗██╗ ██████╗    ███╗   ███╗ █████╗ ███████╗███████╗        **");
            Console.WriteLine(@"**        ██╔══██╗██╔══██╗██╔════╝██║██╔════╝    ████╗ ████║██╔══██╗╚══███╔╝██╔════╝        **");
            Console.WriteLine(@"**        ██████╔╝███████║███████╗██║██║         ██╔████╔██║███████║  ███╔╝ █████╗          **");
            Console.WriteLine(@"**        ██╔══██╗██╔══██║╚════██║██║██║         ██║╚██╔╝██║██╔══██║ ███╔╝  ██╔══╝          **");
            Console.WriteLine(@"**        ██████╔╝██║  ██║███████║██║╚██████╗    ██║ ╚═╝ ██║██║  ██║███████╗███████╗        **");
            Console.WriteLine(@"**        ╚═════╝ ╚═╝  ╚═╝╚══════╝╚═╝ ╚═════╝    ╚═╝     ╚═╝╚═╝  ╚═╝╚══════╝╚══════╝        **");
            Console.WriteLine(@"**                    ███████╗ ██████╗ ██╗    ██╗   ██╗███████╗██████╗                      **");
            Console.WriteLine(@"**                    ██╔════╝██╔═══██╗██║    ██║   ██║██╔════╝██╔══██╗                     **");
            Console.WriteLine(@"**                    ███████╗██║   ██║██║    ██║   ██║█████╗  ██████╔╝                     **");
            Console.WriteLine(@"**                    ╚════██║██║   ██║██║    ╚██╗ ██╔╝██╔══╝  ██╔══██╗                     **");
            Console.WriteLine(@"**                    ███████║╚██████╔╝███████╗╚████╔╝ ███████╗██║  ██║                     **");
            Console.WriteLine(@"**                    ╚══════╝ ╚═════╝ ╚══════╝ ╚═══╝  ╚══════╝╚═╝  ╚═╝                     **");
            Console.WriteLine(@"**********************************************************************************************");
            Console.WriteLine(@"**********************************************************************************************");
        }
    }
}
