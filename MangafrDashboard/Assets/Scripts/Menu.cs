using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_Text[] buttonsText;
    public GameObject[] pannels;
    private bool[] menuStates;

    void Start()
    {
        menuStates = new bool[buttonsText.Length];
        EnablePannel(0);
    }

    public void OnClick_ApplicationQuit()
    {
        Application.Quit();
    }

    public void EnablePannel(int buttonId)
    {
        for (int i = 0; i < menuStates.Length; i++)
        {
            //Set the states of the bool
            menuStates[i] = i == buttonId ? true : false;

            pannels[i].SetActive(menuStates[i]);

            //Set the color based on the button state
            buttonsText[i].color = menuStates[i] == true ? buttonsText[i].color = Color.red : buttonsText[i].color = Color.white;
        }
    }
}
