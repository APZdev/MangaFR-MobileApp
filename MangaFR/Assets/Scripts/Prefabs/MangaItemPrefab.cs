using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MangaItemPrefab : MonoBehaviour
{
    private Main main;
    [SerializeField] private GameObject chapterItemPrefab;
    private RawImage mangaThumbnail;

    private string mangaName;
    private string authorName;
    private string mangaStatus;
    private string mangaGenre;
    private string mangaSummary;
    private Texture2D thumbnailTexture;
    private Texture2D bannerTexture;

    public void Awake()
    {
        mangaThumbnail = GetComponentInChildren<RawImage>();
    }

    public void Start()
    {
        main = transform.root.GetComponent<Main>();
    }

    public void SetItem(string mangaName, string authorName, string status, string genre, string summary, Texture2D thumbnail, Texture2D banner)
    {
        this.mangaName = mangaName;
        this.authorName = authorName;
        mangaStatus = status;
        mangaGenre = genre;
        mangaSummary = summary;
        thumbnailTexture = thumbnail;
        bannerTexture = banner;

        mangaThumbnail.texture = thumbnailTexture;
    }

    public void OnClick_LoadMangaProfile()
    {
        //Call the method to get the number of chapters in the manga
        StartCoroutine(AsyncSetChapterNumber());

        //Set the manga page description to the manga details
        main.UpdateSelectedMangaUI(bannerTexture, mangaName, authorName, mangaStatus, mangaGenre, mangaSummary, true);
    }

    private IEnumerator AsyncSetChapterNumber()
    {
        CoroutineWithData coroutineData = new CoroutineWithData(this, main.GetMangaChapterCount(mangaName));

        //Wait for the number of chapter to get received
        yield return coroutineData.coroutine;

        int numberOfChapters = (int)coroutineData.result;

        //Clear the chapter item holder
        foreach (Transform child in main.chapterItemHolder.transform)
        {
            Destroy(child.gameObject);
        }

        //Create the chapter items based on the available chapters of the selected manga
        for (int i = 0; i < numberOfChapters; i++)
        {
            GameObject go = Instantiate(chapterItemPrefab);
            go.transform.SetParent(main.chapterItemHolder.transform);

            go.GetComponent<ChapterItemPrefab>().SetItem(mangaName, $"{mangaName}  {i + 1}", i + 1, true, "");
        }
    }
}
