using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class Liquid : Element {

    // Variables specifically for Liquid element properties
    protected float viscosity;

    int moveDirection = UnityEngine.Random.Range(0, 2) * 2 - 1; // This determines whether the liquid will be moving to the left or right when flowing

    Color lineColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

    protected Liquid(int x, int y, PixelGrid grid) {
        // Variables for Element properties

        // Variables for simulation
        pixelX = x;
        pixelY = y;
        this.grid = grid;
        consecutiveDirChange = 0;
    }

    public override void step(PixelGrid grid) {
        if (hasStepped) return; // Prevents cells from running twice in the same step
        hasStepped = true;

        isMoving = isMoving || CheckShouldMove(); // If isMoving is true, keep it. If not, see if it should be and set it appropriately
        //Debug.Log("isMOving: " + isMoving);
        if (!isMoving) return; // If is not moving, skip this step
        if (GetPixelByOffset(0, 1).elementType == ElementType.EMPTYCELL || GetPixelByOffset(0, 1).isMoving) ApplyGravity();

        Element[] targetedPositions = CalculateVelocityTravel();
        // Item1 <- last empty cell that the velocity path found on calculation (The cell the pixel should move to... can be self if no cell found)
        // Item2 <- first non-empty cell that the path found (basically, what this cell hit when trying to move... is null if not stopped (or hit boundary, need to fix this))

        //Debug.DrawLine(new(pixelX / Screen.width * PixelGrid.gridWidth, pixelX / Screen.width * PixelGrid.gridWidth), new((pixelX +velocity.x) / Screen.width * PixelGrid.gridWidth, (pixelY+velocity.y) / Screen.width * PixelGrid.gridWidth), Color.red);


        //int randomDirection = UnityEngine.Random.Range(0, 2) * 2 - 1; // Returns -1 or 1 randomly
        
        if (targetedPositions[0] != this) { // Basically, if the pixel is moving (targetCell is not itself)
            if (targetedPositions[1] != null /*&& (targetedPositions[1].elementType != elementType || !targetedPositions[1].isMoving)*//* && Mathf.Approximately(Mathf.Floor(targetedPositions[1].velocity.y), 0)*/) { // If it was stopped by something
                if (targetedPositions[1].elementType == elementType) {
                    if(velocity.magnitude > targetedPositions[1].velocity.magnitude) {
                        velocity = targetedPositions[1].velocity;
                    }
                    else {
                        targetedPositions[1].velocity = velocity;
                    }
                    return;
                }
                float newY = Mathf.Min(velocity.y, targetedPositions[1].velocity.y);
                float newX = /*Mathf.Approximately(velocity.x, 0) ? moveDirection : */velocity.x + (newY * Mathf.Sign(velocity.x));

                velocity.x = newX * viscosity;
                velocity.y = newY;
            }
            //Debug.Log((pixelX - targetedPositions.Item1.pixelX) + ", " + (pixelY - targetedPositions.Item1.pixelY));
            SwapPixel(grid, this, targetedPositions[0]);
            if (velocity.y == 0 && Mathf.Abs(velocity.x) > 1) velocity.x *= 0.9f; 
        }
        else { // If the pixel has not moved
            //color = moveDirection == 1 ? Color.red : Color.blue;


            // I believe what's happening is that velocity y is slowing down as water travels on a surface, and newX doesnt necessarily always keep up. So it eventually slows down until it doesnt have enough velocity, and then changes direction. 
            if (Mathf.Abs(velocity.x) >= 1 && !CanMakeMove(moveDirection, 0) && CanMakeMove(-moveDirection, 0)) { // Despite not moving, the pixel still has horizontal velocity (something blocked it probably... )
                //Debug.DrawLine(new(pixelX - 0.5f, -pixelY + 0.5f, 0), new(targetedPositions[1].pixelX - 0.5f, -targetedPositions[1].pixelY + 0.5f, 0), lineColor, 0, false);
                if (GetPixelByOffset(-moveDirection, 1).elementType == ElementType.EMPTYCELL) SwapPixel(grid, this, GetPixelByOffset(0, 1));
                //else if (Mathf.Approximately(GetPixelByOffset(0, -1).velocity.x,velocity.x)) color = Color.magenta;
                //velocity.y = 0;
                velocity.x = -velocity.x/4;
                moveDirection = -moveDirection;
                // no return because we are in if/else rn
            }
            //Debug.Log("Did not move: " + velocity.y);

            else {
                moveDirection = UnityEngine.Random.Range(0, 2) * 2 - 1;
                //velocity.x = UnityEngine.Random.Range(0, 2) * 2 - 1;
                if (CanMakeMove(moveDirection, 1)) {
                    SwapPixel(grid, this, GetPixelByOffset(moveDirection, 1));
                    //Debug.Log("Manually ran for [" + moveDirection + ", 1]");
                    velocity.x = moveDirection * viscosity;
                }
                else if (CanMakeMove(-moveDirection, 1)) {
                    SwapPixel(grid, this, GetPixelByOffset(-moveDirection, 1));
                    //Debug.Log("Manually ran for [" + -moveDirection + ", 1]");
                    velocity.x = -moveDirection * viscosity;
                }
                else if (CanMakeMove(moveDirection, 0)) {
                    SwapPixel(grid, this, GetPixelByOffset(moveDirection, 0));
                    //Debug.Log("Manually ran for [" + moveDirection + ", 0]");
                    velocity.x = moveDirection * viscosity;
                }
                else if (CanMakeMove(-moveDirection, 0)) {
                    SwapPixel(grid, this, GetPixelByOffset(-moveDirection, 0));
                    //Debug.Log("Manually ran for [" + -moveDirection + ", 0]");
                    velocity.x = -moveDirection * viscosity;
                }
                else {
                    velocity = Vector2.zero;
                    isMoving = false;
                }
            }
        }

        Debug.DrawLine(new(pixelX - 0.5f, -pixelY + 0.5f, 0), new(pixelX + (int)velocity.x - 0.5f, -pixelY - (int)velocity.y + 0.5f, 0), lineColor, 0, false);

        //if (hasStepped) return; // Prevents cells from running twice in the same step
        //hasStepped = true;

        //int randomDirection = UnityEngine.Random.Range(0, 2) * 2 - 1; // Returns -1 or 1 randomly

        //isMoving = isMoving || (CanMakeMove(0, 1) || CanMakeMove(-1, 0) || CanMakeMove(1, 0)); // If isMoving is true, keep it. If not, see if it should be and set it appropriately
        //if (!isMoving) return; // If is not moving, skip this step

        //Element cellBelow = GetPixelByOffset(0, 1);
        //if (cellBelow != null && cellBelow.elementType == ElementType.EMPTYCELL) {
        //    ApplyGravity();
        //}

        //Tuple<Element, Element> targetedPositions = CalculateVelocityTravel();
        //// Item1 <- last empty cell that the velocity path found on calculation (The cell the pixel should move to... can be self if no cell found)
        //// Item2 <- first non-empty cell that the path found (basically, what this cell hit when trying to move... is null if not stopped (or hit boundary, need to fix this))

        //if (targetedPositions.Item1 != this) { // Basically, if the pixel is moving (targetCell is not itself)
        //    SwapPixel(grid, this, targetedPositions.Item1);
        //    if (targetedPositions.Item2 != null) {
        //        if (targetedPositions.Item2.isMoving) {
        //            velocity.x = Mathf.Clamp(velocity.x + velocity.y * viscosity * (velocity.x < 0 ? -1 : velocity.x > 0 ? 1 : moveDirection), -10, 10f);
        //            velocity.y = 0;
        //            if (Mathf.Approximately(velocity.x, 0)) {
        //                velocity = Vector2.zero;
        //                isMoving = false;
        //            }
        //        }
        //        else {
        //            if (CanMakeMove(moveDirection, 0)) {
        //                velocity.x = moveDirection;
        //                isMoving = true;
        //            }
        //            else if (CanMakeMove(-moveDirection, 0)) {
        //                velocity.x = -moveDirection;
        //            }
        //        }
        //    }
        //}
        //else { // If the pixel has not moved
        //    if (Mathf.Abs(velocity.x) >= 1) { // Despite not moving, the pixel still has horizontal velocity (something blocked it probably... )
        //        velocity.x = -velocity.x;
        //        moveDirection = -moveDirection;
        //        return;
        //    }
        //    else if (CanMakeMove(moveDirection, 0)) {
        //        //SwapPixel(grid, this, GetPixelByOffset(moveDirection, 0));
        //        velocity.x += moveDirection;
        //        velocity.y = 0;
        //    }
        //    else if (CanMakeMove(-moveDirection, 0)) {
        //        //SwapPixel(grid, this, GetPixelByOffset(-moveDirection, 0));
        //        velocity.x += -moveDirection;
        //        velocity.y = 0;
        //    }
        //    else {
        //        velocity = Vector2.zero;
        //        isMoving = false;
        //    }
        //}
    }

    private Element[] CalculateVelocityTravel() {

        Element[] returnArray = { this, null };

        //Element lastAvailableCell, firstUnavailableCell = null;

        Vector2Int lastValidPos = new(pixelX, pixelY); // Gets current position

        float xAbs = Mathf.Abs(velocity.x);
        if (xAbs < 0) xAbs = 1;
        float yAbs = Mathf.Abs(velocity.y);
        if (yAbs < 0) yAbs = 1;

        int upperBound = Mathf.Max((int)xAbs, (int)yAbs);
        int lowerBound = Mathf.Min((int)xAbs, (int)yAbs);
        float slope = (lowerBound == 0 || upperBound == 0) ? 0 : ((float)lowerBound / upperBound);

        int smallerCount;
        for (int i = 1; i <= upperBound; i++) {
            smallerCount = Mathf.RoundToInt(i * slope);

            int xIncrease = (xAbs > yAbs) ? i : smallerCount;
            int yIncrease = (xAbs <= yAbs) ? i : smallerCount;

            int newX = pixelX + (xIncrease * (velocity.x < 0 ? -1 : 1)); // All of the above is basically converting velocity into a point and calculating a slope from it and traversing the slope cell by cell
            int newY = pixelY + (yIncrease * (velocity.y < 0 ? -1 : 1)); // Kind of messy but should wor
                                                                         // k
            if (!PixelGrid.IsInBounds(newX, newY)) {
                returnArray[1] = grid.boundaryHit;
                break;
            }

            Element targetCell = grid.GetPixel(newX, newY);

            //if (targetCell == this) continue;
            if (targetCell.elementType != ElementType.EMPTYCELL) {
                returnArray[1] = targetCell;
                break;
            }
            lastValidPos = new Vector2Int(newX, newY);
            //if(velocity.y == 0 && targetCell.GetPixelByOffset(0, 1) != null && targetCell.GetPixelByOffset(0,1).elementType == ElementType.EMPTYCELL) {
            //    SwapPixel(grid, this, targetCell.GetPixelByOffset(0, 1));
            //    break;
            //}
        }

        returnArray[0] = grid.GetPixel(lastValidPos.x, lastValidPos.y);
        return returnArray;
        //return new Tuple<Element, Element>(lastAvailableCell, firstUnavailableCell);
    }

    // Not a very good name, change later... checks if the space should enable isMoving kinda
    public override bool CheckShouldMove() {
        return IsMovableCell(GetPixelByOffset(0, 1)) || 
               IsMovableCell(GetPixelByOffset(moveDirection, 1)) ||
               IsMovableCell(GetPixelByOffset(-moveDirection, 1)) ||
               IsMovableCell(GetPixelByOffset(moveDirection, 0)) ||
               IsMovableCell(GetPixelByOffset(-moveDirection, 0));
    }
}