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

    public UnityEngine.Color color;

    public float density;
    public float frictionFactor;
    public float bounciness;
    public float inertiaResistance;

    public bool isMoving;

    // Variables for simulation
    public static UnityEngine.Vector2 gravity = new UnityEngine.Vector2(0, 10); // Note: Vertical movement is inverted... positive is downwards
    public PixelGrid grid;
    public bool hasStepped;

    public void SwapPixel(PixelGrid grid, Element element1, Element element2) {
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
        if (!PixelGrid.IsInBounds(pixelX + xOffset, pixelY + yOffset)) return null;
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

    public bool IsMovableCell(Element cellToCheck) { // This should be a PixelGrid method tbh
        return cellToCheck != null && (cellToCheck.elementType == ElementType.EMPTYCELL || cellToCheck.isMoving);
    }

    protected void ApplyGravity() {
        velocity = Vector2.ClampMagnitude(velocity + (gravity * Time.deltaTime), 10f); // Adds gravity to velocity, clamps it to be between -10f and 10f
        if (velocity.y > 0 && velocity.y < 1) velocity.y = 1f; // This basically ensures that if it just started falling, it will actually register as falling
    }

    public abstract void step(PixelGrid grid);
    public abstract bool CheckShouldMove();
}
