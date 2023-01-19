using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStartButtonPressed()
    {
        GameManager.Instance.start = true;
        GameManager.Instance.HideButtons();
    }
    
    public void OnHowToPlayButtonPressed()
    {
        GameManager.Instance.howToPlay = true;
        GameManager.Instance.HideButtons();
    }
    
    public void OnExitButtonPressed()
    {
        GameManager.Instance.exit = true;
        GameManager.Instance.HideButtons();
    }
}
