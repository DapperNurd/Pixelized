using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steam : Gas
{
    UnityEngine.Color lightGrey = new UnityEngine.Color(0.9f,0.9f,0.9f);

    public Steam(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.STEAM;
        color = lightGrey;
        density = 1f;
    }
}
