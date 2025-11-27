using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class APUI : MonoBehaviour
{
    [SerializeField] GameObject APCircle;

    [SerializeField] Color availableAPColor;
    [SerializeField] Color unavailableAPColor;
    [SerializeField] Color actionAPColor;


    void Start()
    {
        PlayerManager.instance.playerCharacter.OnAPChanged += ChangeAPUI;
        PlayerManager.onNewAction += ChangeAPUI;

        ChangeAPUI(PlayerManager.instance.playerCharacter.currentAP, PlayerManager.instance.playerCharacter.maxAP);
    }

    public void ChangeAPUI(int currentStamina, int maxStamina)
    {
        DestroyAllChildren();
        for (int i = maxStamina; i > 0; i--)
        {
            GameObject newAPCircle = Instantiate(APCircle, transform);
            Image newAPCircleImage = newAPCircle.GetComponent<Image>();
            if (i > currentStamina)
            {
                newAPCircleImage.color = unavailableAPColor;
            }
            else
            {
                newAPCircleImage.color = availableAPColor;
            }
        }
    }
    public void ChangeAPUI(int currentStamina, int maxStamina, int currentActionAP)
    {
        DestroyAllChildren();
        for (int i = maxStamina; i > 0; i--)
        {
            GameObject newAPCircle = Instantiate(APCircle, transform);
            Image newAPCircleImage = newAPCircle.GetComponent<Image>();
            if (i > currentStamina)
            {
                newAPCircleImage.color = unavailableAPColor;
            }
            else if(currentActionAP > 0)
            {
                newAPCircleImage.color = actionAPColor;
                currentActionAP -= 1;
            }
            else
            {
                newAPCircleImage.color = availableAPColor;
            }
        }
    }
    public void DestroyAllChildren()
    {
        // Iterate backward through the children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            // Destroy the GameObject of the current child
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
