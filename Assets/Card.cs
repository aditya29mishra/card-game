using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    public int pairID;
    public Image cardFront;
    public Image cardBack;
    public bool isFlipped = false;
    private bool isAnimating = false;

    // Use Awake to set up the button listener
    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnCardClick);
        }
    }

    public void SetCard(int id, Sprite frontSprite, Sprite backSprite)
    {
        pairID = id;
        cardFront.sprite = frontSprite;
        cardBack.sprite = backSprite;
        // Initially, only show the back of the card
        cardFront.gameObject.SetActive(false);
        cardBack.gameObject.SetActive(true);
        isFlipped = false;
    }

    public void OnCardClick() {
        if (!isFlipped && !isAnimating) {
            Flip();
            // Tell the GameManager this card was clicked
            GameManager.Instance.OnCardClicked(this);
        }
    }

    public void Flip()
    {
        if (isAnimating) return;
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        isAnimating = true;
        float time = 0f;
        float duration = 0.3f;

        // Shrink (scale along the X-axis)
        while (time < duration / 2)
        {
            float scaleX = Mathf.Lerp(1f, 0f, time / (duration / 2));
            transform.localScale = new Vector3(scaleX, 1f, 1f);
            time += Time.deltaTime;
            yield return null;
        }

        // Toggle visibility
        isFlipped = !isFlipped;
        cardFront.gameObject.SetActive(isFlipped);
        cardBack.gameObject.SetActive(!isFlipped);

        // Expand
        time = 0f;
        while (time < duration / 2)
        {
            float scaleX = Mathf.Lerp(0f, 1f, time / (duration / 2));
            transform.localScale = new Vector3(scaleX, 1f, 1f);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
        isAnimating = false;
    }
}