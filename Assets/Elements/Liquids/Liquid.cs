using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class Liquid : Element {

    // Variables specifically for Liquid element properties
    protected float viscosity;

    Color lineColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

    protected Liquid(int x, int y, PixelGrid grid) {
        // Variables for Element properties
        moveDirection = UnityEngine.Random.Range(0, 2) * 2 - 1; // This determines whether the liquid will be moving to the left or right when flowing

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
        if (!isMoving) return; // If is not moving, skip this step
        if (GetPixelByOffset(0, 1).elementType == ElementType.EMPTYCELL || GetPixelByOffset(0, 1).isMoving) ApplyGravity();

        Element[] targetedPositions = CalculateVelocityTravel();
        // Item1 <- last empty cell that the velocity path found on calculation (The cell the pixel should move to... can be self if no cell found)
        // Item2 <- first non-empty cell that the path found (basically, what this cell hit when trying to move... is null if not stopped (or hit boundary, need to fix this))

        if (targetedPositions[0] != this) { // Basically, if the pixel is moving (targetCell is not itself)
            SwapPixel(grid, this, targetedPositions[0]);
            if (targetedPositions[1] != null && targetedPositions[1].velocity.y < 1) { // If it was stopped by something
                float newY = Mathf.Min(velocity.y, targetedPositions[1].velocity.y);
                float newX = (velocity.y * Mathf.Sign(velocity.x));

                velocity.x = newX * viscosity;
                velocity.y = newY;
                if (GetPixelByOffset(0, 1).elementType == ElementType.EMPTYCELL) ApplyGravity();
            }
        }
        else { // If the pixel has not moved
            if (targetedPositions[1] != null && targetedPositions[1].elementType == elementType) {
                if (velocity.magnitude < targetedPositions[1].velocity.magnitude) {
                    velocity = targetedPositions[1].velocity * 0.9f; // Might be able to remove these 0.9fs, idk if they do anything besides add more unneccessary calculation
                }
                else {
                    targetedPositions[1].velocity = velocity * 0.9f;
                }
            }
            if (Mathf.Abs(velocity.x) >= 1 && !CanMakeMove(moveDirection, 0) && CanMakeMove(-moveDirection, 0)) { // Despite not moving, the pixel still has horizontal velocity (something blocked it probably... )
                //if (GetPixelByOffset(-moveDirection, 1).elementType == ElementType.EMPTYCELL) SwapPixel(grid, this, GetPixelByOffset(0, 1));

                velocity.x = -velocity.x / 4;
                moveDirection = -moveDirection;
            }

            else {
                
                if (CanMakeMove(moveDirection, 1)) {
                    SwapPixel(grid, this, GetPixelByOffset(moveDirection, 1));
                    //Debug.Log("Manually ran for [" + moveDirection + ", 1]");
                    ApplyGravity();
                    velocity.x = moveDirection * viscosity;
                }
                else if (CanMakeMove(-moveDirection, 1)) {
                    SwapPixel(grid, this, GetPixelByOffset(-moveDirection, 1));
                    //Debug.Log("Manually ran for [" + -moveDirection + ", 1]");
                    ApplyGravity();
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
            if (!PixelGrid.IsInBounds(newX, newY)) {
                returnArray[1] = grid.boundaryHit;
                break;
            }

            Element targetCell = grid.GetPixel(newX, newY);

            if (targetCell.elementType != ElementType.EMPTYCELL) {
                returnArray[1] = targetCell;
                break;
            }
            lastValidPos = new Vector2Int(newX, newY);
        }

        returnArray[0] = grid.GetPixel(lastValidPos.x, lastValidPos.y);
        return returnArray;
    }

    //// Not a very good name, change later... checks if the space should enable isMoving kinda
    public override bool CheckShouldMove() {
        return IsMovableCell(GetPixelByOffset(0, 1)) ||
               IsMovableCell(GetPixelByOffset(moveDirection, 1)) ||
               IsMovableCell(GetPixelByOffset(-moveDirection, 1)) ||
               IsMovableCell(GetPixelByOffset(moveDirection, 0)) ||
               IsMovableCell(GetPixelByOffset(-moveDirection, 0));
    }
}