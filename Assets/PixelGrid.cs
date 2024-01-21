using System;
using Unity.VisualScripting;
using UnityEngine;

public class PixelGrid : MonoBehaviour
{
    public static int gridWidth = 320;
    public static int gridHeight = 180;
    // public static int gridWidth = 6;
    // public static int gridHeight = 4;

    public int radius = 2;
    public bool isPaused = false;

    public GameObject cursor;

    private int currentElement = 0; // temporary

    private Element[,] grid; // This is private because it should be access via other functions

    private int stepCount;

    void Awake() {
        InitializeGrid(); // Fills the initial grid with empty cells
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

    // Runs once every frame
    void Update() {

        // More temporary stuff for testing... Selecting element and placing on grid
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            currentElement = 0;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            currentElement = 1;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3)) {
            currentElement = 2;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha4)) {
            currentElement = 3;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha5)) {
            currentElement = 4;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha6)) {
            currentElement = 5;
        }
        else if(Input.GetKeyDown(KeyCode.R)) {
            FillGrid(this, ElementType.EMPTYCELL);
            isPaused = true;
        }
        else if(Input.GetKeyDown(KeyCode.Space)) {
            isPaused = !isPaused;
        }

        int mouseX = Mathf.RoundToInt(Input.mousePosition.x/Screen.width * gridWidth);
        int mouseY = gridHeight - Mathf.RoundToInt(Input.mousePosition.y/Screen.height * gridHeight);

        int scroll = (int)(Input.GetAxis("Mouse ScrollWheel")*15); // Will be (positive or negative) 1, 3, or 4 depending on scroll speed (ScrollWheel axis returns 0.1, 0.2, or 0.3)
        if (scroll != 0) {
            radius += scroll; 
            if(radius < 0) radius = 0; // Keeps it greater or equal to zero
            cursor.transform.localScale = (Vector2.one * 2 * radius) + Vector2.one;
        }
        if (!isPaused) cursor.transform.position = new Vector3(mouseX-0.5f, -mouseY+0.5f, -5);

        if (IsInBounds(mouseX, mouseY)) {
            if(Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
                int elementToSpawn = Input.GetMouseButton(1) ? -1 : currentElement;
                for(int x = mouseX - radius; x <= mouseX + radius; x++) {
                    for(int y = mouseY - radius; y <= mouseY + radius; y++) {
                        if(!IsInBounds(x, y)) continue;
                        Element elementAtMouse = GetPixel(x, y);
                        switch (elementToSpawn) {
                            case -1:
                                if (elementAtMouse is EmptyCell) continue;
                                SetPixel(x, y, new EmptyCell(x, y, this));
                                break;
                            case 0:
                                if (elementAtMouse is Stone) continue;
                                SetPixel(x, y, new Stone(x, y, this));
                                break;
                            case 1:
                                if (elementAtMouse is Sand) continue;
                                SetPixel(x, y, new Sand(x, y, this));
                                break;
                            case 2:
                                if (elementAtMouse is Dirt) continue;
                                SetPixel(x, y, new Dirt(x, y, this));
                                break;
                            case 3:
                                if (elementAtMouse is Water) continue;
                                SetPixel(x, y, new Water(x, y, this));
                                break;
                            case 4:
                                if (elementAtMouse is Oil) continue;
                                SetPixel(x, y, new Oil(x, y, this));
                                break;
                            default:
                                if (elementAtMouse is Stone) continue;
                                SetPixel(x, y, new Stone(x, y, this));
                                break;
                        }
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
        for (int y = 0; y < gridHeight; y++) {
            for (int x = 0; x < gridWidth; x++) {
                pixelGrid.grid[x, y] = Element.CreateElement(elementType, x, y, pixelGrid);
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
