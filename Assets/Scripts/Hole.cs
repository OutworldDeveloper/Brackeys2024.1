using UnityEngine;

public sealed class Hole : MonoBehaviour
{

    [SerializeField] private Item _ropeItem;
    [SerializeField] private Item _hookItem;
    [SerializeField] private Item _rewardItem;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animationDuration;
    [SerializeField] private MoviePawn _moviePawn;

    public bool IsItemExtracted { get; private set; }

    private void Start()
    {
        _moviePawn.SetDuration(_animationDuration);
    }

    public bool TryExtractItem(PlayerCharacter player)
    {
        if (IsItemExtracted == true)
            return false;

        bool hasRope = player.Inventory.HasItem(_ropeItem);
        bool hasHook = player.Inventory.HasItem(_hookItem);

        if (hasRope == false)
        {
            Notification.Show(GetFailResponse(hasRope, hasHook), 1.5f);
            return false;
        }

        bool isSuccess = hasRope == true && hasHook == true;

        if (isSuccess == true)
        {
            player.Inventory.RemoveItem(_ropeItem);
            player.Inventory.RemoveItem(_hookItem);
            IsItemExtracted = true;
            Delayed.Do(() => GiveReward(player), _animationDuration - 0.2f);
        }
        else
        {
            Delayed.Do(() => GiveNothing(), _animationDuration - 0.2f);
        }

        player.Player.Possess(_moviePawn);
        _animator.Play(isSuccess ? "Extraction" : "ExtractionFailed", 0);
        return true;
    }

    private void GiveReward(PlayerCharacter player)
    {
        player.Inventory.AddItem(_rewardItem);
        Notification.Show($"{_rewardItem.DisplayName}!");
    }

    private void GiveNothing()
    {
        Notification.Show("I need a hook or something...");
    }

    private string GetFailResponse(bool hasRope, bool hasHook)
    {
        return "There is something interesting down there. How do I reach it?";

        if (hasRope == true && hasHook == false)
            return "I need a hook";

        if (hasRope == false && hasHook == true)
            return "I need a fishing rode";

        return "There is something interesting down there. But I cannot reach.";
    }

}
