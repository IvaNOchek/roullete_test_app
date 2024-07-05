using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class RouletteWheel_1 : MonoBehaviour
{
    [System.Serializable]
    public struct Prize
    {
        public string name;
        public int minAmount;
        public int maxAmount;
        public float weight;
        public float probabilityWeight;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<PrizeData> wonPrizes = new List<PrizeData>();
    }

    [System.Serializable]
    public class PrizeData
    {
        public string name;
        public int amount;
        public float weight;

        public PrizeData(string name, int amount, float weight)
        {
            this.name = name;
            this.amount = amount;
            this.weight = weight;
        }
    }

    public float rotationSpeed = 500f;
    public float spinDuration = 3f;
    public int manaCost = 1;
    public int maxMana = 10;
    public float manaRegenTime = 1f;

    public TextMeshProUGUI manaText;
    public Button spinButton;
    public TextMeshProUGUI prizeText;
    public TextMeshProUGUI wonPrizesText;
    public TextMeshProUGUI NumberwonPrizesText;
    public TextMeshProUGUI AllwonPrizesText;
    public Button resetButton;

    public Prize[] prizes;
    public float[][] wheelElementAngles;
    public string[] wheelElementNames;

    private int currentMana;
    private float targetAngle;
    private bool isSpinning = false;
    private float currentSpinTime;
    private SaveData saveData;
    private string savePath;

    void Start()
    {
        savePath = Application.persistentDataPath + "/saveData.json";
        LoadSaveData();

        currentMana = maxMana;
        manaText.text = "Mana: " + currentMana.ToString();
        StartCoroutine(RegenerateMana());

        UpdateWonPrizesText();

        resetButton.onClick.AddListener(ResetData);
    }

    void Update()
    {
        if (isSpinning)
        {
            currentSpinTime += Time.deltaTime;

            float spinSpeed = Mathf.Lerp(rotationSpeed, 0f, currentSpinTime / spinDuration);
            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

            if (currentSpinTime >= spinDuration)
            {
                isSpinning = false;
                DeterminePrize();
            }
        }
    }

    public void Spin()
    {
        if (!isSpinning && currentMana >= manaCost)
        {
            currentMana -= manaCost;
            manaText.text = "Mana: " + currentMana.ToString();

            currentSpinTime = 0f;
            targetAngle = Random.Range(0f, 360f);
            isSpinning = true;

            prizeText.text = "Rolling...";
        }
        else if (isSpinning)
        {
            Debug.Log("Колесо уже крутится!");
        }
        else
        {
            Debug.Log("Недостаточно маны!");
        }
    }

    private void DeterminePrize()
    {
        float finalAngle = Mathf.Repeat(transform.rotation.eulerAngles.z, 360f);
        int winningIndex = -1;

        for (int i = 0; i < wheelElementAngles.Length; i++)
        {
            if (finalAngle >= wheelElementAngles[i][0] && finalAngle <= wheelElementAngles[i][1])
            {
                winningIndex = i;
                break;
            }
        }

        if (winningIndex >= 0 && winningIndex < prizes.Length)
        {
            Prize wonPrize = prizes[winningIndex];
            int amount = Random.Range(wonPrize.minAmount, wonPrize.maxAmount + 1);
            string winMessage = $"{amount} {wonPrize.name}";
            prizeText.text = winMessage;

            PrizeData existingPrize = saveData.wonPrizes.Find(p => p.name == wonPrize.name);
            if (existingPrize != null)
            {
                existingPrize.amount += amount;
                existingPrize.weight += wonPrize.weight * amount;
            }
            else
            {
                saveData.wonPrizes.Add(new PrizeData(wonPrize.name, amount, wonPrize.weight * amount));
            }
        }
        else
        {
            prizeText.text = "No prize";
        }

        SaveSaveData();
        UpdateWonPrizesText();
    }

    IEnumerator RegenerateMana()
    {
        while (true)
        {
            yield return new WaitForSeconds(manaRegenTime);
            if (currentMana < maxMana)
            {
                currentMana++;
                manaText.text = "Mana: " + currentMana.ToString();
            }
        }
    }

    private void UpdateWonPrizesText()
    {
        string prizesText = "";
        string NumberprizesText = "";
        float totalWeight = saveData.wonPrizes.Sum(p => p.weight);

        string AllprizesText = $"All items: {saveData.wonPrizes.Sum(p => p.amount)}\n";
        AllprizesText += $"All Weight: {totalWeight}";

        foreach (PrizeData prize in saveData.wonPrizes)
        {
            prizesText += $"{prize.name}\n";
            NumberprizesText += $"{prize.amount}\t{prize.weight}\n";
        }

        wonPrizesText.text = prizesText;
        NumberwonPrizesText.text = NumberprizesText;
        AllwonPrizesText.text = AllprizesText;
    }

    private void LoadSaveData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            saveData = new SaveData();
        }
    }

    private void SaveSaveData()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    public void ResetData()
    {
        currentMana = 10;
        manaText.text = "Mana: " + currentMana.ToString();
        saveData.wonPrizes.Clear();
        SaveSaveData();
        UpdateWonPrizesText();
    }
}