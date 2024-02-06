using JetBrains.Annotations;
using System;
using System.IO.IsolatedStorage;
using Unity.VisualScripting;
using UnityEngine;

public abstract class MoveableSolid : Element {

    protected MoveableSolid(int x, int y, PixelGrid grid) {
        // Variables for Element properties

        // Variables for simulation
        pixelX = x;
        pixelY = y;
        this.grid = grid;

        // Default values for if I forget to override lol
        density = 1f;
        frictionFactor = 1f;
        inertiaResistance = 0.5f;
}

    public override void step(PixelGrid grid) {
        if (hasStepped) return; // Prevents cells from running twice in the same step
        hasStepped = true;

        lifetime++;

        isMoving = isMoving || CheckShouldMove(); // If isMoving is true, keep it. If not, see if it should be and set it appropriately
        if (!isMoving) return; // If is not moving, skip this step

        ApplyGravity();

        Element[] targetedPositions = CalculateVelocityTravel();
        // Item1 <- last empty cell that the velocity path found on calculation (The cell the pixel should move to... can be self if no cell found)
        // Item2 <- first non-empty cell that the path found (basically, what this cell hit when trying to move... is null if not stopped (or hit boundary, need to fix this))

        int randomDirection = UnityEngine.Random.Range(0, 2)*2 - 1; // Returns -1 or 1 randomly
        if (targetedPositions[0] != this) { // Basically, if the pixel is moving (targetCell is not itself)
            SwapPixel(grid, this, targetedPositions[0]);
            if (targetedPositions[0] is Liquid) targetedPositions[0].velocity = new(UnityEngine.Random.Range(0, 5) - 2, -velocity.y * 0.3f);
            if (targetedPositions[1] != null) { // If it was stopped by something
                velocity.x = velocity.y * System.Math.Sign(velocity.x);
            }
            velocity.x *= frictionFactor * targetedPositions[0].frictionFactor; // Apply friction forces on velocity.x ... probably needs tweaking
        }
        else { // If the pixel has not moved
            if (Mathf.Abs(velocity.x) >= 1) { // Despite not moving, the pixel still has horizontal velocity (something blocked it probably... )
                velocity.x = -velocity.x;
                velocity.x *= frictionFactor * targetedPositions[0].frictionFactor;
                return;
            }
            if (UnityEngine.Random.Range(0, 1f) < inertiaResistance) {
                velocity = Vector2.zero;
                isMoving = false;
                return;
            }
            else if (CanMakeMove(randomDirection, 1)) {
                SwapPixel(grid, this, GetPixelByOffset(randomDirection, 1));
                velocity.x = UnityEngine.Random.Range(0, 2) * 2 - 1;
                ApplyGravity();
            }
            else if (CanMakeMove(-randomDirection, 1)) {
                SwapPixel(grid, this, GetPixelByOffset(-randomDirection, 1));
                velocity.x = -UnityEngine.Random.Range(0, 2) * 2 - 1;
                ApplyGravity();
            }
            else {
                velocity = Vector2.zero;
                isMoving = false;
            }
        }
    }

    private Element[] CalculateVelocityTravel() {

        Element[] returnArray = { this, null };

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
            if (!(targetCell is EmptyCell)) {
                returnArray[1] = targetCell;
                if(targetCell is MoveableSolid || targetCell is ImmoveableSolid) break;
            }
            foreach (Element neighbor in targetCell.GetHorizontalNeighbors()) { // Attempt to set neighbors isMoving true, might make this a function
                if (neighbor == null) continue;
                if (neighbor is MoveableSolid || neighbor is ImmoveableSolid) {
                    neighbor.isMoving = UnityEngine.Random.Range(0, 1f) > neighbor.inertiaResistance || neighbor.isMoving;
                }
            }
            // The idea here is that if the target cell is a liquid (and the current cell is not moving through liquid [hence the check for empty cell],
            // it will swap the liquid 
            //if(targetCell is Liquid) {
            //    if(i == upperBound && grid.GetPixel((Vector2)lastValidPos).elementType == ElementType.EMPTYCELL) {
            //        //SwapPixel(grid, this, grid.GetPixel((Vector2)lastValidPos));
            //        targetCell.velocity = new(UnityEngine.Random.Range(0, 5) - 2, -velocity.y*0.5f);
            //    }
            //}
            /*else*/ lastValidPos = new Vector2Int(newX, newY);
        }

        returnArray[0] = grid.GetPixel(lastValidPos.x, lastValidPos.y);
        return returnArray;
    }

    public override bool CheckShouldMove() {
        return IsMovableCell(GetPixelByOffset(0, 1));
    }
}