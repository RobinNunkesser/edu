namespace Shared.Application;

internal enum NQueensAlgorithmKind
{
    HillClimbing,
    SimulatedAnnealing,
    GeneticAlgorithm
}

internal enum NQueensStepKind
{
    Initial,
    Move,
    RejectedMove,
    Generation,
    Solved,
    Stopped
}

internal sealed record NQueensStepModel(
    int StepNumber,
    NQueensStepKind Kind,
    int[] QueenRows,
    int? CurrentColumn,
    int? CurrentRow,
    IReadOnlyList<int> ConflictedColumns,
    int ConflictCount,
    int? Generation,
    double? Temperature,
    double? Fitness,
    bool AcceptedMove,
    bool Solved);

internal sealed record NQueensAlgorithmSimulation(
    NQueensAlgorithmKind Algorithm,
    int BoardSize,
    int[] InitialQueenRows,
    IReadOnlyList<NQueensStepModel> Steps,
    bool Solved,
    int FinalConflictCount,
    int[] FinalQueenRows);

internal static class NQueensSimulation
{
    private static readonly int[] HillClimbingInitialBoard = [5, 7, 0, 1, 1, 7, 7, 2];
    private static readonly int[] AnnealingInitialBoard = [0, 4, 7, 5, 2, 6, 1, 3];

    public static IReadOnlyList<NQueensAlgorithmSimulation> BuildAll(int boardSize = 8)
    {
        return [
            SimulateHillClimbing(boardSize),
            SimulateSimulatedAnnealing(boardSize),
            SimulateGeneticAlgorithm(boardSize)
        ];
    }

    private static NQueensAlgorithmSimulation SimulateHillClimbing(int boardSize)
    {
        var current = CreateBoard(boardSize, HillClimbingInitialBoard);
        var steps = new List<NQueensStepModel>
        {
            CreateStep(1, NQueensStepKind.Initial, current, null, null, null, null, Fitness(current), true, false)
        };

        var stepNumber = 2;
        while (true)
        {
            var currentConflicts = CountConflicts(current);
            if (currentConflicts == 0)
            {
                steps.Add(CreateStep(stepNumber, NQueensStepKind.Solved, current, null, null, null, null, Fitness(current), true, true));
                return new NQueensAlgorithmSimulation(NQueensAlgorithmKind.HillClimbing, boardSize, CreateBoard(boardSize, HillClimbingInitialBoard), steps, true, 0, current);
            }

            var bestMove = FindBestHillClimbingMove(current);
            if (bestMove is null || bestMove.ConflictCount >= currentConflicts)
            {
                steps.Add(CreateStep(stepNumber, NQueensStepKind.Stopped, current, null, null, null, null, Fitness(current), true, false));
                return new NQueensAlgorithmSimulation(NQueensAlgorithmKind.HillClimbing, boardSize, CreateBoard(boardSize, HillClimbingInitialBoard), steps, false, currentConflicts, current);
            }

            current[bestMove.Column] = bestMove.Row;
            steps.Add(CreateStep(stepNumber, NQueensStepKind.Move, current, bestMove.Column, bestMove.Row, null, null, Fitness(current), true, CountConflicts(current) == 0));
            stepNumber++;
        }
    }

