using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Build;
using UnityEngine;

public enum ElementType
{
    // IMPORTANT!!!
    // When adding a new ElementType, also create a dictionary reference at the beginning of the Element class (below)!!

    // EMPTY
    EMPTYCELL,

    // SOLIDS
    STONE, SAND, DIRT,

    // LIQUIDS
    WATER, OIL,

    // GASSES
    STEAM,
}

public abstract class Element
{
    public static Dictionary<ElementType, Type> elementTypes = new Dictionary<ElementType, Type> {
        { ElementType.EMPTYCELL, typeof(EmptyCell) },
        { ElementType.STONE, typeof(Stone) },
        { ElementType.SAND, typeof(Sand) },
        { ElementType.DIRT, typeof(Dirt) },
        { ElementType.WATER, typeof(Water) },
        { ElementType.OIL, typeof(Oil) },
        { ElementType.STEAM, typeof(Steam) },
    };

    public static Element CreateElement(ElementType element, int x, int y, PixelGrid grid) {
        if (elementTypes.TryGetValue(element, out var elementType)) {
            return (Element)Activator.CreateInstance(elementType, new object[] { x, y, grid });
        }
        throw new ArgumentOutOfRangeException("element");
    }

    // Variables for Element properties
    public ElementType elementType;
    public int pixelX;
    public int pixelY;
    public UnityEngine.Vector2 velocity;

    public int moveDirection;

    public UnityEngine.Color color;

    public float density;
    public float frictionFactor;
    public float inertiaResistance;

    public int lifetime = 0;

    public bool isMoving;

    // Variables for simulation
    public PixelGrid grid;
    public bool hasStepped;

    public void SwapPixel(PixelGrid grid, Element element1, Element element2) {

        //Debug.Log("Moving [" + element1.pixelX + ", " + element1.pixelY + "] to [" + element2.pixelX + ", " + element2.pixelY + "]");
        int x = element1.pixelX, 
            y = element1.pixelY;
        grid.SetPixel(element2.pixelX, element2.pixelY, element1);
        grid.SetPixel(x, y, element2);
    }

    /// <summary>
    /// Checks if pixel at given offset from current pixel can (should) be swapped with current pixel
    /// </summary>
    /// <param name="horizontalOffset">Offset from cell's x position. -left, +right</param>
    /// <param name="verticalOffset">Offset from cell's y position. -up, +down</param>
    /// <returns>bool: whether or not this cell can move to the position of itself plus the given offsets</returns>
    public virtual bool CanMakeMove(int horizontalOffset, int verticalOffset) {
        
        int verticalDir = pixelY+verticalOffset;

        Element targetCell = GetPixelByOffset(horizontalOffset, verticalOffset);

        if (targetCell == null) return false; // Is null if out of bounds
        if((this is MoveableSolid || this is ImmoveableSolid) && (targetCell is MoveableSolid || targetCell is ImmoveableSolid)) return false; // If target pos is an empty cell and this cell is an empty cell, cannot move

        if (targetCell.elementType == ElementType.EMPTYCELL) return true; // If target pos is an empty cell, can move
         
        return (targetCell.density > density && verticalDir < pixelY) || (targetCell.density < density && verticalDir >= pixelY); // If the density of target pos is higher than this cell and is below it (or vice versa), can move (swap)
    }

    public Element GetPixelByOffset(int xOffset, int yOffset) {
        if (!PixelGrid.IsInBounds(pixelX + xOffset, pixelY + yOffset)) return grid.boundaryHit;
        return grid.GetPixel(pixelX + xOffset, pixelY + yOffset);
    }

    public Element[] GetAllNeighbors() {
        Element[] neighbors = new Element[8];
        int index = 0;
        for(int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                if (y == 0 && x == 0) continue; // Skips this cell
                neighbors[index++] = GetPixelByOffset(x, y);
            }
        }
        return neighbors;
    }

    public Element[] GetImmediateNeighbors()
    {
        Element[] neighbors = new Element[4];
        int index = 0;
        // Not sure if this is more efficeint than just running GetPixelByOffset 4 times lol
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (y == 0 ^ x == 0) neighbors[index++] = GetPixelByOffset(x, y); // Uses XOR to ignore corners, im kinda big brain ngl
            }
        }
        return neighbors;
    }

    public Element[] GetHorizontalNeighbors() {
        Element[] neighbors = new Element[2];
        int index = 0;
        for (int x = -1; x <= 1; x+=2) {
            neighbors[index++] = GetPixelByOffset(x, 0);
        }
        return neighbors;
    }

    public Element[] GetVerticalNeighbors() {
        Element[] neighbors = new Element[2];
        int index = 0;
        for (int y = 1; y >= -1; y-=2) { // Decreasing so it goes in a downward direction in the grid... probably doesn't matter
            neighbors[index++] = GetPixelByOffset(0, y);
        }
        return neighbors;
    }

    public Element[] GetDiagonalNeighbors() { // Probably won't ever need this lol
        Element[] neighbors = new Element[4];
        int index = 0;
        for (int y = -1; y <= 1; y+=2) {
            for (int x = -1; x <= 1; x+=2) {
                neighbors[index++] = GetPixelByOffset(x, y);
            }
        }
        return neighbors;
    }

    public bool IsExposed()
    {
        foreach(Element neighbor in GetImmediateNeighbors())
        {
            if (neighbor.elementType == ElementType.EMPTYCELL) return true;
        }
        return false;
    }

    // kinda a bad name lol, checks to see if current element should be marked as moving, not so much that it can move to the cell (of element) its checking
    public bool IsMovableCell(Element cellToCheck) { // This should be a PixelGrid method tbh
        return cellToCheck != null && (cellToCheck is EmptyCell || cellToCheck.isMoving || cellToCheck.density < density);
    }

    protected void ApplyGravity() {
        velocity = Vector2.ClampMagnitude(velocity + (World.gravity * Time.deltaTime), 10f); // Adds gravity to velocity, clamps it to be between -10f and 10f
        if (GetPixelByOffset((int)velocity.x, 1).elementType == ElementType.EMPTYCELL && velocity.y > 0 && velocity.y < 1) velocity.y = 1f; // This basically ensures that if it just started falling, it will actually register as falling
    }

    public abstract void step(PixelGrid grid);
    public abstract bool CheckShouldMove();
}
