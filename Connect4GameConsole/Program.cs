using System;
using System.IO;
using System.Threading;

class MyConnect4Game
{
    private static void MakeSurroundCheckCoordinates()//Making the surround coordinates for game
    {
        int firstDIndex = 0, secondDIndex = 0;
        for (int count0 = -1; count0 < 2; count0++)
        {
            for (int count1 = -1; count1 < 2; count1++)
            {
                surroundCheckCoordinates[firstDIndex, secondDIndex] = count0;
                secondDIndex += 1;
                surroundCheckCoordinates[firstDIndex, secondDIndex] = count1;
                firstDIndex += 1;
                secondDIndex = 0;
            }
        }
    }
    private static void Main()
    {
        MakeSurroundCheckCoordinates();
        Connect4Game myGame = new Connect4Game();
        while (true) { myGame.StartGame(); }
    }
    class Output//Output functions class
    {
        public static void WriteMoveChar(char playerCounter, int playerIndex)
        {
            Console.BackgroundColor = colors[playerIndex];
            Console.Write(playerCounter);
            Console.ResetColor();
        }
        public static void WriteCurrentMoveChar(char playerCount)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(playerCount);
            Console.ResetColor();
        }
        public static void WriteLineColored(string output, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(output);
            Console.ResetColor();
        }
        public static void WriteColored(string output, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(output);
            Console.ResetColor();
        }
    }
    class ValidateInput//Validating input functions class
    {
        public static int NumberEntry(int min, int max, string output)
        {
            int input;
            while (true)
            {
                input = Int(output);
                if (input < min) { Output.WriteLineColored($"Error, number entered is smaller than minimum number {min}... Try again.", errorColor); continue; }
                else if (input > max) { Output.WriteLineColored($"Error, number entered is bigger than maximum number {max}... Try again.", errorColor); continue; }
                return input;
            }
        }
        public static int Int(string output)
        {
            string input;
            int result;
            while (true)
            {
                Console.Write(output);
                input = Console.ReadLine();
                if (!int.TryParse(input, out result)) { Output.WriteLineColored("Error, input entered is not a whole number... Try again.", errorColor); continue; }
                return result;
            }
        }
        public static string Str(string output)
        {
            string input;
            int spaceCount;
            while (true)
            {
                Console.Write(output);
                input = Console.ReadLine();
                spaceCount = 0;
                foreach (char element in input) { if (element == ' ') { spaceCount += 1; } }
                if (input == "") { Output.WriteLineColored("Error, input entered can't be an empty... Try again.", errorColor); continue; }
                else if (spaceCount == input.Length) { Output.WriteLineColored("Error, input entered can't be only spaces... Try again.", errorColor); continue; }
                return input;
            }
        }
        public static bool Bool(string output)
        {
            string input;
            while (true)
            {
                Console.Write(output);
                input = Console.ReadLine().ToLower();
                if (input != "no" && input != "yes") { Output.WriteLineColored("Error, input entered is not a valid choice... Try again.", errorColor); continue; }
                if (input == "yes") { return true; }
                else { return false; }
            }
        }
        public static char Char(string output)
        {
            string input;
            char result;
            while (true)
            {
                Console.Write(output);
                input = Console.ReadLine();
                if (!char.TryParse(input, out result)) { Output.WriteLineColored("Error, input entered is not a character... Try again.", errorColor); continue; }
                return result;
            }
        }

    }
    class InvalidDataTypeEnteredException : Exception
    {
        public InvalidDataTypeEnteredException(string dataType)
        : base($"***************************************************************************************\nError, data type entered doesn't equal to the datatype : \"{dataType}\" that is wanted.\n***************************************************************************************") { }
    }

    static readonly string directory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"../../../../")) + @"Saves";
    static readonly char[] AIPossibleMoveChars = { 'O', 'X', '$', '@', '#', 'A', '£', '%', '*', '?' }, invalidChars = { '|', ' ', '-', '\\', '/' };
    static readonly ConsoleColor[] colors = { ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.DarkGray, ConsoleColor.DarkCyan, ConsoleColor.Yellow };
    static readonly ConsoleColor errorColor = ConsoleColor.Red;
    static int[,] surroundCheckCoordinates = new int[9, 2];
    static readonly int waitTime = 2000;
    static Random random = new Random();

    class Connect4Game//The actual game class
    {
        char[,] connect4Board;
        int connect4BoardColumns, connect4BoardRows, connectNum, playersNumber, AINumber, movesCurrent = 0, lastMove = 0, movesMax;
        int[] playerOrder, columnFillCount;
        string[] playerNames; char[] playerMoveChars;
        bool waitAfterAIMove = false;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Menus and init methods

        public void StartGame()// Start a game after finishing or saving 
        {
            if (!IsTextFilePresent()) { InitiliseNewGame(); }
            else { FileGameMenu("Load"); }
        }
        private void InitiliseNewGame()// Initilising new Game
        {
            Console.WriteLine("\nNew Game");
            connect4BoardColumns = ValidateInput.NumberEntry(4, 20, "Enter the number of columns in the connect 4 game : ");
            connect4BoardRows = ValidateInput.NumberEntry(4, 20, "Enter the number of rows in the connect 4 game : ");
            int smallerDimension;
            if (connect4BoardColumns < connect4BoardRows) { smallerDimension = connect4BoardColumns; }
            else { smallerDimension = connect4BoardRows; }
            connectNum = ValidateInput.NumberEntry(4, smallerDimension, "Enter the number of counter needed to be in line to win : ");

            movesCurrent = 0;
            movesMax = connect4BoardColumns * connect4BoardRows;
            columnFillCount = new int[connect4BoardColumns];

            connect4Board = new char[connect4BoardColumns, connect4BoardRows];
            // Filling board with dummmy, space char values, that represent empty spaces
            for (int rowCount = 0; rowCount < connect4BoardRows; rowCount++) { for (int columnCount = 0; columnCount < connect4BoardColumns; columnCount++) { connect4Board[columnCount, rowCount] = ' '; } }

            playersNumber = ValidateInput.NumberEntry(2, 8, "Enter the number of players that will be playing the game : ");
            AINumber = ValidateInput.NumberEntry(0, playersNumber, "Enter the number of AI players that will be playing the game : ");
            if (AINumber != 0) { waitAfterAIMove = ValidateInput.Bool("Enter yes to have a wait time between AI moves, \nEnter no to have no wait time\nEnter choice : "); }
            playerNames = new string[playersNumber];
            playerMoveChars = new char[playersNumber];
            playerOrder = new int[playersNumber];

            // Setting the order of the players
            for (int count = 0; count < playersNumber; count++) { playerOrder[count] = -1; }
            for (int count = 0; count < playersNumber; count++)
            {
                bool validOrder;
                int newPlayerAdd = 0;
                do
                {
                    validOrder = true;
                    newPlayerAdd = random.Next(0, playersNumber);
                    foreach (int number in playerOrder) { if (number == newPlayerAdd) { validOrder = false; } }
                } while (!validOrder);
                playerOrder[count] = newPlayerAdd;
            }

            // Adding human players
            for (int count = 0; count < playersNumber - AINumber; count++)
            {
                playerNames[count] = ValidateInput.Str($"Enter the name of player {count + 1} : ");
                char playerCounter;
                bool validChar;
                do
                {
                    playerCounter = ValidateInput.Char($"Enter the character that will represent player {playerNames[count]} : ");
                    playerCounter = char.ToUpper(playerCounter);
                    validChar = true;
                    foreach (char playerCounters in playerMoveChars) { if (playerCounter == playerCounters) { validChar = false; Output.WriteLineColored("Player counter entered is already present. Enter a new one.", errorColor); break; } }
                    foreach (char invalidChar in invalidChars) { if (playerCounter == invalidChar) { validChar = false; Output.WriteLineColored("The character entered can't be entered as a counter.", errorColor); break; } }
                } while (!validChar);
                playerMoveChars[count] = playerCounter;
            }

            // Adding AI players
            for (int count = 0; count < AINumber; count++)
            {
                playerNames[playersNumber - AINumber + count] = "AI" + (count + 1).ToString();
                bool charPresent;
                int moveCounter = 0;
                char AICounter;
                do
                {
                    charPresent = false;
                    AICounter = AIPossibleMoveChars[moveCounter];
                    foreach (char presentCounter in playerMoveChars) { if (presentCounter == AICounter) { moveCounter += 1; charPresent = true; break; } }
                } while (charPresent);
                playerMoveChars[playersNumber - AINumber + count] = AICounter;
            }
            int moveEntered = PlayGame();
            if (moveEntered == 0 || moveEntered == 999) { }
            else { FileGameMenu("Save"); }
        }
        public void FileGameMenu(string mode)// Save game menu
        {
            string choice;
            bool savedOrLoaded = false;
            do
            {
                if (IsTextFilePresent())
                {
                    Console.WriteLine("\nFile Game menu");
                    OutputDirectory();
                    if (mode == "Save") { Console.WriteLine("\nDo you want to save game onto a new save file, or overwrite an existing file if present?\n     1 : Make new save file\n     2 : Overwrite save file"); }
                    else { Console.WriteLine("\nDo you want to load an exisiting save file?\n     1 : No\n     2 : Yes"); }

                    choice = Console.ReadLine();
                    if (choice == "1")
                    {
                        if (mode == "Save") { savedOrLoaded = NewSaveMenu(); }
                        else if (mode == "Load") { savedOrLoaded = true; InitiliseNewGame(); }
                    }
                    else if (choice == "2")
                    { savedOrLoaded = LoadSaveMenu(mode); }
                    else { Console.WriteLine("\nError, incorrect choice was entered."); }
                }
                else
                {
                    if (mode == "Save") { savedOrLoaded = NewSaveMenu(); }
                    else { savedOrLoaded = true; InitiliseNewGame(); }
                }

            } while (savedOrLoaded == false);
            StartGame();
        }
        private bool NewSaveMenu()// New save file menu
        {
            string fileName = GetSaveName();
            if (CheckDirectory(fileName))
            {
                Console.WriteLine("\nFile already found...\nDo you want to overwrite the save?\n     Yes : overwrite\n     No : don't overwrite");
                string choice = Console.ReadLine();
                if (choice.ToLower() == "yes")
                {
                    SaveFile(fileName);
                    return true;
                }
                else if (choice.ToLower() == "no") { Console.WriteLine("\nGoing back to previous menu..."); }
                else { Console.WriteLine("\nError, unknown command... Going back to previous menu..."); }
            }
            else
            {
                SaveFile(fileName);
                return true;
            }
            return false;
        }
        private bool LoadSaveMenu(string mode)// Loading save menu
        {
            string fileName = GetSaveName();
            if (CheckDirectory(fileName))
            {
                if (mode == "Load")
                {
                    if (!LoadFile(fileName)) { return false; }

                    int moveEntered = PlayGame();

                    if (moveEntered == 0 || moveEntered == 999) { }
                    else { FileGameMenu("Save"); }
                }
                else if (mode == "Save") { SaveFile(fileName); }
                return true;
            }
            Console.WriteLine("\nFile was not found... Going back to previous menu...");
            return false;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //File managment features

        private void OutputDirectory()// Output directory
        {
            DirectoryInfo dir = new DirectoryInfo(directory);
            FileInfo[] allTextFiles = dir.GetFiles("*.txt");
            if (allTextFiles.Length == 0) { Console.WriteLine("\nNo save files found..."); }
            else
            {
                Console.WriteLine("\nSave files found...");
                foreach (var file in allTextFiles)
                {
                    Console.WriteLine("    {0}", file.Name);
                }
            }
        }
        private bool IsTextFilePresent()// Check directory for files
        {
            DirectoryInfo dir = new DirectoryInfo(directory);
            FileInfo[] allTextFiles = dir.GetFiles("*.txt");
            if (allTextFiles.Length > 0) { return true; }
            return false;
        }
        private bool CheckDirectory(string fileName)// Check directory
        {
            if (!fileName.EndsWith(".txt")) { fileName += ".txt"; }
            DirectoryInfo dir = new DirectoryInfo(directory);
            FileInfo[] allTextFiles = dir.GetFiles("*.txt");
            foreach (var file in allTextFiles)
            {
                if (file.Name == fileName) { return true; }
            }
            return false;
        }
        private void SaveFile(string fileName)// Actually saving the file
        {
            string filePath = directory + "/" + fileName;
            // Creates new file if file is not found, overwrites any existing file under that name
            using (StreamWriter sw = new StreamWriter(filePath, false))
            {
                sw.WriteLine(connect4BoardColumns);
                sw.WriteLine(connect4BoardRows);
                sw.WriteLine(connectNum);
                for (int columnCount = 0; columnCount < connect4BoardColumns; columnCount++)
                {
                    for (int rowCount = 0; rowCount < connect4BoardRows; rowCount++) { sw.Write(connect4Board[columnCount, rowCount]); }
                    sw.WriteLine();
                }
                sw.WriteLine(playersNumber);
                sw.WriteLine(AINumber);
                for (int count = 0; count < playersNumber; count++) { sw.WriteLine(playerOrder[count]); }
                for (int count = 0; count < playersNumber - AINumber; count++)
                {
                    sw.WriteLine(playerNames[count]);
                    sw.WriteLine(playerMoveChars[count]);
                }
                for (int count = 0; count < AINumber; count++)
                {
                    sw.WriteLine(playerNames[playersNumber - AINumber + count]);
                    sw.WriteLine(playerMoveChars[playersNumber - AINumber + count]);
                }
                sw.WriteLine(movesCurrent);
                sw.WriteLine(lastMove);
                sw.WriteLine(movesMax);
                foreach (int element in columnFillCount) { sw.WriteLine(element); }
                sw.WriteLine(waitAfterAIMove);
                Output.WriteLineColored($"File {fileName} was saved successfully...", ConsoleColor.Green);
            }
        }
        private bool LoadFile(string fileName)// Loading the file contects
        {
            string filePath = directory + "/" + fileName;
            bool corruptFile = false;
            using (StreamReader sr = new StreamReader(filePath))
            {
                try
                {
                    connect4BoardColumns = int.Parse(sr.ReadLine());
                    connect4BoardRows = int.Parse(sr.ReadLine());
                    connect4Board = new char[connect4BoardColumns, connect4BoardRows];
                    connectNum = int.Parse(sr.ReadLine());
                    columnFillCount = new int[connect4BoardColumns];

                    for (int columnCount = 0; columnCount < connect4BoardColumns; columnCount++)
                    {
                        for (int rowCount = 0; rowCount < connect4BoardRows; rowCount++)
                        {
                            connect4Board[columnCount, rowCount] = (char)sr.Read();
                        }
                        sr.ReadLine();
                    }
                    playersNumber = int.Parse(sr.ReadLine());
                    AINumber = int.Parse(sr.ReadLine());
                    playerNames = new string[playersNumber];
                    playerMoveChars = new char[playersNumber];
                    playerOrder = new int[playersNumber];
                    for (int count = 0; count < playersNumber; count++) { playerOrder[count] = int.Parse(sr.ReadLine()); }
                    for (int count = 0; count < playersNumber - AINumber; count++)
                    {
                        playerNames[count] = sr.ReadLine();
                        playerMoveChars[count] = char.Parse(sr.ReadLine());
                    }
                    for (int count = 0; count < AINumber; count++)
                    {
                        playerNames[playersNumber - AINumber + count] = sr.ReadLine();
                        playerMoveChars[playersNumber - AINumber + count] = char.Parse(sr.ReadLine());
                    }
                    movesCurrent = int.Parse(sr.ReadLine());
                    lastMove = int.Parse(sr.ReadLine());
                    movesMax = int.Parse(sr.ReadLine());
                    for (int count = 0; count < connect4BoardColumns; count++) { columnFillCount[count] = int.Parse(sr.ReadLine()); }
                    waitAfterAIMove = bool.Parse(sr.ReadLine());
                }
                catch { Output.WriteLineColored("Error, file save is corrupted, and is now deleted...\nGoing back to previous menu", ConsoleColor.Red); corruptFile = true; }

            }
            if (corruptFile) { File.Delete(filePath); return false; }
            return true;
        }
        private string GetSaveName()// Get name of save file
        {
            string fileName = ValidateInput.Str("Enter name of save file : ");
            if (!fileName.EndsWith(".txt")) { fileName += ".txt"; }
            return fileName;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Gameplay features

        private int AIMove(string AIName, char playerCounter)// AI move
        {
            Console.Write("\nAI named : \"{0}\" move, as ", AIName); Output.WriteMoveChar(playerCounter, ReturnPlayerIndex(playerCounter)); Console.WriteLine();
            int moveColumn = BestMove(playerCounter);
            Console.WriteLine("{0} move ", moveColumn);
            AddMove(playerCounter, moveColumn);
            OuputConnect4Board(moveColumn);
            if (waitAfterAIMove) { Thread.Sleep(waitTime); }
            return moveColumn;
        }
        private int BestMove(char playerCounter)// AI picking best move
        {
            int bestScore = -100, bestMove = 0, score;

            for (int count = 0; count < connect4BoardColumns; count++)
            {
                score = CalculateScoreOfMove(playerCounter, count);
                if (score > bestScore) { bestScore = score; bestMove = count; }
            }

            // If no possible good moves, a random move is played
            while (bestScore == 0)
            {
                bestMove = random.Next(0, connect4BoardColumns);
                if (columnFillCount[bestMove] >= connect4BoardRows) { continue; }
                Output.WriteLineColored("Random move...", ConsoleColor.Magenta);
                break;
            }
            return bestMove;
        }
        private int CalculateScoreOfMove(char playerCounter, int columnMove)// AI calculating move score
        {
            int score = 0;
            int[] tempColumnFillCount = (int[])columnFillCount.Clone();

            if (columnFillCount[columnMove] < connect4BoardRows)
            {
                tempColumnFillCount[columnMove] += 1;

                // Get count of maximum of AI
                int inLineCountMy = GetMaxInLineCounters(playerCounter, columnMove, tempColumnFillCount);
                if (inLineCountMy >= connectNum) { score += 100; }
                if (inLineCountMy == connectNum - 1) { score += 9; }
                if (inLineCountMy == connectNum - 2) { score += 8; }
                if (columnMove == connect4BoardColumns / 2) { score += 4; }

                int[] inLineCountOpponents = new int[playersNumber - 1];
                int addCount = 0;

                for (int count = 0; count < playersNumber; count++)
                {
                    if (playerMoveChars[count] != playerCounter)
                    {
                        inLineCountOpponents[addCount] = GetMaxInLineCounters(playerMoveChars[count], columnMove, tempColumnFillCount);
                        if (inLineCountOpponents[addCount] >= connectNum) { score += 100; }
                        if (inLineCountOpponents[addCount] == connectNum - 1) { score += 18; }
                        if (inLineCountOpponents[addCount] == connectNum - 2) { score += 2; }

                        addCount += 1;
                    }
                }

                tempColumnFillCount[columnMove] += 1;
                if (connect4BoardRows == tempColumnFillCount[columnMove]) { Console.WriteLine("Why"); Thread.Sleep(5000); return score; }
                // Get count of maximum in each other player
                inLineCountOpponents = new int[playersNumber - 1];
                addCount = 0;
                for (int count = 0; count < playersNumber; count++)
                {
                    if (playerMoveChars[count] != playerCounter)
                    {
                        inLineCountOpponents[addCount] = GetMaxInLineCounters(playerMoveChars[count], columnMove, tempColumnFillCount);

                        if (inLineCountOpponents[addCount] >= connectNum) { score -= 5000; Output.WriteLineColored("Opponent Can win", ConsoleColor.Red); Thread.Sleep(5000); }
                        if (inLineCountOpponents[addCount] == connectNum - 1) { score -= 9; }
                        if (inLineCountOpponents[addCount] == connectNum - 2) { score -= 1; }

                        addCount += 1;
                    }
                }
            }
            else { score -= 9999; } //Invalid move score
            return score;
        }
        private int PlayGame()// Main game loop
        {
            int currentMove = -1;
            string winner = null;

            do
            {
                int playerGoIndex = playerOrder[lastMove];
                if (playerNames[playerGoIndex].StartsWith("AI")) { currentMove = AIMove(playerNames[playerGoIndex], playerMoveChars[playerGoIndex]); }
                else
                {
                    currentMove = PlayerMove(playerNames[playerGoIndex], playerMoveChars[playerGoIndex], currentMove);
                    if (currentMove == -999 || currentMove == 999) { return currentMove; }
                }
                lastMove = (lastMove + 1) % playersNumber;
                if (WinningMove(playerMoveChars[playerGoIndex], currentMove)) { winner = playerNames[playerGoIndex]; break; }
                if (movesCurrent == movesMax) { break; }
            } while (movesCurrent < movesMax && winner == null);

            OuputConnect4Board(currentMove);
            if (winner == null) { Output.WriteLineColored("The game resulted in a draw... All players are equally bad!", ConsoleColor.Green); }
            else { Output.WriteLineColored($"Congratulations player : \"{winner}\", you have won!\nThe final number of moves is equal to : {movesCurrent}", ConsoleColor.Green); }
            return 0;
        }
        private bool WinningMove(char playerCounter, int currentColumnMove)// Checks if a player has won
        {
            if (GetMaxInLineCounters(playerCounter, currentColumnMove, columnFillCount) >= connectNum) { return true; }
            return false;
        }
        private int GetMaxInLineCounters(char playerCounter, int currentColumnMove, int[] columnFill)// Count max counters in line
        {
            int countsTogetherCurrent = 1;
            int countsTogetherMax = 1;
            int currentRowMove = connect4BoardRows - columnFill[currentColumnMove];
            int surroundCount;

            for (int winningDirections = 0; winningDirections < 4; winningDirections++)
            {
                surroundCount = winningDirections;
                for (int inLineCount = 0; inLineCount < 2; inLineCount++)
                {
                    for (int lineCount = 1; lineCount < connectNum; lineCount++)
                    {
                        try
                        {
                            if (connect4Board[currentColumnMove + surroundCheckCoordinates[surroundCount, 0] * lineCount, currentRowMove + surroundCheckCoordinates[surroundCount, 1] * lineCount] == playerCounter) { countsTogetherCurrent += 1; }
                            else { break; }
                        }
                        catch (IndexOutOfRangeException) { break; }
                    }
                    surroundCount = 8 - surroundCount;
                }
                // May go over connectNum if more in line together
                if (countsTogetherCurrent > countsTogetherMax) { countsTogetherMax = countsTogetherCurrent; }
                countsTogetherCurrent = 1;
            }
            return countsTogetherMax;
        }
        private int PlayerMove(string playerName, char playerCounter, int currentMove)// Move player
        {
            //OuputConnect4Board(currentMove);
            currentMove = GetPlayerGo(playerName, playerCounter, currentMove);
            if (currentMove == -999 || currentMove == 999) { return currentMove; }
            AddMove(playerCounter, currentMove);
            OuputConnect4Board(currentMove);
            return currentMove;
        }
        private int GetPlayerGo(string playerName, char playerCounter, int currentMove)// Get player move
        {
            int move;
            while (true)
            {
                OuputConnect4Board(currentMove);
                Console.Write($"\nPlayer named : {playerName} as "); Output.WriteMoveChar(playerCounter, ReturnPlayerIndex(playerCounter)); Console.Write(", enter the number of the column you want your counter in.\nOr enter -999 to save and exit, just 999 to just exit.\n");
                move = ValidateInput.Int("Enter move : ");
                if (move == -999 || move == 999) { return move; }
                else if (move < 0 || move > connect4BoardColumns - 1) { Output.WriteLineColored("Illegal move, column entered is not found.", errorColor); continue; }
                else if (columnFillCount[move] == connect4BoardRows) { Output.WriteLineColored("Illegal move, the column is full, can't add more counters to it.", errorColor); continue; }
                return move;
            }
        }
        private void AddMove(char playerCounter, int currentMove)// Add counter to a board
        {
            connect4Board[currentMove, connect4BoardRows - 1 - columnFillCount[currentMove]] = playerCounter;
            columnFillCount[currentMove] += 1;
            movesCurrent += 1;
        }
        private void OuputConnect4Board(int currentColumnMove)// Outputs the board onto the screen
        {
            int currentRowMove = -1;
            if (currentColumnMove < 0 || currentColumnMove > connect4BoardColumns - 1) { currentColumnMove = -1; }
            if (currentColumnMove != -1) { currentRowMove = connect4BoardRows - columnFillCount[currentColumnMove]; }

            Console.WriteLine("\nConnect 4 Board Game");
            for (int count = 0; count < playersNumber; count++)
            {
                int playerIndex = playerOrder[count];
                Console.Write("Player counter : "); Output.WriteMoveChar(playerMoveChars[playerIndex], playerIndex); Console.WriteLine(" as player {0}", playerNames[playerIndex]);
            }
            Console.WriteLine("Number of current moves : {0}", movesCurrent);
            Console.WriteLine("Number of counters needed to be in line to win : {0}", connectNum);

            Console.Write("   ");
            for (int columnCount = 0; columnCount < connect4BoardColumns; columnCount++)
            {
                if (columnCount < 10) { Console.Write(" {0} ", columnCount); Console.Write(' '); }
                else { Console.Write(" {0} ", columnCount); }
            }
            Console.WriteLine();

            for (int rowCount = 0; rowCount < connect4BoardRows; rowCount++)
            {
                Console.Write("  |");
                for (int columnCount = 0; columnCount < connect4BoardColumns; columnCount++) { Console.Write("---|"); }
                Console.WriteLine();
                if (rowCount < 10) { Console.Write(" {0}|", rowCount); }
                else { Console.Write("{0}|", rowCount); }

                for (int columnCount = 0; columnCount < connect4BoardColumns; columnCount++)
                {
                    char character = connect4Board[columnCount, rowCount];
                    Console.Write(' ');
                    if (rowCount == currentRowMove && columnCount == currentColumnMove) { Output.WriteCurrentMoveChar(character); }
                    else
                    {
                        if (ReturnPlayerIndex(character) == -1) { Console.Write(' '); }
                        else { Output.WriteMoveChar(character, ReturnPlayerIndex(character)); }
                    }
                    Console.Write(" |");
                }
                Console.Write(rowCount);
                Console.WriteLine();
            }
            Console.Write("  |");
            for (int columnCount = 0; columnCount < connect4BoardColumns; columnCount++) { Console.Write("---|"); }
            Console.WriteLine();
            Console.Write("   ");
            for (int columnCount = 0; columnCount < connect4BoardColumns; columnCount++)
            {
                if (columnCount < 10) { Console.Write(" {0} ", columnCount); Console.Write(' '); }
                else if (columnCount < 100) { Console.Write(" {0} ", columnCount); }
                else { Console.Write(" {0}", columnCount); }
            }
            Console.WriteLine();
        }
        private int ReturnPlayerIndex(char playerCounter)// Return index of player in playersArray
        {
            int index = -1;
            for (int count = 0; count < playersNumber; count++) { if (playerCounter == playerMoveChars[count]) { index = count; break; } }
            return index;
        }
    }
}