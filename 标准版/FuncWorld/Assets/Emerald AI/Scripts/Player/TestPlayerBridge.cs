using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI;

public class TestPlayerBridge : EmeraldPlayerBridge
{
    public override void Start()
    {
        //You should set the StartHealth and Health variables equal to that of your character controller here.
    }

    public override void DamageCharacterController(int DamageAmount, Transform Target)
    {
        //The code for damaging your character controller should go here.
    }

    public override bool IsAttacking()
    {
        //Used for detecting when this target is attacking.
        return false;
    }

    public override bool IsBlocking()
    {
        //Used for detecting when this target is blocking.
        return false;
    }

    public override bool IsDodging()
    {
        //Used for detecting when this target is dodging.
        return false;
    }

    public override void TriggerStun(float StunLength)
    {
        //Custom trigger mechanics can go here, but are not required
    }
}
