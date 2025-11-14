using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MoveToCharacterAction : IAction
{
    List<Vector3Int> path;
    Vector3Int actorPosition;

    public MoveToCharacterAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
    }

    public override bool UpdateContext(ActionContext newContext)
    {
        if (this.context.Equals(newContext))
        {
            return false;
        }

        this.context = newContext;
        path = GridEntitiesManager.instance.FindPathToCharacter(actor.transform.position, context.targetedCharacter.transform.position, GridEntityType.CHARACTER);
        for(int i = 1; i < actor.basicAttackRange; i++)
        {
            path.RemoveAt(path.Count - 1);
        }

        APcost = path.Count;
        for (int i = APcost; APcost > actor.currentAP; APcost--)
        {
            path.RemoveAt(path.Count - 1);
        }
        APcost = path.Count;

        return true;
    }

    public override async Task<bool> Execute()
    {
        if (actor.currentAP >= APcost && APcost != 0)
        {
            Vector3 newCharacterPosition = GridEntitiesManager.instance.MoveEntityToTilePosition(actorPosition, path[path.Count - 1], GridEntityType.CHARACTER);

            if (this.actor.currentFreeMovement > 0)
                this.actor.currentFreeMovement -= 1;
            else
                this.actor.ChangeAP(-APcost);

            this.actor.MoveCharacter(newCharacterPosition);
            return true;
        }
        return false;
    }

    public override void RedrawTiles()
    {
        SelectedTilesManager.instance.DrawTargetingPath(path, actor.currentAP, new TileStyle(TileColor.YELLOW, TileType.XTILE, TileLayer.TARGETING));
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.ClearRangeTiles();
    }
}