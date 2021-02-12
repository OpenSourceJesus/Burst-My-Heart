using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveAndLoadValue : Attribute
{
    // public bool appliesToAllAccounts;

    // public SaveAndLoadValue (bool appliesToAllAccounts = false)
    // {
    //     this.appliesToAllAccounts = appliesToAllAccounts;
    // }
}
