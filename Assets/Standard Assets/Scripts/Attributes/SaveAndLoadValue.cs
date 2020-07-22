using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveAndLoadValue : Attribute
{
    public bool appliesOnlyToAccountThatSaved;

    public SaveAndLoadValue (bool appliesOnlyToAccountThatSaved = true)
    {
        this.appliesOnlyToAccountThatSaved = appliesOnlyToAccountThatSaved;
    }
}
