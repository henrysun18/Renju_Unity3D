using System;
using System.Collections.Generic;

public class IllegalMovesCalculator
{
    private RenjuBoard RenjuBoard;
    private LinkedList<Stone> StonesToCheckAround;
    private Point CurrentPointBeingChecked;

    public IllegalMovesCalculator(RenjuBoard boardToPerformCalculationsOn, LinkedList<Stone> stonesToCheckAround)
    {
        RenjuBoard = boardToPerformCalculationsOn;
        StonesToCheckAround = stonesToCheckAround;
    }

    public List<IllegalMove> CalculateIllegalMoves()
    {
        List<IllegalMove> illegalPoints = new List<IllegalMove>();
        bool[,] potentialMovesAlreadyChecked = new bool[15,15]; //dynamic programming implementation 

        foreach (Stone stone in StonesToCheckAround)
        {
            //check the adjacent 8 positions around each stone on board to find potential illegal moves (instead of the entire board)
            int startingX = Math.Max(0, stone.point.X - 2);
            int endingX = Math.Min(GameConfiguration.BOARD_SIZE - 1, stone.point.X + 2);
            int startingY = Math.Max(0, stone.point.Y - 2);
            int endingY = Math.Min(GameConfiguration.BOARD_SIZE - 1, stone.point.Y + 2);

            for (int x = startingX; x <= endingX; x++)
            {
                for (int y = startingY; y <= endingY; y++)
                {
                    CurrentPointBeingChecked = Point.At(x, y);

                    if (potentialMovesAlreadyChecked[x,y])
                    {
                        continue; //already reached this point through a neighbour
                    }

                    if (RenjuBoard.GetPointOnBoardOccupancyState(CurrentPointBeingChecked) == OccupancyState.Black ||
                        RenjuBoard.GetPointOnBoardOccupancyState(CurrentPointBeingChecked) == OccupancyState.White)
                    {
                        continue; //can't move to an occupied position
                    }

                    if (MoveProducesFiveToWin(CurrentPointBeingChecked, OccupancyState.Black))
                    {
                        //stop checking; if it's a winning move, it's automatically allowed (make sure this move doesn't product overline though)
                    }
                    else if(MoveProducesOverline(CurrentPointBeingChecked))
                    {
                        illegalPoints.Add(new IllegalMove(CurrentPointBeingChecked, IllegalMoveReason.Overline));
                    }
                    else if (CountOpenThrees(CurrentPointBeingChecked) >= 2)
                    {
                        illegalPoints.Add(new IllegalMove(CurrentPointBeingChecked, IllegalMoveReason.Double3));
                    }
                    else if (CountOpenFours(CurrentPointBeingChecked) >= 2)
                    {
                        illegalPoints.Add(new IllegalMove(CurrentPointBeingChecked, IllegalMoveReason.Double4));
                    }
                    else
                    {
                        //unoccupied position AND not illegal move
                        potentialMovesAlreadyChecked[x,y] = true;
                    }
                }
            }
        }

        return illegalPoints;
    }

