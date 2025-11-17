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

    public void BasicAttack(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new BasicAttackAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void Hook(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new PullEnemyAction(playerCharacter)); 
            currentState.Enter();
        }
    }

    public void HookSelf(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new PullSelfAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void Whirlpool(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new WhirlpoolAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void Sink(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new TripleAttackAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void Torpedostorm(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new TorpedostormAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void ThrowTorpedo(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new ThrowTorpedoAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void ForceSlam(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new ForceSlamAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void JumpSlam(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new JumpSlamAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void Maelstorm(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new MaelstormAction(playerCharacter));
            currentState.Enter();
        }
    }
    public void EngineOff(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new EngineOffAction(playerCharacter));
            currentState.Enter();
        }
    }

    public void SpareChain(InputAction.CallbackContext context)
    {
        if (context.performed && currentState is not WaitingForTurnState)
        {
            currentState.Exit();
            currentState = new TargetingState(new SpareChainAction(playerCharacter));
            currentState.Enter();
        }
    }
}
