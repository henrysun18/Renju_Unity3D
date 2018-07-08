using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllegalMovesCalculator
{
    private static OccupancyState[,] board;
    private List<IllegalMove> illegalPoints;

    public IllegalMovesCalculator(OccupancyState[,] b)
    {
        board = b;
        illegalPoints = new List<IllegalMove>();
    }

    public List<IllegalMove> CalculateIllegalMoves()
    {
        for (int x = 0; x < RenjuBoard.BOARD_SIZE; x++)
        {
            for (int y = 0; y < RenjuBoard.BOARD_SIZE; y++)
            {
                if (board[x, y] != OccupancyState.None) continue; //can't move to an occupied position
                
                if (MoveProducesOverline(x, y))
                {
                    illegalPoints.Add(new IllegalMove(new Point(x, y), IllegalMoveReason.Overline));
                } 
                else if (MoveProducesFiveToWin(x, y, OccupancyState.Black))
                {
                    continue; //five in a row has priority over 3x3 or 4x4, but not overline
                }
                else if(CountOpenThrees(x, y) >= 1)
                {
                    illegalPoints.Add(new IllegalMove(new Point(x, y), IllegalMoveReason.Double3));
                }
                else if (CountOpenFours(x, y) >= 1)
                {
                    illegalPoints.Add(new IllegalMove(new Point(x, y), IllegalMoveReason.Double4));
                }
            }
        }

        return illegalPoints;
    }

    private bool MoveProducesOverline(int X, int Y)
    {
        int startingXIndexOfCheck, startingYIndexOfCheck;

        //S N
        startingYIndexOfCheck = Y - 4;
        for (int y = startingYIndexOfCheck; y < startingYIndexOfCheck + 4; y++)
        {
            if (y < 0 || y > RenjuBoard.BOARD_SIZE - 6) {
                continue; //if board is 15x15, ensure start is between 0 and 9
            }

            for (int i = 0; i < 6; i++) //check 6 consecutive points
            {
                int yIndexToCheck = y + i;
                if (board[X, yIndexToCheck] != OccupancyState.Black && yIndexToCheck != Y) //break if any of the 6 consecutive points (including our move) is not black
                {
                    break;
                }

                if (i == 5) return true; //didn't break for all 6 consecutive points = overline found
            }
        }

        //SW NE
        startingXIndexOfCheck = X - 4;
        startingYIndexOfCheck = Y - 4;
        for (int x = startingXIndexOfCheck; x < startingXIndexOfCheck + 4; x++)
        {
            int y = startingYIndexOfCheck + (x - startingXIndexOfCheck); //y is actually following along with the changes of x
            if (x < 0 || y < 0 || x > RenjuBoard.BOARD_SIZE - 6 || y > RenjuBoard.BOARD_SIZE - 6)
            {
                continue;
            }

            for (int i = 0; i < 6; i++)
            {
                int xIndexToCheck = x + i;
                int yIndexToCheck = y + i;
                if (board[xIndexToCheck, yIndexToCheck] != OccupancyState.Black && xIndexToCheck != X && yIndexToCheck != Y)
                {
                    break;
                }

                if (i == 5) return true;
            }
        }

        //W E
        startingXIndexOfCheck = X - 4; //BBBB_B needs to start checking from index X-4
        for (int x = startingXIndexOfCheck; x < startingXIndexOfCheck + 4; x++)
        {
            if (x < 0 || x > RenjuBoard.BOARD_SIZE - 6)
            {
                continue;
            } 

            for (int i = 0; i < 6; i++) 
            {
                int xIndexToCheck = x + i;
                if (board[xIndexToCheck, Y] != OccupancyState.Black && xIndexToCheck != X)
                {
                    break;
                }

                if (i == 5) return true;
            }
        }

        //NE SW
        startingXIndexOfCheck = X - 4;
        startingYIndexOfCheck = Y + 4;
        for (int x = startingXIndexOfCheck; x < startingXIndexOfCheck + 4; x++)
        {
            int y = startingYIndexOfCheck - (x - startingXIndexOfCheck);
            if (x < 0 || y < 5 || x > RenjuBoard.BOARD_SIZE - 6 || y > RenjuBoard.BOARD_SIZE - 1)
            {
                continue;
            }

            for (int i = 0; i < 6; i++)
            {
                int xIndexToCheck = x + i;
                int yIndexToCheck = y - i;
                if (board[xIndexToCheck, yIndexToCheck] != OccupancyState.Black && xIndexToCheck != X && yIndexToCheck != Y)
                {
                    break;
                }

                if (i == 5) return true;
            }
        }

        return false;
    }

    public static bool MoveProducesFiveToWin(int X, int Y, OccupancyState playerColourOccupancyState)
    {
        int startingXIndexOfCheck, startingYIndexOfCheck;

        //S N
        startingYIndexOfCheck = Y - 4;
        for (int y = startingYIndexOfCheck; y < startingYIndexOfCheck + 5; y++) // 5 possible starting points of 5-in-a-row
        {
            if (y < 0 || y > RenjuBoard.BOARD_SIZE - 5)
            {
                continue; //if board is 15x15, ensure start is between 0 and 10
            }

            for (int i = 0; i < 5; i++) //check 5 consecutive points
            {
                int yIndexToCheck = y + i;
                if (board[X, yIndexToCheck] != playerColourOccupancyState && yIndexToCheck != Y) //break if any of the 5 consecutive points (including our move) is not player colour
                {
                    break;
                }

                if (i == 4) return true; //didn't break for all 5 consecutive points = WIN found
            }
        }

        //SW NE
        startingXIndexOfCheck = X - 4;
        startingYIndexOfCheck = Y - 4;
        for (int x = startingXIndexOfCheck; x < startingXIndexOfCheck + 5; x++)
        {
            int y = startingYIndexOfCheck + (x - startingXIndexOfCheck); //y is actually following along with the changes of x
            if (x < 0 || y < 0 || x > RenjuBoard.BOARD_SIZE - 5 || y > RenjuBoard.BOARD_SIZE - 5)
            {
                continue;
            }

            for (int i = 0; i < 5; i++)
            {
                int xIndexToCheck = x + i;
                int yIndexToCheck = y + i;
                if (board[xIndexToCheck, yIndexToCheck] != playerColourOccupancyState && xIndexToCheck != X && yIndexToCheck != Y)
                {
                    break;
                }

                if (i == 4) return true;
            }
        }

        //W E
        startingXIndexOfCheck = X - 4; //BBBB_B needs to start checking from index X-4
        for (int x = startingXIndexOfCheck; x < startingXIndexOfCheck + 5; x++)
        {
            if (x < 0 || x > RenjuBoard.BOARD_SIZE - 5)
            {
                continue;
            }

            for (int i = 0; i < 5; i++)
            {
                int xIndexToCheck = x + i;
                if (board[xIndexToCheck, Y] != playerColourOccupancyState && xIndexToCheck != X)
                {
                    break;
                }

                if (i == 4) return true;
            }
        }

        //NE SW
        startingXIndexOfCheck = X - 4;
        startingYIndexOfCheck = Y + 4;
        for (int x = startingXIndexOfCheck; x < startingXIndexOfCheck + 5; x++)
        {
            int y = startingYIndexOfCheck - (x - startingXIndexOfCheck);
            if (x < 0 || y < 4 || x > RenjuBoard.BOARD_SIZE - 5 || y > RenjuBoard.BOARD_SIZE - 1)
            {
                continue;
            }

            for (int i = 0; i < 5; i++)
            {
                int xIndexToCheck = x + i;
                int yIndexToCheck = y - i;
                if (board[xIndexToCheck, yIndexToCheck] != playerColourOccupancyState && xIndexToCheck != X && yIndexToCheck != Y)
                {
                    break;
                }

                if (i == 4) return true;
            }
        }

        return false;
    }

    private int CountOpenThrees(int X, int Y)
    {
        int numOpenThrees = 0;
        int startingXIndexOfCheck, startingYIndexOfCheck;

        //S N



        //SW NE

        //W E
        //if (X-3, X-2 are black and X-4, X-1, X+1 are empty OR X-3, X-1 are black and X-4, X-2, X+1 are empty)
        if (X-4 >= 0 && X+1 < RenjuBoard.BOARD_SIZE && 
            (board[X - 3, Y] == OccupancyState.Black && board[X - 2, Y] == OccupancyState.Black &&
            board[X - 4, Y] == OccupancyState.None && board[X - 1, Y] == OccupancyState.None && board[X + 1, Y] == OccupancyState.None ||
            board[X - 3, Y] == OccupancyState.Black && board[X - 1, Y] == OccupancyState.Black &&
            board[X - 4, Y] == OccupancyState.None && board[X - 2, Y] == OccupancyState.None && board[X + 1, Y] == OccupancyState.None))
        {
            //  if (X-5 is on board)
            //      if (X-5 is black) then do nothing
            //  if (X+2 is on board)
            //      if  (X+2 is black) then do nothing
            //  numOpenThrees++; //no overline hazard! gj for making it here
            if (X - 5 >= 0 && board[X - 5, Y] == OccupancyState.Black) ;
            else if (X + 2 < RenjuBoard.BOARD_SIZE && board[X + 2, Y] == OccupancyState.Black) ;
            else
            {
                numOpenThrees++;
            }
        }
        else if (X-3 >= 0 && X+1 < RenjuBoard.BOARD_SIZE && 
                 board[X-2, Y] == OccupancyState.Black && board[X-1, Y] == OccupancyState.Black &&
                 board[X-3, Y] == OccupancyState.None && board[X+1, Y] == OccupancyState.None)
        {
            //if (X-4 >= 0 && board[X-4, Y] )
        }
            


        //NE SW
        return numOpenThrees;
    }

    private int CountOpenFours(int X, int Y)
    {
        return 0;
    }
}

public struct IllegalMove
{
    public IllegalMove(Point p, IllegalMoveReason r) : this()
    {
        point = p;
        reason = r;
    }

    public Point point { get; private set; }

    public IllegalMoveReason reason { get; private set; }
}


