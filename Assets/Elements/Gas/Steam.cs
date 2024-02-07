using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steam : Gas
{
    UnityEngine.Color _colorMin = new UnityEngine.Color(0.9f,0.9f,0.9f);
    UnityEngine.Color _colorMax = new UnityEngine.Color(0.85f, 0.85f, 0.85f);

    int lifetimeThreshold = 50 + Random.Range(0, 26);

    public Steam(int x, int y, PixelGrid grid) : base(x, y, grid) {
        // Variables for Element properties
        elementType = ElementType.STEAM;
        color = UnityEngine.Color.Lerp(_colorMin, _colorMax, UnityEngine.Random.Range(0, 1f));
        density = 0.1f;
    }

    public override bool CheckLifetime() {
        if (lifetime > lifetimeThreshold) {
            if (!IsExposed()) lifetime -= 60;
            else {
                if (Random.Range(0, 20) == 0) {
                    grid.SetPixel(pixelX, pixelY, CreateElement(ElementType.WATER, pixelX, pixelY, grid));
                }
                else grid.SetPixel(pixelX, pixelY, CreateElement(ElementType.EMPTYCELL, pixelX, pixelY, grid));
                return true;
            }
        }
        return false;
    }
}
