using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Если хочешь переключать сцену

public class ComicReveal : MonoBehaviour
{
    [System.Serializable]
    public class ComicPiece
    {
        public Image image;              // UI Image на Canvas
        public float delayBefore;        // Задержка перед появлением
        public float fadeDuration = 1f;  // Длительность плавного появления
    }

    public List<ComicPiece> comicPieces = new List<ComicPiece>();
    public float delayAfterAll = 2f; // Задержка перед переходом в игру

    private void Awake()
    {
        // Прячем все картинки заранее, чтобы не мигали при старте
        foreach (ComicPiece piece in comicPieces)
        {
            if (piece.image != null)
            {
                piece.image.canvasRenderer.SetAlpha(0f);
                piece.image.gameObject.SetActive(true); // Активируем, чтобы CrossFade работал
            }
        }
    }

    private void Start()
    {
        StartCoroutine(ShowComic());
    }

    private IEnumerator ShowComic()
    {
        foreach (ComicPiece piece in comicPieces)
        {
            yield return new WaitForSeconds(piece.delayBefore);

            piece.image.CrossFadeAlpha(1f, piece.fadeDuration, false);

            yield return new WaitForSeconds(piece.fadeDuration);
        }

        yield return new WaitForSeconds(delayAfterAll);

        Debug.Log("Комикс показан! Переход в игру...");
        // SceneManager.LoadScene("GameScene"); // Раскомментируй, если нужно переключение
    }
}