using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class JigsawPuzzle : Pawn
{

    [SerializeField] private Material _material;
    [SerializeField] private Texture2D _texture;
    [SerializeField] private int _size;
    [SerializeField] private JigsawTile[] _tiles;

    [SerializeField] private float _tileScale = 0.1f;

    [SerializeField] private Ease _ease;

    private int _selection = 0;

    [ContextMenu("Create")]
    private void Create()
    {
        if (_tiles != null)
        {
            foreach (var tile in _tiles)
            {
                if (tile != null)
                {
                    DestroyImmediate(tile.gameObject);
                }
            }
        }

        _tiles = new JigsawTile[_size * _size];

        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int index = x + y * _size;
                _tiles[index] = CreateTile(x, y, index);
            }
        }

        UpdatePositions();
    }

    [ContextMenu("Randomize Order")]
    private void RandomizeOrder()
    {
        for (int i = 0; i < 500; i++)
        {
            var legalMoves = GetLegalMoves();
            var randomMove = legalMoves[UnityEngine.Random.Range(0, legalMoves.Length)];

            TryMove(randomMove);
        }

        UpdatePositions();
    }

    private int[] GetLegalMoves()
    {
        var legalMoves = new List<int>();

        int holeIndex = 0;

        for (int i = 0; i < _tiles.Length; i++)
        {
            if (_tiles[i] == null)
            {
                holeIndex = i;
                continue;
            }
        }

        var holePosition = IndexToPosition(holeIndex);

        TryAdd(new Vector2Int(-1, 0));
        TryAdd(new Vector2Int(0, 1));
        TryAdd(new Vector2Int(1, 0));
        TryAdd(new Vector2Int(0, -1));

        void TryAdd(Vector2Int offset)
        {
            var position = holePosition + offset;

            if (InBounds(position) == true)
            {
                int index = IndexFromPosition(position);
                legalMoves.Add(index);
            }
        }

        return legalMoves.ToArray();
    }

    private Vector3 GetLocalPositionOf(Vector2Int position)
    {
        Vector3 localOrigin = new Vector3(-_size * 0.5f * _tileScale + _tileScale * 0.5f, -_size * 0.5f * _tileScale + _tileScale * 0.5f, 0f);
        return localOrigin + new Vector3(position.x * _tileScale, position.y * _tileScale, 0);
    }

    private void UpdatePositions()
    {
        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                int index = x + y * _size;
                var tile = _tiles[index];

                if (tile != null)
                {
                    tile.transform.localPosition = GetLocalPositionOf(new Vector2Int(x, y));
                }
            }
        }
    }

    private Transform _selectorTransform;

    private void Start()
    {
        _selectorTransform = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        _selectorTransform.localScale = Vector3.one * 0.05f;
    }

    public override void InputTick()
    {
        _selectorTransform.position = 
            transform.TransformPoint(GetLocalPositionOf(IndexToPosition(_selection))) - transform.forward * 0.05f;
        _selectorTransform.gameObject.SetActive(true);

        Vector2Int moveSelection = new Vector2Int()
        {
            x = Input.GetKeyDown(KeyCode.A) ? -1 : Input.GetKeyDown(KeyCode.D) ? 1 : 0,
            y = Input.GetKeyDown(KeyCode.S) ? -1 : Input.GetKeyDown(KeyCode.W) ? 1 : 0,
        };

        var currentPosition = IndexToPosition(_selection);

        if (InBounds(currentPosition + moveSelection) == true)
        {
            _selection = IndexFromPosition(currentPosition + moveSelection);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F))
        {
            JigsawTile tileToMove = _tiles[_selection];

            if (_timeSinceLastMove > 0.2f && TryMove(_selection) == true)
            {
                _timeSinceLastMove = new TimeSince(Time.time);
                tileToMove.transform.DOLocalMove(GetLocalPositionOf(tileToMove.CurrentPosition), 0.195f).SetEase(_ease);
            }
        }
    }

    private TimeSince _timeSinceLastMove;

    private bool TryMove(int index)
    {
        JigsawTile tile = _tiles[index];

        if (tile == null)
            return false;

        Vector2Int position = IndexToPosition(index);
        int destinationIndex = 0;
        bool isLegalMove = false;

        CheckMove(new Vector2Int(-1, 0));
        CheckMove(new Vector2Int(0, 1));
        CheckMove(new Vector2Int(1, 0));
        CheckMove(new Vector2Int(0, -1));

        void CheckMove(Vector2Int dir)
        {
            var potentialDestination = position + dir;

            if (InBounds(potentialDestination) == false)
                return;

            var potentialDestinationIndex = IndexFromPosition(potentialDestination);

            if (_tiles[potentialDestinationIndex] == null)
            {
                destinationIndex = potentialDestinationIndex;
                isLegalMove = true;
                return;
            }
        }

        if (isLegalMove == false)
            return false;

        _tiles[index] = null;
        _tiles[destinationIndex] = tile;
        tile.OnMoved(IndexToPosition(destinationIndex));

        return true;
    }

    private bool InBounds(Vector2Int position)
    {
        return 
            position.x >= 0 && 
            position.x < _size && 
            position.y >= 0 && 
            position.y < _size;
    }

    private int IndexFromPosition(Vector2Int position)
    {
        return position.x + position.y * _size;
    }

    private Vector2Int IndexToPosition(int index)
    {
        return new Vector2Int()
        {
            x = index % _size,
            y = index / _size
        };
    }

    private JigsawTile CreateTile(int x, int y, int index)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(transform, false);
        cube.transform.localScale = new Vector3(_tileScale, _tileScale, 0.05f);
        cube.transform.Rotate(Vector3.up, 180f);
        var meshRenderer = cube.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Instantiate(_material);
        meshRenderer.sharedMaterial.SetInt("_PuzzleSize", _size);
        meshRenderer.sharedMaterial.SetVector("_TileIndex", new Vector2(x, y));
        meshRenderer.sharedMaterial.SetTexture("_PuzzleTexture", _texture);

        return cube.AddComponent<JigsawTile>().Init(index, new Vector2Int(x, y));
    }

}
