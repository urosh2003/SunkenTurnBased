using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PullSelfAction : IAction
{
    Vector3Int actorPosition;
    int direction;
    public PullSelfAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = 3;
        this.baseAPcost = 2;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
        this.cooldown = 3;
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            this.actor.currentAP >= this.APcost &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) == null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            !resolving
            )
        {
            resolving = true;
            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, GridEntitiesManager.instance.GetCellCenter(context.targetedTile));
            }
            await CalculateCooldown();
            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.MinigameDone();
            }
            this.actor.ChangeAP(-this.APcost);
            Vector3 newCharacterPosition = GridEntitiesManager.instance.MoveEntityToTilePosition(actorPosition, context.targetedTile, GridEntityType.CHARACTER);
            int distanceHooked = (int)GridEntitiesManager.instance.DistanceToTileWorld(newCharacterPosition, actor.transform.position);
            this.actor.MoveCharacter(newCharacterPosition, true, distanceHooked);
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
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range && !resolving)
        {
            SelectedTilesManager.instance.DrawSingleUnoccupied(this.context.targetedTile, new TileStyle(TileColor.GREEN, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawCircle(actorPosition, this.range, new TileStyle(TileColor.GREEN, TileType.DEFAULT, TileLayer.RANGE));
    }
}