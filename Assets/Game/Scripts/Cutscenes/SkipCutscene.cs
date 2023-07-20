using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SkipCutscene : MonoBehaviour
{
    PlayableDirector director;
    SignalReceiver receiver;
    TimelineAsset timeline;
    private bool skipped = false;
    private double skipTime;

    void Start()
    {
        director = GetComponent<PlayableDirector>();
        receiver = GetComponent<SignalReceiver>();
        timeline = director.playableAsset as TimelineAsset;
    }
    private void Skip() {
        //Находим все сигнальные маркеры в таймлайне
        var markers = timeline.markerTrack.GetMarkers();
        foreach(var marker in markers)
        {
            if (marker is SignalEmitter)
            {
                //Вызываем еще не вызванные сигналы
                if (marker.time>skipTime)
                {
                    var asset = (marker as SignalEmitter).asset;
                    receiver.GetReaction(asset)?.Invoke();
                }
            }
        }
    }

    void Update()
    {
        //Пропуск катсцены по нажатию клавиши Enter
        if (!skipped&&Input.anyKeyDown)
        {
            skipped=true;
            skipTime=director.time;
            director.time=director.duration;
            //Задержка перед уничтожением, чтобы director успед изменить позиции подконтрольных объектов
            Invoke("Skip",0.025f);
        }
    }
}
