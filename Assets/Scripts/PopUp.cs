using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UI;
using UnityEngine;

public class PopUp : MonoBehaviour {

    public GameObject windowUI;
    //public Text messageField;
    public static bool GameIsPaused=false;

    private void Start()
    {
        Hide();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Hide();
            }
            else {
                Show();
            }
        }
    }

    public void Show()
    {

        // messageField.text = message;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        windowUI.SetActive(true);
        
        GameIsPaused = true;

    }

    public void Hide()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        windowUI.SetActive(false);
        
        GameIsPaused = false;

    }
}
