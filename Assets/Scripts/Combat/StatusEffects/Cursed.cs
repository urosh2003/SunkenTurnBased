using System;
using System.Collections.Generic;

public class Cursed : StatusEffect
{
    int damage;
    public Cursed(Character character, Character relatedCharacter, int damage) : base(1, character, relatedCharacter)
    {
        this.name = StatusEffectName.CURSE;
        this.type = StatusEffectType.DEBUFF;
        this.damage = damage;
    }

    public override void Initialize()
    {
        relatedCharacter.OnCharacterAttack += CharacterAttacked;
    }

    public override void OnRemove()
    {
        relatedCharacter.OnCharacterAttack -= CharacterAttacked;
    }

    void CharacterAttacked(List<Character> targets)
    {
        if(targets.Contains(owner))
        {
            owner.TakeDamage(damage);
        }
        if(owner)
        {
            owner.RemoveStatusEffect(StatusEffectName.CURSE);
        }
    }
}