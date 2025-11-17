using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class GridEntitiesManager : MonoBehaviour
{
    public static GridEntitiesManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public Dictionary<Vector3Int, Dictionary<GridEntityType, GameObject>> gridEntities = new Dictionary<Vector3Int, Dictionary<GridEntityType, GameObject>>();
    public Tilemap baseTilemap;

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


    void Start()
    {
        baseTilemap = Resources.FindObjectsOfTypeAll<Tilemap>()
        .FirstOrDefault(obj => obj.name == "BaseTilemap");
        // Load save file and set the entities on the map
    }

    public Character GetCharacterAtTile(Vector3Int tilePosition)
    {
        gridEntities.TryGetValue(tilePosition, out Dictionary<GridEntityType, GameObject> gameObjects);

        if (gameObjects != null)
        {
            gameObjects.TryGetValue(GridEntityType.CHARACTER, out GameObject gameObject);
            if (gameObject != null)
            {
                return gameObject.GetComponent<Character>();
            }
        }

        return null;
    }

    public bool IsTileWalkable(Vector3Int tilePosition, GridEntityType gridEntityType)
    {
        gridEntities.TryGetValue(tilePosition, out Dictionary<GridEntityType, GameObject> gameObjects);
        if (gameObjects != null)
        {
            foreach (var gameObject in gameObjects)
            {
                if (!gameObject.Value.GetComponent<GridEntity>().IsWalkable() || gameObject.Key.Equals(gridEntityType))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void AddGridEntity(Vector3Int tilePosition, GameObject gameObject, GridEntityType gridEntityType)
    {
        gridEntities.TryGetValue(tilePosition, out Dictionary<GridEntityType, GameObject> gameObjects);
        if (gameObjects == null)
        {
            gameObjects = new Dictionary<GridEntityType, GameObject>();
            gameObjects.Add(gridEntityType, gameObject);
            gridEntities.Add(tilePosition, gameObjects);
        }
        else if (!gameObjects.ContainsKey(gridEntityType))
        {
            gameObjects.Add(gridEntityType, gameObject);
        }
    }

    public void RemoveGridEntityWorld(Vector3 position, GridEntityType gridEntityType)
    {
        RemoveGridEntity(baseTilemap.WorldToCell(position), gridEntityType);
    }

    public Vector3Int GetCellFromPosition(Vector3 position)
    {
        return baseTilemap.WorldToCell(position);
    }

    public void RemoveGridEntity(Vector3Int tilePosition, GridEntityType gridEntityType)
    {
        gridEntities.TryGetValue(tilePosition, out Dictionary<GridEntityType, GameObject> gameObjects);
        if (gameObjects != null)
        {
            gameObjects.Remove(gridEntityType);
        }
    }

    public List<Vector3Int> FindPathWorldPosition(Vector3 oldWorldPosition, Vector3 newWorldPosition, GridEntityType gridEntityType)
    {
        Vector3Int oldTilePosition = baseTilemap.WorldToCell(oldWorldPosition);
        Vector3Int newTilePosition = baseTilemap.WorldToCell(newWorldPosition);

        if (!IsTileWalkable(newTilePosition, gridEntityType))
            return null;

        return FindPath(oldTilePosition, newTilePosition, gridEntityType);
    }

    public List<Vector3Int> FindPathToCharacter(Vector3 currentPosition, Vector3 target, GridEntityType gridEntityType)
    {
        Vector3Int currentTilePosition = baseTilemap.WorldToCell(currentPosition);
        Vector3Int targetTilePosition = baseTilemap.WorldToCell(target);

        List<Vector3Int> path = FindPath(currentTilePosition, targetTilePosition, gridEntityType);

        return path;
    }

    private List<Vector3> TilePathToWorld(List<Vector3Int> pathTiles)
    {
        List<Vector3> path = new List<Vector3>();
        foreach (Vector3Int pathTile in pathTiles)
        {
            path.Add(baseTilemap.GetCellCenterWorld(pathTile));
        }
        return path;
    }
    public Vector3 MoveEntityToTilePositionWorld(Vector3 originalPosition, Vector3Int targetPosition, GridEntityType gridEntityType)
    {
        Vector3Int originalTilePosition = baseTilemap.WorldToCell(originalPosition);

        return MoveEntityToTilePosition(originalTilePosition, targetPosition, gridEntityType);
    }

    public Vector3 MoveEntityToTilePosition(Vector3Int originalTilePosition, Vector3Int newTilePosition, GridEntityType gridEntityType)
    {
        gridEntities.TryGetValue(originalTilePosition, out Dictionary<GridEntityType, GameObject> gameObjects); //Check what objects are on tile
        gameObjects.TryGetValue(gridEntityType, out GameObject gameObject);                                     //Check if the desired object is among them

        if (gameObject != null && IsTileWalkable(newTilePosition, gridEntityType))
        {
            gameObjects.Remove(gridEntityType);
            if(!gameObjects.Any())
            {
                gridEntities.Remove(originalTilePosition);
            }
            AddGridEntity(newTilePosition, gameObject, gridEntityType);

            return baseTilemap.GetCellCenterWorld(newTilePosition);
        }

        return baseTilemap.GetCellCenterWorld(originalTilePosition);
    }

    public Vector3 MoveEntityInDirection(Vector3Int originalTilePosition, Vector3Int tileDirection, GridEntityType gridEntityType)
    {
        Vector3Int newTilePosition = originalTilePosition + tileDirection;

        return MoveEntityToTilePosition(originalTilePosition, newTilePosition, gridEntityType);
    }

    public Vector3 MoveCharacterInDirection(Vector3Int tilePosition, int chosenDirection)
    {
        Vector3Int direction;

        if (tilePosition.y % 2 == 0)
            direction = evenYNeighboursDirectionVectors[chosenDirection];
        else
            direction = oddYNeighboursDirectionVectors[chosenDirection];
        return MoveEntityInDirection(tilePosition, direction, GridEntityType.CHARACTER);
    }


    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal, GridEntityType gridEntityType)
    {
        PriorityQueue<Vector3Int> frontier = new PriorityQueue<Vector3Int>();
        frontier.Add(start, 0);
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, float> costToTile = new Dictionary<Vector3Int, float>();

        cameFrom.Add(start, start);
        costToTile.Add(start, 0);

        while (!frontier.Empty)
        {
            Vector3Int current = frontier.Get();

            if (current == goal)
                break;

            foreach (Vector3Int neighbour in WalkableNeighbours(current, goal,gridEntityType))
            {
                float neighbourCost = costToTile[current] + CalculateCost(current, neighbour);

                if (!costToTile.ContainsKey(neighbour) || costToTile[neighbour] > neighbourCost)
                {
                    costToTile[neighbour] = neighbourCost;
                    float priority = neighbourCost + DistanceToTile(current, neighbour);
                    frontier.Add(neighbour, priority);
                    cameFrom[neighbour] = current;
                }
            }
        }

        return ReconstructPath(cameFrom, start, goal);
    }

    private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int start, Vector3Int goal)
    {
        Vector3Int current = goal;
        List<Vector3Int> path = new List<Vector3Int>{current};
        while(current != start)
        {
            Vector3Int next = cameFrom[current];
            path.Add(next);
            current = next;
        }
        if (GetCharacterAtTile(goal) != null && path.Count > 1)
        {
            path.RemoveAt(0);
        }
        path.Reverse();
        path.RemoveAt(0);
        return path;
    }

    public float DistanceToTile(Vector3Int start, Vector3Int goal)
    {
        Vector3Int a = start;
        Vector3Int b = goal;

        int dx = b.x - a.x;
        int dy = b.y - a.y;
        int dxAbs = Mathf.Abs(dx);
        int dyAbs = Mathf.Abs(dy);

        // Base distance calculation
        int floorHalfDy = dyAbs / 2;
        int baseDistance = dxAbs + floorHalfDy;

        // Penalty conditions (using Vector3Int's y component)
        bool isAYEven = (a.y % 2) == 0;
        bool isBYEven = (b.y % 2) == 0;
        bool condition1 = isAYEven && !isBYEven && (a.x < b.x);
        bool condition2 = isBYEven && !isAYEven && (b.x < a.x);
        int penalty = (condition1 || condition2) ? 1 : 0;

        // Final distance
        return Mathf.Max(dyAbs, baseDistance + penalty);
    }

    public float DistanceToTileWorld(Vector3 start, Vector3 goal)
    {
        Vector3Int a = baseTilemap.WorldToCell(start);
        Vector3Int b = baseTilemap.WorldToCell(goal);

        int dx = b.x - a.x;
        int dy = b.y - a.y;
        int dxAbs = Mathf.Abs(dx);
        int dyAbs = Mathf.Abs(dy);

        // Base distance calculation
        int floorHalfDy = dyAbs / 2;
        int baseDistance = dxAbs + floorHalfDy;

        // Penalty conditions (using Vector3Int's y component)
        bool isAYEven = (a.y % 2) == 0;
        bool isBYEven = (b.y % 2) == 0;
        bool condition1 = isAYEven && !isBYEven && (a.x < b.x);
        bool condition2 = isBYEven && !isAYEven && (b.x < a.x);
        int penalty = (condition1 || condition2) ? 1 : 0;

        // Final distance
        return Mathf.Max(dyAbs, baseDistance + penalty);
    }

    private float CalculateCost(Vector3Int current, Vector3Int neighbour)
    {
        return 1;
    }

    private List<Vector3Int> WalkableNeighbours(Vector3Int tilePosition, Vector3Int goal,GridEntityType gridEntityType)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>();

        List<Vector3Int> directions;

        if (tilePosition.y % 2 == 0)
            directions = new List<Vector3Int>(evenYNeighboursDirectionVectors);
        else
            directions = new List<Vector3Int>(oddYNeighboursDirectionVectors);


        foreach (Vector3Int direction in directions)
        {
            if((tilePosition+direction == goal) || IsTileWalkable(tilePosition + direction, gridEntityType))
            {
                neighbours.Add(tilePosition + direction);
            }
        }

        return neighbours;
    }

    private List<GameObject> VisibleGameObjectsFullCircle(Vector3Int tilePosition, int range)
    {
        List<GameObject> gameObjects = new List<GameObject>();



        return gameObjects;
    }


    public List<Character> GetAdjacentGameObjects(Vector3Int tilePosition)
    {
        List<Vector3Int> directions;

        List<Character> neighbours = new List<Character>();

        if (tilePosition.y % 2 == 0)
            directions = new List<Vector3Int>(evenYNeighboursDirectionVectors);
        else
            directions = new List<Vector3Int>(oddYNeighboursDirectionVectors);

        foreach (Vector3Int direction in directions)
        {
            gridEntities.TryGetValue(tilePosition + direction, out Dictionary<GridEntityType, GameObject> gameObjects);
            if(gameObjects != null)
            {
                gameObjects.TryGetValue(GridEntityType.CHARACTER, out GameObject characterGameObject);
                if(characterGameObject != null)
                {
                    neighbours.Add(characterGameObject.GetComponent<Character>());
                }
            }
        }

        return neighbours;
    }


    public Character GetFirstCharacterInDirection(Vector3Int actorPosition, int range, int direction)
    {
        Character character = null;

        Vector3Int offset;
        Vector3Int pos;

        offset = actorPosition.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];
        pos = actorPosition;
        for (int i = 1; i <= range; i++)
        {
            pos += offset;
            offset = pos.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];

            Character target = GridEntitiesManager.instance.GetCharacterAtTile(pos);
            if (target != null)
            {
                character = target;
                break;
            }
        }

        return character;
    }

    public int GetDirection(Vector3Int startPosition, int range, Vector3Int targetedTile)
    {
        Vector3Int offset;
        Vector3Int pos;
        int direction = -1;
        for (int dir = 0; dir < 6; dir++)
        {
            offset = startPosition.y % 2 == 0 ? evenYNeighboursDirectionVectors[dir] : oddYNeighboursDirectionVectors[dir];
            pos = startPosition;

            for (int i = 1; i <= range; i++)
            {
                pos += offset;
                offset = pos.y % 2 == 0 ? evenYNeighboursDirectionVectors[dir] : oddYNeighboursDirectionVectors[dir];

                if (pos == targetedTile)
                {
                    direction = dir;
                    return direction;
                }
            }
        }
        return direction;
    }

    public Vector3 HookCharacter(Vector3Int actorPosition, int range, int direction)
    {
        Character character = null;


        Vector3Int offset;
        Vector3Int pos;

        offset = actorPosition.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];
        pos = actorPosition;
        for (int i = 1; i <= range; i++)
        {
            pos += new Vector3Int(offset.x, offset.y, 0);
            offset = pos.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];

            Character target = GridEntitiesManager.instance.GetCharacterAtTile(pos);
            if (target != null)
            {
                character = target;
                break;
            }
        }

        offset = actorPosition.y % 2 == 0 ? evenYNeighboursDirectionVectors[direction] : oddYNeighboursDirectionVectors[direction];
        Vector3Int newPosition = actorPosition + offset;
        MoveEntityToTilePosition(pos, newPosition, GridEntityType.CHARACTER);
        return baseTilemap.GetCellCenterWorld(newPosition);
    }

    public Vector3 GetCellCenter(Vector3Int targetedTile)
    {
        return baseTilemap.GetCellCenterWorld(targetedTile);
    }

    public List<Character> GetCharactersInRange(Vector3Int startTile, int range)
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<(Vector3Int pos, int dist)> frontier = new Queue<(Vector3Int, int)>();

        List<Character> result = new List<Character>();

        frontier.Enqueue((startTile, 0));
        visited.Add(startTile);

        while (frontier.Count > 0)
        {
            var (currentTile, distance) = frontier.Dequeue();

            // Check for a character on this tile (but skip the center unless you want self)
            if (gridEntities.TryGetValue(currentTile, out var gameObjects))
            {
                if (gameObjects.TryGetValue(GridEntityType.CHARACTER, out GameObject charObj))
                {
                    if (distance != 0)
                    {
                        result.Add(charObj.GetComponent<Character>());
                    }
                }
            }

            // Stop expanding when max range reached
            if (distance >= range)
                continue;

            // Get the correct neighbor list (odd/even offset hex)
            Vector3Int[] dirs = (currentTile.y % 2 == 0)
                ? evenYNeighboursDirectionVectors
                : oddYNeighboursDirectionVectors;

            // Expand to neighbors
            foreach (var dir in dirs)
            {
                Vector3Int nextTile = currentTile + dir;

                if (visited.Contains(nextTile))
                    continue;

                visited.Add(nextTile);
                frontier.Enqueue((nextTile, distance + 1));
            }
        }

        return result;
    }
        public List<Vector3Int> StepTowardTile(Vector3Int from, Vector3Int to)
        {
            Vector3Int[] dirs = (from.y % 2 == 0)
                ? evenYNeighboursDirectionVectors
                : oddYNeighboursDirectionVectors;

            List<Vector3Int> old = new List<Vector3Int>();
            old.Add(from);
            int orgDist = (int)DistanceToTile(from, to);
            List<Vector3Int> candidates = new List<Vector3Int>();

            foreach (var dir in dirs)
            {
                Vector3Int candidate = from + dir;
                int dist = (int)DistanceToTile(candidate, to);
                if (dist < orgDist)
                {
                    candidates.Add(candidate);
                }
            }
            if (candidates.Count > 0) {
                return candidates;
            }
            return old;
        }
        public Vector3Int TryMoveCharacter(Character character, List<Vector3Int> destinations)
        {
            Vector3Int oldPos = GetCellFromPosition(character.transform.position);
            foreach (Vector3Int destination in destinations)
            {
                if (GetCharacterAtTile(destination))
                    continue;

                // Remove from old tile
                gridEntities[oldPos].Remove(GridEntityType.CHARACTER);

                // Add to new tile
                if (!gridEntities.ContainsKey(destination))
                    gridEntities[destination] = new Dictionary<GridEntityType, GameObject>();

                gridEntities[destination][GridEntityType.CHARACTER] = character.gameObject;
                return destination;
            }
            return Vector3Int.back;
        }

}
