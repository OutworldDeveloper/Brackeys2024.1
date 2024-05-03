using System;
using UnityEngine;

public sealed class FeedBenInteraction : Interaction
{

    [SerializeField] private Ben _ben;

    public override string Text => "Give food";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _ben.IsHungry == true;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Player.OpenItemSelection(new PizzaSelector(_ben));
    }

    private sealed class PizzaSelector : ItemSelector
    {

        private readonly Ben _ben;

        public PizzaSelector(Ben ben)
        {
            _ben = ben;
        }

        public override bool CanAccept(IReadOnlyStack stack)
        {
            return _ben.CanEat(stack);
        }

        public override void Select(ItemStack stack)
        {
            var foodStack = stack.Take(1);
            _ben.TryFeed(foodStack);
        }

        public override string GetRejectionReason(IReadOnlyStack stack)
        {
            return "I don't think he... it... can eat that";
        }

    }

}

