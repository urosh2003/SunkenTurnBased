using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class SelectedTilesManager : MonoBehaviour
{
    public static SelectedTilesManager instance;
    public Tilemap targetingTilemap;
    public Tilemap rangeTilemap;
    public Tile redTile;
    public Tile redXTile;
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
            directions = new List<Vector3Int>(evenYNeighboursDirectionVectors);
        else
            directions = new List<Vector3Int>(oddYNeighboursDirectionVectors);


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

    public void DrawRedXTargetingEmpty(Vector3Int targetedTile)
    {
        ClearTargetingTiles();
        Character target = GridEntitiesManager.instance.GetGameObjectAtTile(targetedTile);
        if (target == null)
        {
            targetingTilemap.SetTile(targetedTile, redXTile);
        }
    }

    public void DrawAllDirections(Vector3Int actorPosition, int range)
    {
        for (int dir = 0; dir < 6; dir++)
        {
            Vector3Int offset = actorPosition.y % 2 == 0 ? evenYNeighboursDirectionVectors[dir] : oddYNeighboursDirectionVectors[dir];
            Vector3Int pos = actorPosition;

            for (int i = 1; i <= range; i++)
            {
                pos += offset;
                offset = pos.y % 2 == 0 ? evenYNeighboursDirectionVectors[dir] : oddYNeighboursDirectionVectors[dir];

                rangeTilemap.SetTile(pos, redTile);
            }
        }
    }

    public void DrawOneDirectionTarget(Vector3Int actorPosition, int range, int direction)
    {
        ClearTargetingTiles();

        if(direction == -1)
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

            targetingTilemap.SetTile(pos, redXTile);
            Character target = GridEntitiesManager.instance.GetGameObjectAtTile(pos);
            if (target != null)
            {
                target.GetComponent<Highlight>().EnableHighlight();
                currentlyTargeting.Add(target);
                break;
            }
        }
    }

    internal void DrawCircleRedXTargeting(Vector3Int startPosition, int range)
    {
        ClearTargetingTiles();

        if (range <= 0)
        {
            return;
        }

        List<Vector3Int> directions;

        if (startPosition.y % 2 == 0)
            directions = new List<Vector3Int>(evenYNeighboursDirectionVectors);
        else
            directions = new List<Vector3Int>(oddYNeighboursDirectionVectors);


        foreach (Vector3Int direction in directions)
        {
            targetingTilemap.SetTile(startPosition + direction, redXTile);
            DrawCircleRange(startPosition + direction, range - 1);
        }
    }
}