using System;
using Unity.VisualScripting;
using UnityEngine;

public class PixelGrid : MonoBehaviour
{

    public static int gridWidth = 160;
    public static int gridHeight = 95;
    // public static int gridWidth = 6;
    // public static int gridHeight = 4;

    int currentElement = 0; // temporary

    [SerializeField]
    public Element[,] grid;

    void Awake() {
        grid = Element.CreateAndFillGrid(gridWidth, gridHeight, this); // Fills the initial grid with empty cells

        Debug.Log("Sand"); // This is just for my own brain lol, temporary
    }

    // Debug function, just prints out the grid in the console with the numeric types of each element... only really useful for small grids and when the rendering isnt working
    void DisplayPsuedoGrid() {
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
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            currentElement = 0;
            Debug.Log("Sand");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            currentElement = 1;
            Debug.Log("Dirt");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3)) {
            currentElement = 2;
            Debug.Log("Water");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha4)) {
            currentElement = 3;
            Debug.Log("Oil");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha5)) {
            currentElement = 4;
            Debug.Log("N/A = Water");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha6)) {
            currentElement = 5;
            Debug.Log("N/A = Water");
        }

        // Optimize this monstrosity lol
        float mouseX = Input.mousePosition.x/Screen.width;
        float mouseY = Input.mousePosition.y/Screen.height;
        if(mouseX > 0 && mouseX < 1 && mouseY > 0 && mouseY < 1) {
            if(Input.GetMouseButton(0)) {
                switch(currentElement) {
                    case 0:
                        grid[(int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight)] = new Sand((int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight), this);
                        break;
                    case 1:
                        grid[(int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight)] = new Dirt((int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight), this);
                        break;
                    case 2:
                        grid[(int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight)] = new Water((int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight), this);
                        break;
                    case 3:
                        grid[(int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight)] = new Oil((int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight), this);
                        break;
                    default:
                        grid[(int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight)] = new Water((int)Mathf.Floor(mouseX*gridWidth), gridHeight-(int)Mathf.Floor(mouseY*gridHeight), this);
                        break;
                }
            }
        }
    }

    // Runs at a fixed rate, regardless of framerate
    void FixedUpdate() {
        IterateSteps();
    }

    // Sets a given pixel to a given element in the grid
    public void SetPixel(int x, int y, Element element) {
        grid[x, y] = element;
        element.pixelX = x;
        element.pixelY = y;
    }

    // Runs through the entire grid of Elements and executes their respective step() function
    void IterateSteps() {
        for(int y = gridHeight-1; y >= 0; y--) { // Starts at the gridHeight and decreases so it runs from bottom to top (Needed for the simulation)
            for(int x = 0; x < gridWidth; x++) {
                grid[x, y].step(this);
            }
        }
        for(int y = gridHeight-1; y >= 0; y--) { // Starts at the gridHeight and decreases so it runs from bottom to top (Needed for the simulation)
            for(int x = 0; x < gridWidth; x++) {
                grid[x, y].hasStepped = false;
            }
        }
    }

    // Checks if a given pixel (coord) is inside the grid
    public static bool IsInBounds(int x, int y) {
        return x < gridWidth && x >= 0 && y < gridHeight && y >= 0;
    }

}
