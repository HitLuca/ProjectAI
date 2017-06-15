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

    private void StartTimer(float time)
    {
        timeLeft = time;
        timerTrigger = true;
    }

    private void StopTimer()
    {
        timerTrigger = false;
    }

    private void UpdateTimer()
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

    private void TimerDone()
    {


        if (onTimerDoneListener != null)
            onTimerDoneListener.OnTimerDone();

        //Debug.Log("Done");
        //actionFinished = true;
        //actionSequence.advance();
    }

    
}
