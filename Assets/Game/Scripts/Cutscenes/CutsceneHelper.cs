using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CutsceneHelper : MonoBehaviour
{
    CinemachineVirtualCamera _vcam;
    float _zoom, fixedDeltaTimeDefault;

    public float zoomChangingIntensity;
    public float zoomChangingInterval;

    private void Awake() {
        fixedDeltaTimeDefault = Time.fixedDeltaTime;
    }

    public void SetCameraFollowPlayer()
    {
        if (_vcam==null)
            _vcam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
        _vcam.Follow = Character.player.transform;
    }
    public void SetPlayerInput(bool isActive)
    {
        //Character.player.gameObject.GetComponent<CharacterControl>().Move(new UnityEngine.InputSystem.InputAction.CallbackContext());
        //var input = InputManager.instance.gameObject.GetComponent<PlayerInput>();
        //input.;
        //input.enabled = isActive;
    }
    public void SetCameraZoomSmooth(float zoom)
    {
        if (_vcam==null)
            _vcam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
        _zoom = zoom;
        StartCoroutine(ChangeZoom());
    }

    public void SlowMotion(float duration)
    {
        Time.timeScale = 0.25f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        Invoke("SetTimeScaleDefault",duration);
    }
    void SetTimeScaleDefault()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = fixedDeltaTimeDefault;
    }

    IEnumerator ChangeZoom()
    {
        while(_vcam.m_Lens.OrthographicSize!=_zoom)
        {
            _vcam.m_Lens.OrthographicSize -= Mathf.Sign(_vcam.m_Lens.OrthographicSize-_zoom)*zoomChangingIntensity;
            if (Mathf.Abs(_vcam.m_Lens.OrthographicSize-_zoom)<zoomChangingIntensity)
            {
                _vcam.m_Lens.OrthographicSize=_zoom;
                yield break;
            }
            yield return new WaitForSeconds(zoomChangingInterval);
        }
    }
}