    private bool MoveProducesOverline(Point point)
    {
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Point fourBefore = point.GetPointNStepsAfter(-4, dir);
            Point threeBefore = point.GetPointNStepsAfter(-3, dir);
            Point twoBefore = point.GetPointNStepsAfter(-2, dir);
            Point oneBefore = point.GetPointNStepsAfter(-1, dir);
            Point oneAfter = point.GetPointNStepsAfter(1, dir);
            Point twoAfter = point.GetPointNStepsAfter(2, dir);
            Point threeAfter = point.GetPointNStepsAfter(3, dir);
            Point fourAfter = point.GetPointNStepsAfter(4, dir);

            if (RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == OccupancyState.Black &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black)
            {
                if (RenjuBoard.GetPointOnBoardOccupancyState(fourBefore) == OccupancyState.Black &&
                    RenjuBoard.GetPointOnBoardOccupancyState(threeBefore) == OccupancyState.Black &&
                    RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == OccupancyState.Black)
                {
                    return true;
                }

                if (RenjuBoard.GetPointOnBoardOccupancyState(threeBefore) == OccupancyState.Black &&
                    RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == OccupancyState.Black &&
                    RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black)
                {
                    return true;
                }

                if (RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == OccupancyState.Black &&
                    RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black &&
                    RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black)
                {
                    return true;
                }

                if (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black &&
                    RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black &&
                    RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) == OccupancyState.Black)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool MoveProducesFiveToWin(Point point, OccupancyState playerColourOccupancyState)
    {
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Point fiveBefore = point.GetPointNStepsAfter(-5, dir);
            Point fourBefore = point.GetPointNStepsAfter(-4, dir);
            Point threeBefore = point.GetPointNStepsAfter(-3, dir);
            Point twoBefore = point.GetPointNStepsAfter(-2, dir);
            Point oneBefore = point.GetPointNStepsAfter(-1, dir);
            Point oneAfter = point.GetPointNStepsAfter(1, dir);
            Point twoAfter = point.GetPointNStepsAfter(2, dir);
            Point threeAfter = point.GetPointNStepsAfter(3, dir);
            Point fourAfter = point.GetPointNStepsAfter(4, dir);
            Point fiveAfter = point.GetPointNStepsAfter(5, dir);

            if (RenjuBoard.GetPointOnBoardOccupancyState(fiveBefore) != playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(fourBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(threeBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) != playerColourOccupancyState)
            {
                return true;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(fourBefore) != playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(threeBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) != playerColourOccupancyState)
            {
                return true;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(threeBefore) != playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) != playerColourOccupancyState)
            {
                return true;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) != playerColourOccupancyState && 
                RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) != playerColourOccupancyState)
            {
                return true;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) != playerColourOccupancyState && 
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(fiveAfter) != playerColourOccupancyState)
            {
                return true;
            }
        }

        return false;
    }

    #region 3x3 logic

    private int CountOpenThrees(Point point)
    {
        int numOpenThrees = 0;

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Point threeBefore = point.GetPointNStepsAfter(-3, dir);
            Point twoBefore = point.GetPointNStepsAfter(-2, dir);
            Point oneBefore = point.GetPointNStepsAfter(-1, dir);

            if (isOpenBBNBstartingAt(threeBefore, dir) ||
                isOpenBNBBstartingAt(threeBefore, dir) ||
                isOpenBBBstartingAt(twoBefore, dir) ||
                isOpenBNBBstartingAt(twoBefore, dir) ||

                isOpenBBBstartingAt(oneBefore, dir) ||

                isOpenBBNBstartingAt(oneBefore, dir) ||
                isOpenBBBstartingAt(point, dir) ||
                isOpenBBNBstartingAt(point, dir) ||
                isOpenBNBBstartingAt(point, dir))
            {
                numOpenThrees++;
            }
        }

        return numOpenThrees;
    }

    private bool isOpenBBNBstartingAt(Point point, Direction dir)
    {
        Point oneBefore = point.GetPointNStepsAfter(-1, dir);
        Point oneAfter = point.GetPointNStepsAfter(1, dir);
        Point twoAfter = point.GetPointNStepsAfter(2, dir);
        Point threeAfter = point.GetPointNStepsAfter(3, dir);
        Point fourAfter = point.GetPointNStepsAfter(4, dir);

        if (RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == OccupancyState.None &&
           (RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || CurrentPointBeingChecked.Equals(point))  &&
           (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(oneAfter)) &&
            RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.None &&
           (RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(threeAfter)) &&
            RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) == OccupancyState.None)
        {
            return isNoOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(point, dir);
        }

        return false;
    }

    private bool isOpenBNBBstartingAt(Point point, Direction dir)
    {
        Point oneBefore = point.GetPointNStepsAfter(-1, dir);
        Point oneAfter = point.GetPointNStepsAfter(1, dir);
        Point twoAfter = point.GetPointNStepsAfter(2, dir);
        Point threeAfter = point.GetPointNStepsAfter(3, dir);
        Point fourAfter = point.GetPointNStepsAfter(4, dir);

        if (RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == OccupancyState.None &&
           (RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || CurrentPointBeingChecked.Equals(point)) &&
            RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.None &&
           (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(twoAfter)) &&
           (RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(threeAfter)) &&
            RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) == OccupancyState.None)
        {
            return isNoOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(point, dir);
        }

        return false;
    }

    private bool isOpenBBBstartingAt(Point point, Direction dir)
    {
        Point oneBefore = point.GetPointNStepsAfter(-1, dir);
        Point twoBefore = point.GetPointNStepsAfter(-2, dir);
        Point oneAfter = point.GetPointNStepsAfter(1, dir);
        Point twoAfter = point.GetPointNStepsAfter(2, dir);
        Point threeAfter = point.GetPointNStepsAfter(3, dir);
        Point fourAfter = point.GetPointNStepsAfter(4, dir);

        if (RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == OccupancyState.None &&
           (RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || CurrentPointBeingChecked.Equals(point)) &&
           (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(oneAfter)) &&
           (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(twoAfter)) &&
            RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.None)
        {
            if (RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == OccupancyState.None) //check if open 4 can be made by assuming black piece appended to head of open 3
            {
                return isNoOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(oneBefore, dir);
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) == OccupancyState.None) //check if open 4 can be made by assuming black piece appended to tail of open 3
            {
                return isNoOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(point, dir);
            }
            
        }

        return false;
    }

    private bool isNoOverlineHazardSurroundingNBBBBNwithFirstBstartingAt(Point point, Direction dir)
    {
        Point twoBefore = point.GetPointNStepsAfter(-2, dir);
        Point fiveAfter = point.GetPointNStepsAfter(5, dir);

        if (RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == OccupancyState.Black ||
            RenjuBoard.GetPointOnBoardOccupancyState(fiveAfter) == OccupancyState.Black)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region 4x4 logic

    private int CountOpenFours(Point point)
    {
        int numOpenFours = 0;

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            //may have two in the same direction!!
            for (int i = -4; i <= 0; i++)
            {
                Point iBefore = point.GetPointNStepsAfter(i, dir);
                if (isNotClosedBBBNBorBBNBBorBNBBBstartingAt(iBefore, dir))
                {
                    numOpenFours++;
                }
            }

            //may only have one BBBB in a given direction
            Point threeBefore = point.GetPointNStepsAfter(-3, dir);
            Point twoBefore = point.GetPointNStepsAfter(-2, dir);
            Point oneBefore = point.GetPointNStepsAfter(-1, dir);
            if (isNotClosedBBBBstartingAt(threeBefore, dir) ||
                isNotClosedBBBBstartingAt(twoBefore, dir) ||
                isNotClosedBBBBstartingAt(oneBefore, dir) ||
                isNotClosedBBBBstartingAt(point, dir))
            {
                numOpenFours++;
            }
        }

        return numOpenFours;
    }

    private bool isNotClosedBBBBstartingAt(Point point, Direction dir)
    {
        Point oneBefore = point.GetPointNStepsAfter(-1, dir);
        Point twoBefore = point.GetPointNStepsAfter(-2, dir);
        Point oneAfter = point.GetPointNStepsAfter(1, dir);
        Point twoAfter = point.GetPointNStepsAfter(2, dir);
        Point threeAfter = point.GetPointNStepsAfter(3, dir);
        Point fourAfter = point.GetPointNStepsAfter(4, dir);
        Point fiveAfter = point.GetPointNStepsAfter(5, dir);

        if ((RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || CurrentPointBeingChecked.Equals(point)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(oneAfter)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(twoAfter)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(threeAfter)))
        {
            if (RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == OccupancyState.None) //check if open 4 can be made by assuming black piece appended to tail of open 3
            {
                return RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) != OccupancyState.Black;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) == OccupancyState.None) //check if open 4 can be made by assuming black piece appended to head of open 3
            {
                return RenjuBoard.GetPointOnBoardOccupancyState(fiveAfter) != OccupancyState.Black;
            }
        }

        return false;
    }

    private bool isNotClosedBBBNBorBBNBBorBNBBBstartingAt(Point point, Direction dir)
    {
        int numBlackPiecesFound = 0;

        Point oneBefore = point.GetPointNStepsAfter(-1, dir);
        Point oneAfter = point.GetPointNStepsAfter(1, dir);
        Point twoAfter = point.GetPointNStepsAfter(2, dir);
        Point threeAfter = point.GetPointNStepsAfter(3, dir);
        Point fourAfter = point.GetPointNStepsAfter(4, dir);
        Point fiveAfter = point.GetPointNStepsAfter(5, dir);

        if (RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == OccupancyState.Black ||
            RenjuBoard.GetPointOnBoardOccupancyState(fiveAfter) == OccupancyState.Black)
        {
            return false; //cannot produce 5 in a row due to overline hazard
        }

        if (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.White ||
            RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.White ||
            RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.White)
        {
            //ensure no white pieces in the middle
            return false;
        }

        if (RenjuBoard.GetPointOnBoardOccupancyState(point) != OccupancyState.Black && !CurrentPointBeingChecked.Equals(point) ||
            RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) != OccupancyState.Black && !CurrentPointBeingChecked.Equals(fourAfter))
        {
            //ensure leftmost and rightmost pieces are black
            return false;
        }

        if (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(oneAfter))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(twoAfter))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black || CurrentPointBeingChecked.Equals(threeAfter))
        {
            numBlackPiecesFound++;
        }

        return numBlackPiecesFound == 2;
    }

    #endregion
}