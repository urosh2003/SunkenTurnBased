using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectedTilesManager : MonoBehaviour
{
    public static SelectedTilesManager instance;
    public Tilemap targetingTilemap;
    public Tilemap rangeTilemap;
    public Tile redTile;
    public Tile redXTile;
    public List<Character> currentlyTargeting;

    public List<Vector3Int> oddYNeighboursDirectionVectors = new List<Vector3Int>{
            new Vector3Int(+1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, +1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(+1, +1, 0),
            new Vector3Int(+1, -1, 0),
        };

    public List<Vector3Int> evenYNeighboursDirectionVectors = new List<Vector3Int>{
            new Vector3Int(+1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, +1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(-1, +1, 0),
        };


    public void DrawTargetingPath(List<Vector3Int> tiles, int length)
    {
        ClearTargetingTiles();
        int pathLength = 1;
        foreach (Vector3Int tile in tiles)
        {
            if (pathLength <= length)
                targetingTilemap.SetTile(tile, redXTile);
            pathLength++;
        }
    }

    public void ClearTargetingTiles()
    {
        targetingTilemap.ClearAllTiles();
        foreach(Character target in currentlyTargeting)
        {
            target.GetComponent<Highlight>().DisableHighlight();
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

    public void DrawCircleRange(Vector3Int startPosition, int range)
    {
        if(range <= 0)
        {
            return;
        }

        List<Vector3Int> directions;

        if (startPosition.y % 2 == 0)
            directions = evenYNeighboursDirectionVectors;
        else
            directions = oddYNeighboursDirectionVectors;


        foreach (Vector3Int direction in directions)
        {
            rangeTilemap.SetTile(startPosition + direction, redTile);
            DrawCircleRange(startPosition + direction, range-1);
        }
    }

    public void DrawRedXTargeting(Vector3Int targetedTile)
    {
        ClearTargetingTiles();
        targetingTilemap.SetTile(targetedTile, redXTile);
        Character target = GridEntitiesManager.instance.GetGameObjectAtTile(targetedTile);
        if(target != null)
        {
            target.GetComponent<Highlight>().EnableHighlight();
            currentlyTargeting.Add(target);
        }
    }
}