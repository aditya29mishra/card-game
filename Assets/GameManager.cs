using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<Sprite> allFaceSprites;
    public Sprite cardBackSprite;

    public GameObject cardUIPrefab;
    public Transform gridContainer;

    public int rows = 5;
    public int cols = 6;

    // Define the size of each card in pixels
    public Vector2 cardSize = new Vector2(100f, 150f);

    // Define the spacing between cards
    public Vector2 spacing = new Vector2(10f, 10f);
        private Card[] cards;
    public static GameManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    public void OnCardClicked(Card card)
    {
        Debug.Log("Card with pair ID " + card.pairID + " was clicked!");

    }
    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        // Clear any existing cards
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

        // 1. Randomly choose unique cards for the game
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

        // 2. Duplicate the chosen cards to create pairs
        List<int> faces = new List<int>();
        for (int i = 0; i < totalPairs; i++)
        {
            faces.Add(choiceIndices[i]);
            faces.Add(choiceIndices[i]);
        }

        // 3. Shuffle the entire list of pairs
        ShuffleList(faces);

        cards = new Card[totalCards];

        // 4. Instantiate and assign cards
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
    // Simple Fisher-Yates shuffle algorithm
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