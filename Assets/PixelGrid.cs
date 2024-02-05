using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PixelGrid : MonoBehaviour
{
    public static int gridWidth = 320;
    public static int gridHeight = 180;
    // public static int gridWidth = 6;
    // public static int gridHeight = 4;

    public int radius = 2;
    public bool readMode = false;
    public bool isPaused = false;

    public GameObject cursor;

    private ElementType elementToSpawn;
    int elementToPlace = 1; // temporary

    private Element[,] grid; // This is private because it should be access via other functions

    private int stepCount;

    public Element boundaryHit;

    void Awake() {
        InitializeGrid(); // Fills the initial grid with empty cells
        boundaryHit = Element.CreateElement(ElementType.STONE, 0, 0, this);
        stepCount = 0;
    }

    // Debug function, just prints out the grid in the console with the numeric types of each element... only really useful for small grids and when the rendering isnt working
    void PrintPsuedoGrid() {
        string output = "[";
        for(int y = 0; y < gridHeight; y++) {
            for(int x = 0; x < gridWidth; x++) {
                output += (int)grid[x,y].elementType + ", ";
            }
            output += "]\n";
        }
        Debug.Log(output);
    }

    private readonly int[] keyCodes = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    // Runs once every frame
    void Update() {

        if(readMode) {
            if (!isPaused) isPaused = true;
            radius = 0;
            if (Input.GetMouseButtonDown(0)) {
                Element selectedElement = GetElementAtMouse();
                foreach (Element neighor in selectedElement.GetAllNeighbors())
                {
                    neighor.color = Color.blue;
                }
                foreach (Element neighor in selectedElement.GetImmediateNeighbors())
                {
                    neighor.color = Color.red;
                }
                //Color temp = selectedElement.color;
                //selectedElement.color = Color.red;
                //Debug.Log(selectedElement.velocity);
            }
            return;
        }

        if (Input.anyKeyDown) {
            if (Input.GetKey(KeyCode.R)) {
                FillGrid(this, ElementType.EMPTYCELL);
                isPaused = true;
            }
            else if (Input.GetKey(KeyCode.Space)) {
                isPaused = !isPaused;
            }
            else {
                // KeyCode.Alpha1 = 49
                foreach (int num in keyCodes) {
                    if (Input.GetKey((KeyCode)(num+48))) {
                        Debug.Log((ElementType)elementToPlace);
                        elementToPlace = num;
                    }
                }
            }
        }

        Vector2Int mousePos = GetMousePositionRelativeToGrid();

        int scroll = (int)(Input.GetAxis("Mouse ScrollWheel")*15); // Will be (positive or negative) 1, 3, or 4 depending on scroll speed (ScrollWheel axis returns 0.1, 0.2, or 0.3)
        if (scroll != 0) {
            radius += scroll; 
            if(radius < 0) radius = 0; // Keeps it greater or equal to zero
            cursor.transform.localScale = (Vector2.one * 2 * radius) + Vector2.one;
        }
        if (!isPaused) cursor.transform.position = new Vector3(mousePos.x- 0.5f, -mousePos.y+ 0.5f, -5);

        if (IsInBounds(mousePos.x, mousePos.y)) {
            if(Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
                elementToSpawn = Input.GetMouseButton(1) ? ElementType.EMPTYCELL : (ElementType)elementToPlace;
                for(int x = mousePos.x - radius; x <= mousePos.x + radius; x++) {
                    for(int y = mousePos.y - radius; y <= mousePos.y + radius; y++) {
                        if(!IsInBounds(x, y)) continue;
                        if (GetPixel(x, y).elementType == elementToSpawn) continue;
                        SetPixel(x, y, Element.CreateElement(elementToSpawn, x, y, this));
                    }
                }
            }
        }
    }

    // Runs at a fixed rate, regardless of framerate... Defined in Unity Settings (default is 0.02, or 50 times a second).
    void FixedUpdate() {
        if (isPaused) return;
        IterateSteps();
    }

    public Vector2Int GetMousePositionRelativeToGrid() {
        return new(Mathf.RoundToInt(Input.mousePosition.x / Screen.width * gridWidth), gridHeight - Mathf.RoundToInt(Input.mousePosition.y / Screen.height * gridHeight));
    }

    public Element GetElementAtMouse() {
        return GetPixel(GetMousePositionRelativeToGrid());
    }

    /// <summary>
    /// Runs through the entire grid of pixels (Elements) and executes their respective step() function. Then runs through it again and disables all pixels' hasStepped property
    /// </summary>
    void IterateSteps() {
        stepCount = (stepCount + 1) % 60; // Increases, loops back to 0 once reached 60
        bool evenFrame = (stepCount % 2 == 0); // We are using stepCount instead of unity's frameCount because this is ran in FixedUpdate
        for (int y = gridHeight-1; y >= 0; y--) { // Starts at the gridHeight and decreases so it runs from bottom to top (Needed for the simulation)
            for (int x = evenFrame ? 0 : gridWidth - 1; evenFrame ? x < gridWidth : x > 0; x += evenFrame ? 1 : -1) { // This ugly mess alternates the direction of column based on frame count... this ensures better randomness for movement like flowing liquids
                GetPixel(x, y).step(this);
            }
        }
        for(int y = 0; y < gridHeight; y++) { // Loops through all pixels, order doesnt matter... resets the hasStepped flag... might be a better way to do this
            for(int x = 0; x < gridWidth; x++) {
                GetPixel(x, y).hasStepped = false;
            }
        }
    }

    /// <summary>
    /// Checks and returns whether or not the provided coordinates are within the bounds of the grid's width and height
    /// </summary>
    /// <param name="x">The x coordinate to check</param>
    /// <param name="y">the y coordinate to check</param>
    /// <returns>bool: if the position is in bounds</returns>
    public static bool IsInBounds(int x, int y) {
        return x < gridWidth && x >= 0 && y < gridHeight && y >= 0;
    }

    public Element GetPixel(Vector2 xy) {
        return GetPixel((int)xy.x, (int)xy.y);
    }

    /// <summary>
    /// Gets the pixel located at the given coordinates in the grid
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>The element found at the coordinates, or null if out of bounds</returns>
    public Element GetPixel(int x, int y) {
        if (!IsInBounds(x, y)) return null;
        return grid[x, y];
    }

    /// <summary>
    /// Sets the pixel at the given coordinates
    /// </summary>
    /// <param name="x">The x coordinate to check</param>
    /// <param name="y">The y coordinate to check</param>
    /// <param name="element">The element to put in place</param>
    public void SetPixel(int x, int y, Element element) {
        if (!IsInBounds(x, y)) return;
        grid[x, y] = element;
        element.pixelX = x;
        element.pixelY = y;
    }

    /// <summary>
    /// Fills the given PixelGrid's grid with objects corresponding to the given ElementType
    /// </summary>
    /// <param name="pixelGrid">The PixelGrid object whose grid property is to be filled</param>
    /// <param name="elementType">The type of of the element to fill the grid with</param>
    public static void FillGrid(PixelGrid pixelGrid, ElementType elementType) {
        for (int y = gridHeight - 1; y >= 0; y--) {
            for (int x = 0; x < gridWidth; x++) {
                pixelGrid.SetPixel(x, y, Element.CreateElement(elementType, x, y, pixelGrid));
            }
        }
    }

    /// <summary>
    /// Initializes this objects grid property, and fills it with empty cells
    /// </summary>
    public void InitializeGrid() {
        this.grid = new Element[gridWidth, gridHeight];
        FillGrid(this, ElementType.EMPTYCELL);
    }
}
