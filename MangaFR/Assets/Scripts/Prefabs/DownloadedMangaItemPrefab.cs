using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;


public class DownloadedMangaItemPrefab : MonoBehaviour
{
    private Essentials essentials;

    [SerializeField] private TMP_Text mangaNameText;
    [SerializeField] private RawImage mangaThumbnail;

    private string mangaName;
    private string authorName;
    private string mangaStatus;
    private string mangaGenre;
    private string mangaSummary;
    private Texture2D bannerTexture;
    private Texture2D thumbnailTexture;
    private string downloadPath;

    public void Start()
    {
        essentials = transform.root.GetComponent<Essentials>();
    }

    public void SetItem(string mangaName, string authorName, string mangaStatus, string mangaGenre, string mangaSummary, Texture2D mangaThumbnailTexture, Texture2D mangaBannerTexture, string downloadPath)
    {
        this.mangaName = mangaName;
        this.authorName = authorName;
        this.mangaStatus = mangaStatus;
        this.mangaGenre = mangaGenre;
        this.mangaSummary = mangaSummary;
        bannerTexture = mangaBannerTexture;
        thumbnailTexture = mangaThumbnailTexture;

        mangaNameText.text = mangaName;
        mangaThumbnail.texture = thumbnailTexture;

        this.downloadPath = downloadPath;
    }

    public void OnClick_LoadSelectedMangaPage()
    {
        //Clear the chapter item holder
        foreach (Transform child in essentials.main.chapterItemHolder.transform)
        {
            Destroy(child.gameObject);
        }

        //Get all the chapter directories
        string[] paths = Directory.GetDirectories($"{downloadPath}/{mangaName}/content");

        //Create the chapter items based on the available downloaded chapters of the selected manga
        for (int i = 0; i < paths.Length; i++)
        {

            string[] parsedPath = paths[i].Split('\\');
            string chapterId = parsedPath[parsedPath.Length - 1];

            GameObject go = Instantiate(essentials.chapterItemPrefab);
            go.transform.SetParent(essentials.main.chapterItemHolder.transform);

            go.GetComponent<ChapterItemPrefab>().SetItem(mangaName, $"{mangaName}  {chapterId}", int.Parse(chapterId), false, downloadPath);
        }

        //Set the manga page description to the manga details
        essentials.main.UpdateSelectedMangaUI(bannerTexture, mangaName, authorName, mangaStatus, mangaGenre, mangaSummary, true);
    }
}
