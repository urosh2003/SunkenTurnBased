using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PullEnemyAction : IAction
{
    Vector3Int actorPosition;
    int direction;
    public PullEnemyAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = 3;
        this.baseAPcost = 2;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
        this.cooldown = 3;
    }

    public override bool UpdateContext(ActionContext newContext)
    {
        if (this.context.Equals(newContext) || resolving)
        {
            return false;
        }

        this.context = newContext;
        direction = GridEntitiesManager.instance.GetDirection(actorPosition, range, context.targetedTile);

        return true;
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            this.actor.currentAP >= this.APcost &&
            direction != -1 &&
            !resolving
            )
        {
            resolving = true;
            SelectedTilesManager.instance.LockHighlights();

            Character target = GridEntitiesManager.instance.GetFirstCharacterInDirection(actorPosition, range, direction);
            if (target)
            {
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, target.transform);
                }
                await CalculateCooldown();
                if(target)
                {
                    Vector3 newCharacterPosition = GridEntitiesManager.instance.HookCharacter(actorPosition, range, direction);
                    if(newCharacterPosition != target.transform.position)
                    {
                        int distanceHooked = (int)GridEntitiesManager.instance.DistanceToTileWorld(newCharacterPosition, target.transform.position);
                        target.MoveCharacter(newCharacterPosition, false, distanceHooked);
                        actor.CharacterMovedSomeone(target, newCharacterPosition);
                    }


                }
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.MinigameDone();
                }
            }
            this.actor.ChangeAP(-this.APcost);
            return true;
        }
        return false;
    }

    private async Task CalculateCooldown()
    {
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameTwo();
            if (results[0])
            {
                cooldown -= 1;
            }
            if (results[1])
            {
                cooldown -= 1;
            }
        }
    }

    public override void RedrawTiles()
    {
        if (this.context.targetedTile != null && !resolving)
        {
            SelectedTilesManager.instance.DrawOneDirection(actorPosition, range, direction, new TileStyle(TileColor.YELLOW, TileType.XTILE, TileLayer.TARGETING));
        }
        else if(!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawAllDirections(actorPosition, this.range, new TileStyle(TileColor.YELLOW, TileType.DEFAULT, TileLayer.RANGE));
    }
}