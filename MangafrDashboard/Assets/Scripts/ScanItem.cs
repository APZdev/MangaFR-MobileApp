using UnityEngine;
using UnityEngine.UI;

public class ScanItem : MonoBehaviour
{
    public Text imageId;
    public Text imagePath;

    public void SetItemProperties(string name, string path)
    {
        imageId.text = name;
        imagePath.text = path;
    }
}
