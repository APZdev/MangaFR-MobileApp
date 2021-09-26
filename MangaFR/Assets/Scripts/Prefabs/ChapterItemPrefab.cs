using UnityEngine;
using TMPro;
using System.IO;

public class ChapterItemPrefab : MonoBehaviour
{
    private Essentials essentials;

    [SerializeField] private TMP_Text chapterItemName;
    [SerializeField] private GameObject downloadButtonObject;

    private string mangaName;
    private string chapterName;
    private int chapterId;
    private bool isOnlineChapter;
    private string downloadPath;

    public void Start()
    {
        essentials = transform.root.GetComponent<Essentials>();
    }

    public void SetItem(string mangaName, string chapterName, int id, bool isOnline, string downloadPath)
    {
        this.mangaName = mangaName;
        chapterItemName.text = chapterName;
        this.chapterName = chapterName;
        chapterId = id;
        isOnlineChapter = isOnline;

        //If it's a downloaded chapter item, don't display the download button
        if(!isOnlineChapter)
        {
            downloadButtonObject.SetActive(false);
            this.downloadPath = downloadPath;
        }
    }

    public void OnClick_LoadScans()
    {
        if(isOnlineChapter)
        {
            //Set the readingmode to true, so the UI updates correctly
            essentials.main.isReadingScan = true;
            //Get the chapter scans count to generate slots for scans
            essentials.main.StartCoroutine(essentials.main.GetScansCount(mangaName, chapterId.ToString()));
            transform.root.GetComponent<Essentials>().readerControls.SetReaderInfo(mangaName, chapterId.ToString(), true);
        }
        else
        {
            essentials.main.isReadingScan = true;

            transform.root.GetComponent<Essentials>().readerControls.SetReaderInfo(mangaName, chapterId.ToString(), false);
            essentials.readerControls.scansCount = Directory.GetFiles($"{downloadPath}/{mangaName}/content/{chapterId}").Length;
            essentials.readerControls.downloadPath = downloadPath;
            essentials.readerControls.downloadedMangaLoaded = false;
        }

        Utilities.LockScreenOrientation(true);
    }

    public void OnClick_DownloadChapter()
    {
        essentials.main.StartCoroutine(essentials.main.AsyncDownloadChapterScans(mangaName, chapterId.ToString()));
    }
}
