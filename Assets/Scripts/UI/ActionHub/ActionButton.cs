using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] string title;
    [SerializeField] TextMeshProUGUI keybindText;
    [SerializeField] int number;
    [SerializeField] GameObject actionDetails;

    [SerializeField] TextMeshProUGUI actionName;
    [SerializeField] TextMeshProUGUI actionRange;
    [SerializeField] TextMeshProUGUI actionCost;
    [SerializeField] TextMeshProUGUI actionBaseCooldown;
    [SerializeField] TextMeshProUGUI actionDescription;

    [SerializeField] GameObject cooldownRemaining;
    [SerializeField] GameObject cooldownMask;

    public ActionButton SetUpActionButton(int number, ActionData actionData)
    {
        this.number = number;
        if(number < 10)
        {
            keybindText.text = number.ToString();
        }
        if (number == 10)
        {
            keybindText.text = "0";
        }
        if (number == 11)
        {
            keybindText.text = "-";
        }
        if (number == 12)
        {
            keybindText.text = "=";
        }
        icon.sprite = actionData.uiSprite;
        actionName.text = actionData.name;
        actionDescription.text = actionData.actionDescription;
        actionBaseCooldown.text = actionData.baseCooldown.ToString();
        if (actionData.baseCooldown > 10)
        {
            actionBaseCooldown.text = "Once per combat";
        }
        actionCost.text = actionData.baseAPCost.ToString();
        if(actionData.baseRange == 1)
        {
            actionRange.text = "Melee";
        }
        else
        {
            actionRange.text = actionData.baseRange.ToString();
        }
        return this;
    }

    public void UseAbility()
    {
        PlayerManager.instance.useAbility(number);
    }

    public void EnableActionDetails()
    {
        actionDetails.SetActive(true);
    }

    public void DisableActionDetails()
    {
        actionDetails.SetActive(false);
    }

    public void CooldownUpdated(int remaining)
    {
        if (remaining > 0)
        {
            GetComponent<Button>().enabled = false;
            cooldownMask.SetActive(true);
            if (remaining < 10)
            {
                cooldownRemaining.SetActive(true);
                cooldownRemaining.GetComponent<TextMeshProUGUI>().text = remaining.ToString();
            }
        }
        else
        {
            GetComponent<Button>().enabled = true;
            cooldownMask.SetActive(false);
            cooldownRemaining.SetActive(false);
        }
    }
}