using Alchemy.Inspector;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(Order.UI)]
[DisableAlchemyEditor]
public class UI_InventoryScreen : UI_Panel
{

    [SerializeField] private UI_InventoryGrid _inventoryGrid;
    [SerializeField] private UI_InventoryGrid _hotbarGrid;
    [SerializeField] private RectTransform _itemMovePreviewGO;
    [SerializeField] private Image _itemMovePreview;
    [SerializeField] private TextMeshProUGUI _itemsCountPreview;

    [SerializeField] private TextMeshProUGUI _selectedNameLabel;
    [SerializeField] private TextMeshProUGUI _selectedDescriptionLabel;

    [SerializeField] private GameObject _contextMenuBackground;
    [SerializeField] private RectTransform _itemActionsMenu;
    [SerializeField] private Prefab<UI_ItemActionButton> _itemContextButtonPrefab;
    [SerializeField] private Prefab<UI_YesNoWindow> _yesNoWindowPrefab;

    private StackMoveInfo _currentMove;

    protected PlayerCharacter Character { get; private set; }

    private bool IsContextMenuOpen 
    { 
        get
        {
            return _itemActionsMenu.gameObject.activeSelf;
        }
        set
        {
            _contextMenuBackground.gameObject.SetActive(value);
        }
    }

    private bool IsMovingStack => _currentMove != null;

    protected virtual bool ShowDestroyOption => true;

    public void SetTarget(PlayerCharacter character)
    {
        Character = character;
        _inventoryGrid.SetTarget(character.Inventory);
        _hotbarGrid.SetTarget(character.Equipment);
    }

    protected virtual void Start()
    {
        RegisterGrid(_inventoryGrid);
        RegisterGrid(_hotbarGrid);

        IsContextMenuOpen = false;

        _selectedNameLabel.text = string.Empty;
        _selectedDescriptionLabel.text = string.Empty;
    }

    private void Update()
    {
        _itemMovePreviewGO.position = Input.mousePosition;
    }

    protected void RegisterSlot(UI_Slot slot)
    {
        slot.Selected += OnSlotSelected;
        slot.SelectedAlt += OnSlotSelectedAlt;
        slot.Hovered += OnSlotHovered;
    }

    protected void RegisterGrid(UI_InventoryGrid grid)
    {
        grid.SlotSelected += OnSlotSelected;
        grid.SlotSelectedAlt += OnSlotSelectedAlt;
        grid.SlotHovered += OnSlotHovered; 
    }

    protected virtual void OnSlotSelected(UI_Slot slot)
    {
        if (IsContextMenuOpen == true)
        {
            IsContextMenuOpen = false;
        }

        if (IsMovingStack == false)
        {
            if (slot.TargetSlot.IsEmpty == false)
            {
                if (Input.GetKey(KeyCode.LeftShift) == true)
                {
                    HandleQuickAction(slot);
                }
                else
                {
                    StartMove(slot, slot.TargetSlot.Stack.Count);
                }
            }
        }
        else
        {
            if (_currentMove.From.TargetSlot == slot.TargetSlot)
            {
                StopMove();
            }
            else
            {
                int toi = _currentMove.Amount;
                for (int i = 0; i < toi; i++)
                {
                    if (InventoryManager.TryTransfer(_currentMove.From.TargetSlot, slot.TargetSlot, 1) == true)
                    {
                        _currentMove.Amount--;
                    }
                }

                if (_currentMove.Amount == 0)
                    StopMove();
                else
                    RefreshMoveVisuals();
            }
        }
    }

    protected virtual void OnSlotSelectedAlt(UI_Slot slot)
    {
        if (IsMovingStack == false)
        {
            var corners = new Vector3[4];
            slot.GetComponent<RectTransform>().GetWorldCorners(corners);
            _itemActionsMenu.position = corners[0] + Vector3.right * slot.GetComponent<RectTransform>().rect.width / 2 * GetComponentInParent<Canvas>().scaleFactor;

            foreach (Transform button in _itemActionsMenu)
            {
                Destroy(button.gameObject);
            }

            var actions = new List<ItemAction>();
            CreateActionsFor(slot, actions);

            foreach (var action in actions)
            {
                var actionButton = _itemContextButtonPrefab.Instantiate();
                actionButton.transform.SetParent(_itemActionsMenu, false);
                actionButton.SetAction(this, action);
            }

            if (actions.Count > 0)
                IsContextMenuOpen = true;
        }
        else
        {
            if (_currentMove.From.TargetSlot == slot.TargetSlot)
            {
                _currentMove.Amount -= 1;

                if (_currentMove.Amount <= 0)
                    StopMove();

                RefreshMoveVisuals();
                return;
            }

            if (InventoryManager.TryTransfer(_currentMove.From.TargetSlot, slot.TargetSlot, 1) == true)
            {
                _currentMove.Amount -= 1;

                if (_currentMove.Amount <= 0)
                    StopMove();

                RefreshMoveVisuals();
            }
        }
    }

