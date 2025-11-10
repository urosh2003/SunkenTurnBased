using System.Collections;
using System.Collections.Generic;
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

    public void Execute(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            bool finished = currentState.Execute();
            if(finished)
            {
                currentState.Exit();
                ResetState();
            }
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
}
