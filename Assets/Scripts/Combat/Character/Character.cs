using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] public string characterName;

    public int maxHealth;
    public int currentHealth;

    public int currentAP;
    public int maxAP;


    public int maxFreeMovement;
    public int currentFreeMovement;

    public int initiative;

    public static event Action OnEndTurn;
    public static event Action<GameObject> OnStartTurn;

    public Sprite avatar;

    public SpriteRenderer sprite;

    public int basicAttackDamage;
    public int basicAttackRange;

    public List<StatusEffect> activeEffects = new();
    public event Action OnCharacterStartTurn;
    public event Action OnCharacterEndTurn;
    public event Action OnCharacterMove;
    public event Action<List<Character>> OnCharacterAttack;
    public event Action OnCharacterAct;

    public bool canMove = true;
    public bool canAct = true;
    public bool canAttack = true;

    private void Start()
    {
        sprite = this.gameObject.gameObject.GetComponent<SpriteRenderer>();
    }

    public void InvokeOnStartCharacterTurn()
    {
        OnCharacterStartTurn?.Invoke();
    }
    public virtual void StartTurn()
    {
        RefreshResources();
        OnCharacterStartTurn?.Invoke();
        OnStartTurn?.Invoke(this.gameObject);
    }

    public virtual void EndTurn()
    {
        OnCharacterEndTurn?.Invoke();
        OnEndTurn?.Invoke();
    }

    public virtual void MoveCharacter(Vector3 target, bool wholeAction = false) {
        OnCharacterMove?.Invoke();     
    }

    public virtual void CharacterAttacked(List<Character> targets)
    {
        OnCharacterAttack?.Invoke(targets);
    }

    public virtual void CharacterActed()
    {
        OnCharacterAct?.Invoke();
    }

    public virtual void RefreshResources()
    {
        this.currentFreeMovement = this.maxFreeMovement;
        this.currentAP += this.maxAP;
        this.canMove = true;
        this.canAttack = true;
        this.canAct = true;
    }

    public virtual void ChangeAP(int value)
    {
        this.currentAP += value;
    }

    public virtual void TakeDamage(int value)
    {
        this.currentHealth -= value;
        StartCoroutine(TurnRed());
    }

    private float redTime = 0.2f;

    private IEnumerator TurnRed()
    {
        this.sprite.color = Color.red;

        yield return new WaitForSeconds(redTime);

        this.sprite.color = Color.white;
    }

    public override bool Equals(object other)
    {
        if(other == null) return false;
        if(other is Character)
        {
            Character co = (Character)other;
            return this.characterName.Equals(co.characterName);
        }
        return false;
    }

    public virtual bool CanMove()
    {
        return this.currentAP > 0 || this.currentFreeMovement > 0;
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        // Replace existing effect of the same typeName
        StatusEffect existing = activeEffects.Find(e => e.name == effect.name);
        if (existing != null)
        {
            //existing.duration = effect.duration;   // refresh duration
            return;
        }

        activeEffects.Add(effect);
    }

    public void RemoveStatusEffect(StatusEffectName name)
    {
        // Collect the ones we are going to remove
        var removeList = activeEffects.FindAll(e => e.name == name);

        // Call Remove
        foreach (var effect in removeList)
            effect.Remove();
    }


    public List<StatusEffect> GetActiveBuffs()
    {
        return activeEffects.FindAll(e => e.type == StatusEffectType.BUFF);
    }
    public List<StatusEffect> GetActiveDebuffs()
    {
        return activeEffects.FindAll(e => e.type == StatusEffectType.DEBUFF);
    }
    public bool HasStatusEffect(StatusEffectName name)
    {
        return activeEffects.Exists(e => e.name == name);
    }
    public void RemoveAllBuffs()
    {
        var removeList = activeEffects.FindAll(e => e.type == StatusEffectType.BUFF);

        foreach (var effect in removeList)
            effect.Remove();
    }


    public void RemoveAllDebuffs()
    {
        var removeList = activeEffects.FindAll(e => e.type == StatusEffectType.DEBUFF);

        foreach (var effect in removeList)
            effect.Remove();
    }


    public void CantMove()
    {
        this.canMove = false;
    }
    public void CantAct()
    {
        this.canAct = false;
    }
    public void CantAttack()
    {
        this.canAttack = false;
    }

    internal int GetCostModifiers(IAction action)
    {
        int cost = 0;
        if (HasStatusEffect(StatusEffectName.ENGINEOFF_PENALTY))
        {
            cost += 2;
        }
        return cost;
    }
}
