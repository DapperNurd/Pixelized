using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    int width = PixelGrid.gridWidth;
    int height = PixelGrid.gridHeight;

    Texture2D texture;

    PixelGrid grid;

    public bool drawModeDebug = false;
    public enum DrawBy {
        isMoving,
        velocity,
    }
    public DrawBy debugMode;

    void Start()
    {
        texture = new Texture2D(width, height);
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        grid = GetComponent<PixelGrid>();

        // RenderGrid();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        RenderGrid();
    }

    void RenderGrid() {
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                Color colorToDraw = grid.GetPixel(x, y).color;
                if (drawModeDebug) colorToDraw = GetDebugColor(grid.GetPixel(x, y));
                texture.SetPixel(x, y, colorToDraw);
            }
        }
        texture.Apply();
    }

    private Color GetDebugColor(Element element) {
        switch(debugMode) {
            case DrawBy.isMoving:
                if (element.elementType == ElementType.EMPTYCELL || element is ImmoveableSolid) return element.color;
                return element.isMoving ? Color.red : Color.blue;
            case DrawBy.velocity:
                if (element.elementType == ElementType.EMPTYCELL) return Color.white;
                if (element is ImmoveableSolid) return element.color;
                Vector2 absVel = new(Mathf.Abs(element.velocity.x), Mathf.Abs(element.velocity.y));
                return new Color(-element.velocity.normalized.x, absVel.normalized.y, absVel.normalized.x * Mathf.Sign(element.velocity.x));
            default:
                Debug.LogError("ERROR ON DEBUG COLOR GETTING!!");
                return Color.cyan;
        }
    }
}
