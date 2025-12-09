using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public IState currentState;
    public PlayerCharacter playerCharacter;
    [SerializeField] public LayerMask npcLayerMask;
    public List<ActionHolder> availableActions = new();
    public List<ActionData> allActionsData = new();
    public int currentActionIndex;

    public static event Action turnOnAllHighlights;
    public static event Action turnOffAllHighlights;
    public static event Action<int,int,int> onNewAction;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        this.playerCharacter = GetComponent<PlayerCharacter>();

        currentState = new WaitingForTurnState();
        currentState.Enter();
        PlayerCharacter.animationDone += ResetState;
        PlayerCharacter.OnStartPlayerTurn += StartTurn;
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(MoveAction), new ActionData()));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(BasicAttackAction), allActionsData[0]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(PullEnemyAction), allActionsData[1]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(PullSelfAction), allActionsData[2]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(WhirlpoolAction), allActionsData[3]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(TorpedostormAction), allActionsData[4]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(MaelstormAction), allActionsData[5]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(ThrowTorpedoAction), allActionsData[6]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(SpareChainAction), allActionsData[7]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(ForceSlamAction), allActionsData[8]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(TripleAttackAction), allActionsData[9]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(ChargeAction), allActionsData[10]));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(EngineOffAction), allActionsData[11]));

        this.playerCharacter.activePerks.Add(new PullPerk(this.playerCharacter));
        this.playerCharacter.activePerks.Add(new KillRushPerk(this.playerCharacter, 2));
        this.playerCharacter.activePerks.Add(new SkilledPerk(this.playerCharacter));
    }

    // Update is called once per frame
    void Update()
    {
        if (MinigameManager.instance.isActive || (currentState.selectedAction!=null && currentState.selectedAction.resolving))
        {
            onNewAction?.Invoke(playerCharacter.currentAP, playerCharacter.maxAP, currentState.selectedAction.APcost);

            return;
        }
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            onNewAction?.Invoke(playerCharacter.currentAP, playerCharacter.maxAP, 0);

            SelectedTilesManager.instance.ClearTargetingTiles();
            return;
        }
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        currentState.Update(worldPosition);
        if (currentState.selectedAction != null)
        {
            onNewAction?.Invoke(playerCharacter.currentAP, playerCharacter.maxAP, currentState.selectedAction.APcost);
        }
        else
        {
            onNewAction?.Invoke(playerCharacter.currentAP, playerCharacter.maxAP, 0);
        }
    }

    public void UpdateAPUI()
    {
    }

    void ResetState()
    {
        currentActionIndex = 0;
        currentState = new DefaultTurnState();
        currentState.Enter();
    }

    public void Click(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (MinigameManager.instance.isActive)
            {
                MinigameManager.instance.Hit();
            }
            else
            {
                _ = Execute();
            }
        }
    }

    public async Task Execute()
    {
        bool finished = await currentState.Execute();
        if (finished)
        {
            playerCharacter.CharacterActed(currentState.selectedAction);
            availableActions[currentActionIndex].SetCooldown(currentState.selectedAction.cooldown);
            currentState.Exit();
            ResetState();
        }
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && !currentState.selectedAction.resolving)
        {
            currentState.Exit();
            ResetState();
        }
    }

    public void EndTurn()
    {
        if (TurnManager.instance.currentTurn == playerCharacter)
        {
            playerCharacter.EndTurn();
            currentState.Exit();
            currentState = new WaitingForTurnState();
            currentState.Enter();
        }
    }

    public void StartTurn()
    {
        currentState.Exit();
        ResetState();
    }

    public void Highlights(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            turnOnAllHighlights?.Invoke();
        }
        if(context.canceled)
        {
            turnOffAllHighlights?.Invoke();
        }
    }

    public void Ability1(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 1)
        {
            bool created = availableActions[1].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 1;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability2(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 2)
        {
            bool created = availableActions[2].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 2;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability3(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 3)
        {
            bool created = availableActions[3].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 3;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability4(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 4)
        {
            bool created = availableActions[4].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 4;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability5(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 5)
        {
            bool created = availableActions[5].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 5;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability6(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 6)
        {
            bool created = availableActions[6].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 6;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability7(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 7)
        {
            bool created = availableActions[7].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 7;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability8(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 8)
        {
            bool created = availableActions[8].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 8;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability9(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 9)
        {
            bool created = availableActions[9].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 9;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability10(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 10)
        {
            bool created = availableActions[10].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 10;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability11(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 11)
        {
            bool created = availableActions[11].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 11;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }

    public void Ability12(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState && availableActions.Count > 12)
        {
            bool created = availableActions[12].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = 12;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }
    public void useAbility(int number)
    {
        if (currentState is not WaitingForTurnState && availableActions.Count > number)
        {
            bool created = availableActions[number].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = number;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }
}
