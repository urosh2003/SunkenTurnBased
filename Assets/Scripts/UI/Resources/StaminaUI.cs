using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    void Start()
    {
        text = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
        .FirstOrDefault(obj => obj.name.Equals("StaminaText"));
        PlayerCharacter.OnPlayerStaminaChange += ChangeStaminaUI;
    }

    public void ChangeStaminaUI(int value)
    {
        text.text = "Current Stamina: " + value.ToString();
    }
}
