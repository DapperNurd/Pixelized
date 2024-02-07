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
    }

    public override void step(PixelGrid grid) {
        if (hasStepped) return; // Prevents cells from running twice in the same step
        hasStepped = true;

        lifetime++;
        if (CheckLifetime()) return;

        isMoving = isMoving || CheckShouldMove();
        if (!isMoving) return;

        moveDirection = Random.Range(0, 3) - 1; // This determines whether the liquid will be moving to the left or right when flowing

        if (CanMakeMove(moveDirection, -1)) {
            SwapPixel(grid, this, GetPixelByOffset(moveDirection, -1));
        }
        else if (CanMakeMove(moveDirection, 0))
        {
            SwapPixel(grid, this, GetPixelByOffset(moveDirection, 0));
        }
        else if (CanMakeMove(-moveDirection, 0))
        {
            SwapPixel(grid, this, GetPixelByOffset(-moveDirection, 0));
        }
        else if(moveDirection != 0) { // Changes horizontal move direction when it can't move horizontally... This is implemented so gas continues to flow the same direction until obstructed, then changes direction
            isMoving = false;
        }
    }

    public override bool CanMakeMove(int horizontalOffset, int verticalOffset)
    {

        int verticalDir = pixelY + verticalOffset;

        Element targetCell = GetPixelByOffset(horizontalOffset, verticalOffset);

        if (targetCell == null) return false; // Is null if out of bounds
        if (targetCell is MoveableSolid || targetCell is ImmoveableSolid) return false; // If target pos is an empty cell and this cell is an empty cell, cannot move

        if (targetCell.elementType == ElementType.EMPTYCELL) return true; // If target pos is an empty cell, can move

        return (targetCell.density > density && verticalDir < pixelY) || (targetCell.density < density && verticalDir >= pixelY); // If the density of target pos is higher than this cell and is below it (or vice versa), can move (swap)
    }

    public override bool CheckShouldMove() {
        return IsMovableCell(GetPixelByOffset(0, -1)) ||
        IsMovableCell(GetPixelByOffset(moveDirection, -1)) ||
        IsMovableCell(GetPixelByOffset(-moveDirection, -1)) ||
        IsMovableCell(GetPixelByOffset(moveDirection, 0)) ||
        IsMovableCell(GetPixelByOffset(-moveDirection, 0));
    }

    public virtual bool CheckLifetime() { return false; }
}
