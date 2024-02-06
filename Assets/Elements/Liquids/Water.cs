public class Water : Liquid {

    UnityEngine.Color _colorMin = new UnityEngine.Color(0, 0.5f, 1);
    UnityEngine.Color _colorMax = new UnityEngine.Color(0f, 0.45f, 1f);

    public Water(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.WATER;
        color = UnityEngine.Color.Lerp(_colorMin, _colorMax, UnityEngine.Random.Range(0, 1f));
        density = 1f;
        flowRate = 4f;
    }

}