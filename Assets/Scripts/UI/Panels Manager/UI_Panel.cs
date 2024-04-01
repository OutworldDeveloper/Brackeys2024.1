using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_Panel : MonoBehaviour
{

    [field: SerializeField] public bool HidePanelsBelow { get; private set; }

    public UI_PanelsManager Owner { get; private set; }

    public virtual bool CanUserClose() => true;
    public virtual void InputUpdate() { }

    public void CloseAndDestroy()
    {
        Owner.RemoveAndDestroy(this);
    }

    public virtual void OnAddedToStack(UI_PanelsManager owner)
    {
        Owner = owner;
    }

}