    private static NQueensAlgorithmSimulation SimulateSimulatedAnnealing(int boardSize)
    {
        var current = CreateBoard(boardSize, AnnealingInitialBoard);
        var steps = new List<NQueensStepModel>
        {
            CreateStep(1, NQueensStepKind.Initial, current, null, null, null, 18.0, Fitness(current), true, false)
        };

        var random = new Random(73);
        const double initialTemperature = 18.0;
        const double coolingFactor = 0.92;
        const double minimumTemperature = 0.2;
        const int maxIterations = 30;

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var currentConflicts = CountConflicts(current);
            if (currentConflicts == 0)
            {
                steps.Add(CreateStep(steps.Count + 1, NQueensStepKind.Solved, current, null, null, null, TemperatureAt(iteration, initialTemperature, coolingFactor), Fitness(current), true, true));
                return new NQueensAlgorithmSimulation(NQueensAlgorithmKind.SimulatedAnnealing, boardSize, CreateBoard(boardSize, AnnealingInitialBoard), steps, true, 0, current);
            }

            var temperature = TemperatureAt(iteration, initialTemperature, coolingFactor);
            if (temperature < minimumTemperature)
            {
                break;
            }

            var candidateColumn = random.Next(boardSize);
            var candidateRow = random.Next(boardSize - 1);
            if (candidateRow >= current[candidateColumn])
            {
                candidateRow++;
            }

            var candidate = CreateBoard(boardSize, current);
            candidate[candidateColumn] = candidateRow;

            var candidateConflicts = CountConflicts(candidate);
            var accepted = candidateConflicts <= currentConflicts
                || Math.Exp((currentConflicts - candidateConflicts) / temperature) > random.NextDouble();

            if (accepted)
            {
                current = candidate;
                steps.Add(CreateStep(steps.Count + 1, NQueensStepKind.Move, current, candidateColumn, candidateRow, null, temperature, Fitness(current), true, CountConflicts(current) == 0));
            }
            else
            {
                steps.Add(CreateStep(steps.Count + 1, NQueensStepKind.RejectedMove, current, candidateColumn, candidateRow, null, temperature, Fitness(current), false, false));
            }
        }

