using System;
using System.Collections.Generic;

namespace Connect4Ai
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            new Game();
            
            Console.WriteLine("Done");
        }
    }
    
    class Game
    {
        /**
         * Class - Game:
         * A connect four game with options for playing a game, modiying the board, and adding AI players.
         */

        private Board _board;
        private Player _player1, _player2;
        private bool _clear;

        public Game()
        {
            _board = new Board();
            _clear = true;
            Menu();
        }

        private void Menu()
        {
            while (true)
            {
                Console.WriteLine("Please select an option:");
                Console.WriteLine("1 - Play");
                Console.WriteLine("2 - Enter board position");
                Console.WriteLine("3 - Toggle console history");
                Console.WriteLine("4 - Print move list from last game");
                Console.WriteLine("5 - Exit");
                var input = Console.ReadLine() ?? "1";

                switch (input)
                {
                    case "1":
                        if (_board.IsGameOver())
                        {
                            _board = new Board();
                        }
                        PlayerSetup();
                        Run();
                        break;
                    case "2":
                        _board = new Board();
                        Console.WriteLine("Please enter the desired board through a sequence of columns (moves 0-" + _board.Width + "):");
                        var moves = Console.ReadLine();
                        if (!string.IsNullOrEmpty(moves))
                        {
                            var cArray = moves.ToCharArray();
                            var success = false;
                            foreach (var move in cArray)
                            {
                                try
                                {
                                    success = !_board.MakeMove(int.Parse(move.ToString()));
                                }
                                catch(FormatException e)
                                {
                                    Console.WriteLine("Error parsing input string.");
                                    break;
                                }
                                
                                if (!success)
                                {
                                    Console.WriteLine("Error importing board, game is over.");
                                    break;
                                }
                            }

                            if (success)
                            {
                                Console.WriteLine("New board imported.");
                                _board.Print();
                            }
                        }
                        break;
                    case "3":
                        _clear = !_clear;
                        if (_clear)
                        {
                            Console.WriteLine("History turned off");
                        }
                        else
                        {
                            Console.WriteLine("History turned on");
                        }
                        break;
                    case "4":
                        PrintMoveList();
                        break;
                    case "5":
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private void PlayerSetup()
        {
            // Ask the user if Player1 is an AI, if an AI, they may specify a depth, but a default of 4 is used.
            Console.WriteLine("Is player 1 an AI? Yes (y), No (n). If yes, then you may specify a difficulty (depth - integer, default = 5) separated by a space.");
            var input = Console.ReadLine() ?? "n";
            var isPlayer1Ai = input.Length > 0 ? input.ToCharArray()[0] == 'y' : false;
            var player1AiDepth = input.Length > 2 ? int.Parse(input[2..]) : 5;
            _player1 = !isPlayer1Ai ? new Player(1) : new Ai(1, player1AiDepth);

            // Ask the user if Player2 is an AI, if an AI, they may specify a depth, but a default of 4 is used.
            Console.WriteLine("Is player 2 an AI? Yes (y), No (n). If yes, then you may specify a difficulty (depth - integer, default = 5) separated by a space.");
            input = Console.ReadLine() ?? "n";
            var isPlayer2Ai = input.Length > 0 ? input.ToCharArray()[0] == 'y' : false;
            var player2AiDepth = input.Length > 2 ? int.Parse(input[2..]) : 5;
            _player2 = !isPlayer2Ai ? new Player(-1) : new Ai(-1, player2AiDepth);
        }

        private void Run()
        {
            _board.Print();

            while (true)
            {
                // If Player1's turn, let them make a move, if they won the game, stop.
                if (_board.Turn % 2 == 0 && _board.MakeMove(_player1.GetMove(_board)))
                {
                    break;
                }

                _board.Print(_clear);

                // If Player2's turn, let them make a move, if they won the game, stop.
                if (_board.Turn % 2 == 1 && _board.MakeMove(_player2.GetMove(_board)))
                {
                    break;
                }

                _board.Print(_clear);
            }

            _board.Print(_clear);
            Console.WriteLine(_board.GetWinner() + " is the winner after " + _board.Turn + " turns.");
        }

        private void PrintMoveList()
        {
            foreach (var move in _board.MoveList)
            {
                Console.Write(move);
            }
            Console.WriteLine();
        }
    }

    class Board
    {
        /**  
         * The board is represented by a 2D array of integers, either 0, 1, or -1.
         * This allows for a winning sequence to have a sum of either 4 or -4.
         * Player 1 will have pieces represented as a 1, Player 2 as a -1, and
         * an empty space will be 0. When printed, Player1 will be 'X', Player2
         * will be 'O', and an empty space will be '-'.
         */
        public readonly int[,] Array;
        public readonly int Width, Height;
        public bool FirstTurn = true;

        private static readonly char SymbolEmpty = '-';
        private static readonly char SymbolPlayer1 = 'X';
        private static readonly char SymbolPlayer2 = 'O';

        private static readonly Dictionary<int, char> Symbols = new Dictionary<int, char>()
        {
            {0, SymbolEmpty},
            {1, SymbolPlayer1},
            {-1, SymbolPlayer2}
        };

        public int WinningSum;
        public int Turn;
        public List<int> MoveList;

        public Board(int width = 7, int height = 6)
        {
            Width = width;
            Height = height;
            Array = new int[height, width];
            Turn = 0;
            MoveList = new List<int>();
        }

        /* An additional constructor to allow for a variation of the board
         * to be created but with a single move added.
         */
        public Board(Board board, int move)
        {
            Width = board.Width;
            Height = board.Height;
            Array = new int[Height, Width];
            Turn = board.Turn;
            MoveList = new List<int>();
            foreach (var iMove in board.MoveList)
            {
                MoveList.Add(iMove);
            }

            ApplyBoard(board.Array);
            MakeMove(move);
        }

        public void Print(bool clear = false)
        {
            if (clear)
            {
                Console.Clear();
            }

            Console.WriteLine("|0|1|2|3|4|5|6|");
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    Console.Write("|" + Symbols.GetValueOrDefault(Array[y,x]));
                }
                Console.WriteLine("|");
            }
        }

        public void ApplyBoard(int[,] array)
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    Array[y, x] = array[y, x];
                }
            }
        }

        public List<int> GetValidMoves()
        {
            var validMoves = new List<int>();

            for (var move = 0; move < Width; move++)
            {
                // If there is no piece at the top of the column then it's a valid move.
                if (Array[0, move] == Symbols.GetValueOrDefault(SymbolEmpty))
                {
                    validMoves.Add(move);
                }
            }

            return validMoves;
        }

        public bool MakeMove(int moveX)
        {
            int moveY;

            // Find the lowest row (moveY) in the specified column (moveX) to place a piece.
            for (moveY = 0; moveY < Height; moveY++)
            {
                if (Symbols.GetValueOrDefault(Array[moveY, moveX]) != SymbolEmpty)
                {
                    break;
                }
            }

            // Is the Column full?
            if (moveY == 0)
            {
                return true;
            }

            // Update the board with the move.
            Array[moveY - 1, moveX] = (Turn % 2 == 0) ? 1 : -1;

            FirstTurn = false;
            Turn++;
            MoveList.Add(moveX);

            return IsGameOver();
        }

        public bool IsGameOver()
        {
            if (Turn == 42)
            {
                return true;
            }

            // Rows
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x + 3 < Width; x++)
                {
                    WinningSum = Array[y, x] + Array[y, x + 1] + Array[y, x + 2] + Array[y, x + 3];
                    if (Convert.ToInt16(Math.Abs(WinningSum)) == 4)
                    {
                        return true;
                    }
                }
            }

            // Columns
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y + 3 < Height; y++)
                {
                    WinningSum = Array[y, x] + Array[y + 1, x] + Array[y + 2, x] + Array[y + 3, x];
                    if (Convert.ToInt16(Math.Abs(WinningSum)) == 4)
                    {
                        return true;
                    }
                }
            }

            // '\' Diagonals
            for (var x = 0; x + 3 < Width; x++)
            {
                for (var y = 0; y + 3 < Height; y++)
                {
                    WinningSum = Array[y, x] + Array[y + 1, x + 1] + Array[y + 2, x + 2] + Array[y + 3, x + 3];
                    if (Convert.ToInt16(Math.Abs(WinningSum)) == 4)
                    {
                        return true;
                    }
                }
            }

            // '/' Diagonals
            for (var x = 0; x + 3 < Width; x++)
            {
                for (var y = 0; y + 3 < Height; y++)
                {
                    WinningSum = Array[y + 3, x] + Array[y + 2, x + 1] + Array[y + 1, x + 2] + Array[y, x + 3];
                    if (Convert.ToInt16(Math.Abs(WinningSum)) == 4)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public string GetWinner()
        {
            return WinningSum switch
            {
                4  => "Player1",
                -4 => "Player2",
                _  => "No one",
            };
        }
    }
    
    class Player
    {
        /**
         * A player of the game, will either be Player1 or Player2.
         * PlayerNumber will either be 1, or -1 respectively.
         * Is able to get a read a move from input.
         */

        public readonly int PlayerNumber;
        public Player(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public virtual int GetMove(Board board)
        {
            var move = -1;

            while (!board.GetValidMoves().Contains(move))
            {
                Console.WriteLine("Player"+ (PlayerNumber == 1 ? "1" : "2") + " Move: ");
                var input = Console.ReadLine();
                if (input == null || input.Length == 0)
                {
                    continue;
                }
                move = int.Parse(input);
            }

            return move;
        }
    }
    
    class Ai : Player
    {
        /**
         * A variation of a Player with modified functions to generate moves without user input.
         * The get move function will return a move to the board as if a user has entered it.
         * Includes an evaluation function as well as a helper function to determine 
         * how favourable a board position is.
         * Also includes implementations of the MiniMax function to search 
         * board trees for the best possible move.
         */

        private readonly int _aiDepth;
        private const int WinningGroupSize = 4;
        public Ai(int playerNumber, int depth) : base(playerNumber)
        {
            _aiDepth = depth;
        }

        // Returns a move as if a user has ented it.
        public override int GetMove(Board board)
        {
            // If the AI plays the first turn, play in the centre.
            if (board.FirstTurn)
            {
                return 3;
            }

            var moves = new List<KeyValuePair<int, int>>();
            var bestMove = -1;
            var bestMoveValue = -int.MaxValue;
            var validMoves = board.GetValidMoves();

            // Sort the moves by the proximity to the centre column.
            validMoves.Sort((x, y) => Math.Abs((board.Width / 2) - x).CompareTo(Math.Abs((board.Width / 2) - y)));

            foreach (var move in validMoves)
            {
                Console.WriteLine("Checking move " + move);

                // Using alpha-beta pruning for better performance. See MiniMax function definitions for regular algorithm implementation.

                // Regular MiniMax.
                // var value = MiniMax(new Board(board, move), _aiDepth - 1, false);

                // MiniMax with Alpha-Beta pruning.
                var value = MiniMax(new Board(board, move), _aiDepth - 1, -int.MaxValue, int.MaxValue, false);

                Console.WriteLine("Value is " + value);
                moves.Add(new KeyValuePair<int, int>(move, value));
            }

            // Sort the moves by the highest value.
            moves.Sort((x,y) => y.Value.CompareTo(x.Value));

            /* Take a subset with all the moves with the highest value.
             * If there are multiple moves of the same value, take a random one.
             */
            moves = moves.FindAll(x => x.Value == moves[0].Value);
            if (moves.Count > 1)
            {
                Random random = new Random();
                var best = moves[random.Next(moves.Count)];
                bestMove = best.Key;
                bestMoveValue = best.Value;
            }
            else if (moves.Count == 1)
            {
                bestMove = moves[0].Key;
                bestMoveValue = moves[0].Value;
            }

            if (bestMove == -1)
            {
                Console.WriteLine("AI failed to determine a best move.");
                if (board.GetValidMoves().Count == 0)
                {
                    Console.WriteLine("No moves available.");
                    return -1;
                }

                // If the AI failed to find a move, return a random one.
                Random random = new Random();
                bestMove = board.GetValidMoves()[random.Next(board.GetValidMoves().Count - 1)];
            }

            Console.WriteLine("Best move was " + bestMove + ", with value of " + bestMoveValue);
            return bestMove;
        }

        // Regular MiniMax algorithm implementation.
        public int MiniMax(Board board, int depth, bool isMaxPlayer)
        {
            //Console.WriteLine("Minimax:" + depth + ", " + isMaxPlayer);
            var validMoves = board.GetValidMoves();

            if (depth == 0 || board.IsGameOver() || validMoves.Count == 0)
            {
                return EvaluateBoard(board);
            }

            if (isMaxPlayer)
            {
                var value = -int.MaxValue;

                foreach (var move in validMoves)
                {
                    value = Math.Max(value, MiniMax(new Board(board, move), depth - 1, false));
                }

                return value;
            }
            else
            {
                var value = int.MaxValue;

                foreach (var move in validMoves)
                {
                    value = Math.Min(value, MiniMax(new Board(board, move), depth - 1, true));
                }

                return value;
            }
            
        }

        // MiniMax algorithm with alpha-beta pruning.
        public int MiniMax(Board board, int depth, int a, int b, bool isMaxPlayer)
        {
            //Console.WriteLine("Minimax:" + depth + ", " + isMaxPlayer);
            var validMoves = board.GetValidMoves();

            if (depth == 0 || board.IsGameOver() || validMoves.Count == 0)
            {
                return EvaluateBoard(board);
            }

            if (isMaxPlayer)
            {
                var value = -int.MaxValue;

                foreach (var move in validMoves)
                {
                    value = Math.Max(value, MiniMax(new Board(board, move), depth - 1, a, b, false));
                    if (value >= b)
                    {
                        break;
                    }
                    a = Math.Max(a, value);
                }

                return value;
            }
            else
            {
                var value = int.MaxValue;

                foreach (var move in validMoves)
                {
                    value = Math.Min(value, MiniMax(new Board(board, move), depth - 1, a, b, true));
                    if (value <= a)
                    {
                        break;
                    }
                    b = Math.Min(b, value);
                }

                return value;
            }

        }

        public int EvaluateBoard(Board board)
        {
            var score = 0;

            if (board.IsGameOver())
            {
                return int.MaxValue * (board.WinningSum == PlayerNumber * WinningGroupSize ? 1 : -1);
            }

            // Groups of 4.
            //score += 100000 * NumGroups(board, PlayerNumber * WinningGroupSize);
            //score -= 100000 * NumGroups(board, PlayerNumber * -WinningGroupSize);

            // Groups of 3.
            score += 100 * NumGroups(board, PlayerNumber * (WinningGroupSize - 1));
            score -= 100 * NumGroups(board, PlayerNumber * -(WinningGroupSize - 1));

            // Groups of 2
            score += NumGroups(board, PlayerNumber * (WinningGroupSize - 2));
            score -= NumGroups(board, PlayerNumber * -(WinningGroupSize - 2));

            return score - board.Turn;
        } 

        public static int NumGroups(Board board, int size)
        {
            var total = 0;

            // Rows
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x + WinningGroupSize - 1 < board.Width; x++)
                {
                    var sum = 0;

                    for (int i = 0; i < WinningGroupSize - 1; i++)
                    {
                        sum += board.Array[y, x + i];
                    }

                    total += (sum == size ? 1 : 0);
                }
            }

            // Columns
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y + WinningGroupSize - 1 < board.Height; y++)
                {
                    var sum = 0;

                    for (int i = 0; i < WinningGroupSize - 1; i++)
                    {
                        sum += board.Array[y + i, x];
                    }

                    total += (sum == size ? 1 : 0);
                }
            }

            // '\' Diagonals
            for (var x = 0; x + WinningGroupSize - 1 < board.Width; x++)
            {
                for (var y = 0; y + WinningGroupSize - 1 < board.Height; y++)
                {
                    var sum = 0;

                    for (int i = 0; i < WinningGroupSize - 1; i++)
                    {
                        sum += board.Array[y + i, x + i];
                    }

                    total += (sum == size ? 1 : 0);
                }
            }

            // '/' Diagonals
            for (var x = 0; x + WinningGroupSize - 1 < board.Width; x++)
            {
                for (var y = 0; y + WinningGroupSize - 1 < board.Height; y++)
                {
                    var sum = 0;

                    for (int i = 0; i < WinningGroupSize - 1; i++)
                    {
                        sum += board.Array[y + (WinningGroupSize - 1) - i, x + i];
                    }

                    total += (sum == size ? 1 : 0);
                }
            }

            return total;
        }
    }
}
