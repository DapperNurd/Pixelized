using JetBrains.Annotations;
using System;
using System.IO.IsolatedStorage;
using Unity.VisualScripting;
using UnityEngine;

public abstract class MoveableSolid : Element {

    protected MoveableSolid(int x, int y, PixelGrid grid) {
        // Variables for Element properties
        isSolid = true;

        // Variables for simulation
        pixelX = x;
        pixelY = y;
        this.grid = grid;
    }

    public override void step(PixelGrid grid) {
        if (hasStepped) return; // Prevents cells from running twice in the same step
        hasStepped = true;

        velocity = Vector2.ClampMagnitude(velocity + (gravity * Time.deltaTime), 10f); // Adds gravity to velocity, clamps it to be between -10f and 10f

        Element cellBelow = GetPixelByOffset(0, 1);
        if(cellBelow != null) {
            if (GetPixelByOffset(0, 1) is MoveableSolid || GetPixelByOffset(0, 1) is ImmoveableSolid) {
                if (!isMoving) return;
                velocity.y /= 2f;
            }
            else { // If cell below is Liquid, Gas, Empty, or anything but solid
                isMoving = true;
            }
        }

        Tuple<Element, Element> targetedPositions = CalculateVelocityTravel();
        // Item1 <- last empty cell that the velocity path found on calculation
        // Item2 <- first non-empty cell that the path found (basically, what this cell hit when trying to move)

        int randomDirection = UnityEngine.Random.Range(0, 2)*2 - 1; // Returns -1 or 1 randomly
        //int randomDirection = pixelX%2==0 ? 1 : -1; // Returns -1 or 1 whether the pixelX count is even or odd
        // Possible TODO: Add a hasNotMoved bool or something similar, make it so at the beginning of step it checks for this and checks for an empty cell below...
        ///// If it has not moved and the below cell is not empty (or liquid i guess) then just return and skip any processing
        if (targetedPositions.Item1 != this) { // Basically, if the pixel is moving (targetCell is not itself)
            SwapPixel(grid, this, targetedPositions.Item1);
            if(targetedPositions.Item2 != null) { // If it was stopped by something
                velocity.x = velocity.y * System.Math.Sign(velocity.x);
            }
            velocity.x *= frictionFactor * targetedPositions.Item1.frictionFactor; // Apply friction forces on velocity.x ... probably needs tweaking
        }
        else { // If the pixel has not moved
            if (Mathf.Abs(velocity.x) >= 1) { // Despite not moving, the pixel still has horizontal velocity (something blocked it probably... might check if its Abs() is >= 1)
                velocity.x = -velocity.x;
                velocity.x *= frictionFactor * targetedPositions.Item1.frictionFactor;
                return;
            }
            //else if (isMoving) {
            if (UnityEngine.Random.Range(0, 1f) < inertiaResistance) {
                isMoving = false;
                return;
            }
            else if (CanMakeMove(randomDirection, 1)) {
                SwapPixel(grid, this, GetPixelByOffset(randomDirection, 1));
                velocity.x = UnityEngine.Random.Range(0, 2) * 2 - 1;
                velocity.y += velocity.y + (gravity.y * Time.deltaTime);
            }
            else if (CanMakeMove(-randomDirection, 1)) {
                SwapPixel(grid, this, GetPixelByOffset(-randomDirection, 1));
                velocity.x = -UnityEngine.Random.Range(0, 2) * 2 - 1;
                velocity.y += velocity.y + (gravity.y * Time.deltaTime);
            }
            else {
                isMoving = false;
            }
        }
        //else if (velocity.x != 0) { // If the cell has not moved, but has horizontal velocity
        //    if (CanMakeMove(xDirection, 1)) {
        //        SwapPixel(grid, this, GetPixelByOffset(xDirection, 1));
        //        if (xDirection != 0) velocity.x = xDirection;
        //    }
        //    else velocity.x = -velocity.x;
        //}
        //else velocity.x = ; // If the cell has not moved and has no horizontal velocity

        //else if (CanMakeMove(moveDirection, 1)) { // DownLeft
        //    SwapPixel(grid, this, grid.grid[pixelX + moveDirection, pixelY + 1]);
        //}
        //else if (CanMakeMove(-moveDirection, 1)) { // DownRight
        //    SwapPixel(grid, this, grid.grid[pixelX - moveDirection, pixelY + 1]);
        //}


        //if (isFalling) velocity += Element.gravity; // Adds gravity to the element
        ////if (isFalling) {
        ////    velocity.x *= 0.9f; // This is sort of simulating air resistance... slows horiztonal movement while in air
        ////}

        //float velX = Mathf.Abs(velocity.x) / 10;
        //float velY = Mathf.Abs(velocity.y) / 10;

        ////Vector2 startPos = new(pixelX, pixelY); // Will probably need later
        //lastValidPos = new(pixelX, pixelY);
        //IterateThroughSteps(velX, velY);
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
            if (!PixelGrid.IsInBounds(newX, newY)) break;

            Element targetCell = grid.GetPixel(newX, newY);

            if (targetCell == this) continue;
            if (targetCell.elementType != ElementType.EMPTYCELL) {
                firstUnavailableCell = targetCell;
                break;
            }
            else {
                foreach (Element neighbor in targetCell.GetHorizontalNeighbors()) {
                    if (neighbor == null) continue;
                    if (neighbor is MoveableSolid || neighbor is ImmoveableSolid) {
                        neighbor.isMoving = UnityEngine.Random.Range(0, 1f) > neighbor.inertiaResistance || neighbor.isMoving;
                    }
                }
            }
            lastValidPos = new Vector2Int(newX, newY);
        }

