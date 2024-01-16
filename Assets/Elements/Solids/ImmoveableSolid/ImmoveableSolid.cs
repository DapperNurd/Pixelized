using Unity.VisualScripting;
using UnityEngine;

public abstract class ImmoveableSolid : Element {

    protected ImmoveableSolid(int x, int y, PixelGrid grid) {
        // Variables for Element properties
        isSolid = true;

        // Variables for simulation
        pixelX = x;
        pixelY = y;
        this.grid = grid;
    }

    public override void step(PixelGrid grid)
    {
        return;
    }

}