using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class TorpedostormAction : IAction
{
    Vector3Int actorPosition;
    int direction;
    int currentStep = 0;
    public TorpedostormAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = 3;
        this.baseAPcost = 2;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
        this.cooldown = 999;
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
        int distance = (int)GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile);
        Character target = GridEntitiesManager.instance.GetFirstCharacterInDirection(actorPosition, distance, direction);
        if (this.context.targetedTile != null &&
            this.actor.currentAP >= this.APcost &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) == null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) > 0 &&
            direction != -1 &&
            range != 0 &&
            !resolving &&
            target == null
            )
        {
            resolving = true;
            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, GridEntitiesManager.instance.GetCellCenter(context.targetedTile));
            }
            this.currentStep = 0;
            while (currentStep <= distance)
            {
                {
                    List<Character> targets = GridEntitiesManager.instance.GetAdjacentGameObjects(actorPosition);
                    if (targets.Count != 0)
                    {
                        List<int> damage = await CalculateDamage(targets);


                        for (int j = 0; j < targets.Count; j++)
                        {
                            targets[j].TakeDamage(damage[j]);
                        }
                    }
                    if (currentStep < distance)
                    {
                        Vector3 newCharacterPosition = GridEntitiesManager.instance.MoveCharacterInDirection(actorPosition, direction);
                        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(newCharacterPosition);
                        actor.MoveCharacter(newCharacterPosition, false);
                    }
                    currentStep++;
                    this.actor.CharacterAttacked(targets);

                    // Small delay for visual effect
                    await Task.Delay(100);
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

    private async Task<List<int>> CalculateDamage(List<Character> targets)
    {
        List<int> damage = new();
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameFour(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                damage.Add(1);

                if (results[i])
                    damage[i] += 1;
            }
        }
        return damage;
    }

    public override void RedrawTiles()
    {
        int distance = (int)GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile);
        if (this.context.targetedTile != null && !resolving &&
            distance <= this.range &&
            direction != -1 &&
            GridEntitiesManager.instance.GetFirstCharacterInDirection(actorPosition, distance, direction) == null
            )
        {
            SelectedTilesManager.instance.DrawTorpedostorm(actorPosition,
                (int)GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile), direction);
        }
        else if (!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawAllDirections(actorPosition, this.range, new TileStyle(TileColor.GREEN, TileType.DEFAULT, TileLayer.RANGE));
    }
}