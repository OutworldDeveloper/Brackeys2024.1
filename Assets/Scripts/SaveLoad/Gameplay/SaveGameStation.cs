using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameStation : MonoBehaviour
{

    public void SaveGame(PlayerCharacter character)
    {
        character.Player.OpenItemSelection(new SaveItemSelector(character));
    }

    private sealed class SaveItemSelector : IItemSelector
    {

        private readonly PlayerCharacter _playerCharacter;

        public SaveItemSelector(PlayerCharacter playerCharacter)
        {
            _playerCharacter = playerCharacter;
        }

        public bool CanAccept(IReadOnlyStack stack)
        {
            return true;
        }

        public void Select(ItemStack stack)
        {
            _playerCharacter.Player.OpenSaveScreen();
        }

    }

}
