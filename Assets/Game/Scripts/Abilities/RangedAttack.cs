using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D.IK;
using UnityEngine.EventSystems;

public class RangedAttack : CharacterAbility
{
    protected bool curInput;
    [Tooltip("Снаряды используемые при стрельбе")]
    [SerializeField]
    protected GameObject projectile;
    [Tooltip("Точка из которой вылетают снаряды")]
    [SerializeField]
    protected Transform projStartPos;
    [Tooltip("Насколько будет повернут снаряд к горизонту при старте, относительно начального поворота")]
    [SerializeField]
    protected float spawnAngleOffset;
    
    [SerializeField]
    protected float delayBeforeAttack;
    [SerializeField]
    protected float attackTime;
    [SerializeField] 
    protected float delayAfterAttack;
    [SerializeField]
    protected float timeBetweenAttacks;
    [SerializeField]
    protected float baseDamage;
    [SerializeField] 
    protected float baseDamageToEThroughShield;
    [SerializeField]
    protected EffectDescription[] attackEffects;
    [SerializeField]
    protected EffectDescription[] throughShieldEffects;
    [Tooltip("Следует ли использовать инвентарь для получения информации о зарядке оружия или делать это самостоятельно")] 
    [SerializeField]
    protected bool UseInventoryToManageAmmo;
    [Tooltip("Имя оружия для обращения к инвентарю")]
    [SerializeField]
    protected string weaponName;
    [Tooltip("Следует ли при выстреле рассчитывать угола выстрела так, чтобы попасть по заданным координатам?"+
    "Полезно для ботов")]
    [SerializeField]
    protected bool UseBalliscticToTarget;
    [Tooltip("Следует ли рассчитывать баллистику по координатам курсора, если персонажем управляет игрок")]
    [SerializeField]
    protected bool UseCursorToAimBallistic;
    [Tooltip("Если эта настройка включена, то при удержании кнопки выстрела" +
             "Оружие будет прицеливаться до тех пор пока кнопка выстрела не будет " +
             "отпущена")]
    [SerializeField]
    protected bool holdShootToAim;
    [Tooltip("Следует ли управлять скелетом персонажа для наведения на цель")]
    [SerializeField]
    protected bool aimIK;
    [Tooltip("Следует ли наводить оружие всегда или только во время атаки")]
    [SerializeField]
    protected bool aimAlways;
    [Tooltip("Тот солвер, что нам надо наводить")]
    [SerializeField]
    protected Solver2D aimSolver;
    [Tooltip("Расстояние от точки спавна пули до воображаемого центра вращения оружия")]
    [SerializeField]
    protected float projStartPosDistToCenter;
    [SerializeField]
    protected Transform IKtarget;
    [Tooltip("Скорость перемещние IK оружия")]
    [SerializeField]
    protected float IKMoveSpeed;
    [SerializeField]
    protected AdditionalIKPar[] additionalIKTargets;
    
    [Header("Animations")] 
    [SerializeField]
    protected string AttackPreparingParameter = "RangeAttackPreparing";
    [SerializeField]
    protected string AttackingParameter = "RangeAttacking";
    [SerializeField]
    protected string AttackSpeedAnimParameter = "RangeAttackSpeed";
    [SerializeField]
    protected string AttackPreparingSpeedAnimParameter = "RangeAttackPreparingSpeed";
    
    [Tooltip("Фидбэк, вызываемый при выстреле")]
    [SerializeField]
    protected MMFeedbacks shotFeedback;

    public Transform ProjStartPosition
    {
        get
        {
            if (!aimIK) return projStartPos;

            return IKtarget;
        }

    }

    /// <summary>
    /// Координаты по которым будет производится выстрел
    /// </summary>
    protected Vector2 curTargetPos;

    protected ObjectProperty damageProperty;
    protected ObjectProperty damageToEThroughShieldProperty;
    /// <summary>
    /// Следует ли при рассчете баллистической траектори ориентироваться на фазу подъема или на фазу спуска
    /// </summary>
    protected bool useDirectFire = true;

    protected InventoryHandler _inventoryHandler;

    protected float lastAttackTime=-10000;

    private bool shouldAim;

    private bool isAiming;

