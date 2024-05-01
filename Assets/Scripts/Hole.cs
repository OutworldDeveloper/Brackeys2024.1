using System;
using UnityEngine;

public sealed class Hole : MonoBehaviour
{

    [SerializeField] private ItemTag _ropeItemTag;
    [SerializeField] private ItemTag _hookItemTag;
    [SerializeField] private Item _reward;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animationDuration;
    [SerializeField] private MoviePawn _moviePawn;
    [SerializeField] private GameObject _keyPreview;

    public bool IsItemExtracted { get; private set; }

    private void Start()
    {
        _moviePawn.SetDuration(_animationDuration);
    }

    public bool TryExtractItem(PlayerCharacter player)
    {
        throw new NotImplementedException();

        //if (IsItemExtracted == true)
        //    return false;
        //
        //bool hasRope = player.Inventory.TryGetItemWithTag(_ropeItemTag, out Item ropeItem);
        //bool hasHook = player.Inventory.TryGetItemWithTag(_hookItemTag, out Item hookItem);

        //if (hasRope == false)
        //{
        //    Notification.Show(GetFailResponse(hasRope, hasHook), 1.5f);
        //    return false;
        //}
        //
        //bool isSuccess = hasRope == true && hasHook == true;
        //
        //if (isSuccess == true)
        //{
        //    //player.Inventory.RemoveAndDestroyItem(ropeItem);
        //    //player.Inventory.RemoveAndDestroyItem(hookItem);
        //    IsItemExtracted = true;
        //    Delayed.Do(() => GiveReward(player), _animationDuration - 0.3f);
        //    _keyPreview?.SetActive(false);
        //}
        //else
        //{
        //    Delayed.Do(() => GiveNothing(), _animationDuration - 0.3f);
        //}

        //player.Player.Possess(_moviePawn);
        //_animator.Play(isSuccess ? "Extraction" : "ExtractionFailed", 0);
        //return true;
    }

    private void GiveReward(PlayerCharacter player)
    {
        //var rewardItem = _reward.Instantiate();
        //player.Inventory.AddItem(rewardItem);
        //Notification.Show($"{rewardItem.DisplayName}!");
    }

    private void GiveNothing()
    {
        Notification.Show("I need a hook or something...");
    }

    private string GetFailResponse(bool hasRope, bool hasHook)
    {
        if (hasRope == true && hasHook == false)
            return "I need a hook";

        if (hasRope == false && hasHook == true)
            return "I need a rope";

        return "I cannot reach";
    }

}
