using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ChapterItem : MonoBehaviour
{
    public GameObject scanHolder;
    public GameObject scansItemPrefab;
    public Text imageId;
    public Text imagePath;
    public string[] scansPath;

    public void SetItemProperties(string name, string path, string[] scansPath, GameObject scanHolder)
    {
        imageId.text = name;
        imagePath.text = path;
        this.scansPath = scansPath;
        this.scanHolder = scanHolder;
    }

    //Used on a button event
    public void OnClick_DisplayChapterScans()
    {
        //Clear the scan list
        foreach (Transform child in scanHolder.transform)
        {
            Destroy(child.gameObject);
        }

        //Add the scan elements to the scans pannel
        for (int i = 0; i < scansPath.Length; i++)
        {
            GameObject go = Instantiate(scansItemPrefab);
            go.GetComponent<ScanItem>().SetItemProperties(i.ToString(), scansPath[i]);
            go.transform.SetParent(scanHolder.transform);
        }
        //Resize the scans holder height so we can scroll properly
        Vector2 size = scanHolder.GetComponent<RectTransform>().sizeDelta;
        float finlaSizeY = (scansItemPrefab.GetComponent<RectTransform>().rect.height + scanHolder.GetComponent<VerticalLayoutGroup>().spacing) * scansPath.Length;
        scanHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, finlaSizeY);
    }
}
