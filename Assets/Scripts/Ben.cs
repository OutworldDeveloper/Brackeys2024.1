using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Door))]
public sealed class Ben : MonoBehaviour
{

    private const float answerDelay = 1.8f;
    private const float textDelay = 1.84f;
    private const float feedDelay = 0.5f;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _hungrySound;
    [SerializeField] private Sound _happySound;
    [SerializeField] private Sound _killSelfAdviceSound;
    [SerializeField] private Sound _goodbyeSound;
    [SerializeField] private Sound _screamSound;
    [SerializeField] private ItemTag _foodTag;
    [SerializeField] private Code _codeReward;

    [SerializeField] private SafeLock _safeLock;
    [SerializeField] private Door _trapSwitch;
    [SerializeField] private PlayerTrigger _trapRoomTrigger;
    [SerializeField] private Door _finalDoor;

    private TimeSince _timeSinceLastSpoke = new TimeSince(float.NegativeInfinity);

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

        foreach (var item in character.Inventory.Items)
        {
            if (item.HasTag(_foodTag) == false)
                continue;

            character.Inventory.RemoveItem(item);
            IsHungry = false;
            TrySayNextAdvice(true);
            return true;
        }

        Delayed.Do(() => _hungrySound.Play(_audioSource), feedDelay);
        return false;
    }

    private void OnDoorKnocked()
    {
        if (IsHungry == true)
        {
            TrySay(_hungrySound, "Is he... hungry?", 1.5f);
            return;
        }

        if (IsAngryGhostNear(out var ghost) == true)
        {
            Delayed.Do(() => MakeGhostLeave(ghost), answerDelay);
            return;
        }

        TrySayNextAdvice();
    }

    private void TrySayNextAdvice(bool ignoreCooldown = false)
    {
        if (_finalDoor.IsOpen == true)
        {
            TrySay(_goodbyeSound, $"Is he saying... goodbye?");
            return;
        }

        if (_safeLock.IsOpen == false)
        {
            TrySay(_happySound, $"Is he saying... {_codeReward.Value}?", ignoreCooldown: ignoreCooldown);
            return;
        }

        if (_trapSwitch.IsOpen == true && _trapRoomTrigger.EverVisited == false)
        {
            TrySay(_killSelfAdviceSound, "When there is not enough time, dying might be the only option.", 3f, ignoreCooldown);
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
        _screamSound.Play(_audioSource);
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

    private void TrySay(Sound sound, string text, float notificationDuration = 2.5f, bool ignoreCooldown = false)
    {
        if (_timeSinceLastSpoke < 3.4f && ignoreCooldown == false)
            return;

        _timeSinceLastSpoke = new TimeSince(Time.time);

        Delayed.Do(() => sound.Play(_audioSource), answerDelay);
        Delayed.Do(() => Notification.Show(text, notificationDuration), textDelay);
    }

}