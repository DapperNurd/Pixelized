using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public abstract class Liquid : Element {

    int moveDirection = Random.Range(1, 3) == 1 ? 1 : -1; // This determines whether the liquid will be moving to the left or right when flowing

    protected Liquid(int x, int y, PixelGrid grid) {
        // Variables for Element properties
        isSolid = false;

        // Variables for simulation
        pixelX = x;
        pixelY = y;
        this.grid = grid;
    }

    public override void step(PixelGrid grid)
    {
        if(hasStepped) return; // Prevents cells from running twice in the same step
        hasStepped = true;

        if(CanMakeMove(0, 1)) { // Down
            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX, pixelY+1]);
        }
        else if(CanMakeMove(-1, 1)) { // DownLeft
            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX-1, pixelY+1]);
        }
        else if(CanMakeMove(1, 1)) { // DownRight
            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX+1, pixelY+1]);
        }
        else if(CanMakeMove(moveDirection, 0)) { // Right or Left, depending on moveDirection (defaults to left)
            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX+moveDirection, pixelY]);
        }
        else { // Changes horizontal move direction when it can't move horizontally... This is implemented so liquid continues to flow the same direction until obstructed, then changes direction
            moveDirection = -moveDirection;
        }
    }
}