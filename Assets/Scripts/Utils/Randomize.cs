using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Randomize
{

    public static int Int(int from, int to)
    {
        return Random.Range(from, to);
    }

    public static float Float(float from, float to)
    {
        return Random.Range(from, to);
    }

    public static bool Bool()
    {
        return Random.Range(0, 2) == 1;
    }

}