    protected Transform memorizedSolverTarget;
   /// <summary>
   /// Вектор разницы между начальной позицией старта пуль центром игрока
   /// </summary>
    protected Vector2 projStartPosDiff;
    protected override void PreInitialize()
    {
        base.PreInitialize();
        _inventoryHandler = GetComponent<InventoryHandler>();
        damageProperty=owner.PropertyManager.AddProperty("RangedAttackDamage", baseDamage);
        damageToEThroughShieldProperty =
            owner.PropertyManager.AddProperty("RangeDamageToEThroughShield", baseDamageToEThroughShield);
        projStartPosDiff = projStartPos.position - transform.position;
    }
    private void Update()
    {
        if (curInput && CanAttack())
        {
            StartCoroutine(Attack());
        }

        if (aimIK && AbilityAuthorized)
        {
            AimIK();
        }

        if (aimIK && isAiming && !AbilityAuthorized)
        {
            StopAimIK();
        }
    }

    protected IEnumerator Attack()
    {
        owner.AttackingState.ChangeState(CharacterAttackingState.RangeAttackPreparing);
        
        yield return new WaitForSeconds(delayBeforeAttack);
        shouldAim = true;
        if (holdShootToAim)
        {
            yield return new WaitWhile(()=>curInput);
        }

        owner.AttackingState.ChangeState(CharacterAttackingState.RangeAttacking);
        if (UseInventoryToManageAmmo) _inventoryHandler.Shoot(weaponName);
        
        shotFeedback?.PlayFeedbacks();
        
        SpawnProjectile();
        yield return new WaitForSeconds(attackTime);
        lastAttackTime = Time.time;
        shouldAim = false;
        yield return new WaitForSeconds(delayAfterAttack);
        
        owner.AttackingState.ChangeState(CharacterAttackingState.Idle);
    }

    protected void SpawnProjectile()
    {
        ///Добавляем offset если не надо рассчитывать точный угол
        Quaternion rot = (UseBalliscticToTarget)
            ? projStartPos.rotation
            : Quaternion.Euler(projStartPos.rotation.eulerAngles + new Vector3(0, 0, spawnAngleOffset));
        var proj = Instantiate(projectile, (aimIK)?IKtarget.position:projStartPos.position, 
            rot)
            .GetComponent<Projectile>();
        if (UseBalliscticToTarget)
        {
            if (UseCursorToAimBallistic)
            {
                if (Mouse.current != null)
                {
                    SetTargetPos(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
                }
                else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed && !EventSystem.current.IsPointerOverGameObject())
                {
                    SetTargetPos(Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue()));
                }
            }
            
            proj.SetAngleToPosition(curTargetPos,useDirectFire);
        }

        var eff = proj.GetComponent<EffectOnTouch>();
        eff.SetOwner(owner);
        for (int i = 0; i < attackEffects.Length; i++)
        {
            eff.AddEffect(new Effect(attackEffects[i],owner.PropertyManager));
        }
        for (int i = 0; i < throughShieldEffects.Length; i++)
        {
            eff.AddThroughShieldEffect(new Effect(throughShieldEffects[i],owner.PropertyManager));
        }
        
