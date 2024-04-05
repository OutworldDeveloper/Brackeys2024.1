using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public sealed class ItemAddedSoundEffect : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _playerCharacter;
    [SerializeField] private Sound _sound;

    private void OnEnable()
    {
        _playerCharacter.Inventory.ItemAdded += OnItemAdded;
    }

    private void OnDisable()
    {
        _playerCharacter.Inventory.ItemAdded -= OnItemAdded;
    }

    private void OnItemAdded(ItemStack item)
    {
        var audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        _sound.Play(audioSource);
    }

}
