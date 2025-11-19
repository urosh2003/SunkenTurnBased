using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

[Serializable]
public class ActionHolder
{
    public Type actionType { get; private set; }    // runtime-only (not serializable by Unity inspector)
    public int cooldown { get; private set; }
    public string displayName { get; private set; }
    public Character actor;
    public ActionData data;
    ActionButton relatedButton;

    // ctor used from code: new ActionHolder(typeof(FireballAction), 5f, "Fireball")
    public ActionHolder(Character actor, Type actionType, ActionData data)
    {
        if (actionType == null) throw new ArgumentNullException(nameof(actionType));
        if (!typeof(IAction).IsAssignableFrom(actionType))
            throw new ArgumentException("actionType must derive from IAction", nameof(actionType));

        this.data = data;
        this.actionType = actionType;
        cooldown = 0;
        actor.OnCharacterStartTurn += DecreaseCooldown;
        this.actor=actor;
        if(PlayerManager.instance.availableActions.Count!=0)
        {
            relatedButton = ActionHub.instance.AddActionButton(PlayerManager.instance.availableActions.Count, data);
            relatedButton.CooldownUpdated(this.cooldown);
        }
    }

    public void DecreaseCooldown()
    {
        cooldown -= 1;
        if(cooldown < 0)
        {
            cooldown = 0;
        }
        if (relatedButton != null)
        {
            relatedButton.CooldownUpdated(this.cooldown);
        }
    }
    public bool IsOnCooldown => cooldown > 0f;

    // Try to create a new IAction instance. Returns false if on cooldown.
    // Requires that the IAction implementation has a constructor (Character actor)
    public bool TryCreateAction(out IAction action)
    {
        action = null;
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        if (IsOnCooldown) return false;

        try
        {
            var obj = Activator.CreateInstance(actionType, new object[] { actor });
            action = obj as IAction;
            if (action == null) throw new InvalidOperationException("Created instance is not an IAction");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to instantiate action {actionType}: {ex}");
            return false;
        }
    }

    public void SetCooldown(int cooldown)
    {
        this.cooldown = cooldown;
        relatedButton.CooldownUpdated(this.cooldown);
    }
}