        var solved = CountConflicts(current) == 0;
        steps.Add(CreateStep(steps.Count + 1, solved ? NQueensStepKind.Solved : NQueensStepKind.Stopped, current, null, null, null, minimumTemperature, Fitness(current), true, solved));
        return new NQueensAlgorithmSimulation(NQueensAlgorithmKind.SimulatedAnnealing, boardSize, CreateBoard(boardSize, AnnealingInitialBoard), steps, solved, CountConflicts(current), current);
    }

    private static NQueensAlgorithmSimulation SimulateGeneticAlgorithm(int boardSize)
    {
        const int populationSize = 24;
        const int maxGenerations = 30;
        const double mutationRate = 0.14;
        var random = new Random(113);
        var population = Enumerable.Range(0, populationSize)
            .Select(_ => Enumerable.Range(0, boardSize).Select(__ => random.Next(boardSize)).ToArray())
            .ToList();

        var initialBest = population.OrderBy(CountConflicts).First();
        var steps = new List<NQueensStepModel>
        {
            CreateStep(1, NQueensStepKind.Initial, initialBest, null, null, 0, null, Fitness(initialBest), true, CountConflicts(initialBest) == 0)
        };

        for (var generation = 1; generation <= maxGenerations; generation++)
        {
            var best = population.OrderBy(CountConflicts).First();
            var bestConflicts = CountConflicts(best);
            var solved = bestConflicts == 0;

            steps.Add(CreateStep(steps.Count + 1, solved ? NQueensStepKind.Solved : NQueensStepKind.Generation, best, null, null, generation, null, Fitness(best), true, solved));
            if (solved)
            {
                return new NQueensAlgorithmSimulation(NQueensAlgorithmKind.GeneticAlgorithm, boardSize, CreateBoard(boardSize, initialBest), steps, true, 0, best);
            }

            var nextPopulation = new List<int[]> { CreateBoard(boardSize, best) };
            while (nextPopulation.Count < populationSize)
            {
                var parentA = TournamentSelect(population, random);
                var parentB = TournamentSelect(population, random);
                var child = Crossover(parentA, parentB, random);
                Mutate(child, boardSize, mutationRate, random);
                nextPopulation.Add(child);
            }

            population = nextPopulation;
        }

        var finalBest = population.OrderBy(CountConflicts).First();
        steps.Add(CreateStep(steps.Count + 1, NQueensStepKind.Stopped, finalBest, null, null, maxGenerations + 1, null, Fitness(finalBest), true, false));
        return new NQueensAlgorithmSimulation(NQueensAlgorithmKind.GeneticAlgorithm, boardSize, CreateBoard(boardSize, initialBest), steps, false, CountConflicts(finalBest), finalBest);
    }

    private static HillClimbingMove? FindBestHillClimbingMove(int[] board)
    {
        HillClimbingMove? bestMove = null;
        for (var column = 0; column < board.Length; column++)
        {
            for (var row = 0; row < board.Length; row++)
            {
                if (row == board[column])
                {
                    continue;
                }

                var candidate = CreateBoard(board.Length, board);
                candidate[column] = row;
                var conflicts = CountConflicts(candidate);
                if (bestMove is null
                    || conflicts < bestMove.ConflictCount
                    || (conflicts == bestMove.ConflictCount && (column < bestMove.Column || (column == bestMove.Column && row < bestMove.Row))))
                {
                    bestMove = new HillClimbingMove(column, row, conflicts);
                }
            }
        }

        return bestMove;
    }

    private static int[] TournamentSelect(IReadOnlyList<int[]> population, Random random)
    {
        return Enumerable.Range(0, 3)
            .Select(_ => population[random.Next(population.Count)])
            .OrderBy(CountConflicts)
            .First();
    }

    private static int[] Crossover(int[] parentA, int[] parentB, Random random)
    {
        var crossoverPoint = random.Next(1, parentA.Length - 1);
        var child = new int[parentA.Length];
        for (var index = 0; index < parentA.Length; index++)
        {
            child[index] = index < crossoverPoint ? parentA[index] : parentB[index];
        }

        return child;
    }

    private static void Mutate(int[] board, int boardSize, double mutationRate, Random random)
    {
        for (var column = 0; column < board.Length; column++)
        {
            if (random.NextDouble() < mutationRate)
            {
                board[column] = random.Next(boardSize);
            }
        }
    }

    private static NQueensStepModel CreateStep(
        int stepNumber,
        NQueensStepKind kind,
        int[] board,
        int? currentColumn,
        int? currentRow,
        int? generation,
        double? temperature,
        double? fitness,
        bool acceptedMove,
        bool solved)
    {
        var queenRows = CreateBoard(board.Length, board);
        return new NQueensStepModel(
            stepNumber,
            kind,
            queenRows,
            currentColumn,
            currentRow,
            GetConflictedColumns(queenRows),
            CountConflicts(queenRows),
            generation,
            temperature,
            fitness,
            acceptedMove,
            solved);
    }

    private static int[] CreateBoard(int boardSize, IReadOnlyList<int> source)
    {
        var board = new int[boardSize];
        for (var index = 0; index < boardSize; index++)
        {
            board[index] = source[index];
        }

        return board;
    }

    private static IReadOnlyList<int> GetConflictedColumns(IReadOnlyList<int> board)
    {
        var conflicted = new HashSet<int>();
        for (var leftColumn = 0; leftColumn < board.Count; leftColumn++)
        {
            for (var rightColumn = leftColumn + 1; rightColumn < board.Count; rightColumn++)
            {
                if (Conflicts(leftColumn, board[leftColumn], rightColumn, board[rightColumn]))
                {
                    conflicted.Add(leftColumn);
                    conflicted.Add(rightColumn);
                }
            }
        }

        return conflicted.OrderBy(value => value).ToArray();
    }

    private static int CountConflicts(IReadOnlyList<int> board)
    {
        var conflicts = 0;
        for (var leftColumn = 0; leftColumn < board.Count; leftColumn++)
        {
            for (var rightColumn = leftColumn + 1; rightColumn < board.Count; rightColumn++)
            {
                if (Conflicts(leftColumn, board[leftColumn], rightColumn, board[rightColumn]))
                {
                    conflicts++;
                }
            }
        }

        return conflicts;
    }

    private static bool Conflicts(int leftColumn, int leftRow, int rightColumn, int rightRow)
        => leftRow == rightRow || Math.Abs(leftRow - rightRow) == Math.Abs(leftColumn - rightColumn);

    private static double Fitness(IReadOnlyList<int> board)
    {
        var maxPairs = (board.Count * (board.Count - 1)) / 2.0;
        return (maxPairs - CountConflicts(board)) / maxPairs;
    }

    private static double TemperatureAt(int iteration, double initialTemperature, double coolingFactor)
        => initialTemperature * Math.Pow(coolingFactor, iteration);

    private sealed record HillClimbingMove(int Column, int Row, int ConflictCount);
}