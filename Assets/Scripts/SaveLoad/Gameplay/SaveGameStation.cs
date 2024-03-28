using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameStation : MonoBehaviour
{
    public void SaveGame()
    {
        FindObjectOfType<SaveLoadScene>().SaveGame();
    }

}
