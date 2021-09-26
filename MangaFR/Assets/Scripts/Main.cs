using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using NaughtyAttributes;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using TMPro;

public class Main : MonoBehaviour
{
    private string backendPath = "http://89.3.198.197/";
    private Essentials essentials;
    private ReaderControls readerControls;

    [BoxGroup("General")] [SerializeField] private GameObject mainCanvas;
    public GameObject menuBar;
    public Animator appAnimator;

    [BoxGroup("Menu")] public GameObject menuPannel;
    [BoxGroup("Menu")] public GameObject mangaItemPrefab;
    [BoxGroup("Menu")] [SerializeField] private GameObject topMangaItemHolder;
    [BoxGroup("Menu")] [SerializeField] private GameObject lastPublishedItemHolder;
    [BoxGroup("Menu")] [SerializeField] private GameObject alphabeticalSortMangaItemHolder;
    private List<MangaItem> mangaList = new List<MangaItem>();
    private List<string> topManga = new List<string>();
    private bool menuReceived;
    private bool topMangaLoaded;

    [BoxGroup("Menu")] public GameObject downloadPannel;
    [BoxGroup("Download")] [SerializeField] private GameObject downloadItemPrefab;
    [BoxGroup("Download")] [SerializeField] private Transform downloadedMangaItemHolder;

    [BoxGroup("Headline Image")] [SerializeField] private GameObject headlineMangaItem;
    [BoxGroup("Headline Image")] [SerializeField] private TMP_Text headlineGenre;
    private List<string> headlineManga = new List<string>();

    [BoxGroup("Selected Menu")] public GameObject selectedMangaPannel;
    [BoxGroup("Selected Menu")] public GameObject chapterItemHolder;

    [BoxGroup("Scan Viewer")] [SerializeField] private GameObject scanViewerPannel;
    [BoxGroup("Scan Viewer")] public GameObject scanHolder;
    [BoxGroup("Scan Viewer")] public GameObject scanPagePrefab;

    [HideInInspector] public bool favoritePannelOpen;
    [HideInInspector] public bool scanCountReceived;
    [HideInInspector] public int scanCount;

    [HideInInspector] public bool isReadingScan;

    /*
        //Sort the pages by id
        //Lambda expression works like this "() => { <function body code here> }"
        scanList.Sort((p1, p2) => p1.pageId.CompareTo(p2.pageId)); 
     */
    private void Start()
    {
        #if UNITY_EDITOR
            //Delay the scaling because when using UnityRemote app, the screen size detection is delayed
            Invoke("ScaleCanvas", 1f);
        #endif
        #if UNITY_ANDROID // Set the screen rotation to portrait only
            ScaleCanvas();
            Utilities.LockScreenOrientation(false);
        #endif

        essentials = GetComponent<Essentials>();
        appAnimator = GetComponent<Animator>();
        readerControls = GetComponent<ReaderControls>();

        //Initialize the list of manga available
        menuReceived = false;
        topMangaLoaded = false;

        favoritePannelOpen = true;

        //Load downloaded manga
        OnClick_LoadDownloadedManga();

        //Get data from database
        StartCoroutine(GetHeadlineManga());
        StartCoroutine(GetTopManga());
        StartCoroutine(GetMangaDetails());
    }

    private void Update()
    {
        //Load UI based on database content
        if (menuReceived)
        {
            ReloadMenuUI();
            menuReceived = false;
        }

        if(isReadingScan)
        {
            menuBar.SetActive(false);
            selectedMangaPannel.SetActive(false);
            scanViewerPannel.SetActive(true);
            menuPannel.SetActive(false);
            downloadPannel.SetActive(false);

        }
        else
        {
            menuBar.SetActive(true);
            scanViewerPannel.SetActive(false);
        }
    }


    #region Menu Methods

