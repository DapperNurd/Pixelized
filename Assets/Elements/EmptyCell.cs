using UnityEngine;

public class EmptyCell : Element {

    public EmptyCell(int x, int y, PixelGrid grid) {
        pixelX = x;
        pixelY = y;
        elementType = ElementType.EMPTYCELL;
        color = UnityEngine.Color.black;
        this.grid = grid;
    }

    public override void step(PixelGrid grid)
    {
        return;
    }

}