        proj.enabled = true;
    }

    protected bool CanAttack()
    {
        return IsLoaded() && owner.AttackingState.CurrentState == CharacterAttackingState.Idle && AbilityAuthorized;
    }

    protected bool IsLoaded()
    {
        if (UseInventoryToManageAmmo)
        {
            return _inventoryHandler.CanShoot(weaponName);
        }

        return Time.time - lastAttackTime > timeBetweenAttacks;
    }

    public void ProcessInput(bool input)
    {
        curInput = input;
    }
    /// <summary>
    /// Ввод данных позиции, куда надо стрелять 
    /// </summary>
    /// <param name="target"></param>
    public void SetTargetPos(Vector2 target)
    {
        curTargetPos = target;
    }

    public void SetUseDirectFire(bool flag)
    {
        useDirectFire = flag;
    }

    protected void AimIK()
    {
        if (!(owner.AttackingState.CurrentState == CharacterAttackingState.RangeAttacking
                           || owner.AttackingState.CurrentState == CharacterAttackingState.RangeAttackPreparing
                           || owner.AttackingState.CurrentState == CharacterAttackingState.Idle))
        {
            StopAimIK();
            return;
        }
        
        if ((shouldAim || aimAlways) && !isAiming)
        {
            StartAimIK();
        }
        else if(isAiming && (shouldAim || aimAlways))
        {
            Debug.Log("Aim");
            if (UseCursorToAimBallistic && owner.IsPlayer)
            {
                if (Mouse.current != null)
                {
                    SetTargetPos(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
                }
                else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed && !EventSystem.current.IsPointerOverGameObject())
                {
                    SetTargetPos(Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue()));
                }
            }
            float angle;
            var p = projectile.GetComponent<Projectile>();
            Vector2 curSpawnPosition = projStartPos.position;
            for (int i = 0; i < 4; i++)
            {
                var f = p.CalculateAngleToHitDesignatedPosition(
                    curTargetPos, curSpawnPosition,
                    out angle, useDirectFire);
                if (f)
                {
                    MMDebug.DrawPoint(curSpawnPosition,Color.red, 1);
                    var diff = (Vector2) (projStartPos.right * projStartPosDistToCenter);
                    if (projStartPos.right.x < 0)
                    {
                        angle = 180 - angle;
                        diff = -diff;
                    }
                    
                    diff = diff.MMRotate(angle);
                    curSpawnPosition = (Vector2) projStartPos.position - 
                        (Vector2)(projStartPos.right * projStartPosDistToCenter) + diff;
                    MMDebug.DebugDrawArrow((Vector2) projStartPos.position - 
                                           (Vector2)(projStartPos.right * projStartPosDistToCenter),
                        diff*100,Color.magenta);
                }
            }

            IKtarget.position =
                (Vector2)IKtarget.position + (curSpawnPosition - (Vector2) IKtarget.position).normalized 
                * Mathf.Min(IKMoveSpeed,(curSpawnPosition - (Vector2) IKtarget.position).magnitude);
            for (int i = 0; i < additionalIKTargets.Length; i++)
            {
                additionalIKTargets[i].target.position = IKtarget.position;
            }
        }
        else if((!shouldAim && !aimAlways) && isAiming)
        {
            StopAimIK();
        }
        
    }

    protected void StartAimIK()
    {
        memorizedSolverTarget = aimSolver.GetChain(0).target;
        IKtarget.position = projStartPos.position;
        //aimSolver.constrainRotation = true;
        aimSolver.GetChain(0).target = IKtarget;
        for (int i = 0; i < additionalIKTargets.Length; i++)
        {
            additionalIKTargets[i].memorizedTarget = additionalIKTargets[i].solver.GetChain(0).target;
            additionalIKTargets[i].target.position = projStartPos.position;
            additionalIKTargets[i].solver.GetChain(0).target = additionalIKTargets[i].target;
        }
        isAiming = true;
    }

    protected void StopAimIK()
    {
        aimSolver.GetChain(0).target = memorizedSolverTarget;
        
        for (int i = 0; i < additionalIKTargets.Length; i++)
        {
            additionalIKTargets[i].solver.GetChain(0).target = additionalIKTargets[i].memorizedTarget;
        }
        
        isAiming = false;
        //aimSolver.constrainRotation = false;
    }

    protected override void UpdateAnimator()
    {
        base.UpdateAnimator();
        owner.Animator.SetBool(AttackingParameter, owner.AttackingState.CurrentState == CharacterAttackingState.RangeAttacking);
        owner.Animator.SetBool(AttackPreparingParameter, owner.AttackingState.CurrentState == CharacterAttackingState.RangeAttackPreparing);
        owner.Animator.SetFloat(AttackPreparingSpeedAnimParameter, 1/delayBeforeAttack);
        owner.Animator.SetFloat(AttackSpeedAnimParameter, 1/attackTime);
        
    }

    private void OnDrawGizmos()
    {
        if (UseBalliscticToTarget)
        {
            var p = projectile.GetComponent<Projectile>();
            var f = p.CalculateAngleToHitDesignatedPosition(
                Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), projStartPos.position,
                out float angle,useDirectFire);
            if (f)
            {
                Vector2 curPos = projStartPos.position;
                Vector2 curVelocity;
                if (projStartPos.right.x >= 0)
                {
                    curVelocity = new Vector2(p.InitialVelocity, 0).MMRotate(angle);
                }
                else
                {
                    curVelocity = new Vector2(p.InitialVelocity, 0).MMRotate(180 - angle);
                }

                float step = 0.04f;
                for (int i = 0; i < 50; i++)
                {
                    Vector2 newPos = curPos + curVelocity * step;
                    curVelocity += new Vector2(0, Physics2D.gravity.y * p.RigidBody.gravityScale * step);
                    Gizmos.DrawLine(curPos, newPos);
                    curPos = newPos;
                }
            }
        }
    }
}
[Serializable]
public class AdditionalIKPar
{
    public Solver2D solver;
    public Transform target;
    [NonSerialized]
    public Transform memorizedTarget;
}
