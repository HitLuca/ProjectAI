using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSequence {

    public class Action {
        public string name;
        public int duration;
        public int waitingTime;
    }

    List<Action> sequence;
    int index = 0;

    public ActionSequence(int totalTime, int minDuration, int maxDuration, int minWaiting, int maxWaiting, string[] actionNames)
    {
        sequence = new List<Action>();
        int currentTime = 0;

        int actionIndex = 0;

        while(currentTime < totalTime)
        {
            Action action = new Action();
            action.duration = Random.Range(minDuration, maxDuration+1);
            action.waitingTime = Random.Range(minWaiting, maxWaiting + 1);
            //action.name = actionNames[Random.Range(0, actionNames.Length)];
            action.name = actionNames[actionIndex];
            sequence.Add(action);
            currentTime += action.duration + action.waitingTime;
            actionIndex = (actionIndex + 1) % actionNames.Length;
        }

        ShuffleActionsList(sequence);
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


    void ShuffleActionsList(List<Action> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Action value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
