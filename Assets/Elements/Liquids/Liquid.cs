using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public abstract class Liquid : Element {

    // Variables specifically for Liquid element properties
    protected float viscosity = 1f;

    int moveDirection = UnityEngine.Random.Range(1, 3) == 1 ? 1 : -1; // This determines whether the liquid will be moving to the left or right when flowing

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

        int randomDirection = UnityEngine.Random.Range(0, 2) * 2 - 1; // Returns -1 or 1 randomly

        if (CanMakeMove(0,1)) {
            SwapPixel(grid, this, GetPixelByOffset(0, 1));
            velocity = Vector2.ClampMagnitude(velocity + (gravity * Time.deltaTime), 10f); // Adds gravity to velocity, clamps it to be between -10f and 10f
            if (velocity.y > 0 && velocity.y < 1) velocity.y = 1f; // This basically ensures that if it just started falling, it will actually register as falling
            isMoving = true;
        }
        else if (CanMakeMove(randomDirection, 0)) {
            SwapPixel(grid, this, GetPixelByOffset(randomDirection, 0));
            velocity.x = randomDirection;
            isMoving = true;
        }
        else if (CanMakeMove(-randomDirection, 0))
        {
            SwapPixel(grid, this, GetPixelByOffset(-randomDirection, 0));
            velocity.x = -randomDirection;
            isMoving = true;
        }
        else if (!isMoving) return;

        Tuple<Element, Element> targetedPositions = CalculateVelocityTravel();
        // Item1 <- last empty cell that the velocity path found on calculation (The cell the pixel should move to... can be self if no cell found)
        // Item2 <- first non-empty cell that the path found (basically, what this cell hit when trying to move... is null if not stopped (or hit boundary, need to fix this))

        if (targetedPositions.Item1 != this) { // Basically, if the pixel is moving (targetCell is not itself)
            SwapPixel(grid, this, targetedPositions.Item1);
            if (targetedPositions.Item2 != null) {
                velocity.x = velocity.y * viscosity * (velocity.x < 0 ? -1 : velocity.x > 0 ? 1 : randomDirection);
                velocity.y /= 2f;
            }
        }
        else { // If the pixel has not moved
            if (Mathf.Abs(velocity.x) >= 1) { // Despite not moving, the pixel still has horizontal velocity (something blocked it probably... )
                velocity.x = -velocity.x;
                return;
            }
            else {
                velocity = Vector2.zero;
                isMoving = false;
            }
        }
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
            lastValidPos = new Vector2Int(newX, newY);
        }

        lastAvailableCell = grid.GetPixel(lastValidPos.x, lastValidPos.y);
        return new Tuple<Element, Element>(lastAvailableCell, firstUnavailableCell);
    }

    public override bool CheckShouldMove() {
        Element[] elementsToCheck = { GetPixelByOffset(-1, 0), GetPixelByOffset(1, 0), GetPixelByOffset(0, 1) };
        foreach(Element element in elementsToCheck) {
            if (!(element == null || (element is MoveableSolid && !element.isMoving) || element is ImmoveableSolid)) return true;
        }
        return false;
    }
}