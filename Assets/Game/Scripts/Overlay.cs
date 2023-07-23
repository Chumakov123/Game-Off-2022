using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class Overlay : MonoBehaviour
{
    [HideInInspector] public Character player;
    PauseGame _pauseGame;
    RespawnManager _respawnManager;
    public BloodScreen bloodScreen;
    public BoltsCounter boltsCounter;

    private void Start()
    {
        _respawnManager = GetComponent<RespawnManager>();
        _pauseGame = GetComponent<PauseGame>();
        _respawnManager.owner = this;
        _pauseGame.owner = this;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
        _pauseGame.Initialize();
        _respawnManager.Initialize();
        bloodScreen.SetOwner(player);
        boltsCounter.SetOwner(player);
        _pauseGame.playerInput = player.GetComponent<PlayerInput>();
        var bars = GetComponentsInChildren<PropertyProgressBar>();
        foreach (var x in bars)
        {
            x.SetOwner(player);
        }
        InputManager.instance.UpdateControl(player.gameObject.GetComponent<CharacterControl>());
        InputManager.instance.SubscribePause(_pauseGame);
        InputManager.instance.SubscribeRespawn(_respawnManager);
    }
}
