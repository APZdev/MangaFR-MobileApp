using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ReaderControls : MonoBehaviour
{
    private Main main;

    [SerializeField] private GameObject scanHolder;
    [SerializeField] private float scrollSpeed = 50f;

    [SerializeField] private float scrollThreshold = 100f;
    [SerializeField] private float pageTurnTime = 1.0f;
    [SerializeField] private int pageId;
    [SerializeField] private bool screenIsRotating;

    public bool canScroll;
    public List<Transform> scans = new List<Transform>();
    private bool firstLoad;
    private bool isScreenPortrait;
    private bool previousScreenOrientation;
    private bool couldBeSwipe;
    private Vector2 startPos;

    [HideInInspector] public bool scanReceived;
    private bool scansLoaded;

    private bool[] scanDownloaded;

    public int scansCount;
    public bool scanCountFinished;

    private string mangaName;
    private string mangaChapter;

    [HideInInspector] public bool isReadingOnline;
    [HideInInspector] public bool downloadedMangaLoaded;
    [HideInInspector] public string downloadPath;

    private void Start()
    {
        main = transform.root.GetComponent<Essentials>().main;

        scanCountFinished = false;
        scanReceived = false;
        scansLoaded = false;
        firstLoad = true;
        downloadedMangaLoaded = true;
    }

    private void Update()
    {
        //Check if we read online or downloaded scans 
        if(isReadingOnline)
        {
            if(scanCountFinished)
            {
                scansLoaded = true;
                scanReceived = false;
                //Create scan slots to prepare for download
                for (int i = 0; i < scansCount; i++)
                {
                    GameObject go = Instantiate(main.scanPagePrefab);
                    go.transform.SetParent(scanHolder.transform);
                }
                scanDownloaded = new bool[scansCount];
                //Set this to false so the scans slots spawn only once
                scanCountFinished = false;
                GetScans();
            }
        }
        else
        {
            if(!downloadedMangaLoaded)
            {
                for (int i = 0; i < scansCount; i++)
                {
                    GameObject go = Instantiate(main.scanPagePrefab);
                    go.transform.SetParent(scanHolder.transform);

                    Texture2D scanPageTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);

                    //Get the manga page path
                    string scanPagePath = $"{downloadPath}/{mangaName}/content/{mangaChapter}/{i}.png";
                    if (File.Exists(scanPagePath))
                    {
                        //Encode the scanpage to the PNG format
                        byte[] bytes = File.ReadAllBytes(scanPagePath);
                        scanPageTexture.LoadImage(bytes);
                        //Create the png image to the corresponding scan page directory
                        File.WriteAllBytes(scanPagePath, bytes);
                    }
                    go.GetComponent<RawImage>().texture = scanPageTexture;
                }

                //Set this to true so the scans slots spawn only once
                downloadedMangaLoaded = true;
                scansLoaded = true;
                GetScans();
            }
        }

        if (scansLoaded)
        {
            GetScreenOrientation();

            if (previousScreenOrientation != isScreenPortrait || firstLoad)
            {
                StartCoroutine(ResizeScans(isScreenPortrait));
                firstLoad = false;
            }
            previousScreenOrientation = isScreenPortrait;

            PageScroll();
        }
    } 

    public void SetReaderInfo(string name, string chapter, bool isOnline)
    {
        isReadingOnline = isOnline;
        mangaName = name;
        mangaChapter = chapter;
    }

    private void GetScans()
    {
        scans.Clear();
        //Get the child of the menuItemHolder
        foreach (Transform child in scanHolder.transform)
        {
            scans.Add(child);
        }
    }

    private void PageScroll()
    {
        float deltaMove;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startPos = touch.position;
                    couldBeSwipe = true;
                    break;
                case TouchPhase.Moved:
                    deltaMove = touch.position.x - startPos.x;
                    if (Mathf.Abs(deltaMove) > scrollThreshold)
                    {
                        if(couldBeSwipe)
                        {
                            if(deltaMove > 0)
                            {
                                pageId--;
                                couldBeSwipe = false;
                            }
                            else if (deltaMove < 0)
                            {
                                pageId++;
                                couldBeSwipe = false;
                            }
                        }
                    }
                    break;
                case TouchPhase.Ended:
                    couldBeSwipe = true;
                    break;
            }

            //Clamp the page scroll -> number of pages
            pageId = Mathf.Clamp(pageId, 0, scans.Count - 1);
        }

        if(isReadingOnline)
        {
            if(scanDownloaded[pageId] != true)
            {
                //Download and display the scan
                main.StartCoroutine(main.GetScanPage(mangaName, mangaChapter, $"{pageId}", scans[pageId].GetComponent<RawImage>()));
                scanDownloaded[pageId] = true;
            }
            //Check if the pages to load exists
            if ((pageId + 1) <= scanDownloaded.Length - 1)
            {
                if(scanDownloaded[pageId + 1] != true)
                {
                    main.StartCoroutine(main.GetScanPage(mangaName, mangaChapter, $"{pageId + 1}", scans[pageId + 1].GetComponent<RawImage>()));
                    scanDownloaded[pageId + 1] = true;
                }
            }
            if ((pageId + 2) <= scanDownloaded.Length - 1)
            {
                if(scanDownloaded[pageId + 2] != true)
                {
                    main.StartCoroutine(main.GetScanPage(mangaName, mangaChapter, $"{pageId + 2}", scans[pageId + 2].GetComponent<RawImage>()));
                    scanDownloaded[pageId + 2] = true;
                }
            }
        }

        RectTransform scanHolderRect = scanHolder.GetComponent<RectTransform>();
        //Space the pages based on screen width -> number of pages
        for (int i = 0; i < scans.Count; i++)
        {
            Vector2 finalPos;
            if (isScreenPortrait)
            {
                finalPos = new Vector2((Screen.width * i), scans[i].GetComponent<RectTransform>().localPosition.y);
            }
            else
            {
                finalPos = new Vector2((Screen.width + Screen.width / 2) * i, scans[i].GetComponent<RectTransform>().localPosition.y);

            }
            scans[i].GetComponent<RectTransform>().localPosition = finalPos;
        }

        float finalPosX;
        //Calculate the final position of the scan holder to based on the current pageId
        if (isScreenPortrait)
            finalPosX = -(Screen.width * pageId);
        else
            finalPosX = -((Screen.width + Screen.width / 2) * pageId);

        //Apply the calculations
        if (screenIsRotating)//Teleport to the right page other wise the normal lerping is jittery when changing from portrait to landscape and vice versa
            scanHolderRect.localPosition = new Vector2(finalPosX, 0);
        else
            scanHolderRect.localPosition = Vector2.Lerp(scanHolderRect.localPosition, new Vector2(finalPosX, 0), pageTurnTime * Time.fixedDeltaTime);
    }

    private IEnumerator ResizeScans(bool screenOrientation)
    {
        screenIsRotating = true;
        
        yield return new WaitForSeconds(0.5f);

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        for (int i = 0; i < scans.Count; i++)
        {
            //Set the object to the original proportions
            scans[i].GetComponent<RawImage>().SetNativeSize();
            scans[i].GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            Rect rect = scans[i].GetComponent<RectTransform>().rect;
            float rapport = 0;

            //Calculate the final scale based on the phone's screen orientation
            if (screenOrientation) //Portrait
            {
                if (rect.width > screenWidth)
                {
                    rapport = screenWidth / rect.width;
                }
                else
                {
                    rapport = screenHeight / rect.height;
                }
            }
            else //Landscape
            {   
                rapport = (screenWidth / rect.width) - 1;
            }

            //Apply the calculated sizes
            scans[i].GetComponent<RectTransform>().sizeDelta = new Vector2(rect.width * rapport, rect.height * rapport);
            scans[i].GetComponent<RectTransform>().localPosition = new Vector2(Screen.width * i, 0);
            screenIsRotating = false;
        }
    }

    private void GetScreenOrientation()
    {
#if UNITY_EDITOR
        //Skip the screen orientation change because it's a case that doesn't change anything
        if (Input.deviceOrientation == DeviceOrientation.FaceDown || Input.deviceOrientation == DeviceOrientation.FaceUp)
        {
            return;
        }

        isScreenPortrait =  (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown);
#elif UNITY_ANDROID

        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            isScreenPortrait = true;
        }
        else if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            isScreenPortrait = false;
        }
#endif
    }
}
