using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class IllegalMovesCalculator
{
    private static OccupancyState[,] board;
    private List<IllegalMove> illegalPoints;
    private Point currentPointBeingChecked;

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
                currentPointBeingChecked = Point.At(x, y);

                if (board[x, y] == OccupancyState.Black || board[x, y] == OccupancyState.White) continue; //can't move to an occupied position

                if (MoveProducesOverline(x, y))
                {
                    illegalPoints.Add(new IllegalMove(Point.At(x, y), IllegalMoveReason.Overline));
                }
                else if (MoveProducesFiveToWin(x, y, OccupancyState.Black))
                {
                    continue; //five in a row has priority over 3x3 or 4x4, but not overline
                }
                else if (CountOpenThrees(x, y) >= 2)
                {
                    illegalPoints.Add(new IllegalMove(Point.At(x, y), IllegalMoveReason.Double3));
                }
                else if (CountOpenFours(x, y) >= 2)
                {
                    illegalPoints.Add(new IllegalMove(Point.At(x, y), IllegalMoveReason.Double4));
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
            if (y < 0 || y > RenjuBoard.BOARD_SIZE - 6)
            {
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

    #region 3x3 logic

    private int CountOpenThrees(int X, int Y)
    {
        int numOpenThrees = 0;

        //S N
        if (isOpenBBNBstartingAt(Point.At(X, Y - 3), Direction.S_N) ||
            isOpenBNBBstartingAt(Point.At(X, Y - 3), Direction.S_N) ||
            isOpenBBBstartingAt(Point.At(X, Y - 2), Direction.S_N) ||
            isOpenBNBBstartingAt(Point.At(X, Y - 2), Direction.S_N) ||

            isOpenBBBstartingAt(Point.At(X, Y - 1), Direction.S_N) ||

            isOpenBBNBstartingAt(Point.At(X, Y - 1), Direction.S_N) ||
            isOpenBBBstartingAt(Point.At(X, Y), Direction.S_N) ||
            isOpenBBNBstartingAt(Point.At(X, Y), Direction.S_N) ||
            isOpenBNBBstartingAt(Point.At(X, Y), Direction.S_N))
        {
            numOpenThrees++;
        }

        //SW NE
        if (isOpenBBNBstartingAt(Point.At(X - 3, Y - 3), Direction.SW_NE) ||
            isOpenBNBBstartingAt(Point.At(X - 3, Y - 3), Direction.SW_NE) ||
            isOpenBBBstartingAt(Point.At(X - 2, Y - 2), Direction.SW_NE) ||
            isOpenBNBBstartingAt(Point.At(X - 2, Y - 2), Direction.SW_NE) ||

            isOpenBBBstartingAt(Point.At(X - 1, Y - 1), Direction.SW_NE) ||

            isOpenBBNBstartingAt(Point.At(X - 1, Y - 1), Direction.SW_NE) ||
            isOpenBBBstartingAt(Point.At(X, Y), Direction.SW_NE) ||
            isOpenBBNBstartingAt(Point.At(X, Y), Direction.SW_NE) ||
            isOpenBNBBstartingAt(Point.At(X, Y), Direction.SW_NE))
        {
            numOpenThrees++;
        }

        //W E
        if (isOpenBBNBstartingAt(Point.At(X - 3, Y), Direction.W_E) ||
            isOpenBNBBstartingAt(Point.At(X - 3, Y), Direction.W_E) ||
            isOpenBBBstartingAt(Point.At(X - 2, Y), Direction.W_E) ||
            isOpenBNBBstartingAt(Point.At(X - 2, Y), Direction.W_E) ||
            
            isOpenBBBstartingAt(Point.At(X - 1, Y), Direction.W_E) ||
            
            isOpenBBNBstartingAt(Point.At(X - 1, Y), Direction.W_E) ||
            isOpenBBBstartingAt(Point.At(X, Y), Direction.W_E) ||
            isOpenBBNBstartingAt(Point.At(X, Y), Direction.W_E) ||
            isOpenBNBBstartingAt(Point.At(X, Y), Direction.W_E))
        {
            numOpenThrees++;
        }

        //NW SE
        if (isOpenBBNBstartingAt(Point.At(X - 3, Y + 3), Direction.NW_SE) ||
            isOpenBNBBstartingAt(Point.At(X - 3, Y + 3), Direction.NW_SE) ||
            isOpenBBBstartingAt(Point.At(X - 2, Y + 2), Direction.NW_SE) ||
            isOpenBNBBstartingAt(Point.At(X - 2, Y + 2), Direction.NW_SE) ||

            isOpenBBBstartingAt(Point.At(X - 1, Y + 1), Direction.NW_SE) ||

            isOpenBBNBstartingAt(Point.At(X - 1, Y + 1), Direction.NW_SE) ||
            isOpenBBBstartingAt(Point.At(X, Y), Direction.NW_SE) ||
            isOpenBBNBstartingAt(Point.At(X, Y), Direction.NW_SE) ||
            isOpenBNBBstartingAt(Point.At(X, Y), Direction.NW_SE))
        {
            numOpenThrees++;
        }

        return numOpenThrees;
    }

    private bool isOpenBBNBstartingAt(Point point, Direction dir)
    {
        int X = point.X;
        int Y = point.Y;

        Point OneBefore = point.GetPointNStepsAfter(-1, dir);
        Point OneAfter = point.GetPointNStepsAfter(1, dir);
        Point TwoAfter = point.GetPointNStepsAfter(2, dir);
        Point ThreeAfter = point.GetPointNStepsAfter(3, dir);
        Point FourAfter = point.GetPointNStepsAfter(4, dir);

        if (isSpaceAvailableForNBBBBNwithFirstBstartingAt(point, dir) &&
            (board[X, Y] == OccupancyState.Black || currentPointBeingChecked.Equals(point))  &&
            (board[OneAfter.X, OneAfter.Y] == OccupancyState.Black || currentPointBeingChecked.Equals(OneAfter)) &&
            (board[ThreeAfter.X, ThreeAfter.Y] == OccupancyState.Black || currentPointBeingChecked.Equals(ThreeAfter)) &&
             board[OneBefore.X, OneBefore.Y] == OccupancyState.None && 
             board[TwoAfter.X, TwoAfter.Y] == OccupancyState.None && 
             board[FourAfter.X, FourAfter.Y] == OccupancyState.None)
        {
            if (noOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(point, dir))
            {
                return true;
            }
        }

        return false;
    }

    private bool isOpenBNBBstartingAt(Point point, Direction dir)
    {
        int X = point.X;
        int Y = point.Y;

        Point OneBefore = point.GetPointNStepsAfter(-1, dir);
        Point OneAfter = point.GetPointNStepsAfter(1, dir);
        Point TwoAfter = point.GetPointNStepsAfter(2, dir);
        Point ThreeAfter = point.GetPointNStepsAfter(3, dir);
        Point FourAfter = point.GetPointNStepsAfter(4, dir);

        if (isSpaceAvailableForNBBBBNwithFirstBstartingAt(point, dir) &&
            (board[X, Y] == OccupancyState.Black || currentPointBeingChecked.Equals(point)) &&
            (board[TwoAfter.X, TwoAfter.Y] == OccupancyState.Black || currentPointBeingChecked.Equals(TwoAfter)) &&
            (board[ThreeAfter.X, ThreeAfter.Y] == OccupancyState.Black || currentPointBeingChecked.Equals(ThreeAfter)) &&
             board[OneBefore.X, OneBefore.Y] == OccupancyState.None && 
             board[OneAfter.X, OneAfter.Y] == OccupancyState.None && 
             board[FourAfter.X, FourAfter.Y] == OccupancyState.None)
        {
            if (noOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(point, dir))
            {
                return true;
            }
        }

        return false;
    }

    private bool isOpenBBBstartingAt(Point point, Direction dir)
    {
        int X = point.X;
        int Y = point.Y;

        Point OneBefore = point.GetPointNStepsAfter(-1, dir);
        Point TwoBefore = point.GetPointNStepsAfter(-2, dir);
        Point OneAfter = point.GetPointNStepsAfter(1, dir);
        Point TwoAfter = point.GetPointNStepsAfter(2, dir);
        Point ThreeAfter = point.GetPointNStepsAfter(3, dir);
        Point FourAfter = point.GetPointNStepsAfter(4, dir);

        if (isSpaceAvailableForNBBBNwithFirstBstartingAt(point, dir) &&
            (board[X, Y] == OccupancyState.Black || currentPointBeingChecked.Equals(point)) && 
            (board[OneAfter.X, OneAfter.Y] == OccupancyState.Black || currentPointBeingChecked.Equals(OneAfter)) && 
            (board[TwoAfter.X, TwoAfter.Y] == OccupancyState.Black || currentPointBeingChecked.Equals(TwoAfter)) &&
             board[OneBefore.X, OneBefore.Y] == OccupancyState.None && 
             board[ThreeAfter.X, ThreeAfter.Y] == OccupancyState.None)
        {
            if (RenjuBoard.GetPointOnBoardOccupancyState(FourAfter) == OccupancyState.None) //check if open 4 can be made by assuming black piece appended to tail of open 3
            {
                if (noOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(point, dir))
                {
                    return true;
                }
            }
            if (RenjuBoard.GetPointOnBoardOccupancyState(TwoBefore) == OccupancyState.None) //check if open 4 can be made by assuming black piece appended to head of open 3
            {
                if (noOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(OneBefore, dir))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool isSpaceAvailableForNBBBNwithFirstBstartingAt(Point point, Direction dir)
    {
        return isSpaceAvailableAroundHalfOpenConfigwithFirstBstartingAt(point, dir, HalfOpenConfig.NBBBN);
    }

    private bool isSpaceAvailableForNBBBBNwithFirstBstartingAt(Point point, Direction dir)
    {
        return isSpaceAvailableAroundHalfOpenConfigwithFirstBstartingAt(point, dir, HalfOpenConfig.NBBBBN);
    }

    private bool isSpaceAvailableAroundHalfOpenConfigwithFirstBstartingAt(Point point, Direction dir, HalfOpenConfig config)
    {
        int X = point.X;
        int Y = point.Y;
        int threeOrFour = config == HalfOpenConfig.NBBBN ? 3 : 4;

        int deltaX = dir == Direction.S_N ? 0 : 1;
        int deltaY = 0;
        if (dir == Direction.S_N || dir == Direction.SW_NE) deltaY = 1;
        if (dir == Direction.NW_SE) deltaY = -1;

        return X - deltaX >= 0 && X + threeOrFour * deltaX < RenjuBoard.BOARD_SIZE &&
               Y - deltaY >= 0 && Y - deltaY < RenjuBoard.BOARD_SIZE &&  //need to account for Y - deltaY going to 15+ due to NW_SE direction
               Y + threeOrFour * deltaY >= 0 && Y + threeOrFour * deltaY < RenjuBoard.BOARD_SIZE; //need to account for Y + 3*deltaY or Y + 4*deltaY going to 0- due to NW_SE direction
    }

    private bool noOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(Point point, Direction dir)
    {
        int X = point.X;
        int Y = point.Y;

        Point TwoBefore = point.GetPointNStepsAfter(-2, dir);
        Point FiveAfter = point.GetPointNStepsAfter(5, dir);
        //  if (2 before is on board)
        //      if (2 before is black) then do nothing
        //  if (5 after is on board)
        //      if  (5 after is black) then do nothing
        //  numOpenThrees++; //no overline hazard! gj for making it here
        if (RenjuBoard.GetPointOnBoardOccupancyState(TwoBefore) == OccupancyState.Black)
        {
            return false; //3 return statements to make this logic easier to read
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(FiveAfter) == OccupancyState.Black)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region 4x4 logic

    private int CountOpenFours(int X, int Y)
    {
        int numOpenFours = 0;

        //S_N
        if (isNotClosedBBBBstartingAt(Point.At(X, Y - 3), Direction.S_N) ||
            isNotClosedBBBBstartingAt(Point.At(X, Y - 2), Direction.S_N) ||
            isNotClosedBBBBstartingAt(Point.At(X, Y - 1), Direction.S_N) ||
            isNotClosedBBBBstartingAt(Point.At(X, Y), Direction.S_N) ||

            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X, Y - 4), Direction.S_N) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X, Y - 3), Direction.S_N) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X, Y - 2), Direction.S_N) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X, Y - 1), Direction.S_N) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X, Y), Direction.S_N))
        {
            numOpenFours++;
        }

        //SW_NE
        if (isNotClosedBBBBstartingAt(Point.At(X - 3, Y - 3), Direction.SW_NE) ||
            isNotClosedBBBBstartingAt(Point.At(X - 2, Y - 2), Direction.SW_NE) ||
            isNotClosedBBBBstartingAt(Point.At(X - 1, Y - 1), Direction.SW_NE) ||
            isNotClosedBBBBstartingAt(Point.At(X, Y), Direction.SW_NE) ||

            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 4, Y - 4), Direction.SW_NE) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 3, Y - 3), Direction.SW_NE) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 2, Y - 2), Direction.SW_NE) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 1, Y - 1), Direction.SW_NE) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X, Y), Direction.SW_NE))
        {
            numOpenFours++;
        }

        //W_E
        if (isNotClosedBBBBstartingAt(Point.At(X - 3, Y), Direction.W_E) ||
            isNotClosedBBBBstartingAt(Point.At(X - 2, Y), Direction.W_E) ||
            isNotClosedBBBBstartingAt(Point.At(X - 1, Y), Direction.W_E) ||
            isNotClosedBBBBstartingAt(Point.At(X, Y), Direction.W_E) ||

            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 4, Y), Direction.W_E) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 3, Y), Direction.W_E) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 2, Y), Direction.W_E) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 1, Y), Direction.W_E) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X, Y), Direction.W_E))
        {
            numOpenFours++;
        }

        //NW_SE
        if (isNotClosedBBBBstartingAt(Point.At(X - 3, Y + 3), Direction.NW_SE) ||
            isNotClosedBBBBstartingAt(Point.At(X - 2, Y + 2), Direction.NW_SE) ||
            isNotClosedBBBBstartingAt(Point.At(X - 1, Y + 1), Direction.NW_SE) ||
            isNotClosedBBBBstartingAt(Point.At(X, Y), Direction.NW_SE) ||

            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 4, Y + 4), Direction.NW_SE) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 3, Y + 3), Direction.NW_SE) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 2, Y + 2), Direction.NW_SE) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X - 1, Y + 1), Direction.NW_SE) ||
            isBBBNBorBBNBBorBNBBBstartingAt(Point.At(X, Y), Direction.NW_SE))
        {
            numOpenFours++;
        }

        return numOpenFours;
    }

    private bool isNotClosedBBBBstartingAt(Point point, Direction dir)
    {
        int X = point.X;
        int Y = point.Y;

        Point OneBefore = point.GetPointNStepsAfter(-1, dir);
        Point TwoBefore = point.GetPointNStepsAfter(-2, dir);
        Point OneAfter = point.GetPointNStepsAfter(1, dir);
        Point TwoAfter = point.GetPointNStepsAfter(2, dir);
        Point ThreeAfter = point.GetPointNStepsAfter(3, dir);
        Point FourAfter = point.GetPointNStepsAfter(4, dir);
        Point FiveAfter = point.GetPointNStepsAfter(5, dir);

        if ((RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || currentPointBeingChecked.Equals(point)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(OneAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(OneAfter)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(TwoAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(TwoAfter)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(ThreeAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(ThreeAfter)))
        {
            if (RenjuBoard.GetPointOnBoardOccupancyState(OneBefore) == OccupancyState.None) //check if open 4 can be made by assuming black piece appended to tail of open 3
            {
                if (RenjuBoard.GetPointOnBoardOccupancyState(TwoBefore) != OccupancyState.Black)
                {
                    return true;
                }
            }
            if (RenjuBoard.GetPointOnBoardOccupancyState(FourAfter) == OccupancyState.None) //check if open 4 can be made by assuming black piece appended to head of open 3
            {
                if (RenjuBoard.GetPointOnBoardOccupancyState(FiveAfter) != OccupancyState.Black)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool isBBBNBorBBNBBorBNBBBstartingAt(Point point, Direction dir)
    {
        int numBlackPiecesFound = 0;

        Point OneAfter = point.GetPointNStepsAfter(1, dir);
        Point TwoAfter = point.GetPointNStepsAfter(2, dir);
        Point ThreeAfter = point.GetPointNStepsAfter(3, dir);
        Point FourAfter = point.GetPointNStepsAfter(4, dir);

        if (RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || currentPointBeingChecked.Equals(point))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(OneAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(OneAfter))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(TwoAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(TwoAfter))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(ThreeAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(ThreeAfter))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(FourAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(FourAfter))
        {
            numBlackPiecesFound++;
        }

        if (numBlackPiecesFound == 4)
        {
            return true;
        }
        return false;
    }

    #endregion
}




