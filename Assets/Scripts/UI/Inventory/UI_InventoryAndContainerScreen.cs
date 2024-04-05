using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_InventoryAndContainerScreen : UI_InventoryScreen
{

    [SerializeField] private UI_InventoryGrid _containerGrid;

    public void SetContainer(Inventory inventory)
    {
        _containerGrid.SetTarget(inventory);
    }

    protected override void Start()
    {
        base.Start();
        RegisterGrid(_containerGrid);
    }

}
