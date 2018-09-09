using System;
using System.Collections.Generic;

public class IllegalMovesCalculator
{
    private RenjuBoard RenjuBoard;
    private Point currentPointBeingChecked;

    public IllegalMovesCalculator(RenjuBoard boardToPerformCalculationsOn)
    {
        RenjuBoard = boardToPerformCalculationsOn;
    }

    public List<IllegalMove> CalculateIllegalMoves()
    {
        List<IllegalMove> illegalPoints = new List<IllegalMove>();

        for (int x = 0; x < GameConfiguration.BOARD_SIZE; x++)
        {
            for (int y = 0; y < GameConfiguration.BOARD_SIZE; y++)
            {
                currentPointBeingChecked = Point.At(x, y);

                if (RenjuBoard.GetPointOnBoardOccupancyState(currentPointBeingChecked) == OccupancyState.Black || 
                    RenjuBoard.GetPointOnBoardOccupancyState(currentPointBeingChecked) == OccupancyState.White)
                {
                    continue; //can't move to an occupied position
                }

                if (MoveProducesOverline(currentPointBeingChecked))
                {
                    illegalPoints.Add(new IllegalMove(currentPointBeingChecked, IllegalMoveReason.Overline));
                }
                else if (MoveProducesFiveToWin(currentPointBeingChecked, OccupancyState.Black))
                {
                    //stop checking; five in a row has priority over 3x3 or 4x4, but not overline
                }
                else if (CountOpenThrees(currentPointBeingChecked) >= 2)
                {
                    illegalPoints.Add(new IllegalMove(currentPointBeingChecked, IllegalMoveReason.Double3));
                }
                else if (CountOpenFours(currentPointBeingChecked) >= 2)
                {
                    illegalPoints.Add(new IllegalMove(currentPointBeingChecked, IllegalMoveReason.Double4));
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
            Point fourBefore = point.GetPointNStepsAfter(-4, dir);
            Point threeBefore = point.GetPointNStepsAfter(-3, dir);
            Point twoBefore = point.GetPointNStepsAfter(-2, dir);
            Point oneBefore = point.GetPointNStepsAfter(-1, dir);
            Point oneAfter = point.GetPointNStepsAfter(1, dir);
            Point twoAfter = point.GetPointNStepsAfter(2, dir);
            Point threeAfter = point.GetPointNStepsAfter(3, dir);
            Point fourAfter = point.GetPointNStepsAfter(4, dir);

            if (RenjuBoard.GetPointOnBoardOccupancyState(fourBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(threeBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == playerColourOccupancyState)
            {
                return true;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(threeBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == playerColourOccupancyState)
            {
                return true;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(twoBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == playerColourOccupancyState)
            {
                return true;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(oneBefore) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == playerColourOccupancyState)
            {
                return true;
            }

            if (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == playerColourOccupancyState &&
                RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) == playerColourOccupancyState)
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
           (RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || currentPointBeingChecked.Equals(point))  &&
           (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(oneAfter)) &&
            RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.None &&
           (RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(threeAfter)) &&
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
           (RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || currentPointBeingChecked.Equals(point)) &&
            RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.None &&
           (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(twoAfter)) &&
           (RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(threeAfter)) &&
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
           (RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || currentPointBeingChecked.Equals(point)) &&
           (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(oneAfter)) &&
           (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(twoAfter)) &&
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
            Point fourBefore = point.GetPointNStepsAfter(-4, dir);
            Point threeBefore = point.GetPointNStepsAfter(-3, dir);
            Point twoBefore = point.GetPointNStepsAfter(-2, dir);
            Point oneBefore = point.GetPointNStepsAfter(-1, dir);

            if (isNotClosedBBBBstartingAt(threeBefore, dir) ||
                isNotClosedBBBBstartingAt(twoBefore, dir) ||
                isNotClosedBBBBstartingAt(oneBefore, dir) ||
                isNotClosedBBBBstartingAt(point, dir) ||

                isBBBNBorBBNBBorBNBBBstartingAt(fourBefore, dir) ||
                isBBBNBorBBNBBorBNBBBstartingAt(threeBefore, dir) ||
                isBBBNBorBBNBBorBNBBBstartingAt(twoBefore, dir) ||
                isBBBNBorBBNBBorBNBBBstartingAt(oneBefore, dir) ||
                isBBBNBorBBNBBorBNBBBstartingAt(point, dir))
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

        if ((RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || currentPointBeingChecked.Equals(point)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(oneAfter)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(twoAfter)) &&
            (RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(threeAfter)))
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

    private bool isBBBNBorBBNBBorBNBBBstartingAt(Point point, Direction dir)
    {
        int numBlackPiecesFound = 0;

        Point oneAfter = point.GetPointNStepsAfter(1, dir);
        Point twoAfter = point.GetPointNStepsAfter(2, dir);
        Point threeAfter = point.GetPointNStepsAfter(3, dir);
        Point fourAfter = point.GetPointNStepsAfter(4, dir);

        if (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.White ||
            RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.White ||
            RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.White)
        {
            return false; //ensure no white pieces in the middle
        }

        if (RenjuBoard.GetPointOnBoardOccupancyState(point) == OccupancyState.Black || currentPointBeingChecked.Equals(point))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(oneAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(oneAfter))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(twoAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(twoAfter))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(threeAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(threeAfter))
        {
            numBlackPiecesFound++;
        }
        if (RenjuBoard.GetPointOnBoardOccupancyState(fourAfter) == OccupancyState.Black || currentPointBeingChecked.Equals(fourAfter))
        {
            numBlackPiecesFound++;
        }

        return numBlackPiecesFound == 4;
    }

    #endregion
}