using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class MoveAction : IAction
{
    List<Vector3Int> path;
    Vector3Int actorPosition;

    public MoveAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.cooldown = 0;
    }

    public override bool UpdateContext(ActionContext newContext)
    {
        if (this.context.Equals(newContext) || resolving)
        {
            return false;
        }

        this.context = newContext;
        path = GridEntitiesManager.instance.FindPath(actorPosition, context.targetedTile, GridEntityType.CHARACTER);
        this.baseAPcost = path.Count;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);

        return true;
    }

    public override async Task<bool> Execute()
    {
        if (actor.currentAP >= APcost && APcost != 0 && !resolving)
        {
            resolving = true;
            Vector3 newCharacterPosition = GridEntitiesManager.instance.MoveEntityToTilePosition(actorPosition, path[path.Count-1], GridEntityType.CHARACTER);

            if (this.actor.currentFreeMovement > 0)
                this.actor.currentFreeMovement -= 1;
            else
                this.actor.ChangeAP(-APcost);

            this.actor.MoveCharacter(newCharacterPosition, true, path.Count);
            return true;
        }
        return false;
    }

    public override void RedrawTiles()
    {
        if (!resolving)
            SelectedTilesManager.instance.DrawTargetingPath(path, actor.currentAP, new TileStyle(TileColor.GREEN, TileType.XTILE, TileLayer.TARGETING));
    }

    public override void DrawTiles()
    {
        //SelectedTilesManager.instance.DrawCircleRange(actorPosition, actor.currentAP);
    }
}