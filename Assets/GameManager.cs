using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public AudioSource audioSource;
    public AudioClip sFlip, sMatch, sMismatch, sGameOver;
    [System.Serializable]
    public struct GridConfig
    {
        public int rows;
        public int cols;
    }
    [Header("Assets")]
    public List<Sprite> allFaceSprites;
    public Sprite cardBackSprite;

    [Header("UI")]
    public GameObject cardUIPrefab;
    public Transform gridContainer;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI streakText;
    public GameObject WinGameObject;
    public Button continueButton; // Reference to the Continue button

    [Header("Grid Settings")]
    public List<GridConfig> gridConfigurations;
    public int rows = 4;
    public int cols = 4;
    public Vector2 cardSize = new Vector2(100f, 150f);
    public Vector2 spacing = new Vector2(10f, 10f);

    [Header("Game Logic")]
    public float mismatchDelay = 0.6f;
    public float initialRevealDuration = 2.0f;

    private Card[] cards;
    private List<Card> flippedUnmatched = new List<Card>();
    private int score = 0;
    private int streak = 0;

    public GameObject gameOpenPanel;
    public GameObject playGamePanel;
    public GameObject PauseMenuPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // At awake, check if a save exists and enable the continue button
        if (continueButton != null)
        {
            continueButton.interactable = PlayerPrefs.HasKey("CardMatch_Save");
        }
    }

    // New method to control the game flow from buttons
    public void PlayGame(bool isNewGame)
    {
        // First, hide the main menu panel and show the game panel
        if (gameOpenPanel != null) gameOpenPanel.SetActive(false);
        if (playGamePanel != null) playGamePanel.SetActive(true);

        if (isNewGame)
        {
            StartNewGame();
        }
        else
        {
            // If LoadGame fails, start a new game instead
            if (!LoadGame())
            {
                StartNewGame();
            }
        }
    }

    private void Start()
    {
        // The Start method is now empty. The game will only start when a button is clicked.
    }

    public void SetGridSize(int index)
    {
        if (index >= 0 && index < gridConfigurations.Count)
        {
            rows = gridConfigurations[index].rows;
            cols = gridConfigurations[index].cols;
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        if (cards != null)
        {
            foreach (var card in cards)
            {
                if (card != null) Destroy(card.gameObject);
            }
        }

        int totalCards = rows * cols;
        if (totalCards % 2 != 0)
        {
            Debug.LogError("Grid must have an even number of cards to form pairs.");
            return;
        }

        int totalPairs = totalCards / 2;
        if (totalPairs > allFaceSprites.Count)
        {
            Debug.LogError("Not enough unique card sprites for the chosen grid size!");
            return;
        }

        List<int> choiceIndices = new List<int>();
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < allFaceSprites.Count; i++)
        {
            availableIndices.Add(i);
        }

        for (int i = 0; i < totalPairs; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int chosenIndex = availableIndices[randomIndex];
            choiceIndices.Add(chosenIndex);
            availableIndices.RemoveAt(randomIndex);
        }

        List<int> faces = new List<int>();
        for (int i = 0; i < totalPairs; i++)
        {
            faces.Add(choiceIndices[i]);
            faces.Add(choiceIndices[i]);
        }

        ShuffleList(faces);

        cards = new Card[totalCards];

        float totalGridWidth = cols * (cardSize.x + spacing.x) - spacing.x;
        float totalGridHeight = rows * (cardSize.y + spacing.y) - spacing.y;
        Vector2 startPos = new Vector2(-totalGridWidth / 2f, totalGridHeight / 2f);

        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardInstance = Instantiate(cardUIPrefab, gridContainer);
            RectTransform cardRectTransform = cardInstance.GetComponent<RectTransform>();

            float posX = startPos.x + (i % cols) * (cardSize.x + spacing.x);
            float posY = startPos.y - (i / cols) * (cardSize.y + spacing.y);

            cardRectTransform.anchoredPosition = new Vector2(posX, posY);

            Card cardScript = cardInstance.GetComponent<Card>();
            int faceID = faces[i];
            cardScript.SetCard(faceID, allFaceSprites[faceID], cardBackSprite);
            cards[i] = cardScript;
        }

        score = 0;
        streak = 0;
        UpdateUI();
        SaveGame();
        StartCoroutine(InitialFlipSequence());
    }

    public void OnCardClicked(Card card)
    {
        if (card.IsMatched) return;
        if (flippedUnmatched.Count > 0 && flippedUnmatched[0] == card) return;

        PlaySound(sFlip);
        flippedUnmatched.Add(card);

        if (flippedUnmatched.Count >= 2)
        {
            SetCardsInteractable(false);
            Card a = flippedUnmatched[0];
            Card b = flippedUnmatched[1];

            if (a.pairID == b.pairID)
            {
                a.IsMatched = true;
                b.IsMatched = true;
                score += 10;
                streak++;
                flippedUnmatched.Clear();
                SetCardsInteractable(true);
                UpdateUI();
                CheckGameOver();
                PlaySound(sMatch);
                SaveGame();
            }
            else
            {
                score -= 2;
                streak = 0;
                UpdateUI();
                PlaySound(sMismatch);
                StartCoroutine(FlipBackAfterDelay(a, b, mismatchDelay));
                SaveGame();
            }
        }
    }

    private IEnumerator FlipBackAfterDelay(Card a, Card b, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!a.IsMatched) a.Flip();
        if (!b.IsMatched) b.Flip();

        flippedUnmatched.Clear();
        SetCardsInteractable(true);
    }

    private void CheckGameOver()
    {
        bool allMatched = true;
        foreach (var c in cards)
        {
            if (c != null && !c.IsMatched)
            {
                allMatched = false;
                break;
            }
        }
        if (allMatched)
        {
            PlaySound(sGameOver);
            SetCardsInteractable(false);
            if (WinGameObject != null)
            {
                WinGameObject.SetActive(true);
            }
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        if (streakText != null)
        {
            streakText.text = streak.ToString();
        }
    }

    private void SetCardsInteractable(bool interactable)
    {
        foreach (var card in cards)
        {
            if (card != null)
            {
                Button button = card.GetComponent<Button>();
                if (button != null)
                {
                    if (!card.IsMatched)
                    {
                        button.interactable = interactable;
                    }
                }
            }
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator InitialFlipSequence()
    {
        SetCardsInteractable(false);
        foreach (var card in cards)
        {
            if (card != null)
            {
                card.RevealImmediate();
            }
        }
        yield return new WaitForSeconds(initialRevealDuration);

        foreach (var card in cards)
        {
            if (card != null)
            {
                card.Hide();
            }
        }
        yield return new WaitForSeconds(0.5f);
        SetCardsInteractable(true);
    }

    #region Save/Load
    public void SaveGame()
    {
        SaveData sd = new SaveData();
        sd.rows = rows;
        sd.cols = cols;
        sd.score = score;
        sd.streak = streak;

        int total = rows * cols;
        sd.faces = new int[total];
        sd.matched = new bool[total];

        for (int i = 0; i < total; i++)
        {
            if (cards[i] != null)
            {
                sd.faces[i] = cards[i].pairID;
                sd.matched[i] = cards[i].IsMatched;
            }
        }
        string json = JsonUtility.ToJson(sd);
        PlayerPrefs.SetString("CardMatch_Save", json);
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    public bool LoadGame()
    {
        if (!PlayerPrefs.HasKey("CardMatch_Save"))
        {
            Debug.Log("No saved game found.");
            return false;
        }

        string json = PlayerPrefs.GetString("CardMatch_Save");
        SaveData sd = JsonUtility.FromJson<SaveData>(json);

        // Clear existing grid
        if (cards != null)
        {
            foreach (var card in cards)
            {
                if (card != null) Destroy(card.gameObject);
            }
        }

        rows = sd.rows;
        cols = sd.cols;
        int total = rows * cols;
        cards = new Card[total];

        float totalGridWidth = cols * (cardSize.x + spacing.x) - spacing.x;
        float totalGridHeight = rows * (cardSize.y + spacing.y) - spacing.y;
        Vector2 startPos = new Vector2(-totalGridWidth / 2f, totalGridHeight / 2f);

        for (int i = 0; i < total; i++)
        {
            GameObject cardInstance = Instantiate(cardUIPrefab, gridContainer);
            RectTransform cardRectTransform = cardInstance.GetComponent<RectTransform>();

            float posX = startPos.x + (i % cols) * (cardSize.x + spacing.x);
            float posY = startPos.y - (i / cols) * (cardSize.y + spacing.y);

            cardRectTransform.anchoredPosition = new Vector2(posX, posY);

            Card cardScript = cardInstance.GetComponent<Card>();
            int faceIdx = sd.faces[i];

            if (faceIdx >= 0 && faceIdx < allFaceSprites.Count)
            {
                cardScript.SetCard(faceIdx, allFaceSprites[faceIdx], cardBackSprite);
            }

            cardScript.IsMatched = sd.matched[i];
            if (cardScript.IsMatched)
            {
                cardScript.RevealImmediate();
            }
            cards[i] = cardScript;
        }

        score = sd.score;
        streak = sd.streak;
        UpdateUI();

        SetCardsInteractable(true);
        return true;
    }
    #endregion

    public void RestartGame()
    {
        if (WinGameObject != null)
        {
            WinGameObject.SetActive(false);
        }
        StartNewGame();
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        SaveGame();
        UnityEditor.EditorApplication.isPlaying = false;
#else
        SaveGame();
        Application.Quit();
#endif
    }
}