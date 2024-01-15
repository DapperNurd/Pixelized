public class Sand : MoveableSolid {

    UnityEngine.Color _color = new UnityEngine.Color(0.97f, 0.91f, 0.64f);

    public Sand(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.SAND;
        color = _color;
        density = 3f;
    }

}