using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class JumpSlamAction : IAction
{
    Vector3Int actorPosition;
    Vector3Int targetPosition;
    Character target;
    int phase = 0;
    public JumpSlamAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.APcost = 2;
        this.range = 1;
        this.phase = 1;
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            this.actor.currentAP >= this.APcost &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            this.phase == 1 &&
            !resolving
            )
        {
            phase = 2;
            targetPosition = context.targetedTile;
            target = GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile);
            DrawTiles();

            return false;
        }
        else if (this.phase == 2 && 
            this.context.targetedTile != null &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) == null &&
            GridEntitiesManager.instance.DistanceToTile(targetPosition, this.context.targetedTile) <= 1 &&
            !resolving
            )
        {
            resolving = true;
            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, GridEntitiesManager.instance.GetCellCenter(context.targetedTile));
            }
            List<int> damage = await CalculateCooldown();
            target.TakeDamage(damage[0]);
            if(target)
            {
                Vector3 newTargetPosition = GridEntitiesManager.instance.MoveEntityToTilePosition(targetPosition, context.targetedTile, GridEntityType.CHARACTER);
                target.MoveCharacter(newTargetPosition, false);
                await Task.Delay(200);
            }
           
            Vector3 newCharacterPosition = GridEntitiesManager.instance.MoveEntityToTilePosition(actorPosition, targetPosition, GridEntityType.CHARACTER);
            this.actor.MoveCharacter(newCharacterPosition, false);
            await Task.Delay(200);

            if (target)
            {
                target.TakeDamage(damage[2]);
                await Task.Delay(200);

            }
            this.actor.ChangeAP(-this.APcost);
            this.actor.CharacterAttacked(new List<Character> { target });

            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.MinigameDone();
            }
            return true;
        }
        return false;
    }

    private async Task<List<int>> CalculateCooldown()
    {
        List<int> damage = new();
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameThree();
            for (int i = 0; i < results.Count; i++)
            {
                damage.Add(actor.basicAttackDamage);
                if (results[i])
                { 
                    damage[i] += 1;
                }
            }
        }
        return damage;
    }

    public override void RedrawTiles()
    {
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range && !resolving && this.phase == 1)
        {
            SelectedTilesManager.instance.DrawSingle(this.context.targetedTile, new TileStyle(TileColor.RED, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (this.phase == 2 &&
            GridEntitiesManager.instance.DistanceToTile(targetPosition, this.context.targetedTile) <= 1)
        {
            SelectedTilesManager.instance.DrawSingle(this.context.targetedTile, new TileStyle(TileColor.YELLOW, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (!resolving && this.phase == 2)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
        else if (!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        if (phase == 1)
        {
            SelectedTilesManager.instance.DrawCircle(actorPosition, this.range, new TileStyle(TileColor.RED, TileType.DEFAULT, TileLayer.RANGE));
        }
        else 
        {
            SelectedTilesManager.instance.DrawCircle(targetPosition, 1, new TileStyle(TileColor.YELLOW, TileType.DEFAULT, TileLayer.RANGE));
            SelectedTilesManager.instance.DrawSingle(targetPosition, new TileStyle(TileColor.RED, TileType.XTILE, TileLayer.RANGE));
        }
    }
}