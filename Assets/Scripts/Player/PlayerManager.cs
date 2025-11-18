using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public IState currentState;
    public PlayerCharacter playerCharacter;
    [SerializeField] public LayerMask npcLayerMask;
    public List<ActionHolder> availableActions = new();
    public int currentActionIndex;
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
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(MoveAction), "move action"));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(BasicAttackAction), "basic attack"));
        availableActions.Add(new ActionHolder(this.playerCharacter, typeof(EngineOffAction), "EngineOffAction"));
    }

    // Update is called once per frame
    void Update()
    {
        if (MinigameManager.instance.isActive)
        {
            return;
        }
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        currentState.Update(worldPosition);
    }

    void ResetState()
    {
        currentActionIndex = 0;
        currentState = new TargetingState(new MoveAction(playerCharacter));
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
            playerCharacter.CharacterActed();
            availableActions[currentActionIndex].SetCooldown(currentState.selectedAction.cooldown);
            currentState.Exit();
            ResetState();
        }
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new MoveAction(playerCharacter));
            currentState.Enter();
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
        currentState = new TargetingState(new MoveAction(playerCharacter));
        currentState.Enter();
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
}