        lastAvailableCell = grid.GetPixel(lastValidPos.x, lastValidPos.y);

        return new Tuple<Element, Element>(lastAvailableCell, firstUnavailableCell);
    }
    //protected void IterateThroughSteps(float velX, float velY) {
    //    int xModifier = velocity.x < 0 ? -1 : 1; // Keeps track of negative or positive movement because we are getting the absolute values
    //    int yModifier = velocity.y < 0 ? -1 : 1; //

    //    int upperBound = Mathf.Max((int)velX, (int)velY);
    //    int lowerBound = Mathf.Min((int)velX, (int)velY);
    //    float slope = (lowerBound == 0 || upperBound == 0) ? 0 : ((float)lowerBound / upperBound);

    //    int smallerCount;
    //    for (int i = 1; i <= upperBound; i++) {
    //        smallerCount = Mathf.RoundToInt(i * slope);

    //        int xIncrease = (velX > velY) ? i : smallerCount;
    //        int yIncrease = (velX <= velY) ? i : smallerCount;

    //        int modifiedPixelX = pixelX + (xIncrease * xModifier); // All of the above is basically converting velocity into a point and calculating a slope from it and traversing the slope cell by cell
    //        int modifiedPixelY = pixelY + (yIncrease * yModifier); // Kind of messy but should work
    //        if (PixelGrid.IsInBounds(modifiedPixelX, modifiedPixelY)) {
    //            Element hitCell = grid.grid[modifiedPixelX, modifiedPixelY];
    //            if (hitCell == this) continue;
    //            bool hasStopped = Somethingidk(grid, hitCell, modifiedPixelX, modifiedPixelY, i == 1, i == upperBound, 0);
    //            if (hasStopped) break;

    //            lastValidPos = new Vector2(modifiedPixelX, modifiedPixelY);
    //        }
    //        else {
    //            grid.grid[(int)lastValidPos.x, (int)lastValidPos.y] = this;
    //            velocity.x = 0;
    //            break;
    //        }
    //    }
    //}

    ////protected override bool actOnNeighboringElement(Element neighbor, int modifiedMatrixX, int modifiedMatrixY, PixelGrid grid, bool isFinal, bool isFirst, UnityEngine.Vector2 lastValidLocation, int depth) {
    //protected bool Somethingidk(PixelGrid grid, Element hitCell, int newX, int newY, bool isFirstStep, bool isFinalStep, int depth) {
    //    if(hitCell is EmptyCell) {
    //        if (isFinalStep) {
    //            isFalling = true;
    //            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[newX, newY]);
    //            return true; // This shouldn't really be needed because it should be the last iteration anyways (finalStep) but having it here just to be safe... finalStep being true would just break the loop of checking
    //        }
    //    }
    //    else if (hitCell is MoveableSolid || hitCell is ImmoveableSolid) {
    //        if (depth > 0) return true; // If the function is called recursively (depth is how deep the function call is)
    //        if (isFalling) {
    //            velocity.x = velocity.x < 0 ? -velocity.y : velocity.x > 0 ? velocity.y : velocity.y * moveDirection;
    //        }

    //        Vector2 velocityNormal = velocity.normalized;
    //        int normalizedX = velocityNormal.x > 0.1f ? 1 : velocityNormal.x < -0.1f ? -1 : 0; // Normalized but rounded
    //        int normalizedY = velocityNormal.y > 0.1f ? 1 : velocityNormal.y < -0.1f ? -1 : 0; // 

    //        // This stuff breaks everything lol
    //        //if (PixelGrid.IsInBounds(pixelX + normalizedX, pixelY + normalizedY)) {
    //        //    if (!Somethingidk(grid, grid.grid[pixelX + normalizedX, pixelY + normalizedY], pixelX + normalizedX, pixelY + normalizedY, true, false, depth + 1)) {
    //        //        isFalling = true;
    //        //        return true;
    //        //    }
    //        //}

    //        //if (PixelGrid.IsInBounds(pixelX + normalizedX, pixelY)) {
    //        //    if (!Somethingidk(grid, grid.grid[pixelX + normalizedX, pixelY], pixelX + normalizedX, pixelY, true, false, depth + 1)) {
    //        //        velocity.x = -velocity.x;
    //        //    }
    //        //    else {
    //        //        isFalling = false;
    //        //        return true;
    //        //    }
    //        //}

    //        velocity.y = Mathf.Min(velocity.y, hitCell.velocity.y);//-velocity.y * bounciness * hitCell.bounciness; // Might need to remove
    //        if (isFinalStep && velocity.y == 0) isFalling = false;
    //        velocity.x *= frictionFactor * hitCell.frictionFactor;
    //        if (isFirstStep) velocity.y = 120;

    //        SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[(int)lastValidPos.x, (int)lastValidPos.y]);
    //        return true;
    //    }
    //    else if (hitCell is Liquid) {

    //    }
    //    //else if (hitCell is Gas) {

    //    //}
    //    return false;
    //}



    //public override void step(PixelGrid grid) {
    //    if (hasStepped) return; // Prevents cells from running twice in the same step
    //    hasStepped = true;

    //    if (CanMakeMove(0, 1)) { // Down
    //        SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX, pixelY + 1]);
    //    }
    //    else if (CanMakeMove(moveDirection, 1)) { // DownLeft
    //        SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX + moveDirection, pixelY + 1]);
    //    }
    //    else if (CanMakeMove(-moveDirection, 1)) { // DownRight
    //        SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX - moveDirection, pixelY + 1]);
    //    }
    //}
}