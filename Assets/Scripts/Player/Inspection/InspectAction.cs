using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InspectAction : MonoBehaviour
{
    public abstract string GetText(PlayerCharacter player);
    public virtual bool IsAvaliable(PlayerCharacter player) => true;
    public abstract void Perform(PlayerCharacter player);

}
