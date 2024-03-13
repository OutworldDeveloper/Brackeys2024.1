using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class InspectionPawn : Pawn
{

    public event Action<InspectAction> ActionSelected;

    [SerializeField] private float _inAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve _inEase;
    [SerializeField] private FloatParameter _mouseSensitivity;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private Vector2 _rotationMouseSpeed = new Vector2(2.5f, 3.5f);
    [SerializeField] private Light _light;

    private Item _target;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private TimeSince _timeSinceLastPossess = TimeSince.Never;

    private TargetData _targetData;

    private Tween _introTween;

    private InspectAction[] _targetActions;

    private InspectAction _selectedAction;

    private bool _noAnimation;

    public override bool ShowCursor => true;
    public override bool OverrideCameraPositionAndRotation => true;
    public InspectAction SelectedAction => _selectedAction;

    private void Start()
    {
        _light.enabled = false;
    }

    public void SetTarget(Item target, bool noAnimation = false)
    {
        _target = target;
        _originalPosition = target.transform.position;
        _originalRotation = target.transform.rotation;

        _targetData = new TargetData(target.gameObject);

        _targetActions = target.GetComponentsInChildren<InspectAction>();

        _noAnimation = noAnimation;
    }

    public override void OnPossessed(Player player)
    {
        base.OnPossessed(player);

        _timeSinceLastPossess = TimeSince.Now();

        Vector3 inspectPosition = transform.position + transform.forward * _target.Distance;
        Quaternion inspectRotation =
            Quaternion.AngleAxis(_target.FaceDirection.x, transform.right) *
            Quaternion.AngleAxis(_target.FaceDirection.y, transform.up) *
            Quaternion.AngleAxis(_target.FaceDirection.z, transform.forward) *
            Quaternion.LookRotation(transform.forward, transform.up);

        if (_noAnimation == true)
        {
            _target.transform.position = inspectPosition - transform.up * 0.2f;
            _target.transform.rotation = inspectRotation;
        }

        _introTween = DOTween.Sequence().
            Append(_target.transform.DOMove(inspectPosition, _inAnimationDuration)).
            Join(_target.transform.DORotateQuaternion(inspectRotation, _inAnimationDuration)).
            SetEase(_inEase);      

        _light.enabled = true;

        _targetData.Init();
        _targetData.SetLayers(LayerMask.NameToLayer("Inspected"));

        _target.EnableVisuals();
    }

    public override void OnUnpossessed()
    {
        base.OnUnpossessed();

        _introTween.Kill();

        _target.transform.position = _originalPosition;
        _target.transform.rotation = _originalRotation;

        _light.enabled = false;

        _targetData.RestoreLayers();

        _target.DisableVisuals();

        Debug.Log($"InspectionPawn OnUnpossessed");
    }

    public override void InputTick()
    {
        base.InputTick();

        if (_timeSinceLastPossess < _inAnimationDuration)
            return;

        InspectAction bestAction = null;
        float bestAngle = Mathf.Infinity;

        foreach (var action in _targetActions)
        {
            float angleToAction = Vector3.Angle(-action.transform.forward, transform.forward);

            if (angleToAction < action.MaxAngle && angleToAction < bestAngle)
            {
                bestAction = action;
                bestAngle = angleToAction;
            }
        }

        if (_selectedAction != bestAction)
        {
            _selectedAction = bestAction;
            ActionSelected?.Invoke(_selectedAction);
        }

        float rotX;
        float rotY;
        float rotZ = 0f;

        if (Input.GetKey(KeyCode.Mouse0) == true)
        {
            rotX = -1 * Input.GetAxisRaw("Mouse X") * _rotationMouseSpeed.x * _mouseSensitivity.Value;
            rotY = Input.GetAxisRaw("Mouse Y") * _rotationMouseSpeed.y * _mouseSensitivity.Value;
        }
        else
        {
            rotX = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
            rotY = Input.GetKey(KeyCode.S) ? -1 : Input.GetKey(KeyCode.W) ? 1 : 0;
            rotZ = Input.GetKey(KeyCode.Q) ? -1 : Input.GetKey(KeyCode.E) ? 1 : 0;
        }

        rotX *= _rotationSpeed;
        rotY *= _rotationSpeed;

        _target.transform.rotation =
            Quaternion.AngleAxis(rotX, transform.up) *
            Quaternion.AngleAxis(rotY, transform.right) *
            Quaternion.AngleAxis(-1 * rotZ, transform.forward) *
            _target.transform.rotation;

        if (Input.GetKeyDown(KeyCode.F) == true)
        {
            if (SelectedAction.IsAvaliable(null) == true)
            {
                SelectedAction.Perform(null);
            }
        }
    }

    public override Vector3 GetCameraPosition()
    {
        return transform.position;
    }

    public override Quaternion GetCameraRotation()
    {
        return transform.rotation;
    }

}

public sealed class TargetData
{
  
    private readonly GameObject _target;
    private readonly List<GameObject> _layerTargets = new List<GameObject>();
    private readonly Dictionary<GameObject, int> _originalLayers = new Dictionary<GameObject, int>();

    public TargetData(GameObject target)
    {
        _target = target;
    }

    public void Init()
    {
        Capture(_target);

        void Capture(GameObject gameObject)
        {
            if (gameObject.GetComponent<MeshRenderer>() != null)
            {
                _layerTargets.Add(gameObject);
                _originalLayers.Add(gameObject, gameObject.layer);
            }

            foreach (Transform child in gameObject.transform)
            {
                Capture(child.gameObject);
            }
        }
    }

    public void RestoreLayers()
    {
        foreach (var layerTarget in _layerTargets)
        {
            layerTarget.layer = _originalLayers[layerTarget];
        }
    }

    public void SetLayers(int layer)
    {
        foreach (var layerTarget in _layerTargets)
        {
            layerTarget.layer = layer;
        }
    }

}
