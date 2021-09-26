using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SFB;
using TMPro;

public class PhpWrapper : MonoBehaviour
{
    private string backendPath = "http://89.3.198.197/";

    #region Menu Variables

    [SerializeField] private InputField mangaNameInput;
    [SerializeField] private InputField authorNameInput;
    [SerializeField] private InputField mangaTypeInput;
    [SerializeField] private InputField mangaSummaryInput;
    [SerializeField] private RawImage thumbnailImage;
    [SerializeField] private RawImage bannerImage;

    [SerializeField] private TMP_Text menuDatabaseStatusText;

    #endregion

    #region Scan Variables

    private string chapterPath;
    private string encodedThumbnail;
    private string encodedBanner;

    [SerializeField] private GameObject chapterItemHolder;
    [SerializeField] private GameObject chapterItemPrefab;

    [SerializeField] private GameObject scansItemHolder;

    [SerializeField] private List<Chapter> chapterList = new List<Chapter>();
    [SerializeField] private List<UploadItem> finalItems = new List<UploadItem>();

    [SerializeField] private InputField mangaName;
    [SerializeField] private InputField chapterOffset;

    #endregion

    #region Scan Variables

    [SerializeField] private InputField topMangaInputField;
    [SerializeField] private InputField headlineInputField;

    [SerializeField] private TMP_Text rakingDatabaseStatusText;


    #endregion

    #region Menu Section

    public void OnClick_GetThumbnailInExplorer()
    {
        string[] temp = StandaloneFileBrowser.OpenFilePanel("Select a thumbnail", chapterPath, "", false);
        string thumbnailPath = "";
        //Check this to prevent arrays bouds error
        if (temp.Length > 0)
            thumbnailPath = temp[0];
        
        if (thumbnailPath != "")
        {
            byte[] bytes = File.ReadAllBytes(thumbnailPath);
            //Encode the image to base64 string
            encodedThumbnail = Convert.ToBase64String(bytes);

            Texture2D thumbnail = new Texture2D(1, 1);
            thumbnail.LoadImage(bytes);
            thumbnailImage.texture = thumbnail;
        }
    }

    public void OnClick_GetBannerInExplorer()
    {
        string[] temp = StandaloneFileBrowser.OpenFilePanel("Select a thumbnail", chapterPath, "", false);
        string bannerPath = "";
        if (temp.Length > 0)
            bannerPath = temp[0];

        if (bannerPath != "")
        {
            //Get the scan page image path
            byte[] bytes = File.ReadAllBytes(bannerPath);
            //Encode the image to base64 string
            encodedBanner = Convert.ToBase64String(bytes);

            Texture2D banner = new Texture2D(1, 1);
            banner.LoadImage(bytes);
            bannerImage.texture = banner;
        }
    }

    public void OnClick_UploadMangaToDatabase()
    {
        StartCoroutine(UploadManga(mangaNameInput.text, authorNameInput.text, mangaTypeInput.text, mangaSummaryInput.text, encodedThumbnail, encodedBanner));

        ClearMangaFields();
    }

    private void ClearMangaFields()
    {
        mangaNameInput.text = "";
        authorNameInput.text = "";
        mangaTypeInput.text = "";
        mangaSummaryInput.text = "";
        thumbnailImage.texture = null;
        bannerImage.texture = null;
    }

    private IEnumerator UploadManga(string mangaName, string mangaAuthor, string mangaType, string mangaSummary, string mangaThumbnail, string requestedBanner)
    {
        WWWForm form = new WWWForm();
        //The first parameter has to have the same name as the php one
        //The second parameter is the value that we pass to php
        form.AddField("requestedName", mangaName.Replace("'", " "));
        form.AddField("requestedAuthor", mangaAuthor);
        form.AddField("requestedType", mangaType.Replace(" ", ""));
        form.AddField("requestedSummary", mangaSummary.Replace("'", " "));  
        form.AddField("requestedThumbnail", mangaThumbnail);
        form.AddField("requestedBanner", requestedBanner);

        using (UnityWebRequest www = UnityWebRequest.Post(backendPath + "Dashboard/UploadManga.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                menuDatabaseStatusText.text = www.error;
                menuDatabaseStatusText.color = Color.red;
            }
            else
            {
                menuDatabaseStatusText.text = www.downloadHandler.text;
                menuDatabaseStatusText.color = Color.green;
            }
        }
    }

    #endregion

    #region Scan Section

