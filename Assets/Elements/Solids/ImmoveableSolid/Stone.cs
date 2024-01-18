public class Stone : ImmoveableSolid {

    UnityEngine.Color _color = new UnityEngine.Color(0.8f, 0.8f, 0.8f);

    public Stone(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.STONE;
        color = _color;
        density = 3f;
        frictionFactor = 0.7f;
        bounciness = 0.1f;
    }

}