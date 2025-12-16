using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TripleAttackAction : IAction
{
    Vector3Int actorPosition;
    public TripleAttackAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = actor.basicAttackRange;
        this.baseAPcost = 3;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
        this.cooldown = 999;
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
                List<int> damage = await CalculateDamage();
                for (int i = 0; i < damage.Count; i++)
                {
                    if (target != null)
                    {
                        target.TakeDamage(damage[i]);
                        actor.CharacterDamagedEnemy(target, damage[i]);
                        this.actor.CharacterAttacked(new List<Character> { target });
                        await Task.Delay(600);
                    }
                    if(target == null || target.currentHealth <= 0)
                    {
                        actor.CharacterKilledEnemy();
                        break;
                    }
                }
            }
            this.actor.ChangeAP(-this.APcost);
            if (actor is PlayerCharacter)
            {
               await CameraActionFocus.instance.MinigameDone();
            }
            return true;
        }
        return false;
    }

    private async Task<List<int>> CalculateDamage()
    {
        List<int> damage = new();
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameThree();
            for (int i = 0; i < results.Count; i++)
            {
                damage.Add(actor.basicAttackDamage + bonusDamage);
                if(results[i])
                {
                    damage[i] += minigameBonusDamage;
                }
            }
        }
        return damage;
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