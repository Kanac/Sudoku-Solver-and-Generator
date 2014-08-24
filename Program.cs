using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuPuzzle
    {
        private List<List<int>> _Cells = new List<List<int>>();
        private int Length;
        private int BoxSize { get { return (int)Math.Sqrt(Length); }}
        
        public SudokuPuzzle(int length, string cells)
        {
            if (cells.Length != length*length)
                throw new ArgumentException();
            
            this.Length = length;
            
            int cellIndex = 0;
            foreach (char c in cells)   //Populates the given sudoku with candidates, either given or 1-9
            {
                if (c.ToString().Equals("."))   //Empty boxes, candidates of 1-9  
                    _Cells.Add(new List<int>(Enumerable.Range(1, Length)));  
                else
                    _Cells.Add(new List<int>{(int)Char.GetNumericValue(c)});     //Given value

                cellIndex++;
            }
        }

        private List<List<int>> EliminateCandidates(List<List<int>> Cells)      //Cells could either be its sudoku puzzle given, or test cells
        {
            int eliminated;

            do      //Loop through the sudoku puzzle, each time eliminating cells to 1 candidate, until there are no more eliminations done (solved)
            {
                eliminated = 0;  //Initiates count of elimated cells

                for (int cell = 0; cell < Length * Length; cell++)   //Loop through cells
                {
                    if (Cells[cell].Count > 1)     //Only loop through cells with more than 1 value (not solved yet)
                    {
                        FindCandidates(cell, Cells);

                        if (Cells[cell].Count == 1)  //If the cell is reduced to 1 value, increase solved count
                            eliminated++;

                        else  if (Cells[cell].Count == 0)     //For when testing is used, if contradiction is found, a cell will have 0 candidates 
                            return null;
                    }
                }
            } while (eliminated != 0);   //Stop when no more elimination can be done

            return TestCandidate(Cells);     //If puzzle not solved yet, test candidates

        }

        private List<List<int>> TestCandidate(List<List<int>> Cells)  
        {
            if (Cells.Where(cell => cell.Count == 1).Count() != Length * Length)     
            {
                int minCandidates = Cells.Where(cell => cell.Count > 1).Min(cands => cands.Count);  //Find the lowest amount of candidates of all cells
                int minCell = Cells.FindIndex(cell => cell.Count == minCandidates);     //Find a cell that has that lowest number of candidates

                foreach (int candidate in Cells[minCell])
                {
                    var testCells = new List<List<int>>();
                    CopyCells(testCells, Cells);         //Save cells before testing           
                    testCells[minCell] = new List<int> { candidate };    //Assign test candidate

                    foreach (int peer in FindPeers(minCell))    //Iterate through all the peers of the cell
                    {
                        testCells[peer].Remove(candidate);   //And remove the candidate from the peers
                    }

                    testCells = EliminateCandidates(testCells);     //Have to assign testCells because successive testCells may get held (after making a copy from which the program proceeds with instead) when reversing after solving
                    if (testCells != null)
                    {
                        Cells = testCells;
                        break;
                    }
                }

                if (Cells.Where(cell => cell.Count == 1).Count() != Length * Length)        //If at end of candidates list, candidate of previous stack frame is contradictory
                    return null;
            }

            return Cells;
        }

        private void CopyCells(List<List<int>> testCells, List<List<int>> Cells)
        {
            for (int i = 0; i < Length * Length; i++)     //Deep copy to create seperate reference 
            {
                testCells.Add(new List<int>());

                foreach (int cand in Cells[i])
                {
                    testCells[i].Add(cand);
                }
            }
        }

        private List<int> FindPeers(int cellIndex)  //For a cell, find its related row, column and box peers. (Values will never change throughout)
        {
            var peers = new List<int>();

            for (int peerIndex = 0; peerIndex < Length*Length; peerIndex++)
            {
                if (peerIndex / Length == cellIndex / Length  //If in same row, add to peers
                    | peerIndex % Length == cellIndex % Length  //If in same column, add to peers
                    | (peerIndex /Length / BoxSize == cellIndex / Length / BoxSize && peerIndex % Length / BoxSize == cellIndex % Length / BoxSize) //If in same box, add to peers
                    && peerIndex != cellIndex)  //Can't be same value for peer and cell
                    peers.Add(peerIndex);
            }

            return peers;
        }

        private void FindCandidates(int cellIndex, List<List<int>> Cells)  //Find the possible allowed values for a cell, judging by its peers
        {
            var peers = FindPeers(cellIndex);

            foreach (var peer in peers)
            {
                if (Cells[peer].Count == 1)   //If a peer is already defined by 1 value, remove that value from candidates 
                    Cells[cellIndex].Remove(Cells[peer][0]);

            }
        }

        static void Main(string[] args)
        {
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "..3.2.6..9..3.5..1..18.64....81.29..7.......8..67.82....26.95..8..2.3..9..5.1.3..");
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "8..2..7....5..6....1..5...42..3...5...1.9.4...7...5..23...1..2....8..5....9..4..6");  
            //SudokuPuzzle puzzle = new SudokuPuzzle(9, "...2156...4.....3.7...3....2.......93.9.5.2.84.......7....2...6.7.....1...4876...");
            SudokuPuzzle puzzle = new SudokuPuzzle(9, "8..........36......7..9.2...5...7.......457.....1...3...1....68..85...1..9....4..");  //World's hardest sudoku Puzzle - The Telegraph - solved in seconds
            
            puzzle._Cells = puzzle.EliminateCandidates(puzzle._Cells);
            Console.BufferHeight = 2000;

            for (int w = 0; w < puzzle.Length * puzzle.Length; w++)
            {
                if (w % puzzle.Length == 0)
                    System.Console.WriteLine("");
                if (puzzle._Cells[w].Count() == 1)
                    System.Console.Write("{0} ", puzzle._Cells[w][0]);
                else if (puzzle._Cells[w].Count() == 0)
                    System.Console.Write(". ");
                else
                    System.Console.Write("0 ");
            }

                System.Console.ReadKey();
        }
    }
}
