using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnOrderUI : MonoBehaviour
{
    public Transform avatarsParent;
    public GameObject avatarPrefab;
    public GameObject divider;
    public Sprite defaultAvatar;

    private List<TurnOrderAvatar> avatars = new List<TurnOrderAvatar>();

    void OnEnable()
    {
        TurnManager.combatStart += OnCombatStart;
        TurnManager.combatEnd += OnCombatEnd;
        TurnManager.OnTurnOrderUpdated += UpdateTurnOrder;
        TurnManager.OnCurrentTurnChanged += HighlightCurrentTurn;
    }

    void OnDisable()
    {
        TurnManager.combatStart -= OnCombatStart;
        TurnManager.combatEnd -= OnCombatEnd;
        TurnManager.OnTurnOrderUpdated -= UpdateTurnOrder;
        TurnManager.OnCurrentTurnChanged -= HighlightCurrentTurn;
    }

    void OnCombatStart() => UpdateTurnOrder();
    void OnCombatEnd() => ClearAvatars();

    void UpdateTurnOrder()
    {
        ClearAvatars();
        foreach (Character character in TurnManager.instance.currentTurnOrder)
        {
            GameObject avatarGO = Instantiate(avatarPrefab, avatarsParent);
            TurnOrderAvatar avatar = avatarGO.GetComponent<TurnOrderAvatar>();
            avatar.Initialize(character, defaultAvatar);
            avatars.Add(avatar);
        }
        Instantiate(divider, avatarsParent);
        foreach (Character character in TurnManager.instance.turnOrder)
        {
            GameObject avatarGO = Instantiate(avatarPrefab, avatarsParent);
            TurnOrderAvatar avatar = avatarGO.GetComponent<TurnOrderAvatar>();
            avatar.Initialize(character, defaultAvatar);
            avatars.Add(avatar);
        }
        HighlightCurrentTurn(TurnManager.instance.currentTurn);
    }

    void HighlightCurrentTurn(Character currentCharacter)
    {
        foreach (TurnOrderAvatar avatar in avatars)
        {
            avatar.SetHighlight(avatar.character == currentCharacter);
        }
    }

    void ClearAvatars()
    {
        foreach (Transform child in avatarsParent) Destroy(child.gameObject);
        avatars.Clear();
    }
}