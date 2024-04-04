using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(Order.UI)]
public class UI_InventoryScreen : UI_Panel
{

    [SerializeField] private UI_InventoryGrid _inventoryGrid;
    [SerializeField] private UI_Slot _weaponSlot;
    [SerializeField] private RectTransform _itemMovePreviewGO;
    [SerializeField] private Image _itemMovePreview;
    [SerializeField] private TextMeshProUGUI _itemsCountPreview;

    [SerializeField] private TextMeshProUGUI _selectedNameLabel;
    [SerializeField] private TextMeshProUGUI _selectedDescriptionLabel;

    [SerializeField] private ItemDefinition[] _itemsTest;

    private PlayerCharacter _character;

    private bool _isMovingItem;
    private UI_Slot _movingFromSlot;
    private float _lastMoveStartTime;
    private int _moveAmount;

    public void SetTarget(PlayerCharacter character)
    {
        _character = character;
        _inventoryGrid.SetTarget(character.GetComponent<ExpInventory>());
        _weaponSlot.SetTarget(character.GetComponent<Equipment>().WeaponSlot);
    }

    protected virtual void Start()
    {
        RegisterGrid(_inventoryGrid);
        RegisterSlot(_weaponSlot);
    }

    private void Update()
    {
        if (_isMovingItem == true)
        {
            //_itemMovePreviewGO.position = 
            //    Vector2.Lerp(
            //        _movingFromSlot.GetComponent<RectTransform>().position, 
            //        Input.mousePosition, (Time.unscaledTime - _lastMoveStartTime) * 15f);

            _itemMovePreviewGO.position = Input.mousePosition;
        }

        KeyCode[] keyCodes = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
        };

        for (int i = 0; i < _itemsTest.Length; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]) == true)
            {
                _character.GetComponent<ExpInventory>().TryAdd(new ItemStack(_itemsTest[i]));
            }
        }
    }

    protected void RegisterSlot(UI_Slot slot)
    {
        slot.Selected += OnSlotSelected;
        slot.Hovered += OnSlotHovered;
    }

    protected void RegisterGrid(UI_InventoryGrid grid)
    {
        grid.SlotSelected += OnSlotSelected;
        grid.SlotHovered += OnSlotHovered;
    }

    private void OnSlotSelected(UI_Slot slot)
    {
        Debug.Log(slot.TargetSlot);

        if (_isMovingItem == false)
        {
            if (slot.TargetSlot.IsEmpty == false)
            {
                _isMovingItem = true;
                _movingFromSlot = slot;

                _moveAmount = _movingFromSlot.TargetSlot.Stack.Count;

                slot.Hide();

                _lastMoveStartTime = Time.unscaledTime;

                _itemMovePreview.sprite = _movingFromSlot.TargetSlot.Stack.Item.Sprite;
                _itemMovePreviewGO.gameObject.SetActive(true);

                _itemsCountPreview.gameObject.SetActive(_movingFromSlot.TargetSlot.Stack.Count > 1);
                _itemsCountPreview.text = _movingFromSlot.TargetSlot.Stack.Count.ToString();
            }
        }
        else
        {
            bool shouldStopMoving = false;

            if (_movingFromSlot.TargetSlot != slot.TargetSlot)
            {
                //for (int i = 0; i < _moveAmount; i++)
                //{
                //    InventoryManager.TryTransfer(_movingFromSlot.TargetSlot, slot.TargetSlot);
                //}

                InventoryManager.TryTransfer(_movingFromSlot.TargetSlot, slot.TargetSlot, _moveAmount);

                if (_movingFromSlot.TargetSlot.IsEmpty == true)
                    shouldStopMoving = true;
            }
            else
            {
                shouldStopMoving = true;
            }

            if (shouldStopMoving == true)
            {
                _isMovingItem = false;
                _movingFromSlot.Show();
                _movingFromSlot = null;

                _itemsCountPreview.gameObject.SetActive(false);
                _itemMovePreviewGO.gameObject.SetActive(false);
            }
            else
            {
                _itemsCountPreview.gameObject.SetActive(_movingFromSlot.TargetSlot.Stack.Count > 1);
                _itemsCountPreview.text = _movingFromSlot.TargetSlot.Stack.Count.ToString();
            }
        }
    }

    private void OnSlotHovered(UI_Slot slot)
    {
        _selectedNameLabel.gameObject.SetActive(slot.TargetSlot.IsEmpty == false);
        _selectedDescriptionLabel.gameObject.SetActive(slot.TargetSlot.IsEmpty == false);

        if (slot.TargetSlot.IsEmpty == false)
        {
            _selectedNameLabel.text = slot.TargetSlot.Stack.Item.DisplayName;
            _selectedDescriptionLabel.text = slot.TargetSlot.Stack.Item.Description;
        }
    }

    public override void InputUpdate()
    {
        return;
        if (Input.GetKeyDown(KeyCode.Tab) == true)
        {
            CloseAndDestroy();
        }
    }

}

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_ItemCursor : MonoBehaviour
{

    [SerializeField] private Image _itemMovePreview;
    [SerializeField] private TextMeshProUGUI _itemsCountPreview;

}
