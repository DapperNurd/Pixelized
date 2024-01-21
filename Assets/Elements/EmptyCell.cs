using UnityEngine;

public class EmptyCell : Element {

    public EmptyCell(int x, int y, PixelGrid grid) {
        pixelX = x;
        pixelY = y;
        elementType = ElementType.EMPTYCELL;
        color = UnityEngine.Color.black;
        this.grid = grid;
        frictionFactor = 1f;
    }

    public override void step(PixelGrid grid)
    {
        return;
    }
    //protected override bool actOnNeighboringElement(Element neighbor, int modifiedMatrixX, int modifiedMatrixY, PixelGrid grid, bool isFinal, bool isFirst, UnityEngine.Vector2 lastValidLocation, int depth) {
    //    return false;
    //}
}