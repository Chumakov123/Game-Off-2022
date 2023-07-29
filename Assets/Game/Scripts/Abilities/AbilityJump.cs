using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class AbilityJump : CharacterAbility
{
    [Header("Jump")]
    [Tooltip("На какую высоту прыгает игрок, если будет зажимать прыжок до конца")]
    public float jumpHeight; //Height of the player's jump
    [Tooltip("Время в секундах, за которое персонаж достигает верхней точки прыжка. " +
             "По сути изменяет гравитацию действующую на персонажа")]
    public float jumpTimeToApex; //Time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force.
    [HideInInspector] public float jumpForce; //The actual force applied (upwards) to the player when they jump.

    [Header("Both Jumps")]
    [Tooltip("Модификатор гравитаци, применяемый в том случае, если игрок перестал нажимать на прыжок." +
             "По сути задает разницу между максимальным прыжком и минимальным")]
    public float jumpCutGravityMult; //Multiplier to increase gravity if the player releases the jump button while still jumping
    [Range(0f, 1)] public float jumpHangGravityMult; //Reduces gravity while close to the apex (desired max height) of the jump
    [Tooltip("Как близко к нулю должна быть скорость персонажа, чтобы считалось, что он находится в верхней точке")]
    public float jumpHangTimeThreshold; //Speeds (close to 0) where the player will experience extra "jump hang". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)
    [Space(0.5f)]
    
    [Tooltip("Если игрок нажмет на прыжок чуть раньше, чем достигнет земли, то если время за которое он нажал входит во " +
             "время буфера, то прыжок будет осуществлен сразу как это станет возможно")]
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime; //Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.
    [Header("Gravity")]
    [Tooltip("Модификатор силы гравитации при падении")]
    public float fallGravityMult; //Multiplier to the player's gravityScale when falling.
    [Tooltip("Максимальная скорость падения. Служит для того, чтобы игрок не разгонялся до запредельно больших скоростей")]
    public float maxFallSpeed; //Maximum fall speed (terminal velocity) of the player when falling.
    [Space(5)]
    [Tooltip("Модификатор силы гравитации при падении и зажатой клавиши вниз")]
    public float fastFallGravityMult; //Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed.
    //Seen in games such as Celeste, lets the player fall extra fast if they wish.
    [Tooltip("Максимальная скорость падения при зажатой клавише вниз")]
    public float maxFastFallSpeed; //Maximum fall speed(terminal velocity) of the player when performing a faster fall.
    [Tooltip("Должен ли персонаж автоматически прыгать при нажатой клавише прыжка")]
    [SerializeField]
    protected bool jumpAlways;
    [Tooltip("Количество выносливости, тратимой при прыжке")]
    [SerializeField]
    protected float enduranceCost;

    [SerializeField] 
    protected MMFeedbacks jumpFeedback;
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

    protected ObjectProperty enduranceProperty;
    public float LastPressedJumpTime { get; private set; }
    protected override void PreInitialize()
    {
        base.PreInitialize();
        rigidbody = owner.RigidBody;
        enduranceProperty = owner.PropertyManager.GetPropertyByName("Endurance");
        owner.CalculateGravityStrength(jumpHeight, jumpTimeToApex);

        //Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
        jumpForce = Mathf.Abs(owner.gravityStrength) * jumpTimeToApex;
        LastPressedJumpTime = -100000;
    }

    private void FixedUpdate()
    { 
        ///Проверяем не нажимали ли мы прыжок недавно
        if ((Time.time - LastPressedJumpTime < jumpInputBufferTime || (jumpAlways && curInput)) && CanJump())
        {
            Jump();
        }
    }

    private void Update()
    {
        if (owner.MovementState.CurrentState == CharacterMovementsStates.Jumping && rigidbody.velocity.y < 0)
        {
            owner.MovementState.ChangeState(CharacterMovementsStates.JumpFalling);
        }
        
        //Handle gravity
        //Ускоряем падение при зажатой клавише вниз
        if (rigidbody.velocity.y < 0 && curMoveInputDir.y < 0)
        {
                owner.SetGravityScale(owner.gravityScale * fastFallGravityMult);
                
                //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, Mathf.Max(rigidbody.velocity.y, -maxFastFallSpeed));
        }
        else if(owner.MovementState.CurrentState == CharacterMovementsStates.Jumping && !curInput)
        {
            //Higher gravity if jump button released
            owner.SetGravityScale(owner.gravityScale * jumpCutGravityMult);
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, Mathf.Max(rigidbody.velocity.y, -maxFallSpeed));
        }
        ///Немного уменьшаем гравитацию, когда мы близки наивысшей точке полета
        else if((owner.MovementState.CurrentState == CharacterMovementsStates.Jumping 
                || owner.MovementState.CurrentState == CharacterMovementsStates.JumpFalling) 
                && Mathf.Abs(rigidbody.velocity.y) < jumpHangTimeThreshold)
        {
            owner.SetGravityScale(owner.gravityScale * jumpHangGravityMult);
        }
        else if (rigidbody.velocity.y < 0)
        {
            //Higher gravity if falling
            owner.SetGravityScale(owner.gravityScale * fallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, Mathf.Max(rigidbody.velocity.y, -maxFallSpeed));
        }
        else
        {
            //Default gravity if standing on a platform or moving upwards
            owner.SetGravityScale(owner.gravityScale);
        }
    }

    protected void Jump()
    {
        {
            
            owner.MovementState.ChangeState(CharacterMovementsStates.Jumping);
            
            enduranceProperty?.ChangeCurValue(-enduranceCost);
            //Ensures we can't call Jump multiple times from one press
            LastPressedJumpTime = Time.time - owner.CoyoteTime;

            #region Perform Jump
            //We increase the force applied if we are falling
            //This means we'll always feel like we jump the same amount 
            //(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
            float force = jumpForce;
            if (rigidbody.velocity.y < 0)
                force -= rigidbody.velocity.y;
            rigidbody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            #endregion
            
            jumpFeedback?.PlayFeedbacks();
        }
    }
    
    protected bool CanJump()
    {
        return owner.IsOnGround && owner.MovementState.CurrentState != CharacterMovementsStates.Jumping
                                && owner.MovementState.CurrentState != CharacterMovementsStates.Dashing
            && !owner.IsTired && AbilityAuthorized;
    }

    public void ProcessInput(bool input)
    {
        if (input && !(curMoveInputDir.y < -0.3))
        {
            LastPressedJumpTime = Time.time;
        }

        curInput = input;
    }

    protected override void UpdateAnimator()
    {
        base.UpdateAnimator();
        owner.Animator.SetBool("Jumping", owner.MovementState.CurrentState == CharacterMovementsStates.Jumping);
        owner.Animator.SetBool("Falling", owner.MovementState.CurrentState == CharacterMovementsStates.JumpFalling);
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

        if (owner != null)
        {
            owner.CalculateGravityStrength(jumpHeight, jumpTimeToApex);

            //Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
            jumpForce = Mathf.Abs(owner.gravityStrength) * jumpTimeToApex;
        }
    }
}
