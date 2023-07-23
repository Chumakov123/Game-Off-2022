using Cinemachine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class RespawnManager : MonoBehaviour
{
    public UnityEvent OnRespawn;

    [SerializeField] Secondclasshero playerInput;
    [SerializeField] Vector3 spawnPoint;
    [SerializeField] GameObject playerPref;
    [HideInInspector] public Overlay owner;

    [SerializeField] GameObject DeadScreen;
    [SerializeField] PropertyProgressBar healthBar;
    [SerializeField] PropertyProgressBar enduranceBar;
    [SerializeField] BloodScreen bloodScreen;
    [SerializeField] BoltsCounter boltsCounter;
    [SerializeField] EscMenuUI escMenu;
    [SerializeField] PauseGame pauseGame;
    float fixedDeltaTimeDefault;
    bool canRespawn = false;
    public void Initialize()
    {
        //playerInput = new Secondclasshero();
        fixedDeltaTimeDefault = Time.fixedDeltaTime;
        escMenu.OnCryCravenButtonPressed.AddListener(CryCraven);
        owner.player?.OnDead.AddListener(OnPlayerDead);
        if (owner.player != null)
        {
            spawnPoint = owner.player.transform.position;
        }
    }
    public void KeyPressed(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (canRespawn&&!pauseGame.onPause)
            {
                Respawn();
            }
        }
    }
    private void OnPlayerDead(Character character = null)
    {
        canRespawn = true;
        DeadScreen.SetActive(true);
    }
    private void CryCraven()
    {
        if (owner.player != null)
        {
            owner.player.PropertyManager.GetPropertyByName("Health").ChangeCurValue(-100);
        }
    }
    public void Respawn()
    {
        if (owner.player == null)
        {
            canRespawn = false;
            owner.player = Instantiate(playerPref, spawnPoint,new Quaternion(0,0,0,0)).GetComponent<Character>();
            InputManager.instance.UpdateControl(owner.player.gameObject.GetComponent<CharacterControl>());
            owner.player.gameObject.name = "Player";
            owner.player.OnDead.AddListener(OnPlayerDead);
            healthBar.SetOwner(owner.player);
            enduranceBar.SetOwner(owner.player);
            bloodScreen.SetOwner(owner.player);
            boltsCounter.SetOwner(owner.player);
            DeadScreen.SetActive(false);
            pauseGame.playerInput = owner.player.GetComponent<PlayerInput>();
            CameraParams.SetFollowTarget(owner.player.cameraTarget);
            CameraParams.SetOrthoSizeDefault();
            Time.timeScale = 1f;
            Time.fixedDeltaTime = fixedDeltaTimeDefault;
            if (RecoverableObjects.instance != null)
                RecoverableObjects.instance.Recovery();
            OnRespawn?.Invoke();
        }
    }
}
