using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class MaelstormAction : IAction
{
    Vector3Int actorPosition;
    int direction;
    public MaelstormAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = 2;
        this.baseAPcost = 3;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
        this.cooldown = 2;
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
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            !resolving
            )
        {
            resolving = true;
            List<Character> targets = GridEntitiesManager.instance.GetCharactersInRange(actorPosition, this.range);
            if (targets.Count != 0)
            {
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.FocusOnSingleAsync(actor.transform);
                }
                List<int> damage = await CalculateDamage(targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].TakeDamage(damage[i]);
                }

                await Task.Delay(200);
                for (int i = targets.Count-1; i >= 0; i--)
                {
                    if (targets[i] == null || targets[i].currentHealth <= 0)
                    {
                        targets.RemoveAt(i);
                        actor.CharacterKilledEnemy();
                    }
                }

                List<(Character c, List<Vector3Int> candidates)> pullData =
                new List<(Character, List<Vector3Int>)>();
                // Prepare movement data
                foreach (var t in targets)
                {
                    Vector3Int pos = GridEntitiesManager.instance.GetCellFromPosition(t.transform.position);
                    List<Vector3Int> steps = GridEntitiesManager.instance.StepTowardTile(pos, actorPosition);
                    pullData.Add((t, steps));
                }

                // Move restricted (1-option) characters first
                foreach (var (c, steps) in pullData)
                {
                    if (steps.Count == 1)
                    {
                        Vector3Int newTile = GridEntitiesManager.instance.TryMoveCharacter(c, steps);
                        if (newTile != Vector3Int.back)
                        {
                            c.MoveCharacter(GridEntitiesManager.instance.GetCellCenter(newTile));
                            actor.CharacterMovedSomeone(c, GridEntitiesManager.instance.GetCellCenter(newTile));

                        }
                    }
                }

                // Move flexible (2+-options) characters after
                foreach (var (c, steps) in pullData)
                {
                    if (steps.Count > 1)
                    {
                        Vector3Int newTile = GridEntitiesManager.instance.TryMoveCharacter(c, steps);
                        if (newTile != Vector3Int.back)
                            c.MoveCharacter(GridEntitiesManager.instance.GetCellCenter(newTile));
                    }
                }
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.MinigameDone();
                }
            }
            this.actor.ChangeAP(-this.APcost);
            this.actor.CharacterAttacked(targets);

            return true;
        }
        return false;
    }

    private async Task<List<int>> CalculateDamage(List<Character> targets)
    {
        List<int> damage = new();
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameFour(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                damage.Add(1 + bonusDamage);

                if (results[i])
                    damage[i] += 1;
            }
        }
        return damage;
    }

    public override void RedrawTiles()
    {
        if (this.context.targetedTile != null &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            !resolving
            )
        {
            SelectedTilesManager.instance.DrawCircle(actorPosition, this.range, new TileStyle(TileColor.RED, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawCircle(actorPosition, this.range, new TileStyle(TileColor.RED, TileType.DEFAULT, TileLayer.RANGE));
    }
}