public class Dirt : MoveableSolid {

    UnityEngine.Color _color = new UnityEngine.Color(0.43f, 0.29f, 0.2f);

    public Dirt(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.DIRT;
        color = _color;
        density = 5f;
    }

}