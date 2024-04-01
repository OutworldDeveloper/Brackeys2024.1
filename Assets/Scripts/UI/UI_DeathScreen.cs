using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DeathScreen : UI_Panel
{

    [SerializeField] private Prefab<UI_LoadGameWindow> _loadGameScreen;

    public void OnLoadButtonPressed()
    {
        Owner.InstantiateAndOpenFrom(_loadGameScreen);
    }

    public override bool CanUserClose()
    {
        return false;
    }

}
