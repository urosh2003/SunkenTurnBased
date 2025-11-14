using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum TileColor { RED, GREEN, YELLOW }
public enum TileType { DEFAULT, XTILE }
public enum TileLayer { TARGETING, RANGE }
public struct TileStyle
{
    public TileColor primaryColor;
    public TileColor secondaryColor;
    public TileColor tertiaryColor;
    public TileType type;
    public TileLayer tileLayer;

    public TileStyle(TileColor color, TileType type, TileLayer tileLayer, TileColor secondaryColor = TileColor.RED, TileColor tertiaryColor = TileColor.RED)
    {
        this.primaryColor = color;
        this.type = type;
        this.tileLayer = tileLayer;
        this.secondaryColor = secondaryColor;
        this.tertiaryColor = tertiaryColor;
    }
}

public class SelectedTilesManager : MonoBehaviour
{
    public static SelectedTilesManager instance;
    public Tilemap targetingTilemap;
    public Tilemap rangeTilemap;
    public Tile redTile;
    public Tile redXTile;
    public Tile yellowTile;
    public Tile yellowXTile;
    public Tile greenTile;
    public Tile greenXTile;
    public List<Character> currentlyTargeting;

    public readonly Vector3Int[] oddYNeighboursDirectionVectors = new Vector3Int[]{
            new Vector3Int(+1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, +1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(+1, +1, 0),
            new Vector3Int(+1, -1, 0),
        };

    public readonly Vector3Int[] evenYNeighboursDirectionVectors = new Vector3Int[]{
            new Vector3Int(+1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(-1, +1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(0, +1, 0),
            new Vector3Int(0, -1, 0),
        };

    private Tile GetTileByStyle(TileStyle tileStyle)
    {
        switch ((tileStyle.primaryColor, tileStyle.type))
        {
            case (TileColor.RED, TileType.DEFAULT) :
                return redTile;
            case (TileColor.RED, TileType.XTILE):
                return redXTile;
            case (TileColor.GREEN, TileType.DEFAULT):
                return greenTile;
            case (TileColor.GREEN, TileType.XTILE):
                return greenXTile;
            case (TileColor.YELLOW, TileType.DEFAULT):
                return yellowTile;
            case (TileColor.YELLOW, TileType.XTILE):
                return yellowXTile;
        }
        return redTile;
    }
    private Tilemap GetTilemapByStyle(TileStyle tileStyle)
    {
        switch (tileStyle.tileLayer)
        {
            case (TileLayer.TARGETING):
                ClearTargetingTiles();
                return targetingTilemap;
            case (TileLayer.RANGE):
                return rangeTilemap;

        }
        return targetingTilemap;
    }


    public void DrawTargetingPath(List<Vector3Int> tiles, int length, TileStyle tileStyle)
    {
        Tilemap selectedTilemap = GetTilemapByStyle(tileStyle);
        Tile selectedTile = GetTileByStyle(tileStyle);
        int pathLength = 1;
        foreach (Vector3Int tile in tiles)
        {
            if (pathLength <= length)
                selectedTilemap.SetTile(tile, selectedTile);
            pathLength++;
        }
    }

    public void ClearTargetingTiles()
    {
        targetingTilemap.ClearAllTiles();
        foreach(Character target in currentlyTargeting)
        {
            if (target != null)
            {
                target.GetComponent<Highlight>().DisableHighlight();
            }
        }
        currentlyTargeting.Clear();
    }
    public void ClearRangeTiles()
    {
        rangeTilemap.ClearAllTiles();
    }

    private void Awake()
    {
        instance = this;
        currentlyTargeting = new();
    }

    public void DrawCircle(Vector3Int startPosition, int range, TileStyle tileStyle)
    {
        if(range <= 0)
        {
            return;
        }

        List<Vector3Int> directions;

        if (startPosition.y % 2 == 0)
            directions = new List<Vector3Int>(evenYNeighboursDirectionVectors);
        else
            directions = new List<Vector3Int>(oddYNeighboursDirectionVectors);

        Tile selectedTile = GetTileByStyle(tileStyle);
        Tilemap selectedTilemap = GetTilemapByStyle(tileStyle);

        foreach (Vector3Int direction in directions)
        {
            selectedTilemap.SetTile(startPosition + direction, selectedTile);
            DrawCircle(startPosition + direction, range-1, tileStyle);
        }
    }

    public void DrawSingle(Vector3Int targetedTile, TileStyle tileStyle)
    {
        Tilemap selectedTilemap = GetTilemapByStyle(tileStyle);
        Tile selectedTile = GetTileByStyle(tileStyle);

        selectedTilemap.SetTile(targetedTile, selectedTile);
        Character target = GridEntitiesManager.instance.GetGameObjectAtTile(targetedTile);
        if(target != null)
        {
            target.GetComponent<Highlight>().EnableHighlight();
            currentlyTargeting.Add(target);
        }
    }

    public void DrawSingleUnoccupied(Vector3Int targetedTile, TileStyle tileStyle)
    {
        Tilemap selectedTilemap = GetTilemapByStyle(tileStyle);
        Tile selectedTile = GetTileByStyle(tileStyle);

        Character target = GridEntitiesManager.instance.GetGameObjectAtTile(targetedTile);
        if (target == null)
        {
            selectedTilemap.SetTile(targetedTile, selectedTile);
        }
    }

    public void DrawAllDirections(Vector3Int actorPosition, int range, TileStyle tileStyle)
    {
        Tilemap selectedTilemap = GetTilemapByStyle(tileStyle);
        Tile selectedTile = GetTileByStyle(tileStyle);

        for (int dir = 0; dir < 6; dir++)
        {
            Vector3Int offset = actorPosition.y % 2 == 0 ? evenYNeighboursDirectionVectors[dir] : oddYNeighboursDirectionVectors[dir];
            Vector3Int pos = actorPosition;

            for (int i = 1; i <= range; i++)
            {
                pos += offset;
                offset = pos.y % 2 == 0 ? evenYNeighboursDirectionVectors[dir] : oddYNeighboursDirectionVectors[dir];

                selectedTilemap.SetTile(pos, selectedTile);
            }
        }
    }

    public void DrawOneDirection(Vector3Int actorPosition, int range, int direction, TileStyle tileStyle)
    {
        Tile selectedTile = GetTileByStyle(tileStyle);
        Tilemap selectedTilemap = GetTilemapByStyle(tileStyle);


        if (direction == -1)
        {
            return;
        }

        Vector3Int offset;
        Vector3Int pos;

        offset = actorPosition.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];
        pos = actorPosition;
        for (int i = 1; i <= range; i++)
        {
            pos += new Vector3Int(offset.x, offset.y, 0);
            offset = pos.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];

            selectedTilemap.SetTile(pos, selectedTile);
            Character target = GridEntitiesManager.instance.GetGameObjectAtTile(pos);
            if (target != null)
            {
                target.GetComponent<Highlight>().EnableHighlight();
                currentlyTargeting.Add(target);
                break;
            }
        }
    }

    internal void DrawTorpedostorm(Vector3Int actorPosition, int range, int direction)
    {
        if (direction == -1)
        {
            return;
        }
        ClearTargetingTiles();

        Vector3Int offset;
        Vector3Int pos;

        offset = actorPosition.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];
        pos = actorPosition;

        List<Vector3Int> directions;

        if (pos.y % 2 == 0)
            directions = new List<Vector3Int>(evenYNeighboursDirectionVectors);
        else
            directions = new List<Vector3Int>(oddYNeighboursDirectionVectors);

        targetingTilemap.SetTile(pos, greenXTile);
        foreach (Vector3Int basicDirection in directions)
        {
            if (targetingTilemap.GetTile(pos + basicDirection) == null)
            {
                targetingTilemap.SetTile(pos + basicDirection, redXTile);
                Character target = GridEntitiesManager.instance.GetGameObjectAtTile(pos + basicDirection);
                if (target != null)
                {
                    target.GetComponent<Highlight>().EnableHighlight();
                    currentlyTargeting.Add(target);
                }
            }
        }

        for (int i = 1; i <= range; i++)
        {
            pos += new Vector3Int(offset.x, offset.y, 0);
            offset = pos.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];
            targetingTilemap.SetTile(pos, greenXTile);


            if (pos.y % 2 == 0)
                directions = new List<Vector3Int>(evenYNeighboursDirectionVectors);
            else
                directions = new List<Vector3Int>(oddYNeighboursDirectionVectors);

            foreach (Vector3Int basicDirection in directions)
            {
                if (targetingTilemap.GetTile(pos + basicDirection) == null)
                {
                    targetingTilemap.SetTile(pos + basicDirection, redXTile);
                    Character target = GridEntitiesManager.instance.GetGameObjectAtTile(pos + basicDirection);
                    if (target != null)
                    {
                        target.GetComponent<Highlight>().EnableHighlight();
                        currentlyTargeting.Add(target);
                    }
                }
            }
        }
    }
}