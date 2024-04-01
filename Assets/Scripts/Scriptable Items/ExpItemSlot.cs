using System;

public class ExpItemSlot
{

    public event Action<ExpItemSlot> Changed;

    public ItemStack Stack { get; private set; }
    public bool IsEmpty => Stack == null;

    public ItemStack RemoveStack()
    {
        Stack.Updated -= OnStackUpdated;
        var result = Stack;
        Stack = null;
        Changed?.Invoke(this);
        return result;
    }

    public void SetStack(ItemStack stack)
    {
        // Remove Stack?
        Stack = stack;
        Stack.Updated += OnStackUpdated;
        Changed?.Invoke(this);
    }

    private void OnStackUpdated()
    {
        Changed?.Invoke(this);
    }

    public bool TryAddStack(ItemStack stack)
    {
        if (IsEmpty == true)
        {
            SetStack(stack);
            return true;
        }

        return Stack.TryAddToStack(stack);
    }

}
