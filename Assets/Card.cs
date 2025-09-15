using UnityEngine;

public class Card : MonoBehaviour
{
    public int pairID;
    public SpriteRenderer cardFront;
    public SpriteRenderer cardBack;
    public bool isFlipped = false;
    private bool isAnimating = false;

    public void SetCard(int id, Sprite frontSprite)
    {
        pairID = id;
        cardFront.sprite = frontSprite;
    }

    public void Flip()
    {
        if (isAnimating) return;
        StartCoroutine(FlipAnimation());
    }

    private System.Collections.IEnumerator FlipAnimation()
    {
        isAnimating = true;
        float time = 0f;
        float duration = 0.3f;

        // Shrink
        while (time < duration / 2)
        {
            transform.localScale = new Vector3(Mathf.Lerp(1f, 0f, time / (duration / 2)), 1f, 1f);
            time += Time.deltaTime;
            yield return null;
        }

        // Toggle sprite
        isFlipped = !isFlipped;
        cardFront.gameObject.SetActive(isFlipped);
        cardBack.gameObject.SetActive(!isFlipped);

        // Expand
        time = 0f;
        while (time < duration / 2)
        {
            transform.localScale = new Vector3(Mathf.Lerp(0f, 1f, time / (duration / 2)), 1f, 1f);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
        isAnimating = false;
    }
}
