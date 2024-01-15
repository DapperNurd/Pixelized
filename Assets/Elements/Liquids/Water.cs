public class Water : Liquid {

    UnityEngine.Color blue = new UnityEngine.Color(0,0.5f,1);

    public Water(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.WATER;
        color = blue;
        density = 1f;
    }

}