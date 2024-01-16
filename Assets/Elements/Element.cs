using Unity.VisualScripting;
using UnityEngine;

public enum ElementType
{
    // EMPTY
    EMPTYCELL,

    // SOLIDS
    //   Notes for solids:
    //    - density should always be above 1
    STONE,
    SAND,
    DIRT,

    // LIQUIDS
    WATER,
    OIL,

    // GASSES
}

public abstract class Element
{
    // Variables for Element properties
    public int pixelX;
    public int pixelY;
    public ElementType elementType;
    public UnityEngine.Color color;
    public float density;
    public bool isSolid;

    // Variables for simulation
    public PixelGrid grid;
    public bool hasStepped;

    // This function is static -> Used by the class, not by an object
    // Creates a 2D array of type Element with sizes inputted into parameters, returns the array filled with EmptyCell Elements
    public static Element[,] CreateAndFillGrid(int width, int height, PixelGrid grid) {
        Element[,] tempGrid = new Element[width, height];

        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                tempGrid[x, y] = new EmptyCell(x, y, grid);
            }
        }

        return tempGrid;
    }

    public void SwapPixel(PixelGrid grid, Element element1, Element element2) {
        int x = element1.pixelX, 
            y = element1.pixelY;
        grid.SetPixel(element2.pixelX, element2.pixelY, element1);
        grid.SetPixel(x, y, element2);
    }

    // Checks if pixel at given offset from current pixel can (should) be swapped with current pixel (or null if pixel DNE)
    // HORIZONTAL: left is negative, right is positive
    // VERTICAL: down is positive, up is negative
    public virtual bool CanMakeMove(int horizontalOffset, int verticalOffset) {
        int verticalDir = pixelY+verticalOffset;
        int horizontalDir = pixelX+horizontalOffset;

        if(!PixelGrid.IsInBounds(horizontalDir, verticalDir)) return false;

        if(grid.grid[horizontalDir, verticalDir].elementType == ElementType.EMPTYCELL) return true;

        if(isSolid && grid.grid[horizontalDir, verticalDir].isSolid) return false;
        return (grid.grid[horizontalDir, verticalDir].density > density && verticalDir < pixelY) || 
                (grid.grid[horizontalDir, verticalDir].density < density && verticalDir >= pixelY);
    }

    // I really  don't like this function lol but idk how else to do it
    public virtual int TryDispersion(int horizontalOffset, int verticalOffset) {
        int verticalDir = pixelY+verticalOffset;
        int moveDirection = (horizontalOffset > 0) ? 1 : -1;
        for(int i = 1; i <= Mathf.Abs(horizontalOffset); i++) {
            int horizontalDir = pixelX+(i*moveDirection);
            if (
                (PixelGrid.IsInBounds(horizontalDir, verticalDir)) &&
                ((grid.grid[horizontalDir, verticalDir].density > density && verticalDir < pixelY) || (grid.grid[horizontalDir, verticalDir].density < density && verticalDir >= pixelY)) == true
            ) continue;
            if(i == 1) return 31415; // I hate this but I can't think of a better way... basically returning false, or a number that will never be normally calculated
            return (i-1)*moveDirection;
        }
        return horizontalOffset;
    }

    public abstract void step(PixelGrid grid);
}
