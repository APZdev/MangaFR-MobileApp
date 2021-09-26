using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;

public class SelectedMangaPannel : MonoBehaviour
{
    [SerializeField] private RawImage mangaBanner;
    [SerializeField] private TMP_Text mangaNameText;
    [SerializeField] private TMP_Text mangaAuthorText;
    [SerializeField] private TMP_Text mangaStatusText;
    [SerializeField] private TMP_Text mangaGenreText;
    [SerializeField] private TMP_Text mangaSummaryText;

    public void LoadSelectedMangaInfo(Texture2D bannerTexture, string mangaName, string mangaAuthor, string mangaStatus, string mangaGenre, string mangaSummary)
    {
        mangaBanner.texture = bannerTexture;
        mangaNameText.text = mangaName;
        mangaAuthorText.text = mangaAuthor;
        mangaStatusText.text = mangaStatus;
        mangaGenreText.text = mangaGenre;
        mangaSummaryText.text = mangaSummary;
    }
}
