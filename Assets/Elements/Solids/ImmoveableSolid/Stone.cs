public class Stone : ImmoveableSolid {

    UnityEngine.Color _colorMin = new UnityEngine.Color(0.6f, 0.6f, 0.6f);
    UnityEngine.Color _colorMax = new UnityEngine.Color(0.55f,0.55f,0.55f);

    public Stone(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.STONE;
        color = UnityEngine.Color.Lerp(_colorMin, _colorMax, UnityEngine.Random.Range(0, 1f));
        density = 3f;
        frictionFactor = 0.7f;
        bounciness = 0.05f;
    }

}