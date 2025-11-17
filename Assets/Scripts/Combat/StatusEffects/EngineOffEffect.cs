using System;
using System.Collections.Generic;

public class EngineOffEffect : StatusEffect
{
    public EngineOffEffect(Character character, Character relatedCharacter) : base(2, character, relatedCharacter)
    {
        this.name = StatusEffectName.ENGINEOFF;
        this.type = StatusEffectType.DEBUFF;
    }

    public override void Initialize()
    {
        owner.OnCharacterStartTurn += owner.CantAct;
        relatedCharacter.OnCharacterAct += DecreaseDuration;
    }

    public override void OnRemove()
    {
        owner.OnCharacterStartTurn -= owner.CantAct;
        relatedCharacter.OnCharacterAct -= DecreaseDuration;
    }

    void CharacterAttacked(List<Character> targets)
    {
        Remove();
    }
}