    public void ReloadMenuUI()
    {
        //------------ Headline --------------------------
        //Shuffle the list to randomize the headline content to display
        Utilities.Shuffle(headlineManga);
        foreach (MangaItem manga in mangaList)
        {
            //Select the headline to display on the screen
            if (headlineManga[0] == manga.name)
            {
                headlineMangaItem.GetComponent<MangaItemPrefab>().SetItem(manga.name, manga.author, "En Cours", manga.genre, manga.summary, manga.thumbnail, manga.banner);
                headlineGenre.text = manga.genre;
            }
        }
        //-----------------------------------------------

        float itemSpacing;
        int itemCount;
        RectTransform itemHolderRect;

        //Load the top manga list
        foreach (string topManga in topManga)
        {
            foreach(MangaItem manga in mangaList)
            {
                if (topManga == manga.name)
                {
                    GameObject go = Instantiate(mangaItemPrefab);
                    go.GetComponent<MangaItemPrefab>().SetItem(manga.name, manga.author, "En Cours", manga.genre, manga.summary, manga.thumbnail, manga.banner);
                    go.transform.SetParent(topMangaItemHolder.transform);
                }
            }
        }
        //Set the manga item holder "x" size based on the items inside
        itemHolderRect = topMangaItemHolder.GetComponent<RectTransform>();
        itemSpacing = topMangaItemHolder.GetComponent<HorizontalLayoutGroup>().spacing;
        itemCount = topMangaItemHolder.transform.childCount;
        itemHolderRect.sizeDelta = new Vector2((310 * itemCount) + (itemSpacing * itemCount) + itemSpacing, itemHolderRect.sizeDelta.y);


        //Load the last published manga list
        foreach (MangaItem manga in mangaList)
        {
            GameObject go = Instantiate(mangaItemPrefab);
            go.GetComponent<MangaItemPrefab>().SetItem(manga.name, manga.author, "En Cours", manga.genre, manga.summary, manga.thumbnail, manga.banner);
            go.transform.SetParent(lastPublishedItemHolder.transform);
        }
        //Set the manga item holder "x" size based on the items inside
        itemHolderRect = lastPublishedItemHolder.GetComponent<RectTransform>();
        itemSpacing = lastPublishedItemHolder.GetComponent<HorizontalLayoutGroup>().spacing;
        itemCount = lastPublishedItemHolder.transform.childCount;
        itemHolderRect.sizeDelta = new Vector2((310 * itemCount) + (itemSpacing * itemCount) + itemSpacing, itemHolderRect.sizeDelta.y);


        //Load the A to Z sorted by name manga list
        List<MangaItem> alphabeticalSortList = mangaList;
        alphabeticalSortList.Sort((p1, p2) => p1.name.CompareTo(p2.name));
        foreach (MangaItem manga in alphabeticalSortList)
        {
            GameObject go = Instantiate(mangaItemPrefab);
            go.GetComponent<MangaItemPrefab>().SetItem(manga.name, manga.author, "En Cours", manga.genre, manga.summary, manga.thumbnail, manga.banner);
            go.transform.SetParent(alphabeticalSortMangaItemHolder.transform);
        }
        //Set the manga item holder "x" size based on the items inside
        itemHolderRect = alphabeticalSortMangaItemHolder.GetComponent<RectTransform>();
        itemSpacing = alphabeticalSortMangaItemHolder.GetComponent<HorizontalLayoutGroup>().spacing;
        itemCount = alphabeticalSortMangaItemHolder.transform.childCount;
        itemHolderRect.sizeDelta = new Vector2((310 * itemCount) + (itemSpacing * itemCount) + itemSpacing, itemHolderRect.sizeDelta.y);
    }

