using Unity.VisualScripting;
using UnityEngine;

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

    public override void step(PixelGrid grid)
    {
        if(hasStepped) return; // Prevents cells from running twice in the same step
        hasStepped = true;

        if(CanMakeMove(0, 1)) { // Down
            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX, pixelY+1]);
        }
        else if(CanMakeMove(moveDirection, 1)) { // DownLeft
            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX+moveDirection, pixelY+1]);
        }
        else if(CanMakeMove(-moveDirection, 1)) { // DownRight
            SwapPixel(grid, grid.grid[pixelX, pixelY], grid.grid[pixelX-moveDirection, pixelY+1]);
        }
    }
}