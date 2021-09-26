using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBar : MonoBehaviour
{
    private Main main;
    private Color selectedColor = Color.white;

    public GameObject[] barItems;
    public GameObject[] itemPannels;

    public GameObject selectedMangaPannel;
    private int currentPageId;

    void Start()
    {
        main = transform.root.GetComponent<Essentials>().main;
        OnClick_SelectPannel(0);
    }

    public void OnClick_SelectPannel(int id)
    {
        for (int i = 0; i < barItems.Length; i++)
        {
            if(i == id)
            {
                barItems[id].transform.GetChild(0).GetComponent<Image>().color = selectedColor;
                barItems[id].transform.GetChild(1).GetComponent<Text>().color = selectedColor;
                itemPannels[id].SetActive(true);
            }
            else
            {
                barItems[i].transform.GetChild(0).GetComponent<Image>().color = new Color32(0x8C, 0x8C, 0x8C, 0xFF);
                barItems[i].transform.GetChild(1).GetComponent<Text>().color = new Color32(0x8C, 0x8C, 0x8C, 0xFF);
                itemPannels[i].SetActive(false);
            }
        }
        currentPageId = id;
        selectedMangaPannel.SetActive(false);
    }
}
