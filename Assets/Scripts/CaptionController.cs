using System;
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
    public string filePathOutput;
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
    
    ActionSequence actionSequence;
    WorldController worldController;
    Dictionary<string, string[]> keyBindings =
            new Dictionary<string, string[]>();
    bool shouldReleaseKey = false;
    bool actionFinished = false;

    class DataEntry {
        public string actionName;
        public long messageTimestamp;
        public long downTimestamp;
        public long upTimestamp;

        public DataEntry()
        {
            actionName = "";
        }

        public override string ToString() {
            return actionName + " " + messageTimestamp.ToString() + " " + downTimestamp.ToString() + " " + upTimestamp.ToString() + "\n";
        }
    }

    DataEntry currentEntry = new DataEntry();

    void Start()
    {
        //actions = LoadActions(filePathActions);
        keyBindings = LoadBindings(filePathBindings);
        
        actionSequence = new ActionSequence(totalTime, minDuration, maxDuration, keyBindings.Keys.ToArray());
        worldController = GameObject.Find("WorldController").GetComponent<WorldController>();
        PrepareForAction(actionSequence.get().name);
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

        if (!currentEntry.actionName.Equals(action.name))
        {
            currentEntry.actionName = action.name;
            currentEntry.messageTimestamp = GetCurrentUnixTimestampMillis();
        }

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
            if (GetButtonDown(action.name))
            {
                //StopTimer();
                if (!timerTriggerRelease)
                {
                    currentEntry.downTimestamp = GetCurrentUnixTimestampMillis();
                    StartReleaseTimer(actionTime);
                }
            }
            else if (GetButtonUp(action.name))
            {
                currentEntry.upTimestamp = GetCurrentUnixTimestampMillis();
                StopReleaseTimer();
                TimerReleaseDone();
                shouldReleaseKey = false;
            }
            else if (GetButton(action.name))
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
                    WriteData(filePathOutput, currentEntry.ToString());
                    currentEntry = new DataEntry();
                }

                if (actionSequence.isLast())
                {
                    actionSequence.advance();
                    message = "Experiment complete";
                }
            }
        }

        //Debug.Log("printing");
        worldController.UpdatePlayerRequestText(message);
        
    }

    private bool isWalkDown = false;
    private bool isRunDown = false;
    private bool isTurnLeftDown = false;
    private bool isTurnRightDown = false;
    private bool isCrouchDown = false;

    //***************************
    //Control Handling
    //***************************

    private void PrepareForAction(string actionName)
    {

        //special cases:
        //walk - vertical positive
        //run - run + walk

        

        switch (actionName)
        {
            case "Stand_Up":
                if (!worldController.IsPlayerCrouched())
                {
                    Debug.Log(actionName);
                    worldController.TogglePlayerCrouch();
                }
                break;
            default:
                break;
        }

    }

    private bool GetButtonDown(string actionName)
    {

        //special cases:
        //walk - vertical positive
        //run - run + walk

        switch (actionName)
        {
            case "Walk":
                if (!isWalkDown && Input.GetAxis("Vertical") > 0)
                {
                    isWalkDown = true;
                    return true;
                }
                break;
            case "Run":
                return Input.GetButtonDown(actionName) && GetButtonDown("Walk");
            case "Stand_Up":
                return GetButtonDown("Crouch");
            default:
                return Input.GetButtonDown(actionName);
        }

        return false;
    }

    private bool GetButtonUp(string actionName)
    {

        //special cases:
        //walk - vertical positive
        //run - run + walk

        switch (actionName)
        {
            case "Walk":
                if (isWalkDown && Input.GetAxis("Vertical") <= 0)
                {
                    isWalkDown = false;
                    return true;
                }
                break;
            case "Run":
                return Input.GetButtonUp(actionName) && GetButtonUp("Walk");
            case "Stand_Up":
                return GetButtonUp("Crouch");
            default:
                return Input.GetButtonUp(actionName);
        }

        return false;
    }


    private bool GetButton(string actionName)
    {
        //special cases:
        //walk - vertical positive
        //run - run + walk

        switch (actionName)
        {
            case "Walk":
                if (Input.GetAxis("Vertical") > 0)
                {
                    return true;
                }
                break;
            case "Run":
                return Input.GetButton(actionName) && GetButton("Walk");
            case "Stand_Up":
                return GetButton("Crouch");
            default:
                return Input.GetButton(actionName);
        }

        return false;
    }

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
        PrepareForAction(actionSequence.get().name);
    }

    //***************************
    //BINDINGS AND ACTIONS READING
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
                            if (!entries[0][0].Equals('/'))
                            {
                                Debug.Log("whatever");
                                parsed.Add(entries);
                            }
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

    

    private void WriteData(string filePath, string data)
    {

        // Write the string to a file.
        System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true);
        file.WriteLine(data);

        file.Close();
    }

    private static readonly DateTime UnixEpoch =
    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long GetCurrentUnixTimestampMillis()
    {
        return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
    }

    public static DateTime DateTimeFromUnixTimestampMillis(long millis)
    {
        return UnixEpoch.AddMilliseconds(millis);
    }

    public static long GetCurrentUnixTimestampSeconds()
    {
        return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
    }

    public static DateTime DateTimeFromUnixTimestampSeconds(long seconds)
    {
        return UnixEpoch.AddSeconds(seconds);
    }
}
