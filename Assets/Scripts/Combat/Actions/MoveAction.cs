using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MoveAction : IAction
{
    List<Vector3Int> path;
    Vector3Int actorPosition;

    public MoveAction(Character actor) : base(actor)
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
        path = GridEntitiesManager.instance.FindPath(actorPosition, context.targetedTile, GridEntityType.CHARACTER);
        APcost = path.Count;

        return true;
    }

    public override bool Execute()
    {
        if (actor.currentAP >= APcost && APcost != 0)
        {
            Vector3 newCharacterPosition = GridEntitiesManager.instance.MoveEntityToTilePosition(actorPosition, path[path.Count-1], GridEntityType.CHARACTER);

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
        SelectedTilesManager.instance.DrawTargetingPath(path, actor.currentAP);
    }

    public override void DrawTiles()
    {
        //SelectedTilesManager.instance.DrawCircleRange(actorPosition, actor.currentAP);
    }
}