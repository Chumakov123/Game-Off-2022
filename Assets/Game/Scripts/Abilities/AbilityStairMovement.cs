using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityStairMovement : CharacterAbility
{
    [Header("GetStairMovement")]
    /// Также сохраняем ввод движения для обработки быстрого падения
    /// в случае нажатия кнопки "вниз" во время прыжка
    /// </summary>
    protected Vector2 curMoveInputDir;
    protected Rigidbody2D rigidbody;
    protected override void PreInitialize()
    {
        base.PreInitialize();
        rigidbody = owner.RigidBody;
    }

    private void Update()
    {
        if (owner.IsPlayer)
        {
            /*Debug.Log(!owner.IsOnGroundReal && owner.RigidBody.velocity.y > 0);
            if ((!owner.IsOnGroundReal && owner.RigidBody.velocity.y > 0) || owner.IsOnGround && !owner.StayOnStairway)
            {
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Stairway"),
                    true);
            }
            else
            {
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Stairway"),
                    false);
            }*/

            if (!(owner.StayOnPlatform || owner.StayOnGround) || curMoveInputDir.y > 0.4)
            {
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Stairway"),
                    false);
            }
            else
            {
                if (!owner.StayOnStairway)
                {
                    
                    Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Stairway"),
                        true);
                }
            }

            if (owner.StayOnStairway && curMoveInputDir.y < -0.4)
            {
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Platform"),
                    true);
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Stairway"),
                    false);
            }
            else
            {
                if (!owner.BodyInPLatform())
                    Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Platform"),
                        false);
            }
        }
    }
    public virtual void SetMoveInput(Vector2 input)
    {
        curMoveInputDir = input;
    }

    protected void OnValidate()
    {
        if (owner == null)
        {
            GetOwner();
        }
    }
}
