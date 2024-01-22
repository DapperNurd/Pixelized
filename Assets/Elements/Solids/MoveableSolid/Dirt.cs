public class Dirt : MoveableSolid {

    UnityEngine.Color _colorMin = new UnityEngine.Color(0.43f, 0.29f, 0.2f);
    UnityEngine.Color _colorMax = new UnityEngine.Color(0.22f, 0.15f, 0.1f);

    public Dirt(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.DIRT;
        color = UnityEngine.Color.Lerp(_colorMin, _colorMax, UnityEngine.Random.Range(0, 1f));
        density = 5f;
        frictionFactor = 0.6f;
        bounciness = 0f;
        inertiaResistance = 0.6f;
    }
}