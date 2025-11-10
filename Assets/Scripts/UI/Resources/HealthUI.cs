using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    void Start()
    {
        text = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
        .FirstOrDefault(obj => obj.name.Equals("HealthText"));
        PlayerCharacter.OnPlayerHealthChange += ChangeHealthUI;
    }

    public void ChangeHealthUI(int value)
    {
        text.text = "Current Health: " + value.ToString();
    }
}
