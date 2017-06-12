using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CaptionController : MonoBehaviour
{
    const string MSG_INSTRUCTION = "Please  <b>%1</b>  by  pressing  <b>%2</b>.";
    const string MSG_RELEASE_KEY = "Please  release  the  key.";
    const string MSG_WAIT = "Wait...";
    const string MSG_FINISH = "Experiment  complete";

//PUBLIC PARAMETERS
    public string filePathActions;
    public string filePathBindings;
    public int totalTime;

//ADDITIONAL PARAMETERS
    int minDuration = 2;
    int maxDuration = 5;
    float actionTime = 3;

//INTERNAL VARIABLES
    float timeLeftRelease = -1;
    bool timerTriggerRelease = false;

    float timeLeftWaiting = -1;
    bool timerTriggerWaiting = false;

    ActionController playerScript;
    
    ActionSequence actionSequence;
    WorldController worldController;
    Dictionary<string, string[]> keyBindings =
            new Dictionary<string, string[]>();
    bool shouldReleaseKey = false;
    bool actionFinished = false;

    void Start()
    {
        //actions = LoadActions(filePathActions);
        keyBindings = LoadBindings(filePathBindings);
        
        actionSequence = new ActionSequence(totalTime, minDuration, maxDuration, keyBindings.Keys.ToArray());
        worldController = GameObject.Find("WorldController").GetComponent<WorldController>();
        

        if (worldController.UsingVR())
        {
            playerScript = GameObject.Find("OVRPlayerController").GetComponent<ActionController>();
        }
        else
        {
            playerScript = GameObject.Find("FPSController").GetComponent<ActionController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateReleaseTimer();
        UpdateWaitingTimer();

        if (!actionSequence.isFinished())
            UpdateMessage();
    }

    private void UpdateMessage()
    {
        ActionSequence.Action action = actionSequence.get();
        string action_msg = action.name.Replace('_', ' ');
        string message = MSG_INSTRUCTION.Replace("%1", action_msg);

        string binding = "";
        if (!worldController.UsingVR())
            binding = keyBindings[action.name][0].Replace('_', ' ');
        else
            binding = keyBindings[action.name][1].Replace('_', ' ');

        message = message.Replace("%2", binding);

        if (timerTriggerRelease)
            message += "..." + (System.Math.Round(timeLeftRelease)).ToString();

        if (timerTriggerWaiting)
            message = MSG_WAIT + "  " + (System.Math.Round(timeLeftWaiting)).ToString();
        else if (actionSequence.isFinished())
        {
            message = MSG_FINISH;
        }
        else
        {
            //string keyCode = GetKeyCode(entry[1]);
            if (Input.GetButtonDown(action.name))
            {
                //StopTimer();
                if(!timerTriggerRelease)
                    StartReleaseTimer(actionTime);
            }
            else if (Input.GetButtonUp(action.name))
            {
                StopReleaseTimer();
                TimerReleaseDone();
                shouldReleaseKey = false;
            }
            else if (Input.GetButton(action.name))
            {
                if (!timerTriggerRelease)
                {
                    shouldReleaseKey = true;
                    message = MSG_RELEASE_KEY;
                }
            }
            else {

                if (!actionSequence.isLast() && !timerTriggerRelease && !shouldReleaseKey && actionFinished)
                {
                    StartWaitingTimer(action.duration);
                    message = "Wait..." + (System.Math.Round(timeLeftWaiting)).ToString();
                    actionFinished = true;
                }

                if (actionSequence.isLast())
                {
                    actionSequence.advance();
                    message = "Experiment complete";
                }
            }
        }
        
        worldController.UpdatePlayerRequestText(message);
        
    }

    //private GetButtonDown(string name)
    //{
    //    Input.GetButtonDown(name);
    //}
    


    //***************************
    //TIMER
    //***************************

    private void StartReleaseTimer(float time)
    {
        timeLeftRelease = time;
        timerTriggerRelease = true;
    }

    private void StopReleaseTimer()
    {
        timerTriggerRelease = false;
    }

    private void UpdateReleaseTimer()
    {
        if (timerTriggerRelease)
        {
            timeLeftRelease -= Time.deltaTime;
            if (timeLeftRelease < 0)
            {
                timerTriggerRelease = false;
                TimerReleaseDone();
            }
        }
    }

    private void TimerReleaseDone()
    {
        actionFinished = true;
        actionSequence.advance();
    }



    private void StartWaitingTimer(float time)
    {
        timeLeftWaiting = time;
        timerTriggerWaiting = true;
    }

    private void StopWaitingTimer()
    {
        timerTriggerWaiting = false;
    }

    private void UpdateWaitingTimer()
    {
        if (timerTriggerWaiting)
        {
            timeLeftWaiting -= Time.deltaTime;
            if (timeLeftWaiting < 0)
            {
                timerTriggerWaiting = false;
                TimerWaitingDone();
            }
        }
    }

    private void TimerWaitingDone()
    {
        actionFinished = false;
    }

    //***************************
    //ACTION FILE READING
    //***************************

    private Dictionary<string, string[]> LoadBindings(string fileName)
    {
        ArrayList parsed = Load(fileName);
        Dictionary<string, string[]> bindings = new Dictionary<string, string[]>();
        
        foreach (string[] entry in parsed)
        {
            string action = entry[0];
            string action_msg = action.Replace('_', ' ');
            //entry.RemoveAt(0);
            //String.Join(" ", entry)

            string[] controls = new string[2];
            controls[0] = entry[1];
            controls[1] = entry[2];

            bindings.Add(action, controls);
        }

        return bindings;
    }

    private ArrayList LoadActions(string fileName)
    {
        ArrayList parsed = Load(fileName);
        ArrayList actions = new ArrayList();
        foreach (string[] entry in parsed)
        {
            actions.Add(entry[0]);
        }

        return actions;
    }

    private ArrayList Load(string fileName)
    {
        FileInfo file = new FileInfo(fileName);

        ArrayList parsed = new ArrayList();
        // Handle any problems that might arise when reading the text
        try
        {
            string line;
            StreamReader theReader = new StreamReader(file.FullName);
            using (theReader)
            {
                do
                {
                    line = theReader.ReadLine();
                    if (line != null)
                    {
                        string[] entries = line.Split(' ');
                        if (entries.Length > 0)
                        {
                            parsed.Add(entries);
                        }

                    }
                }
                while (line != null);
                theReader.Close();
                return parsed;
            }
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine("{0}\n", e.Message);
            return null;
        }
    }

    //***************************
    //DATA WRITING
    //***************************


    private void WriteData()
    {

    }
}
