using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityGetDown : CharacterAbility
{
    [Header("GetDown")]
    [Space (5)]
    [Tooltip("Если игрок нажмет на спуск чуть раньше, чем достигнет платформы, то если время за которое он нажал входит во " +
             "время буфера, то спуск с платформы будет осуществлен сразу как это станет возможно")]
    [Range(0.01f, 0.5f)] public float getdownInputBufferTime; //Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.
    [Tooltip("Должен ли персонаж автоматически пропускать платформы при нажатой клавише спуска")]
    [SerializeField]
    protected bool getdownAlways;
    [Tooltip("Количество выносливости, тратимой при спуске")]
    [SerializeField]
    protected float enduranceCost;
    /// <summary>
    /// Последнее направление ввода
    /// </summary>
    protected bool curInput = false;
    /// <summary>
    /// Также сохраняем ввод движения для обработки быстрого падения
    /// в случае нажатия кнопки "вниз" во время прыжка
    /// </summary>
    protected Vector2 curMoveInputDir;
    protected Rigidbody2D rigidbody;
    protected Collider2D collider;

    private HashSet<Collider2D> collidingPlatforms;
    private HashSet<Collider2D> collidingStairways;
    
    private HashSet<Collider2D> ignoringPlatforms;
    private HashSet<Collider2D> ignoringStairways;

    protected ObjectProperty enduranceProperty;
    public float LastPressedGetdownTime { get; private set; }
    protected override void PreInitialize()
    {
        base.PreInitialize();
        rigidbody = owner.RigidBody;
        enduranceProperty = owner.PropertyManager.GetPropertyByName("Endurance");
        LastPressedGetdownTime = -100000;
        collidingPlatforms = new HashSet<Collider2D>();
        collidingStairways = new HashSet<Collider2D>();
        ignoringPlatforms = new HashSet<Collider2D>();
        ignoringStairways = new HashSet<Collider2D>();
        collider = GetComponent<Collider2D>();
    }
    private bool started = false;
    private void FixedUpdate()
    { 
        ///Проверяем не нажимали ли мы спуск недавно
        if ((Time.time - LastPressedGetdownTime < getdownInputBufferTime || (getdownAlways && curInput)) && CanGetdown())
        {
            Getdown();
        }
    }

    private void Update()
    {
        if (started && !owner.BodyInPLatform())
        {
            EndOfGetDown();
        }
    }

    protected void Getdown()
    {
        enduranceProperty?.ChangeCurValue(-enduranceCost);
        //Ensures we can't call GetDown multiple times from one press
        LastPressedGetdownTime = Time.time - owner.CoyoteTime;

        #region Perform Getdown
        //TODO
        started = true;
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Platform"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Stairway"), true);
        foreach (var platform in collidingPlatforms)
        {
            //Debug.Log(platform.gameObject.name);
            ignoringPlatforms.Add(platform);
            Physics2D.IgnoreCollision(collider,platform,true);
        }
        foreach (var stairway in collidingStairways)
        {
            ignoringStairways.Add(stairway);
            //Physics2D.IgnoreCollision(collider,stairway,true);
        }
        owner.MovementState.ChangeState(CharacterMovementsStates.Jumping);

        #endregion
    }
    protected void EndOfGetDown()
    {
        started = false;
        foreach (var platform in ignoringPlatforms)
        {
            Physics2D.IgnoreCollision(collider,platform,false);
        }
        foreach (var stairway in ignoringStairways)
        {
            //Physics2D.IgnoreCollision(collider,stairway,false);
        }
        ignoringPlatforms.Clear();
        ignoringStairways.Clear();
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Platform"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Stairway"), false);
    }
    
    protected bool CanGetdown()
    {
        return !started && owner.IsOnGround && owner.MovementState.CurrentState != CharacterMovementsStates.Jumping
                                && owner.MovementState.CurrentState != CharacterMovementsStates.Dashing
            && !owner.IsTired && (owner.StayOnPlatform || owner.StayOnStairway) && curMoveInputDir.y < -0.3;
    }

    public void ProcessInput(bool input)
    {
        if (input)
        {
            LastPressedGetdownTime = Time.time;
        }

        curInput = input;
    }
    
    public virtual void SetMoveInput(Vector2 input)
    {
        curMoveInputDir = input;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            collidingPlatforms.Add(other.collider);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Stairway"))
        {
            collidingStairways.Add(other.collider);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        //Physics2D.IgnoreCollision(collider,other.collider,false);
        if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            collidingPlatforms.Remove(other.collider);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Stairway"))
        {
            collidingStairways.Remove(other.collider);
        }
    }

    protected void OnValidate()
    {
        if (owner == null)
        {
            GetOwner();
        }
    }
}
