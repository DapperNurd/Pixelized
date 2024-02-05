using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas : Element
{
    protected Gas(int x, int y, PixelGrid grid) {
        // Variables for Element properties

        // Variables for simulation
        pixelX = x;
        pixelY = y;
        this.grid = grid;
        moveDirection = Random.Range(1, 3) == 1 ? 1 : -1; // This determines whether the liquid will be moving to the left or right when flowing
    }

    public override void step(PixelGrid grid) {
        if (hasStepped) return; // Prevents cells from running twice in the same step
        hasStepped = true;

        if (CanMakeMove(0, -1)) { // Up
            SwapPixel(grid, this, GetPixelByOffset(0, -1));
        }
        else if (CanMakeMove(-1, 1)) { // UpLeft
            SwapPixel(grid, this, GetPixelByOffset(-1, -1));
        }
        else if (CanMakeMove(1, 1)) { // UpRight
            SwapPixel(grid, this, GetPixelByOffset(1, -1));
        }
        else if (CanMakeMove(moveDirection, 0)) { // Right or Left, depending on moveDirection (defaults to left)
            SwapPixel(grid, this, GetPixelByOffset(moveDirection, 0));
        }
        else { // Changes horizontal move direction when it can't move horizontally... This is implemented so gas continues to flow the same direction until obstructed, then changes direction
            moveDirection = -moveDirection;
        }
    }

    public override bool CheckShouldMove() { return false; }
}
