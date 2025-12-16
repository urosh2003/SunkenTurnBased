using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterDetails : MonoBehaviour
{
    private SpriteRenderer highlight;
    public EnemyHealthbarUI healthbarUI;

    public int locked = 0;
    public bool allOn = false;

    public void Awake()
    {
        healthbarUI = GetComponentInChildren<EnemyHealthbarUI>();
        highlight = this.gameObject.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(obj => obj.name == "Highlight");
        highlight.enabled = false;
        PlayerManager.turnOnAllHighlights += TurnOnAll;
        PlayerManager.turnOffAllHighlights += TurnOffAll;
    }

    private void TurnOffAll()
    {
        allOn = false;
        DisableHighlight();
    }

    private void TurnOnAll()
    {
        allOn = true;
        EnableHighlight();
    }

    public void EnableHighlight()
    {
        if (locked!=0)
        {
            return;
        }
        highlight.enabled = true;
        if (healthbarUI != null)
        {
            healthbarUI.EnableHealthbar();
        }
    }

    public void DisableHighlight()
    {
        if (locked != 0 || allOn)
        {
            return;
        }
        highlight.enabled = false;
        if (healthbarUI != null)
        {
            healthbarUI.DisableHealthbar();
        }
    }

    public void LockAndEnableHighlight()
    {
        locked += 1;
        highlight.enabled = true;
        if (healthbarUI != null)
        {
            healthbarUI.EnableHealthbar();
        }
    }

    public void UnlockAndDisableHighlight()
    {
        locked -= 1;
        if(locked <= 0)
        {
            locked = 0;
        }
        DisableHighlight();
    }

    
}
