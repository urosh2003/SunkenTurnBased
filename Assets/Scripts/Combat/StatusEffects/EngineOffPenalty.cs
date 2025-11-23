using System;
using System.Collections.Generic;

public class EngineOffPenalty : StatusEffect
{
    public EngineOffPenalty(Character character) : base(2, character)
    {
        this.name = StatusEffectName.ENGINEOFF_PENALTY;
        this.type = StatusEffectType.DEBUFF;
    }

    public override void Initialize()
    {
        owner.OnCharacterActed += (action) => DecreaseDuration();
    }

    public override void OnRemove()
    {
        owner.OnCharacterActed -= (action) => DecreaseDuration();
    }

}