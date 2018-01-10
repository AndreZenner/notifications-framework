using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleContextFunction : ContextFunction
{
    [Range(0,1)]
    public float ExampleContextRating = .5f;

    // returns context rating c
    public override float Evaluate()
    {
        return ExampleContextRating;
    }
}
