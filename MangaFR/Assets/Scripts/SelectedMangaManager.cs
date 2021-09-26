using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedMangaManager : MonoBehaviour
{
    [SerializeField] private GameObject[] mangaButtons;
    [SerializeField] private GameObject selectedUnderline;
    [SerializeField] private GameObject pannelHolder;
    [SerializeField] private float scrollSpeed = 50f;

    [SerializeField] private float scrollThreshold = 100f;
    [SerializeField] private float pageTurnSpeed = 1.0f;
    [SerializeField] private int pageId;

    private RectTransform pannelHolderRect;

    private bool couldBeSwipe;
    private Vector2 startPos;

    private void OnEnable()
    {
        pannelHolderRect = pannelHolder.GetComponent<RectTransform>();
        pageId = 0;
        pannelHolderRect.localPosition = new Vector2((Screen.width / 2), pannelHolderRect.localPosition.y);
        selectedUnderline.transform.localPosition = new Vector2(mangaButtons[0].transform.localPosition.x, selectedUnderline.transform.localPosition.y);
    }

    private void Update()
    {
        PageScroll();
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
                        if (couldBeSwipe)
                        {
                            if (deltaMove > 0)
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
            pageId = Mathf.Clamp(pageId, 0, pannelHolder.transform.childCount - 1);
        }

        float finalPosX = -(Screen.width * pageId) + (Screen.width / 2);

        selectedUnderline.transform.localPosition = Vector2.Lerp(selectedUnderline.transform.localPosition, new Vector2(mangaButtons[pageId].transform.localPosition.x, selectedUnderline.transform.localPosition.y), pageTurnSpeed * Time.deltaTime);

        pannelHolderRect.localPosition = Vector2.Lerp(pannelHolderRect.localPosition, new Vector2(finalPosX, pannelHolderRect.localPosition.y), pageTurnSpeed * Time.deltaTime);
    }

    public void OnClick_SetPannel(int id)
    {
        pageId = id;
    }
}