    //Used on a button event
    public void OnClick_GetChaptersInExplorer()
    {
        chapterPath = StandaloneFileBrowser.OpenFolderPanel("Select a chapter folder", chapterPath, false)[0];
        if (chapterPath != null)
        {
            //Get the scans of the current chapter
            string[] scans = Directory.GetFiles(chapterPath);
            //Register the new chapter
            chapterList.Add(new Chapter(chapterList.Count.ToString(), chapterPath, scans));

            //Clear the item list
            foreach (Transform child in chapterItemHolder.transform)
            {
                Destroy(child.gameObject);
            }

            //Update the chapter to the list
            for (int i = 0; i < chapterList.Count; i++)
            {
                GameObject go = Instantiate(chapterItemPrefab);
                go.GetComponent<ChapterItem>().SetItemProperties(chapterList[i].name, chapterList[i].path, chapterList[i].scansPath, scansItemHolder);
                go.transform.SetParent(chapterItemHolder.transform);
            }

            //Resize the chapter holder height so we can scroll properly
            Vector2 size = chapterItemHolder.GetComponent<RectTransform>().sizeDelta;
            float finlaSizeY = (chapterItemPrefab.GetComponent<RectTransform>().rect.height + chapterItemHolder.GetComponent<VerticalLayoutGroup>().spacing) * chapterList.Count;
            chapterItemHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, chapterItemPrefab.GetComponent<RectTransform>().rect.height * chapterList.Count);
        }

    }

    private void ClearScanFields()
    {
        mangaName.text = "";
        chapterOffset.text = "";

        //Clear the item list
        foreach (Transform child in chapterItemHolder.transform)
        {
            Destroy(child.gameObject);
        }

        //Clear the scan list
        foreach (Transform child in scansItemHolder.transform)
        {
            Destroy(child.gameObject);
        }
    }

    //Used on a button event
    public void OnClick_UploadScanToDatabase()
    {
        PrepareItemsForUpload();

        //Upload the scans to the database 1 by 1
        for (int i = 0; i < finalItems.Count; i++)
        {
            StartCoroutine(UploadScans(finalItems[i].name, finalItems[i].chapter, finalItems[i].pageId, finalItems[i].scanPageString));
        }

        //Clear UI and Lists so we can import safely again
        ClearScanFields();
        chapterList.Clear();
        finalItems.Clear();
        chapterPath = "";
    }

    private void PrepareItemsForUpload()
    {
        finalItems.Clear();

        //Cycle through chapters
        for (int i = 0; i < chapterList.Count; i++)
        {
            //Cycle through scans of chapters
            for (int j = 0; j < chapterList[i].scansPath.Length; j++)
            {
                //Get the scan page image path
                string tempPath = chapterList[i].scansPath[j];

                //Get the scan image data
                byte[] bytes = File.ReadAllBytes(tempPath);

                //Encode the image to base64 string
                string encodedImage = Convert.ToBase64String(bytes);

                //Register items to the final list
                string chapterName = (int.Parse(chapterOffset.text) + i + 1).ToString();
                finalItems.Add(new UploadItem(mangaName.text, chapterName, j.ToString(), encodedImage)); ;
            }
        }
    }

    private IEnumerator UploadScans(string mangaName, string mangaChapter, string mangaPageId, string mangaScanPage)
    {
        WWWForm form = new WWWForm();
        //The first parameter has to have the same name as the php one
        //The second parameter is the value that we pass to php
        form.AddField("requestedName", mangaName);
        form.AddField("requestedChapter", mangaChapter);
        form.AddField("requestedPageId", mangaPageId);
        form.AddField("requestedScanPage", mangaScanPage);

        using (UnityWebRequest www = UnityWebRequest.Post(backendPath + "Dashboard/UploadScans.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                Debug.Log(www.error);
            else
                Debug.Log(www.downloadHandler.text);
        }
    }
    #endregion

    #region Ranking Section

    public void OnClick_UploadTopMangaList()
    {
        string[] topMangaItems = topMangaInputField.text.Split(',');
        for (int i = 0; i < topMangaItems.Length; i++)
        {
            StartCoroutine(UploadTopMangaItem(topMangaItems[i], i.ToString()));
        }
    }

    private IEnumerator UploadTopMangaItem(string mangaName, string rankId)
    {
        WWWForm form = new WWWForm();
        //The first parameter has to have the same name as the php one
        //The second parameter is the value that we pass to php
        form.AddField("requestedName", mangaName);
        form.AddField("requestedId", rankId);

        using (UnityWebRequest www = UnityWebRequest.Post(backendPath + "Dashboard/UploadTopManga.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                rakingDatabaseStatusText.text = www.error;
                rakingDatabaseStatusText.color = Color.red;
            }
            else
            {
                rakingDatabaseStatusText.text = www.downloadHandler.text;
                rakingDatabaseStatusText.color = Color.green;
            }
        }
    }

    public void OnClick_UploadHeadlineList()
    {
        string[] headlineItems = headlineInputField.text.Split(',');
        for (int i = 0; i < headlineItems.Length; i++)
        {
            StartCoroutine(UploadHeadlineItem(headlineItems[i], i.ToString()));
        }
    }

    private IEnumerator UploadHeadlineItem(string mangaName, string rankId)
    {
        WWWForm form = new WWWForm();
        //The first parameter has to have the same name as the php one
        //The second parameter is the value that we pass to php
        form.AddField("requestedName", mangaName);
        form.AddField("requestedId", rankId);

        using (UnityWebRequest www = UnityWebRequest.Post(backendPath + "Dashboard/UploadHeadlineManga.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                rakingDatabaseStatusText.text = www.error;
                rakingDatabaseStatusText.color = Color.red;
            }
            else
            {
                rakingDatabaseStatusText.text = www.downloadHandler.text;
                rakingDatabaseStatusText.color = Color.green;
            }
        }
    }

    #endregion
}

public class UploadItem
{
    public string name;
    public string chapter;
    public string pageId;
    public string scanPageString;

    public UploadItem(string itemName, string itemChapter, string itemPageId, string itemScanPageString)
    {
        name = itemName;
        chapter = itemChapter;
        pageId = itemPageId;
        scanPageString = itemScanPageString;
    }
}

public class Chapter
{
    public string name;
    public string path;
    public string[] scansPath;

    public Chapter(string chapterName, string chapterPath, string[] chapterScansPath)
    {
        name = chapterName;
        path = chapterPath;
        scansPath = chapterScansPath;
    }
}
