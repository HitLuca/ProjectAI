using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSequence{

    public class Action {
        public string name;
        public int duration;
    }

    List<Action> sequence;
    int index = 0;

    public ActionSequence(int totalTime, int minDuration, int maxDuration, string[] actionNames)
    {
        sequence = new List<Action>();
        int currentTime = 0;

        while(currentTime < totalTime)
        {
            Action action = new Action();
            action.duration = Random.Range(minDuration, maxDuration+1);
            action.name = actionNames[Random.Range(0, actionNames.Length)];
            sequence.Add(action);
            currentTime += action.duration;
        }
    }

    public Action get()
    {
        if (isFinished())
            return null;

        return sequence[index];
    }

    public void advance()
    {
         index++;
    }

    public Action next()
    {
        if (isFinished())
            return null;
        return sequence[index++];

    }

    public void reset()
    {
        index = 0;
    }

    public bool isFinished()
    {
        return index >= sequence.Count;
    }

    public bool isLast()
    {
        return index == sequence.Count-1;
    }
}
