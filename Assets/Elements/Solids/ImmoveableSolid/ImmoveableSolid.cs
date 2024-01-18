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

    //protected override bool actOnNeighboringElement(Element neighbor, int modifiedMatrixX, int modifiedMatrixY, PixelGrid grid, bool isFinal, bool isFirst, UnityEngine.Vector2 lastValidLocation, int depth) {
    //    return false;
    //}
}