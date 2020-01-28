namespace SnakeAndLadder.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            GameController gameController = new GameController();

            while (true)
            {
                Console.WriteLine("Want to add a player? (y/n)");
                var key = Console.ReadKey();
                if (key.KeyChar == 'n')
                {
                    break;
                }

                Console.WriteLine("Enter players name:");
                var name = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(name)) 
                {
                    Console.WriteLine("Can't take empty player name");
                    continue;
                }

                gameController.Board.AddPlayer(new Player(name));
            }

            Console.WriteLine("Press any key to start the game");
            Console.ReadLine();

            gameController.PlayGame();            
        }
    }

    internal delegate void PlayerPositionChangeHandler(int position);
    internal delegate void PlayerMovementHandler(int currentPosition, PlayerPositionChangeHandler positionChangeHandler);
    internal delegate bool GameCompletionHandler(int position);

    class Player
    {
        private int Position { get; set; }
        public event PlayerMovementHandler OnMove;
        public event GameCompletionHandler OnReachingEndOfGame;

        public string Name { get; private set; }

        public bool HasCompleted { get; private set; }

        public Player(string name)
        {
            Name = name;
            Position = 0;
        }

        public void Play()
        {
            OnMove(Position, ChangePosition);
            HasCompleted = OnReachingEndOfGame(Position);

            if (HasCompleted)
            {
                Console.WriteLine($"Hurray! {Name} has reached the end.");
            }
        }

        public void ChangePosition(int newPosition)
        {
            Console.WriteLine($"{Name}'s position changed from {Position} to {newPosition}");
            Position = newPosition;
        }
    }

    class Board
    {
        List<Player> _players = null;
        Dictionary<int, int> _snakeAndLadderPositions = null;
        readonly int _endOfGamePosition = 100;

        public Board()
        {
            _players = new List<Player>();
            _snakeAndLadderPositions = new Dictionary<int, int>
            {
                { 4, 23 }, //ladder
                { 97, 49 }, //snake
                { 82, 65 },  //snake
                { 43, 21 },  //snake
                { 59, 8 },  //snake
                { 91, 37 }, //snake
                { 86, 90 }, //ladder 
                { 41, 62 }, //ladder
                { 28, 53 }, //ladder
                { 62, 98 } //ladder
            };
        }

        public void AddPlayer(Player player)
        {
            player.OnReachingEndOfGame += CompletionCheck;
            _players.Add(player);
            Console.WriteLine($"New player added: {player.Name}");
        }

        private bool CompletionCheck(int position)
        {
            return position == _endOfGamePosition;
        }

        public IList<Player> Players
        {
            get
            {
                return _players;
            }
        }

        internal void Clean()
        {
            _players = null;
            _snakeAndLadderPositions = null;
        }

        internal int CalculateNewPosition(int currentPosition, int diceValue)
        {
            int newPosition = (currentPosition + diceValue);

            if (newPosition == _endOfGamePosition)
            {
                return _endOfGamePosition;
            }
            else if (newPosition > _endOfGamePosition)
            {
                return currentPosition;
            }
            else if (_snakeAndLadderPositions.ContainsKey(newPosition))
            {
                return _snakeAndLadderPositions[newPosition];
            }
            return newPosition;
        }
    }

    class GameController
    {
        public GameController()
        {
            if (Board == null)
            {
                Board = new Board();
            }
        }

        public Board Board { get; } = null;

        public void PlayGame()
        {
            var players = Board.Players;

            Setup(players);

            Console.WriteLine("Starting the game");

            while (!players.Any(p => p.HasCompleted))
            {
                foreach (var player in players.Where(p => !p.HasCompleted))
                {
                    Console.WriteLine($"{player.Name}'s turn: (press enter to roll the dice)");
                    while (Console.ReadKey().Key != ConsoleKey.Enter) ;
                    player.Play();
                }
            }
        }

        public void EndGame()
        {
            Console.WriteLine("Game completed!!");
            Board.Clean();
        }

        private void Setup(IList<Player> players)
        {
            foreach (var p in players)
            {
                p.OnMove += NextMove;
            }
        }

        private void NextMove(int currentPosition, PlayerPositionChangeHandler positionChangeHandler)
        {
            int diceValue = RollDice();
            Console.WriteLine($"Dice value: {diceValue}");
            int newPosition = Board.CalculateNewPosition(currentPosition, diceValue);
            positionChangeHandler(newPosition);
        }

        int RollDice()
        {
            return new Random().Next(1, 6);
        }
    }
}
