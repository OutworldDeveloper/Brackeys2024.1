using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InspectAction : MonoBehaviour
{

    [field: SerializeField] public float MaxAngle { get; private set; } = 180f;

    public abstract string GetText(PlayerCharacter player);
    public virtual bool IsAvaliable(PlayerCharacter player) => true;
    public abstract void Perform(PlayerCharacter player);

}
