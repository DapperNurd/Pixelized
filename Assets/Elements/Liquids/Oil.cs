public class Oil : Liquid {

    UnityEngine.Color grey = new UnityEngine.Color(0.25f,0.25f,0.25f);

    public Oil(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.OIL;
        color = grey;
        density = 0.5f;
        viscosity = 0.1f;
    }

}