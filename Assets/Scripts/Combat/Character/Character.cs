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

    private void Start()
    {
        sprite = this.gameObject.gameObject.GetComponent<SpriteRenderer>();
    }

    public virtual void StartTurn()
    {
        RefreshResources();
        OnStartTurn?.Invoke(this.gameObject);
    }

    public virtual void EndTurn()
    {
        OnEndTurn?.Invoke();
    }

    public virtual void MoveCharacter(Vector3 target, bool wholeAction) { }

    public virtual void RefreshResources()
    {
        this.currentFreeMovement = this.maxFreeMovement;
        this.currentAP += this.maxAP;
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
}
