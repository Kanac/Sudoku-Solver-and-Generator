using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SudokuPuzzle
{
    public class SudokuPuzzle
    {
        private List<List<int>> _Cells = new List<List<int>>();     //Initial sudoku puzzle given (don't change it)
        private List<List<List<int>>> _SolvedCells = new List<List<List<int>>>();
        private int _Length;
        private int _BoxSize { get { return (int)Math.Sqrt(_Length); } }

        public SudokuPuzzle(int length, string initCells)
        {
            if (initCells.Length != length * length)
                throw new ArgumentException();

            this._Length = length;

            foreach (char c in initCells)   //Populates the given sudoku with candidates, either given or 1-9
            {
                if (c.ToString().Equals("."))   //Empty boxes, candidates of 1-9  
                    _Cells.Add(new List<int>(Enumerable.Range(1, _Length)));
                else
                    _Cells.Add(new List<int> { (int)Char.GetNumericValue(c) });     //Given value
            }
        }

        public SudokuPuzzle(int length)
        {
            this._Length = length;
        }

        private bool checkIfSolved(List<List<int>> cells)
        {
            foreach (var cell in cells)
            {
                if (cell.Count() != 1)
                    return false;
            }

            return true;
        }

        private void SolveSudoku(int numSolutions)
        {
            var workingCells = new List<List<int>>();
            CopyCells(workingCells, _Cells);    //Keep the starting puzzle and working solution separate

            EliminateCandidates(workingCells, numSolutions);
        }


        private void EliminateCandidates(List<List<int>> cells, int numSolutions)
        {
            for (int cell = 0; cell < _Length * _Length; cell++)
            {
                if (cells[cell].Count > 1)
                {
                    if (FindCandidates(cells, cell) == false)
                        return;
                }
            }

            if (!(checkIfSolved(cells)))
                TestCandidate(cells, numSolutions);     //If puzzle not solved yet, test candidates
            else
            {
                Debug.WriteLine("Solution Found");
                _SolvedCells.Add(cells);
            }
        }

        private void TestCandidate(List<List<int>> cells, int numSolutions)        //Tests every allowed path in case of multiple solutions
        {
            int minCandidates = cells.Where(cell => cell.Count > 1).Min(cands => cands.Count);  //Find the lowest amount of candidates of all cells
            int minCell = cells.FindIndex(cell => cell.Count == minCandidates);     //Find a cell that has that lowest number of candidates

            foreach (int candidate in cells[minCell])
            {
                Debug.WriteLine("Testing Candidate");
                var testCells = new List<List<int>>();
                CopyCells(testCells, cells);         //Save cells before testing           

                testCells[minCell] = new List<int> { candidate };
                if (!(UpdateCandidates(testCells, minCell)))        //If this candidate causes contradiction, move to next one
                    continue;

                EliminateCandidates(testCells, numSolutions);     //Have to assign testCells because successive testCells may get held (after making a copy from which the program proceeds with instead) when reversing after solving

                if (_SolvedCells.Count() == numSolutions)
                    return;
            }

            if (!(checkIfSolved(cells)))        //If at end of candidates list and not solved, candidate of previous stack frame is contradictory
                return;             //Using checkIfSolved since if found 1 solution, still want to check specific cells for other possible solutions 
        }

        private void CopyCells(List<List<int>> testCells, List<List<int>> Cells)
        {
            testCells.Clear();

            for (int i = 0; i < _Length * _Length; i++)     //Deep copy to create seperate reference 
            {
                testCells.Add(new List<int>());

                foreach (int cand in Cells[i])
                {
                    testCells[i].Add(cand);
                }
            }
        }

        private List<int> FindPeers(int cell)  //For a cell, find its related row, column and box peers. (Values will never change throughout)
        {
            var peers = new List<int>();

            for (int peerIndex = 0; peerIndex < _Length * _Length; peerIndex++)
            {
                if (peerIndex / _Length == cell / _Length  //If in same row, add to peers
                    | peerIndex % _Length == cell % _Length  //If in same column, add to peers
                    | (peerIndex / _Length / _BoxSize == cell / _Length / _BoxSize && peerIndex % _Length / _BoxSize == cell % _Length / _BoxSize) //If in same box, add to peers
                    && peerIndex != cell)  //Can't be same value for peer and cell
                    peers.Add(peerIndex);
            }

            return peers;
        }

        private bool FindCandidates(List<List<int>> cells, int cell)  //Find the possible allowed values for a cell, judging by its peers
        {
            var peers = FindPeers(cell);

            foreach (var peer in peers)
            {
                if (cells[peer].Count == 1 && cells[cell].Contains(cells[peer][0]))
                {
                    var cellCount = cells[cell].Count;
                    cells[cell].Remove(cells[peer][0]);

                    if (cellCount == 2)
                    {
                        if (!(UpdateCandidates(cells, cell)))
                            return false;

                        break;
                    }
                }
            }

            return true;
        }

        private bool UpdateCandidates(List<List<int>> cells, int cell)      //If a cell is solved through FindCandidates, update its peers candidates, successively
        {
            var peers = FindPeers(cell);

            foreach (var peer in peers)
            {
                if (cells[peer].Contains(cells[cell][0]))
                {
                    var peerLength = cells[peer].Count();

                    if (peerLength == 1)            //Contradiction -- false solution thus far
                        return false;

                    cells[peer].Remove(cells[cell][0]);

                    if (peerLength == 2)
                    {
                        if (!(UpdateCandidates(cells, peer)))
                            return false;
                    }
                }
            }

            return true;
        }

        private void PrintSudoku(List<List<int>> Cells)
        {
            for (int w = 0; w < _Length * _Length; w++)
            {
                if (w % _Length == 0)
                    System.Console.WriteLine("");
                if (Cells[w].Count() == 1)
                    System.Console.Write("{0} ", Cells[w][0]);
                else if (Cells[w].Count() == 0)
                    System.Console.Write(". ");
                else
                    System.Console.Write("0 ");
            }

            System.Console.WriteLine();
        }

        private void GenerateSudoku(int numStartingCells)
        {
            var rand = new Random();        //Don't put rand inside AssignRandoCandidate since each will create new random objects in quick succession with the same seed - use 1 random object only
            var eliminatedCells = new List<int>();

            do
            {
                Debug.WriteLine("Trying new puzzle");
                _Cells = Enumerable.Repeat(new List<int>(Enumerable.Range(1, 9)), _Length * _Length).ToList();
                _SolvedCells.Clear();
                eliminatedCells.Clear();

                for (int randCell = 0; randCell < numStartingCells; randCell++)     //Iterate and add starting cells to sudoku puzzle
                {
                    if (checkIfSolved(_Cells))
                        break;

                    eliminatedCells.AddRange(AssignRandomCandidate(rand, _Cells));      //Assigns the rand cand and then adds the eliminated cells as a result of this to the list

                    if (eliminatedCells.Contains(-1))
                        break;
                }

                if (eliminatedCells.Contains(-1))
                    continue;

                PrintSudoku(_Cells);
                SolveSudoku(2);

            } while (_SolvedCells.Count() != 1);

            Debug.WriteLine("Eliminated: {0}", eliminatedCells.Count());

            foreach (int c in eliminatedCells)      //Get rid of the excess solved cells from generatedSudoku, for the final puzzle to the user
            {
                _Cells[c] = new List<int>(Enumerable.Range(1, _Length));
            }

            PrintSudoku(_SolvedCells[0]);
            PrintSudoku(_Cells);
        }

        private List<int> AssignRandomCandidate(Random rand, List<List<int>> cells)
        {

            int cell;
            var assignedCells = new List<List<int>>();      //The cells prior to assigning random candidate and possible peers being solved due to this
            var eliminatedCells = new List<int>();

            CopyCells(assignedCells, cells);

            do
            {
                cell = rand.Next(_Length * _Length);   //Repeat until a random cell with more than 1 value is found

            } while (cells[cell].Count() == 1);

            var candidate = cells[cell][rand.Next(cells[cell].Count())];    //Finds a random candidate of the random cell and stores it

            assignedCells[cell] = new List<int> { candidate };

            if (!(UpdateCandidates(assignedCells, cell)))           //If assignment leads to contradiction, restart entire sudoku grid
            {
                eliminatedCells.Add(-1);
                return eliminatedCells;
            }

            for (int c = 0; c < _Length * _Length; c++)        //Find the eliminated cells due to random candidate assignment so that they are not included in the puzzle for the user later
            {
                if (assignedCells[c].Count == 1 && cells[c].Count > 1 && c != cell)
                    eliminatedCells.Add(c);
            }

            CopyCells(cells, assignedCells);
            return eliminatedCells;
        }


        static void Main(string[] args)
        {
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "..3.2.6..9..3.5..1..18.64....81.29..7.......8..67.82....26.95..8..2.3..9..5.1.3..");
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "8..2..7....5..6....1..5...42..3...5...1.9.4...7...5..23...1..2....8..5....9..4..6");  
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "...2156...4.....3.7...3....2.......93.9.5.2.84.......7....2...6.7.....1...4876...");
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "8..........36......7..9.2...5...7.......457.....1...3...1....68..85...1..9....4..");  //World's hardest sudoku Puzzle - The Telegraph - solved in seconds
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "...456789679813245548927136...594678857361492964782513...648957796135824485279361"); //Multiple Solutions
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "..1....6..7.9..2.38.4.5........9.......3.1.....9.6.4........5...4...9.7.6.5.8.9.2");

            //puzzle.SolveSudoku();
            var puzzle = new SudokuPuzzle(9);
            puzzle.GenerateSudoku(26);

            //puzzle.PrintSudoku(puzzle._Cells);
            Console.BufferHeight = 2000;
            Debug.WriteLine("Done");
            System.Console.ReadKey();
        }
    }
}
