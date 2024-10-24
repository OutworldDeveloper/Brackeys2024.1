using UnityEngine;

public sealed class JigsawTile : MonoBehaviour
{
    public int Index { get; private set; }
    public Vector2Int CurrentPosition { get; private set; }

    public JigsawTile Init(int index, Vector2Int position)
    {
        Index = index;
        CurrentPosition = position;
        return this;
    }

    public void OnMoved(Vector2Int position)
    {
        CurrentPosition = position;
    }

}
