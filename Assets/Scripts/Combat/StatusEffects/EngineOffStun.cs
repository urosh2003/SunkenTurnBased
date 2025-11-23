using System;
using System.Collections.Generic;

public class EngineOffStun : StatusEffect
{
    public EngineOffStun(Character character, Character relatedCharacter) : base(2, character, relatedCharacter)
    {
        this.name = StatusEffectName.ENGINEOFF_STUN;
        this.type = StatusEffectType.DEBUFF;
    }

    public override void Initialize()
    {
        owner.OnCharacterStartTurn += owner.CantAct;
        relatedCharacter.OnCharacterActed += (action) => DecreaseDuration();
    }

    public override void OnRemove()
    {
        owner.OnCharacterStartTurn -= owner.CantAct;
        relatedCharacter.OnCharacterActed -= (action) => DecreaseDuration();
    }

    void CharacterAttacked(List<Character> targets)
    {
        Remove();
    }
}