    //Keep track of the id because, some images are received after others and it causes problem
    private IEnumerator GetMangaDetails()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(backendPath + "/MainApp/DownloadMangaDetails.php"))
        {
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            //The download worked 
            else
            {
                //Save the image as a Base64String
                string[] mangaSplit;

                string imageContentString = www.downloadHandler.text;

                mangaSplit = imageContentString.Split('_');
                //Substract 1 because the last one is an empty string
                for (int i = 0; i < mangaSplit.Length - 1; i++)
                {
                    // 0 : name, 1 : author, 2 : type, 3 : summary, 4 : thumbnail, 5 : banner
                    string[] dataSplit = mangaSplit[i].Split('§');
                    //Convert the Base64String to a texture
                    byte[] thumbnailBytes = Convert.FromBase64String(dataSplit[4]);
                    byte[] bannerBytes = Convert.FromBase64String(dataSplit[5]);
                    Texture2D thumbnail = new Texture2D(1, 1);
                    thumbnail.LoadImage(thumbnailBytes);
                    Texture2D banner = new Texture2D(1, 1);
                    banner.LoadImage(bannerBytes);

                    //Register the mangaItem 
                    mangaList.Add(new MangaItem(dataSplit[0], dataSplit[1], dataSplit[2], dataSplit[3], thumbnail, banner));
                }
            }
            menuReceived = true;
        }
    }

    public IEnumerator GetMangaChapterCount(string mangaName)
    {
        WWWForm form = new WWWForm();
        //The first parameter has to have the same name as the php one
        //The second parameter is the value that we pass to php
        form.AddField("requestedName", mangaName);

        using (UnityWebRequest www = UnityWebRequest.Post(backendPath + "/MainApp/GetChapterCount.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string content = www.downloadHandler.text;

                //Get all the chapters id number
                // Distinct() -> Delete the duplicates list elements
                string[] parsedContent = content.Split('_').Distinct().ToArray();
                yield return parsedContent.Length - 1;
            }
        }
    }
    #endregion

    #region Ranking

    private IEnumerator GetHeadlineManga()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(backendPath + "/MainApp/DownloadHeadlineManga.php"))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            //The download worked 
            else
            {
                string downloadedContent = www.downloadHandler.text;

                string[] headlineMangas;
                headlineMangas = downloadedContent.Split('_');
                //Substract 1 because the last one is an empty string
                for (int i = 0; i < headlineMangas.Length - 1; i++)
                {
                    //Register the headline manga name
                    headlineManga.Add(headlineMangas[i]);
                }
            }
        }
    }

    private IEnumerator GetTopManga()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(backendPath + "/MainApp/DownloadTopManga.php"))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            //The download worked 
            else
            {
                string downloadedContent = www.downloadHandler.text;

                string[] topMangas;
                topMangas = downloadedContent.Split('_');
                //Substract 1 because the last one is an empty string
                for (int i = 0; i < topMangas.Length - 1; i++)
                {
                    //Register the headline manga name
                    topManga.Add(topMangas[i]);
                }
            }
            topMangaLoaded = true;
        }
    }

    #endregion

    #region Scan Viewer Methods

    public void UpdateSelectedMangaUI(Texture2D mangaBanner, string mangaName, string mangaAuthor, string mangaStatus, string mangaGenre, string mangaSummary, bool isOnline)
    {
        //Turn on the online pannel
        selectedMangaPannel.SetActive(true);
        if(isOnline)
        {
            //Load the manga items
            essentials.selectedMangaPannel.LoadSelectedMangaInfo(mangaBanner, mangaName, mangaAuthor,
                                                                 mangaStatus, mangaGenre, mangaSummary);
        }
        else
        {
            essentials.selectedMangaPannel.LoadSelectedMangaInfo(mangaBanner, mangaName, mangaAuthor,
                                                                 mangaStatus, mangaGenre, mangaSummary);
        }
    }

    //This need to be a coroutine because the callback is asynchronous and need some time to be received
    public IEnumerator GetScanPage(string mangaName, string mangaChapter, string mangaPageId, RawImage scanPage)
    {
        WWWForm form = new WWWForm();
        //The first parameter has to have the same name as the php one
        //The second parameter is the value that we pass to php
        form.AddField("requestedName", mangaName);
        form.AddField("requestedChapter", mangaChapter);
        form.AddField("requestedPageId", mangaPageId);

        using (UnityWebRequest www = UnityWebRequest.Post(backendPath + "/MainApp/DownloadScans.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string scanContent = www.downloadHandler.text;

                //Get all the scan pages
                string[] scanPageContent = scanContent.Split('§');
                //Convert the Base64String to a texture
                byte[] imageBytes = Convert.FromBase64String(scanPageContent[1]);
                Texture2D finalTexture = new Texture2D(1, 1);
                finalTexture.LoadImage(imageBytes);
                scanPage.texture = finalTexture;
            }
        }
    }

    //This need to be a coroutine because the callback is asynchronous and need some time to be received
    private IEnumerator DownloadScanPage(string mangaName, string mangaChapter, string mangaPageId, Texture2D scanPageTexture)
    {
        WWWForm form = new WWWForm();
        //The first parameter has to have the same name as the php one
        //The second parameter is the value that we pass to php
        form.AddField("requestedName", mangaName);
        form.AddField("requestedChapter", mangaChapter);
        form.AddField("requestedPageId", mangaPageId);

        using (UnityWebRequest www = UnityWebRequest.Post(backendPath + "/MainApp/DownloadScans.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string scanContent = www.downloadHandler.text;

                //Get all the scan pages
                string[] scanPageContent = scanContent.Split('§');
                //Convert the Base64String to a texture
                byte[] imageBytes = Convert.FromBase64String(scanPageContent[1]);
                scanPageTexture.LoadImage(imageBytes);
            }
        }
    }

    //This need to be a coroutine because the callback is asynchronous and need some time to be received
    public IEnumerator GetScansCount(string mangaName, string mangaChapter)
    {
        WWWForm form = new WWWForm();
        //The first parameter has to have the same name as the php one
        //The second parameter is the value that we pass to php
        form.AddField("requestedName", mangaName);
        form.AddField("requestedChapter", mangaChapter);

        using (UnityWebRequest www = UnityWebRequest.Post(backendPath + "/MainApp/GetScanCount.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Get all the scan id to store the number of pages to display later
                //-1 because the last one is empty
                scanCount = www.downloadHandler.text.Split('_').Length - 1;
                readerControls.scansCount = scanCount;
                readerControls.scanCountFinished = true;
                scanCountReceived = true;
                yield return scanCount;
            }
        }
    }

    #endregion

    #region Download

    //Get downloaded manga
    public void OnClick_LoadDownloadedManga()
    {
        //Clear the downloaded items
        foreach (Transform child in downloadedMangaItemHolder.transform)
        {
            Destroy(child.gameObject);
        }

        string downloadedMangaPath = Application.persistentDataPath + "/DownloadedMangas";
        DirectoryInfo rootDir = new DirectoryInfo(downloadedMangaPath);
        //Check if the root directory exists and create it if don't to prevent errors
        if (!rootDir.Exists)
        {
            rootDir.Create();
        }

        string[] downloadedMangaItems = Directory.GetDirectories(downloadedMangaPath);
        foreach (string mangaPath in downloadedMangaItems)
        {
            GameObject go = Instantiate(downloadItemPrefab);
            go.transform.SetParent(downloadedMangaItemHolder);

            string currentMangasPathInfo = $"{mangaPath}/info";
            Texture2D thumbnailTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            Texture2D bannerTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            string[] parsedContent = new string[5];

            string informationsPath = $"{currentMangasPathInfo}/informations.txt";
            //Get the manga info
            if (File.Exists(informationsPath))
            {
                using (StreamReader file = new StreamReader(informationsPath))
                {
                    string content = file.ReadToEnd();
                    file.Close();

                    parsedContent = content.Split('§');
                }
            }

            //Get the manga thumbnail
            string thumbnailPath = $"{currentMangasPathInfo}/thumbnail.png";
            if (File.Exists(thumbnailPath))
            {
                //Encode the thumbnail to the PNG format
                byte[] bytes = File.ReadAllBytes(thumbnailPath);
                thumbnailTexture.LoadImage(bytes);
                //Create the png image to the corresponding chapter directory
                File.WriteAllBytes(thumbnailPath, bytes);
            }

            //Get the manga banner
            string bannerPath = $"{currentMangasPathInfo}/banner.png";
            if (File.Exists(thumbnailPath))
            {
                //Encode the thumbnail to the PNG format
                byte[] bytes = File.ReadAllBytes(bannerPath);
                bannerTexture.LoadImage(bytes);
                //Create the png image to the corresponding chapter directory
                File.WriteAllBytes(bannerPath, bytes);
            }

            go.GetComponent<DownloadedMangaItemPrefab>().SetItem(parsedContent[0], parsedContent[1], parsedContent[2], parsedContent[3], parsedContent[4], thumbnailTexture, bannerTexture, downloadedMangaPath);
        }
    }

    //Download manga to the disk drive
    public IEnumerator AsyncDownloadChapterScans(string mangaName, string mangaChapter)
    {
        string mangaPath = $"{Application.persistentDataPath}/DownloadedMangas/{mangaName}";
        DirectoryInfo mangaDir = new DirectoryInfo(mangaPath + "/info");
        if(!mangaDir.Exists)
        {
            mangaDir.Create();

            foreach(MangaItem item in mangaList)
            {
                if(item.name == mangaName)
                {
                    //Store the manga info
                    string informationsPath = mangaPath + "/info/informations.txt";
                    if(!File.Exists(informationsPath))
                    {
                        using(StreamWriter file = File.CreateText(informationsPath))
                        {
                            string data = $"{item.name}§{item.author}§{"Status"}§{item.genre}§{item.summary}";
                            file.WriteLine(data);
                            file.Close();
                        }
                    }
                    //Store the manga thumbnail
                    string thumbnailPath = mangaPath + "/info/thumbnail.png";
                    if (!File.Exists(thumbnailPath))
                    {
                        //Encode the thumbnail to the PNG format
                        byte[] bytes = item.thumbnail.EncodeToPNG();
                        //Create the png image to the corresponding chapter directory
                        File.WriteAllBytes(thumbnailPath, bytes);
                    }

                    //Store the manga banner
                    string bannerPath = mangaPath + "/info/banner.png";
                    if (!File.Exists(bannerPath))
                    {
                        byte[] bytes = item.banner.EncodeToPNG();

                        File.WriteAllBytes(bannerPath, bytes);
                    }
                }
            }
        }


        string chapterPath = $"{Application.persistentDataPath}/DownloadedMangas/{mangaName}/content/{mangaChapter}";
        DirectoryInfo chapterDir = new DirectoryInfo(chapterPath);

        if (!chapterDir.Exists)
        {
            chapterDir.Create();

            CoroutineWithData coroutineData = new CoroutineWithData(this, GetScansCount(mangaName, mangaChapter));

            //Wait for the number of chapter to get received
            yield return coroutineData.coroutine;

            int numberOfScans = (int)coroutineData.result;

            for (int i = 0; i < numberOfScans; i++)
            {
                //Create a new placeholder texture
                Texture2D texture = new Texture2D(1,1, TextureFormat.RGB24, false);

                //Wait for the scanPage Download to complete and assign it's content to the Texture2D
                yield return StartCoroutine(DownloadScanPage(mangaName, mangaChapter, $"{i}", texture));

                //Encode the scanPage to the PNG format
                byte[] bytes = texture.EncodeToPNG();

                //Create the png image to the corresponding chapter directory
                File.WriteAllBytes(chapterPath + $"/{i}" + ".png", bytes);
            }
        }
    }

    #endregion

    public void OnClick_LoadFavorites(bool buttonIsFavorite)
    {
        //Dio nothing if the favorite pannel is clossed and we presse something else than the favorite button
        if (favoritePannelOpen && !buttonIsFavorite) return;

        favoritePannelOpen = !favoritePannelOpen;

        if(favoritePannelOpen)
            appAnimator.SetTrigger("ExitFavorite");
        else
            appAnimator.SetTrigger("EnterFavorite");
    }

    public void OnClick_LoadHeadlineManga()
    {
        headlineMangaItem.GetComponent<MangaItemPrefab>().OnClick_LoadMangaProfile();
    }

    private void ScaleCanvas()
    {
        //Resize the canvas so it fits every screen based on a reference resolution
        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();

        float sizeX = Screen.width / canvasRect.rect.width;
        float sizeY = Screen.height / canvasRect.rect.height;

        canvasRect.localScale = new Vector2(sizeX, sizeY);
    }
}

public class MangaItem
{
    public string name;
    public string author;
    public string genre;
    public string summary;
    public Texture2D thumbnail;
    public Texture2D banner;

    public MangaItem(string mangaName, string mangaAuthor, string mangaGenre, string mangaSummary, Texture2D mangaThumbnail, Texture2D mangaBanner)
    {
        name = mangaName;
        author = mangaAuthor;
        genre = mangaGenre;
        summary = mangaSummary;
        thumbnail = mangaThumbnail;
        banner = mangaBanner;
    }
}


public class ScanItem
{
    public int pageId;
    public Texture2D pageTexture;

    public ScanItem(int id, Texture2D texture)
    {
        pageId = id;
        pageTexture = texture;
    }
}
