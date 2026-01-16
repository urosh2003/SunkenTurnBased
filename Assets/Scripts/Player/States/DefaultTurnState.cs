
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultTurnState : IPlayerState
{
    Character targetedCharacter;
    public DefaultTurnState()
    {
        this.selectedAction = new MoveAction(PlayerManager.instance.playerCharacter);
        targetedCharacter = null;
    }

    public override async Task<bool> Execute()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        bool finished = await selectedAction.Execute();
        return finished;
    }

    public override void Update(Vector3 mouseWorldPosition)
    {
        if(selectedAction.resolving)
        {
            return;
        }


        Vector3Int targetedTile = GridEntitiesManager.instance.GetCellFromPosition(mouseWorldPosition);
        Character characterOnTile = GridEntitiesManager.instance.GetCharacterAtTile(targetedTile);
        if (targetedCharacter != null)
        {
            targetedCharacter.GetComponent<CharacterDetails>().DisableHighlight();
        }
        if (characterOnTile != null && characterOnTile is not PlayerCharacter)
        {
            characterOnTile.GetComponent<CharacterDetails>().EnableHighlight();

            if(GridEntitiesManager.instance.DistanceToTileWorld(PlayerManager.instance.transform.position, mouseWorldPosition) 
                <= PlayerManager.instance.playerCharacter.basicAttackRange)
            {
                PlayerManager.instance.currentActionIndex = 1;

                selectedAction = new BasicAttackAction(PlayerManager.instance.playerCharacter);
                PlayerManager.instance.playerCharacter.CharacterActionInitiated(selectedAction);

            }
            else
            {
                PlayerManager.instance.currentActionIndex = 0;

                selectedAction = new MoveAction(PlayerManager.instance.playerCharacter);
                PlayerManager.instance.playerCharacter.CharacterActionInitiated(selectedAction);

            }
        }
        else
        {
            selectedAction = new MoveAction(PlayerManager.instance.playerCharacter);
        }
        targetedCharacter = characterOnTile;

        ActionContext newContext = new ActionContext();
        newContext.targetedTile = targetedTile;
        bool contextUpdated = selectedAction.UpdateContext(newContext);

        if (!selectedAction.resolving && contextUpdated)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
            selectedAction.RedrawTiles();
            SelectedTilesManager.instance.DrawSingle(GridEntitiesManager.instance.GetCellFromPosition(PlayerManager.instance.playerCharacter.transform.position)
                , new TileStyle(TileColor.GREEN, TileType.DEFAULT, TileLayer.TARGETING));
        }


    }

    public override void Enter()
    {
        PlayerManager.instance.playerCharacter.CharacterActionInitiated(selectedAction);
        SelectedTilesManager.instance.ClearRangeTiles();
        SelectedTilesManager.instance.ClearTargetingTiles();
        selectedAction.DrawTiles();
    }

    public override void Exit()
    {
        SelectedTilesManager.instance.UnLockHighlights();
        SelectedTilesManager.instance.ClearRangeTiles();
        SelectedTilesManager.instance.ClearTargetingTiles();
    }
}