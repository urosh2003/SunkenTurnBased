using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public RectTransform healthBar;
    public RectTransform transitionBar;

    public Character character;

    public GameObject healthbarObject;

    public CharacterDetails details;

    public float transitionDuration = 0.2f;
    public bool inTransition = false;

    private void Start()
    {
        character.OnHealthChanged += UpdateHealthBar;
        EnableHealthbar();
    }

    public void EnableHealthbar()
    {
        text.text = character.currentHealth.ToString() + " / " + character.maxHealth.ToString();
        float newScale = (float)character.currentHealth / (float)character.maxHealth;
        healthBar.localScale = new Vector3(newScale, 1, 1);
        transitionBar.localScale = new Vector3(newScale, 1, 1);
    }

    public void DisableHealthbar()
    {
        healthbarObject.SetActive(false);
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        text.text = currentHealth.ToString() + " / " + maxHealth.ToString();

        float newScale = (float)currentHealth / (float)maxHealth;
        if (newScale < 0)
            newScale = 0;

        healthBar.localScale = new Vector3(newScale, 1, 1);

        details.LockAndEnableHighlight();
        StartCoroutine(DamageTransition(newScale));
    }

    private IEnumerator DamageTransition(float newScale)
    {
        float oldScale = transitionBar.localScale.x;
        float elapsed = 0f;
        inTransition = true;

        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            float currentScale = Mathf.Lerp(oldScale, newScale, t);
            transitionBar.localScale = new Vector3(currentScale, 1, 1);
            elapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        transitionBar.localScale = new Vector3(newScale, 1, 1);
        inTransition = false;
    }

    public IEnumerator RemoveHighlight()
    {
        yield return new WaitForSeconds(removeDelay);

        if (!inTransition)
        {
            details.UnlockAndDisableHighlight();
        }
    }

    public float removeDelay = 0.9f;
}
