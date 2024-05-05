using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_PanelsManager : MonoBehaviour
{

	[SerializeField] private Transform _backgroundHidder;

    private readonly List<UI_Panel> _panelsStack = new List<UI_Panel>();

	public bool HasActivePanel => _panelsStack.Count > 0;
	public UI_Panel Active => _panelsStack[_panelsStack.Count - 1];

    public bool TryCloseActivePanel()
    {
		if (HasActivePanel == false)
			return false;

		if (Active.CanUserClose() == false)
			return false;

		RemoveAndDestroy(Active);
		return true;
	}

	public T InstantiateAndOpenFrom<T>(Prefab<T> prefab) where T : UI_Panel
	{
		T newPanel = prefab.Instantiate();
		newPanel.transform.SetParent(transform, false);
		_panelsStack.Add(newPanel);
		newPanel.OnAddedToStack(this);
		RefreshView();
		return newPanel;
	}

    public void RemoveAndDestroy(UI_Panel panel)
    {
		_panelsStack.Remove(panel);
		Destroy(panel.gameObject);
		RefreshView();
	}

    private void RefreshView()
    {
        // Disable everything
		_backgroundHidder.SetParent(null, false);
        _backgroundHidder.gameObject.SetActive(false);

        for (int i = 0; i < _panelsStack.Count; i++)
		{
			_panelsStack[i].gameObject.SetActive(false);
        }

        for (int i = _panelsStack.Count - 1; i >= 0; i--)
        {
            UI_Panel inspectedPanel = _panelsStack[i];

			inspectedPanel.gameObject.SetActive(true);

			if (inspectedPanel.HideBackground == true)
			{
                _backgroundHidder.SetParent(transform, false);
                _backgroundHidder.SetSiblingIndex(_panelsStack.Count - i - 1);
                _backgroundHidder.gameObject.SetActive(true);
            }

			// If a panel hides panels below, it basically acts like the last panel
			if (inspectedPanel.HidePanelsBelow == true)
				return;
        }
	}

	public bool RequirePause()
	{
		for (int i = 0; i < _panelsStack.Count; i++)
		{
			if (_panelsStack[i].RequirePause == true)
				return true;
		}

		return false;
	}

}
