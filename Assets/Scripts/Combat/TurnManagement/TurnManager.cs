using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class TurnManager : MonoBehaviour
{
    public List<GameObject> characters;

    public Character currentTurn;

    public static TurnManager instance;

    private void Awake() => instance = this;

    //public static event Action<Character> OnTurn;

    public static event Action combatStart;
    public static event Action combatEnd;

    public bool combatActive;

    public void StartCombat(List<GameObject> characters)
    {
        combatActive = true;
        turnOrder = new List<Character>();
        this.characters = characters;
        Character.OnEndTurn += NextTurn;
        PlayerCharacter.OnEndPlayerTurn += NextTurn;
        combatStart.Invoke();
        StartRound();
    }

    public List<Character> turnOrder;
    public List<Character> currentTurnOrder;
    public static event Action OnTurnOrderUpdated;
    public static event Action<Character> OnCurrentTurnChanged;

    public void CalculateOrder()
    {
        turnOrder = characters
            .Select(go => go.GetComponent<Character>())
            .OrderByDescending(c => c.initiative)
            .ToList();

        currentTurnOrder = characters
            .Select(go => go.GetComponent<Character>())
            .OrderByDescending(c => c.initiative)
            .ToList(); ;

        OnTurnOrderUpdated?.Invoke(); 
    }

    public void StartRound()
    {
        CalculateOrder();
        NextTurn();
    }

    public void NextTurn()
    {
        if(currentTurn!=null) 
        {
            currentTurnOrder.Remove(currentTurn);
            OnTurnOrderUpdated?.Invoke();
            currentTurn = null;
        }

        if (currentTurnOrder.Count == 0)
            StartRound();
        else
        {
            currentTurn = currentTurnOrder.First();
            OnCurrentTurnChanged?.Invoke(currentTurn);
            currentTurn.StartTurn();
        }
    }

    public void RemoveCharacter(GameObject gameObject)
    {
        this.characters.Remove(gameObject);
        this.turnOrder.Remove(gameObject.GetComponent<Character>());
        this.currentTurnOrder.Remove(gameObject.GetComponent<Character>());
        OnTurnOrderUpdated?.Invoke();
        if (currentTurn.Equals(gameObject.GetComponent<Character>()))
            NextTurn();
        if(characters.Count == 1 && characters.Contains(PlayerManager.instance.gameObject))
            EndCombat();
    }

    public void EndCombat()
    {
        combatActive = false;
        this.characters.Clear();
        combatEnd.Invoke();
    }
}
