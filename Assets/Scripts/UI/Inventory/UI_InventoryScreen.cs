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

    [SerializeField] private Prefab<Item> _itemTest1;
    [SerializeField] private Prefab<Item> _itemTest2;

    private PlayerCharacter _character;

    private bool _isMovingItem;
    private UI_Slot _movingFromSlot;
    private float _lastMoveStartTime;

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

        KeyCode[] testKeys = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0,
        };

        for (int i = 0; i < testKeys.Length; i++)
        {
            if (Input.GetKeyDown(testKeys[i]) == true)
            {
                Prefab<Item> toSpawn = Input.GetKey(KeyCode.LeftControl) ? _itemTest1 : _itemTest2;
                _character.GetComponent<ExpInventory>()[i].TryAdd(toSpawn.Instantiate());
            }
        }
    }

    protected void RegisterSlot(UI_Slot slot)
    {
        slot.Selected += OnSlotSelected;
    }

    protected void RegisterGrid(UI_InventoryGrid grid)
    {
        grid.SlotSelected += OnSlotSelected;
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
                slot.Hide();

                _lastMoveStartTime = Time.unscaledTime;

                _itemMovePreview.sprite = _movingFromSlot.TargetSlot.FirstItem.Sprite;
                _itemMovePreviewGO.gameObject.SetActive(true);

                _itemsCountPreview.gameObject.SetActive(_movingFromSlot.TargetSlot.ItemsCount > 1);
                _itemsCountPreview.text = _movingFromSlot.TargetSlot.ItemsCount.ToString();
            }
        }
        else
        {
            bool shouldStopMoving = false;

            if (_movingFromSlot.TargetSlot != slot.TargetSlot)
            {
                int itemsCount = _movingFromSlot.TargetSlot.ItemsCount;

                for (int i = 0; i < itemsCount; i++)
                {
                    InventoryManager.TryTransfer(_movingFromSlot.TargetSlot, slot.TargetSlot);
                }

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
                _itemsCountPreview.gameObject.SetActive(_movingFromSlot.TargetSlot.ItemsCount > 1);
                _itemsCountPreview.text = _movingFromSlot.TargetSlot.ItemsCount.ToString();
            }
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
