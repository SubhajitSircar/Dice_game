using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceGameManager : MonoBehaviour
{
    public Die3D[] dice;
    public Button rollButton;
    public TMP_Text statusText;
    public TMP_Text pityCounterText;

    private int consecutiveLosses = 0;
    private const int PITY_LIMIT = 3;

    private void Start()
    {
        if (statusText != null) statusText.text = "Press ROLL to Start!";
        UpdatePityText();
        rollButton.onClick.AddListener(StartRoll);
    }

    public void StartRoll()
    {
        StartCoroutine(RollSequence());
    }

    private IEnumerator RollSequence()
    {
        rollButton.interactable = false;
        if (statusText != null) statusText.text = "Rolling...";

        bool guaranteeWin = (consecutiveLosses >= PITY_LIMIT);
        int riggedValue = guaranteeWin ? Random.Range(1, 7) : -1;

        for (int i = 0; i < dice.Length; i++)
        {
            float duration = Random.Range(1.1f, 1.9f);
            dice[i].Roll(duration, riggedValue);
        }

        bool rolling = true;
        while (rolling)
        {
            rolling = false;
            foreach (var die in dice)
            {
                if (die.IsRolling) { rolling = true; break; }
            }
            yield return null;
        }

        int d1 = dice[0].CurrentValue;
        int d2 = dice[1].CurrentValue;
        int d3 = dice[2].CurrentValue;

        if (d1 == d2 && d2 == d3)
        {
            consecutiveLosses = 0;
            if (statusText != null) statusText.text = $"JACKPOT! ({d1} - {d2} - {d3})";
        }
        else
        {
            consecutiveLosses++;
            if (statusText != null) statusText.text = $"Try Again ({d1} - {d2} - {d3})";
        }

        UpdatePityText();
        rollButton.interactable = true;
    }

    private void UpdatePityText()
    {
        if (pityCounterText != null)
        {
            pityCounterText.text = consecutiveLosses >= PITY_LIMIT
                ? "Losses: 3/3 (Next Roll GUARANTEED JACKPOT!)"
                : $"Consecutive Losses: {consecutiveLosses} / {PITY_LIMIT}";
        }
    }
}