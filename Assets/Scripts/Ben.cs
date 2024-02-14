using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Door))]
public sealed class Ben : MonoBehaviour
{

    private const float answerDelay = 1.0f;
    private const float textDelay = 1.84f;
    private const float feedDelay = 0.5f;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _hungrySound;
    [SerializeField] private Sound _happySound;
    [SerializeField] private ItemTag _foodTag;
    [SerializeField] private Code _codeReward;

    public ItemTag FoodTag => _foodTag;
    public bool IsHungry { get; private set; } = true;

    private void OnEnable()
    {
        GetComponent<Door>().SomeoneKnocked += OnDoorKnocked;
    }

    private void OnDisable()
    {
        GetComponent<Door>().SomeoneKnocked -= OnDoorKnocked;
    }

    public bool TryFeed(PlayerCharacter character)
    {
        if (IsHungry == false)
            return false;

        foreach (var item in character.Inventory.Content)
        {
            if (item.HasTag(_foodTag) == false)
                continue;

            character.Inventory.RemoveItem(item);
            item.Destroy();
            IsHungry = false;
            SayCode();
            return true;
        }

        Delayed.Do(() => _hungrySound.Play(_audioSource), feedDelay);
        return false;
    }

    private void OnDoorKnocked()
    {
        if (IsHungry == true)
        {
            Delayed.Do(() => _hungrySound.Play(_audioSource), answerDelay);
            Delayed.Do(() => Notification.Do("Is he... hungry?", 1.5f), textDelay);
        }
        else
        {
            if (IsAngryGhostNear(out var ghost) == true)
            {
                Delayed.Do(() => MakeGhostLeave(ghost), answerDelay);
                return;
            }

            SayCode();
        }
    }

    private void MakeGhostLeave(Ghost ghost)
    {
        Delayed.Do(ghost.StartRespawning, 0.5f);
        _hungrySound.Play(_audioSource);
    }

    private bool IsAngryGhostNear(out Ghost ghost)
    {
        ghost = FindObjectOfType<Ghost>();

        if (ghost == null)
            return false;

        if (Vector3.Distance(ghost.transform.position, transform.position) > 7f)
            return false;

        return true;
    }

    private void SayCode()
    {
        Delayed.Do(() => _happySound.Play(_audioSource), answerDelay);
        Delayed.Do(() => Notification.Do($"Is he saying... {_codeReward.Value}?", 2.5f), textDelay);
    }

}