
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridEntity : MonoBehaviour
{
    [SerializeField] private bool walkable;
    [SerializeField] private bool targetable;
    [SerializeField] private bool destroyable;
    [SerializeField] private bool blocksVision;
    [SerializeField] private GridEntityType gridEntityType;

    private GridEntitiesManager gridEntities;

    void Start()
    {
        gridEntities = Resources.FindObjectsOfTypeAll<GridEntitiesManager>()
        .FirstOrDefault();

        Vector3Int tilePosition = gridEntities.baseTilemap.WorldToCell(this.transform.position);
        this.transform.position = gridEntities.baseTilemap.GetCellCenterWorld(tilePosition);

        gridEntities.AddGridEntity(tilePosition, this.gameObject, this.gridEntityType);
    }

    public bool IsWalkable()
    {
        return walkable;
    }
}
