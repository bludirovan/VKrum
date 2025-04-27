using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ���� ������ ����������� �����

public class ComicReveal : MonoBehaviour
{
    [System.Serializable]
    public class ComicPiece
    {
        public Image image;              // UI Image �� Canvas
        public float delayBefore;        // �������� ����� ����������
        public float fadeDuration = 1f;  // ������������ �������� ���������
    }

    public List<ComicPiece> comicPieces = new List<ComicPiece>();
    public float delayAfterAll = 2f; // �������� ����� ��������� � ����
    public string NameStartLevel;

    private void Awake()
    {
        // ������ ��� �������� �������, ����� �� ������ ��� ������
        foreach (ComicPiece piece in comicPieces)
        {
            if (piece.image != null)
            {
                piece.image.canvasRenderer.SetAlpha(0f);
                piece.image.gameObject.SetActive(true); // ����������, ����� CrossFade �������
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

        Debug.Log("������ �������! ������� � ����...");
        SceneManager.LoadScene(NameStartLevel); // ��������������, ���� ����� ������������
    }
}