    protected virtual void HandleQuickAction(UI_Slot slot) 
    {
        if (slot.TargetSlot.Owner == Character.Equipment)
        {
            InventoryManager.TryTransfer(slot.TargetSlot, Character.Inventory, slot.TargetSlot.Stack.Count);
        }

        if (slot.TargetSlot.Owner == Character.Inventory && slot.TargetSlot.Stack.Item is WeaponItem)
        {
            InventoryManager.TryTransfer(slot.TargetSlot, Character.Equipment, 1);
        }
    }

    protected virtual void CreateActionsFor(UI_Slot slot, List<ItemAction> actions)
    {
        if (slot.TargetSlot.IsEmpty == true)
            return;

        if (slot.TargetSlot.Stack.Count > 1)
        {
            actions.Add(new ItemAction("Split", () => StartMove(slot, slot.TargetSlot.Stack.Count / 2)));
        }

        if (slot.TargetSlot.Owner == Character.Equipment)
        {
            actions.Add(new ItemAction("Unequip", () => 
                InventoryManager.TryTransfer(slot.TargetSlot, Character.Inventory, slot.TargetSlot.Stack.Count)));
        }

        if (slot.TargetSlot.Owner == Character.Inventory && slot.TargetSlot.Stack.Item is WeaponItem)
        {
            actions.Add(new ItemAction("Equip", () =>
                InventoryManager.TryTransfer(slot.TargetSlot, Character.Equipment, 1)));
        }

        if (ShowDestroyOption == true)
        {
            actions.Add(new ItemAction("Destroy", () =>
            {
                Owner.InstantiateAndOpenFrom(_yesNoWindowPrefab).
                    Setup(
                    $"Destroy {slot.TargetSlot.Stack.Item.DisplayName}?", 
                    "This action cannot be undone.",
                    () => InventoryManager.TryDestroyStack(slot.TargetSlot, slot.TargetSlot.Stack.Count),
                    true);
            }));
        }
    }

    public void TryExecuteItemAction(ItemAction action)
    {
        action.Action.Invoke();
        IsContextMenuOpen = false;
    }

    public void CloseContextMenu()
    {
        IsContextMenuOpen = false;
    }

    private void StartMove(UI_Slot from, int amount)
    {
        if (IsMovingStack == true)
            throw new Exception("Cannot start more than one move!");

        if (from.TargetSlot.IsEmpty == true)
            throw new Exception("Cannot start move from an empty slot!");

        _currentMove = new StackMoveInfo(from, amount);

        RefreshMoveVisuals();
    }

    private void StopMove()
    {
        _currentMove.From.ClearFakeSubstraction();

        _currentMove = null;

        RefreshMoveVisuals();
    }

    private void RefreshMoveVisuals()
    {
        if (IsMovingStack == false)
        {
            _itemsCountPreview.gameObject.SetActive(false);
            _itemMovePreviewGO.gameObject.SetActive(false);
            return;
        }

        _itemMovePreview.sprite = _currentMove.From.TargetSlot.Stack.Item.Sprite;
        _itemMovePreviewGO.gameObject.SetActive(true);

        _itemsCountPreview.gameObject.SetActive(_currentMove.Amount > 1);
        _itemsCountPreview.text = _currentMove.Amount.ToString();
    }

    private void OnSlotHovered(UI_Slot slot)
    {
        //_selectedNameLabel.gameObject.SetActive(slot.TargetSlot.IsEmpty == false);
        //_selectedDescriptionLabel.gameObject.SetActive(slot.TargetSlot.IsEmpty == false);

        if (IsMovingStack == true)
            return;

        if (slot == null || slot.TargetSlot.IsEmpty)
        {
            _selectedNameLabel.text = string.Empty;
            _selectedDescriptionLabel.text = string.Empty;
        }
        else
        {
            _selectedNameLabel.text = slot.TargetSlot.Stack.Item.DisplayName;
            _selectedDescriptionLabel.text = slot.TargetSlot.Stack.Item.Description;

            if (slot.TargetSlot.Stack.Item is WeaponItem weapon)
            {
                _selectedDescriptionLabel.text += $" Has a maximum ammo capacity of {weapon.MaxAmmo}.";
            }
        }
    }

    public override void InputUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Tab) == true)
        {
            CloseAndDestroy();
        }
    }

}

public sealed class StackMoveInfo
{

    public readonly UI_Slot From;
    private int _amount;

    public StackMoveInfo(UI_Slot from, int amount)
    {
        if (amount <= 0)
            throw new Exception("Invalid amount.");

        From = from;
        Amount = amount;
    }

    // No longer just Info then?
    public int Amount 
    {
        get 
        { 
            return _amount; 
        } 
        set
        {
            _amount = value;
            From.SetFakeSubstraction(_amount);
        } 
    }

}

public sealed class ItemAction
{

    public readonly string DisplayName;
    public readonly Action Action;

    public ItemAction(string displayName, Action action)
    {
        DisplayName = displayName;
        Action = action;
    }

}

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_ItemCursor : MonoBehaviour
{

    [SerializeField] private Image _itemMovePreview;
    [SerializeField] private TextMeshProUGUI _itemsCountPreview;

}
