using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_Panel : MonoBehaviour
{

    [field: SerializeField] public bool HidePanelsBelow { get; private set; } = false;
    [field: SerializeField] public bool RequirePause { get; private set; } = true;
    [field: SerializeField] public bool HideBackground { get; private set; } = true;

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
