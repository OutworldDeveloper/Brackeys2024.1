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

    private sealed class PizzaSelector : IItemSelector
    {

        private readonly Ben _ben;

        public PizzaSelector(Ben ben)
        {
            _ben = ben;
        }

        public bool CanAccept(IReadOnlyStack stack)
        {
            return _ben.CanEat(stack);
        }

        public void Select(ItemStack stack)
        {
            var foodStack = stack.Take(1);
            _ben.TryFeed(foodStack);
        }

    }

}

