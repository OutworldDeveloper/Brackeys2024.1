using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Grip : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;

    [SerializeField] private Material _previewMaterial;
    [SerializeField] private Material _errorMaterial;
    [SerializeField] private LayerMask _placeCheckMask;

    [SerializeField] private Transform _gripTransform;

    private GameObject _previewObject;
    private List<MeshRenderer> _previewRenderers;
    private CharacterModifier _activeModifier;
    private TimeSince _timeSinceLastStateChange = new TimeSince(float.NegativeInfinity);

    public Movable Object { get; private set; }
    public bool IsHolding => Object != null;
    public bool CanPlace { get; private set; }

    public void PickUp(Movable movable)
    {
        if (IsHolding == true)
            return;

        if (_timeSinceLastStateChange < 0.51f)
            return;

        Object = movable;
        _previewRenderers = new List<MeshRenderer>();
        _previewObject = CreatePrieviewObject(movable.gameObject);
        //Object.SetActive(false);

        Object.GetComponent<Movable>().OnPickedUp();

        Object.transform.SetParent(_gripTransform, true);
        Object.transform.DOLocalMove(new Vector3(0.589999974f, -0.889999986f, 0.709999979f), 0.5f);
        Object.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.5f);

        // todo Apply debuff that slows and locks hand interactions
        _character.ApplyModifier(_activeModifier = new CarryingObjectModifier(), -1f);

        _timeSinceLastStateChange = TimeSince.Now();
    }

    public void TryPlace()
    {
        if (_timeSinceLastStateChange < 0.51f)
            return;

        if (CanPlace == false)
            return;

        Object.transform.SetParent(null, true);

        _character.TryRemoveModifier(_activeModifier);
        _character.ApplyModifier(new PlacingObjectModifier(), 0.25f);

        DOTween.Sequence().
            Join(Object.transform.DOMove(_previewObject.transform.position, 0.5f)).
            Join(Object.transform.DORotateQuaternion(_previewObject.transform.rotation, 0.5f)).
            AppendCallback(Object.OnPlaced);

        // todo Apply debuff that doesn't allow movement and interacting

        Object = null;
        Destroy(_previewObject);
        _previewRenderers.Clear();

        _timeSinceLastStateChange = TimeSince.Now();
    }

    private void LateUpdate()
    {
        if (IsHolding == false)
            return;

        // Preview object
        _previewObject.transform.position = transform.position + transform.forward;
        _previewObject.transform.forward = -transform.forward;

        var couldPlace = CanPlace;

        if (Physics.CheckBox(
            _previewObject.transform.position + Object.PlaceCheckOrigin,
            Object.PlaceCheckExtents * 0.5f,
            _previewObject.transform.rotation,
            _placeCheckMask))
        {
            CanPlace = false;
        }
        else
        {
            CanPlace = true;
        }

        if (couldPlace == false && CanPlace == true)
        {
            foreach (var previewRenderer in _previewRenderers)
            {
                previewRenderer.sharedMaterial = _previewMaterial;
            }
        }

        if (couldPlace == true && CanPlace == false)
        {
            foreach (var previewRenderer in _previewRenderers)
            {
                previewRenderer.sharedMaterial = _errorMaterial;
            }
        }
    }

    private GameObject CreatePrieviewObject(GameObject from)
    {
        var preview = new GameObject(from.name);

        if (from.TryGetComponent(out MeshFilter meshFilter) == true)
        {
            var previewFilter = preview.AddComponent<MeshFilter>();
            var previewRenderer = preview.AddComponent<MeshRenderer>();

            previewFilter.sharedMesh = meshFilter.sharedMesh;
            previewRenderer.sharedMaterial = _previewMaterial;

            _previewRenderers.Add(previewRenderer);
        }

        foreach (Transform child in from.transform)
        {
            var previewObject = CreatePrieviewObject(child.gameObject);
            previewObject.transform.SetParent(preview.transform, false);
            previewObject.transform.localPosition = child.localPosition;
            previewObject.transform.localRotation = child.localRotation;
            previewObject.transform.localScale = child.localScale;
        }

        return preview;
    }

    private void OnDrawGizmos()
    {
        if (IsHolding == false)
            return;

        Gizmos.matrix = _previewObject.transform.localToWorldMatrix;
        Gizmos.color = CanPlace ? Color.green : Color.red;
        Gizmos.DrawCube(Object.PlaceCheckOrigin, Object.PlaceCheckExtents);
    }

}

public sealed class CarryingObjectModifier : CharacterModifier
{
    public override bool CanCrouch() => false;
    public override float GetSpeedMultiplier() => 0.45f;

}

public sealed class PlacingObjectModifier : CharacterModifier
{
    public override bool CanInteract() => false;
    public override bool CanCrouch() => false;
    public override float GetSpeedMultiplier() => 0.2f;

}
