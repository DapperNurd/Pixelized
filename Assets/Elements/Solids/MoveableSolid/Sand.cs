public class Sand : MoveableSolid {

    UnityEngine.Color _colorMin = new UnityEngine.Color(0.97f, 0.91f, 0.64f);
    UnityEngine.Color _colorMax = new UnityEngine.Color(0.86f, 0.78f, 0.55f);

    public Sand(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.SAND;
        color = UnityEngine.Color.Lerp(_colorMin, _colorMax, UnityEngine.Random.Range(0, 1f));
        density = 3f;
        frictionFactor = 0.75f;
        inertiaResistance = 0.15f;
    }

}