using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer{
    float timeLeft = -1;
    bool timerTrigger = false;
    delegate void TimerCallback();
    TimerCallback timerCallback;

    public interface OnTimerDoneListener
    {
        void OnTimerDone();
    }

    OnTimerDoneListener onTimerDoneListener;

    public void SetOnTimerDoneListener(OnTimerDoneListener onTimerDoneListener)
    {
        this.onTimerDoneListener = onTimerDoneListener;
    }

    public void StartTimer(float time)
    {
        timeLeft = time;
        timerTrigger = true;
    }

    public void StopTimer()
    {
        timerTrigger = false;
    }

    public void UpdateTimer()
    {
        if (timerTrigger)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                timerTrigger = false;
                TimerDone();
            }
        }
    }

    public bool isFinished()
    {
        return !timerTrigger;
    }

    public float getTimeLeft()
    {
        return timeLeft;
    }

    private void TimerDone()
    {


        if (onTimerDoneListener != null)
            onTimerDoneListener.OnTimerDone();

        //Debug.Log("Done");
        //actionFinished = true;
        //actionSequence.advance();
    }

    
}
