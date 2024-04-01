using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameStation : MonoBehaviour
{

    [SerializeField] private Prefab<UI_SaveGameWindow> _windowPrefab;

    public void SaveGame(PlayerCharacter character)
    {
        character.Player.Panels.InstantiateAndOpenFrom(_windowPrefab);
    }

}
