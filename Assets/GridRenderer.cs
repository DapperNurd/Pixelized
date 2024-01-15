using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    int width = PixelGrid.gridWidth;
    int height = PixelGrid.gridHeight;

    Texture2D texture;

    PixelGrid grid;

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
                texture.SetPixel(x, y, grid.grid[x,y].color);
            }
        }
        texture.Apply();
    }
}
