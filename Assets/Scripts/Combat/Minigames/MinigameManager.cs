using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager instance;

    public IMinigame currentMinigame;
    public GameObject minigame1;
    public GameObject minigame2;
    public GameObject minigame3;
    public GameObject minigame4;

    public float timeTotal;

    public float timeTotalRandomLow;
    public float timeTotalRandomHigh;
    public float successStartRandomLow;
    public float successStartRandomHigh;

    private TaskCompletionSource<List<bool>> minigameComplete;
    public static event Action<List<bool>> minigameDone; 

    public bool isActive;

    private void Awake()
    {
        instance = this;
    }

    public void Hit(InputAction.CallbackContext context)
    {
        if (context.performed && currentMinigame != null && isActive)
        {
            Hit();
        }
    }

    public void Hit()
    {
        if (currentMinigame != null)
        {
            currentMinigame.Hit();
        }
    }

    public async Task<List<bool>> PlayMinigameOne()
    {
        isActive = true;

        minigameComplete = new TaskCompletionSource<List<bool>>();

        minigame1.SetActive(true);
        currentMinigame = minigame1.GetComponent<ChainMinigameOne>();

        float randomTotal = timeTotal + UnityEngine.Random.Range(timeTotalRandomLow, timeTotalRandomHigh);
        float randomSuccessStart = randomTotal * UnityEngine.Random.Range(successStartRandomLow, successStartRandomHigh);
        currentMinigame.StartMinigame(randomTotal, randomSuccessStart, randomSuccessStart + randomTotal * 0.1f);

        List<bool> results = await minigameComplete.Task;
        minigameDone.Invoke(results);
        isActive = false;
        currentMinigame = null;

        return results;
    }

    public async Task<List<bool>> PlayMinigameTwo()
    {
        isActive = true;

        minigameComplete = new TaskCompletionSource<List<bool>>();

        minigame2.SetActive(true);
        currentMinigame = minigame2.GetComponent<ChainMinigameTwo>();

        float randomTotal = timeTotal + UnityEngine.Random.Range(timeTotalRandomLow, timeTotalRandomHigh);
        float randomSuccessStart = randomTotal * UnityEngine.Random.Range(successStartRandomLow, successStartRandomHigh);
        currentMinigame.StartMinigame(randomTotal, randomSuccessStart, randomSuccessStart + randomTotal * 0.1f);

        List<bool> results = await minigameComplete.Task;
        minigameDone.Invoke(results);

        isActive = false;
        currentMinigame = null;

        return results;
    }
    public async Task<List<bool>> PlayMinigameThree()
    {
        isActive = true;

        minigameComplete = new TaskCompletionSource<List<bool>>();

        minigame3.SetActive(true);
        currentMinigame = minigame3.GetComponent<ChainMinigameThree>();

        float randomTotal = timeTotal + UnityEngine.Random.Range(timeTotalRandomLow, timeTotalRandomHigh);
        float randomSuccessStart = randomTotal * UnityEngine.Random.Range(successStartRandomLow, successStartRandomHigh);
        currentMinigame.StartMinigame(randomTotal, randomSuccessStart, randomSuccessStart + randomTotal * 0.1f);

        List<bool> results = await minigameComplete.Task;
        minigameDone.Invoke(results);

        isActive = false;
        currentMinigame = null;

        return results;
    }

    public void EndMinigame(List<bool> results)
    {
        if (minigameComplete?.Task.IsCompleted == false)
            minigameComplete.TrySetResult(results);

        minigame1.SetActive(false);
        minigame2.SetActive(false);
        minigame3.SetActive(false);
        minigame4.SetActive(false);
    }

    internal async Task<List<bool>> PlayMinigameFour(int size)
    {
        isActive = true;

        minigameComplete = new TaskCompletionSource<List<bool>>();

        minigame4.SetActive(true);
        currentMinigame = minigame4.GetComponent<ChainMinigameFour>();
        minigame4.GetComponent<ChainMinigameFour>().SetSize(size);

        float randomTotal = timeTotal + UnityEngine.Random.Range(timeTotalRandomLow, timeTotalRandomHigh);
        float randomSuccessStart = randomTotal * UnityEngine.Random.Range(successStartRandomLow, successStartRandomHigh);
        currentMinigame.StartMinigame(randomTotal+2, randomSuccessStart, randomSuccessStart + randomTotal * 0.1f);

        List<bool> results = await minigameComplete.Task;
        minigameDone.Invoke(results);

        isActive = false;
        currentMinigame = null;

        return results;
    }
}
