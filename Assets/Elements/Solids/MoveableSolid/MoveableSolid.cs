using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class MoveableSolid : Element {

    int moveDirection = Random.Range(1, 3) == 1 ? 1 : -1; // This determines whether the liquid will be moving to the left or right when flowing

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

        if (isFalling) {
            velocity += Element.gravity; // Adds gravity to the element
            velocity.x *= 0.9f; // This is sort of simulating air resistance... slows horiztonal movement while in air
        }

        int xModifier = velocity.x < 0 ? -1 : 1; // Keeps track of negative or positive movement because we are getting the absolute values
        int yModifier = velocity.y < 0 ? -1 : 1; //
        float velXDelta = Mathf.Abs(velocity.x) * 0.1f;
        float velYDelta = Mathf.Abs(velocity.y) * 0.1f;
        float velX = (velXDelta > 0 && velXDelta < 1) ? 1 : velXDelta; // Kind of a janky correction to stop the velocity from stalling when below 1
        float velY = (velYDelta > 0 && velYDelta < 1) ? 1 : velYDelta; //

        int upperBound = Mathf.Max((int)velX, (int)velY);
        int lowerBound = Mathf.Min((int)velX, (int)velY);
        float slope = (lowerBound == 0 || upperBound == 0) ? 0 : ((float)lowerBound / upperBound);

        Vector2 lastPos = new(pixelX, pixelY);
        lastValidPos = lastPos;
        int smallerCount;
        for (int i = 1; i <= upperBound; i++) {
            smallerCount = Mathf.RoundToInt(i * slope);

            int xIncrease = (velX > velY) ? i : smallerCount;
            int yIncrease = (velX > velY) ? smallerCount : i;

            int modifiedPixelX = pixelX + (xIncrease * xModifier); // All of the above is basically converting velocity into a point and calculating a slope from it and traversing the slope cell by cell
            int modifiedPixelY = pixelY + (yIncrease * yModifier); // Kind of messy but should work
            if (PixelGrid.IsInBounds(modifiedPixelX, modifiedPixelY)) {
                Element hitCell = grid.grid[modifiedPixelX, modifiedPixelY];
                if (hitCell == this) continue;
                bool hasStopped = Somethingidk(grid, hitCell, modifiedPixelX, modifiedPixelY, i == upperBound);
                if (hasStopped) break;

                lastValidPos = new Vector2(modifiedPixelX, modifiedPixelY);
            }
            else {
                grid.grid[(int)lastValidPos.x, (int)lastValidPos.y] = this;
                break;
            }
        }
    }

    //protected override bool actOnNeighboringElement(Element neighbor, int modifiedMatrixX, int modifiedMatrixY, PixelGrid grid, bool isFinal, bool isFirst, UnityEngine.Vector2 lastValidLocation, int depth) {
    public bool Somethingidk(PixelGrid grid, Element hitCell, int newX, int newY, bool finalStep) {
        if(hitCell is EmptyCell) {
            if (finalStep) {
                SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[newX, newY]);
                return true; // This shouldn't really be needed because it should be the last iteration anyways (finalStep) but having it here just to be safe.
            }
            else return false;
        }
        else if (hitCell is MoveableSolid || hitCell is ImmoveableSolid) {
            //float absY = Mathf.Max(Mathf.Abs(velocity.y) / 31, 105);
            if (isFalling) {
                velocity.x = velocity.x < 0 ? -velocity.y : velocity.x > 0 ? velocity.y : velocity.y * moveDirection;
                velocity.y = -velocity.y * 0.1f; // Might need to remove
                if(velocity.y == 0) isFalling = false;
                velocity.x *= frictionFactor * hitCell.frictionFactor;
            }
            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[(int)lastValidPos.x, (int)lastValidPos.y]);
            return true;
        }
        else if (hitCell is Liquid) {

        }
        //else if (hitCell is Gas) {

        //}
        return false;
    }




    // if(hasStepped) return; // Prevents cells from running twice in the same step
    // hasStepped = true;

    // if(CanMakeMove(0, 1)) { // Down
    //     SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX, pixelY+1]);
    // }
    // else if(CanMakeMove(moveDirection, 1)) { // DownLeft
    //     SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX+moveDirection, pixelY+1]);
    // }
    // else if(CanMakeMove(-moveDirection, 1)) { // DownRight
    //     SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX-moveDirection, pixelY+1]);
    // }
}