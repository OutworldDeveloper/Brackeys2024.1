using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Final Door Code")]
public sealed class FinalDoorCode : ScriptableObject
{

    [SerializeField] private CodeCharacter[] _characters;

    public CodeCharacter[] Characters => _characters.ToArray();

    public bool IsValid(CodeCharacter[] code)
    {
        if (code.Length != _characters.Length)
            return false;

        for (int i = 0; i < code.Length; i++)
        {
            if (code[i] != _characters[i])
                return false;
        }

        return true;
    }

}

public enum CodeCharacter
{
    A, B, D, E, F
}
