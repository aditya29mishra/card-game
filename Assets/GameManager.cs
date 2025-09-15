using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Assets")]
    public List<Sprite> allFaceSprites;
    public Sprite cardBackSprite;

    [Header("UI")]
    public GameObject cardUIPrefab;
    public Transform gridContainer;

    [Header("Grid Settings")]
    public int rows = 5;
    public int cols = 6;
    public Vector2 cardSize = new Vector2(100f, 150f);
    public Vector2 spacing = new Vector2(10f, 10f);

    [Header("Game Logic")]
    public float mismatchDelay = 0.6f;

    private Card[] cards;
    private List<Card> flippedUnmatched = new List<Card>();

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
    }

    private void Start()
    {
        StartNewGame();
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
    }

    public void OnCardClicked(Card card)
    {
        // Don't do anything if the card is already matched
        if (card.IsMatched) return;

        // If the same card is clicked twice, do nothing
        if (flippedUnmatched.Count > 0 && flippedUnmatched[0] == card) return;

        flippedUnmatched.Add(card);

        // If two cards are now flipped, compare them
        if (flippedUnmatched.Count >= 2)
        {
            Card a = flippedUnmatched[0];
            Card b = flippedUnmatched[1];

            if (a.pairID == b.pairID)
            {
                // It's a match!
                a.IsMatched = true;
                b.IsMatched = true;
                flippedUnmatched.Clear();
                Debug.Log("Match found!");
            }
            else
            {
                // Mismatch. Flip them back after a delay.
                StartCoroutine(FlipBackAfterDelay(a, b, mismatchDelay));
            }
        }
    }

    private IEnumerator FlipBackAfterDelay(Card a, Card b, float delay)
    {
        yield return new WaitForSeconds(delay);
        // Ensure they are still unmatched before flipping back
        if (!a.IsMatched) a.Flip();
        if (!b.IsMatched) b.Flip();

        flippedUnmatched.Clear();
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
}