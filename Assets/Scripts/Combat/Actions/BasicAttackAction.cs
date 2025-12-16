using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BasicAttackAction : IAction
{
    Vector3Int actorPosition;
    public BasicAttackAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = actor.basicAttackRange;
        this.cooldown = 0;
        this.baseAPcost = 2;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range
            && this.actor.currentAP >= this.APcost &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != actor &&
            !resolving
            )
        {
            resolving = true;
            SelectedTilesManager.instance.LockHighlights();
            Character target = GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile);
            if (target != null)
            {
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, target.transform);
                }
                int damage = await CalculateDamage();
                target.TakeDamage(damage);
                actor.CharacterDamagedEnemy(target, damage);
                if (target == null || target.currentHealth <= 0)
                {
                    actor.CharacterKilledEnemy();
                }
            }
            this.actor.ChangeAP(-this.APcost);
            this.actor.CharacterAttacked(new List<Character> { target });
            if (actor is PlayerCharacter)
            {
               await CameraActionFocus.instance.MinigameDone();
            }
            resolving = false;
            return true;
        }
        return false;
    }

    private async Task<int> CalculateDamage()
    {
        int damage = actor.basicAttackDamage;
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameOne();
            if (results[0])
            {
                damage += minigameBonusDamage;
            }
        }
        return damage + bonusDamage;
    }

    public override void RedrawTiles()
    { 
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != actor &&
            !resolving
            )
        {
            SelectedTilesManager.instance.DrawSingle(this.context.targetedTile, new TileStyle(TileColor.YELLOW, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawCircle(actorPosition, this.range, new TileStyle(TileColor.YELLOW, TileType.DEFAULT, TileLayer.RANGE));
    }
}