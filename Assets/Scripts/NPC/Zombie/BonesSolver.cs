using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonesSolver : MonoBehaviour
{

    [SerializeField] private BonesExp[] _bones;

    private void LateUpdate()
    {
        foreach (var item in _bones)
        {
            item.Solve();
        }
    }

}
