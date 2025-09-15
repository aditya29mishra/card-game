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

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        Debug.Log("Starting a new UI game with a manual grid.");

        // Calculate the total grid size
        float totalGridWidth = cols * (cardSize.x + spacing.x) - spacing.x;
        float totalGridHeight = rows * (cardSize.y + spacing.y) - spacing.y;
        
        // Calculate the starting position (top-left corner)
        Vector2 startPos = new Vector2(-totalGridWidth / 2f, totalGridHeight / 2f);

        // Loop to create and position each card
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Instantiate the card as a child of the grid container
                GameObject cardInstance = Instantiate(cardUIPrefab, gridContainer);

                // Get the RectTransform component of the new card
                RectTransform cardRectTransform = cardInstance.GetComponent<RectTransform>();

                // Calculate the card's position
                float posX = startPos.x + x * (cardSize.x + spacing.x);
                float posY = startPos.y - y * (cardSize.y + spacing.y);

                // Set the card's position
                cardRectTransform.anchoredPosition = new Vector2(posX, posY);
            }
        }
    }
}