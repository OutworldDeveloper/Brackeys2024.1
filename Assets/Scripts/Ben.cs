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
    [SerializeField] private Sound _killSelfAdviceSound;
    [SerializeField] private ItemTag _foodTag;
    [SerializeField] private Code _codeReward;

    [SerializeField] private SafeLock _safeLock;
    [SerializeField] private Door _trapSwitch;
    [SerializeField] private PlayerTrigger _trapRoomTrigger;

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
            Say(_hungrySound, "Is he... hungry?", 1.5f);
            return;
        }

        if (IsAngryGhostNear(out var ghost) == true)
        {
            Delayed.Do(() => MakeGhostLeave(ghost), answerDelay);
            return;
        }

        SayNextAdvice();
    }

    private void SayNextAdvice()
    {
        if (_safeLock.IsOpen == false)
        {
            SayCode();
            return;
        }

        if (_trapSwitch.IsOpen == true && _trapRoomTrigger.EverVisited == false)
        {
            Say(_killSelfAdviceSound, "When there is not enough time, dying might be the only option.", 3f);
            // If you don't have enough time, why not kill yourself?
            // When there is not enough time, dying might be the only option.
            // Don't be afraid of the ghost, he might be useful right now
            return;
        }

        //SayNothing();
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

    private void Say(Sound sound, string text, float notificationDuration = 2.5f)
    {
        Delayed.Do(() => sound.Play(_audioSource), answerDelay);
        Delayed.Do(() => Notification.Do(text, notificationDuration), textDelay);
    }

    private void SayNothing()
    {
        Delayed.Do(() => Notification.Do("Nothing..."), textDelay);
    }

    private void SayCode()
    {
        Say(_happySound, $"Is he saying... {_codeReward.Value}?");
    }

}