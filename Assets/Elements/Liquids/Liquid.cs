using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class Liquid : Element {

    // Variables specifically for Liquid element properties
    protected float viscosity = 1f;

    int moveDirection = UnityEngine.Random.Range(0, 2) * 2 - 1; // This determines whether the liquid will be moving to the left or right when flowing

    protected Liquid(int x, int y, PixelGrid grid) {
        // Variables for Element properties

        // Variables for simulation
        pixelX = x;
        pixelY = y;
        this.grid = grid;
    }

    public override void step(PixelGrid grid) {
        if (hasStepped) return; // Prevents cells from running twice in the same step
        hasStepped = true;

        color = Color.white;
        isMoving = isMoving || CheckShouldMove(); // If isMoving is true, keep it. If not, see if it should be and set it appropriately
        //Debug.Log("isMOving: " + isMoving);
        if (!isMoving) return; // If is not moving, skip this step
        color = Color.blue;
        ApplyGravity();

        Tuple<Element, Element> targetedPositions = CalculateVelocityTravel();
        // Item1 <- last empty cell that the velocity path found on calculation (The cell the pixel should move to... can be self if no cell found)
        // Item2 <- first non-empty cell that the path found (basically, what this cell hit when trying to move... is null if not stopped (or hit boundary, need to fix this))

        //Debug.DrawLine(new(pixelX / Screen.width * PixelGrid.gridWidth, pixelX / Screen.width * PixelGrid.gridWidth), new((pixelX +velocity.x) / Screen.width * PixelGrid.gridWidth, (pixelY+velocity.y) / Screen.width * PixelGrid.gridWidth), Color.red);

        Debug.DrawLine(new(pixelX, -pixelY, 0), new(pixelX + velocity.x, -pixelY - velocity.y, 0), Color.red, 0, false);

        //int randomDirection = UnityEngine.Random.Range(0, 2) * 2 - 1; // Returns -1 or 1 randomly
        if (targetedPositions.Item2 != null/* && !targetedPositions.Item2.isMoving*/) { // If it was stopped by something
            //Debug.Log("Hit something: " + velocity.y);
            //velocity.x = velocity.y * System.Math.Sign(velocity.x);
            //velocity.x = Mathf.Clamp(velocity.x + velocity.y * viscosity * (velocity.x < 0 ? -1 : velocity.x > 0 ? 1 : moveDirection), -10, 10f);

            //if(targetedPositions.Item2.elementType == this.elementType) {
            //    Vector2 tempVel = velocity;
            //    velocity = targetedPositions.Item2.velocity;
            //    targetedPositions.Item2.velocity = tempVel;
            //    return;
            //}
            //Debug.Log("vertically moving stuf ye");
            int newY = Mathf.FloorToInt(Mathf.Abs(velocity.y / 2)); // IDK if Abs will break this or not but I'm doing it because of gravity lol
            float newX = Mathf.Approximately(velocity.x, 0) ? moveDirection : velocity.x + (newY * Mathf.Sign(velocity.x));
            
            velocity.x = newX;
            velocity.y = newY;
        }
        if (targetedPositions.Item1 != this) { // Basically, if the pixel is moving (targetCell is not itself)
            //Debug.Log((pixelX - targetedPositions.Item1.pixelX) + ", " + (pixelY - targetedPositions.Item1.pixelY));
            SwapPixel(grid, this, targetedPositions.Item1);
            
        }
        else { // If the pixel has not moved
            //color = moveDirection == 1 ? Color.red : Color.blue;
            if (Mathf.Abs(velocity.x) >= 1 && !IsMovableCell(GetPixelByOffset((int)velocity.x, 0))) { // Despite not moving, the pixel still has horizontal velocity (something blocked it probably... )
                //Debug.Log("changing direction... velX: " + velocity.x); // THIS IS WHERE ITS NOT WORKING I THINK
                velocity.x = -velocity.x;
                moveDirection = -moveDirection;
                color = Color.red;
                //return;
            }
            //Debug.Log("Did not move: " + velocity.y);

            //if (CanMakeMove(moveDirection, 1)) {
            //    SwapPixel(grid, this, GetPixelByOffset(moveDirection, 1));
            //    velocity.x = moveDirection;
            //}
            //else if (CanMakeMove(-moveDirection, 1)) {
            //    SwapPixel(grid, this, GetPixelByOffset(-moveDirection, 1));
            //    velocity.x = --moveDirection;
            //}
            if (CanMakeMove(moveDirection, 0)) {
                SwapPixel(grid, this, GetPixelByOffset(moveDirection, 0));
                velocity.x = moveDirection;
            }
            else if (CanMakeMove(-moveDirection, 0)) {
                SwapPixel(grid, this, GetPixelByOffset(-moveDirection, 0));
                velocity.x = moveDirection;
            }
            else {
                velocity = Vector2.zero;
                isMoving = false;
            }
        }

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

    private Tuple<Element, Element> CalculateVelocityTravel() {

        Element lastAvailableCell, firstUnavailableCell = null;

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
                firstUnavailableCell = grid.boundaryHit;
                break;
            }

            Element targetCell = grid.GetPixel(newX, newY);

            if (targetCell == this) continue;
            if (targetCell.elementType != ElementType.EMPTYCELL) {
                firstUnavailableCell = targetCell;
                break;
            }
            lastValidPos = new Vector2Int(newX, newY);
            //if(velocity.y == 0 && targetCell.GetPixelByOffset(0, 1) != null && targetCell.GetPixelByOffset(0,1).elementType == ElementType.EMPTYCELL) {
            //    SwapPixel(grid, this, targetCell.GetPixelByOffset(0, 1));
            //    break;
            //}
        }

        lastAvailableCell = grid.GetPixel(lastValidPos.x, lastValidPos.y);
        return new Tuple<Element, Element>(lastAvailableCell, firstUnavailableCell);
    }

    public override bool CheckShouldMove() {
        if (IsMovableCell(GetPixelByOffset(0, 1))) {
            return true;
        }
        else if (IsMovableCell(GetPixelByOffset(moveDirection, 1))) {
            return true;
        }
        else if (IsMovableCell(GetPixelByOffset(-moveDirection, 1))) {
            return true;
        }
        else if (IsMovableCell(GetPixelByOffset(moveDirection, 0))) {
            //velocity.x = moveDirection;
            //float absX = Mathf.Abs(velocity.x);
            //if (absX > 0 && absX < 1) velocity.x = 1 * Mathf.Sign(velocity.x); // This basically ensures that if it just started falling, it will actually register as falling
            return true;
        }
        else if (IsMovableCell(GetPixelByOffset(-moveDirection, 0))) {
            //velocity.x = -moveDirection;
            //float absX = Mathf.Abs(velocity.x);
            //if (absX > 0 && absX < 1) velocity.x = 1 * Mathf.Sign(velocity.x); // This basically ensures that if it just started falling, it will actually register as falling
            return true;
        }

        return false;
    }
}