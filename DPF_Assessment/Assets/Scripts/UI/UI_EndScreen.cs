using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_EndScreen : MonoBehaviour
{
    private UI_Menu menu;

    private void Awake()
    {
        menu = GetComponentInChildren<UI_Menu>();
    }
    // Start is called before the first frame update
    void Start()
    {
        menu.Show();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Winner(bool isWinner)
    {

    }
}
