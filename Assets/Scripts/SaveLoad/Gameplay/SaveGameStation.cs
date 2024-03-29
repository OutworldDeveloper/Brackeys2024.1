using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameStation : MonoBehaviour
{
    public void SaveGame()
    {
        //FindObjectOfType<SaveLoadScene>().SaveGame();

        FindObjectOfType<UI_SaveGameWindow>(true).gameObject.SetActive(true);

        Delayed.Do(() =>
        {
            FindObjectOfType<UI_SaveGameWindow>().gameObject.SetActive(false);
        }, 6f);
    }

}
