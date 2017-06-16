using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WindowsInput;


public class CaptionController : MonoBehaviour
{
    const string MSG_INSTRUCTION = "Please  <color=red>%1</color>  \n by  using  <color=red>%2</color> .";
    const string MSG_RELEASE_KEY = "Please  release  the  key.";
    const string MSG_WAIT = "Wait...";
    const string MSG_FINISH = "Experiment  complete";
    const string MSG_PADDING = "  ";

    const string MSG_SIMULATE_INSTRUCTION = "Please  <color=red>%1</color>";

    //PUBLIC PARAMETERS
    public string filePathBindings;
    public string filePathOutput;
    public int totalTime;
    public int mode; //0 - perform actions instructions, 1 - simulate, 2 - FreePlay
    public float actionActivationProbability;
    public float simulateErrorProbability;

    //ADDITIONAL PARAMETERS
    int minDuration = 2;
    int maxDuration = 5;
    int minWaiting = 0;
    int maxWaiting = 0;
    float actionTime = 3;
    float simulateWaitingTime = 2;

    int simulateMinWaiting = 2;
    int simulateMaxWaiting = 4;

    //INTERNAL VARIABLES

    float timeLeftRelease = -1;
    bool timerTriggerRelease = false;

    float timeLeftWaiting = -1;
    bool timerTriggerWaiting = false;

    ActionSequence actionSequence;
    WorldController worldController;
    Dictionary<string, string[]> keyBindingsCaptions =
            new Dictionary<string, string[]>();
    Dictionary<string, string> freePlayData =
        new Dictionary<string, string>();
    bool shouldReleaseKey = false;
    bool actionFinished = false;

    Timer simulateStartTimer = new Timer();
    Timer simulateActionTimer = new Timer();
    Timer simulateWaitingTimer = new Timer();
    Timer simulateErrorTimer = new Timer();

    private bool isWalkDown = false;
    private bool isRunDown = false;
    private bool isTurnLeftDown = false;
    private bool isTurnRightDown = false;
    private bool isCrouchDown = false;
    private bool isTurningLeft = false;
    private bool isTurningRight = false;
    private bool isRunning = false;

    private bool simulateStartTimerDone = false;
    private bool simulateEnabled = false;

    private bool isSimulatedLMouseDown = false;
    private bool isSimulatedRMouseDown = false;

    //Data collection structure
    class DataEntry
    {
        public string actionName;
        public long messageTimestamp;
        public long downTimestamp;
        public long upTimestamp;

        public DataEntry()
        {
            actionName = "";
        }

        public override string ToString()
        {
            return actionName + " " + messageTimestamp.ToString() + " " + downTimestamp.ToString() + " " + upTimestamp.ToString() + "\n";
        }
    }

    DataEntry currentEntry = new DataEntry();

    //Timer Listeners

    class SimulateStartTimerListener : Timer.OnTimerDoneListener
    {
        CaptionController captionController;

        public SimulateStartTimerListener(CaptionController cp)
        {
            captionController = cp;
        }

        public void OnTimerDone()
        {
            Debug.Log("Start Timer done");
            captionController.currentEntry.downTimestamp = GetCurrentUnixTimestampMillis();
            captionController.simulateStartTimerDone = true;
        }
    }

    class SimulateActionTimerListener : Timer.OnTimerDoneListener
    {
        CaptionController captionController;

        public SimulateActionTimerListener(CaptionController cp)
        {
            captionController = cp;
        }

        public void OnTimerDone()
        {
            Debug.Log("Action Timer done");
            captionController.SimmulateActionRelease(captionController.actionSequence.get().name);

            captionController.simulateEnabled = false;
            captionController.currentEntry.upTimestamp = GetCurrentUnixTimestampMillis();
            captionController.actionSequence.advance();
            captionController.WriteData(captionController.filePathOutput, captionController.currentEntry.ToString());

            captionController.simulateWaitingTimer.StopTimer();
            captionController.simulateWaitingTimer.SetOnTimerDoneListener(new SimulateWaitingTimerListener(captionController));
            captionController.simulateWaitingTimer.StartTimer(captionController.simulateWaitingTime);
        }
    }

    class SimulateWaitingTimerListener : Timer.OnTimerDoneListener
    {
        CaptionController captionController;

        public SimulateWaitingTimerListener(CaptionController cp)
        {
            captionController = cp;
        }

        public void OnTimerDone()
        {
            Debug.Log("Waiting Timer done");
            captionController.actionFinished = true;
        }
    }

    class SimulateErrorTimerListener : Timer.OnTimerDoneListener
    {
        CaptionController captionController;

        public SimulateErrorTimerListener(CaptionController cp)
        {
            captionController = cp;
        }

        public void OnTimerDone()
        {
            captionController.SimmulateActionRelease(captionController.actionName);
            captionController.hasNoAction = false;
            Debug.Log("Error Timer done");
        }
    }

    void Start()
    {
        if (mode == 1)
        {
            actionFinished = true;
            minWaiting = simulateMinWaiting;
            maxWaiting = simulateMaxWaiting;
        }
        
        keyBindingsCaptions = LoadBindings(filePathBindings);

        actionSequence = new ActionSequence(totalTime, minDuration, maxDuration, minWaiting, maxWaiting, keyBindingsCaptions.Keys.ToArray());
        worldController = GameObject.Find("WorldController").GetComponent<WorldController>();
        PrepareForAction(actionSequence.get().name);

        WriteData(filePathOutput, "Start " + GetCurrentUnixTimestampMillis().ToString());
    }

    // Update is called once per frame
    void Update()
    {
        UpdateReleaseTimer();
        UpdateWaitingTimer();

        simulateStartTimer.UpdateTimer();
        simulateActionTimer.UpdateTimer();
        simulateWaitingTimer.UpdateTimer();
        simulateErrorTimer.UpdateTimer();

        if (!actionSequence.isFinished())
            UpdateMessage();

    }
    
    private void UpdateMessage()
    {
        switch (mode)
        {
            case 0: UpdateActionMode(); break;
            case 1: UpdateSimulateMode(); break;
            case 2: UpdateFreePlayMode(); break;
        }
    }


    //***************************
    //Free Play
    //***************************

    private void UpdateFreePlayMode()
    {
        foreach (string actionName in keyBindingsCaptions.Keys)
        {
            if (GetButtonDown(actionName))
            {
                freePlayData[actionName] = GetCurrentUnixTimestampMillis().ToString();
            }
            else if(GetButtonUp(actionName))
            {
                WriteData(filePathOutput, actionName + " " + freePlayData[actionName] + " " + GetCurrentUnixTimestampMillis().ToString());
            }
        }
    }


    //***************************
    //Simmulate Mode
    //***************************

    double threshTick;
    string actionName;
    bool hasNoAction = false;

    private void UpdateSimulateMode()
    {
        ActionSequence.Action action = actionSequence.get();
        string action_msg = action.name.Replace("_", MSG_PADDING);
        string message = MSG_SIMULATE_INSTRUCTION.Replace("%1", action_msg);

        if (!currentEntry.actionName.Equals(action.name))
        {
            currentEntry.actionName = action.name;
            currentEntry.messageTimestamp = GetCurrentUnixTimestampMillis();
        }


        if ((action.name.Equals("Crouch") && !isCrouchDown) || (action.name.Equals("Stand_Up") && !isCrouchDown))
            PrepareForAction(action.name);

        if (actionFinished)
        {
            actionFinished = false;
            simulateStartTimer.StopTimer();
            simulateStartTimer.SetOnTimerDoneListener(new SimulateStartTimerListener(this));
            simulateStartTimer.StartTimer(action.waitingTime);
        }

        if (simulateStartTimerDone)
        {
            simulateStartTimerDone = false;
            simulateEnabled = true;
            simulateActionTimer.StopTimer();
            simulateActionTimer.SetOnTimerDoneListener(new SimulateActionTimerListener(this));
            simulateActionTimer.StartTimer(action.duration);
            threshTick = action.duration*100;
        }

        if (simulateEnabled)
        {
            float errorChoice = 1;
            float errorHasAction = 0;
            if (simulateErrorTimer.isFinished() && Math.Floor(simulateActionTimer.getTimeLeft() * 100) <= threshTick)
            {
                threshTick = Math.Floor(simulateActionTimer.getTimeLeft() * 100) - 10;
                errorChoice = UnityEngine.Random.Range(0.0f, 1.0f);
                errorHasAction = UnityEngine.Random.Range(0.0f, 1.0f);
            }

            if (simulateErrorTimer.isFinished())
            {
                actionName = action.name;
            }

            if (errorChoice > simulateErrorProbability && !hasNoAction)
            {
                Debug.Log(actionName);
                SimmulateAction(actionName);
            }
            else
            {
                if (simulateErrorTimer.isFinished())
                {
                    SimmulateActionRelease(actionName);
                    simulateErrorTimer.SetOnTimerDoneListener(new SimulateErrorTimerListener(this));
                    simulateErrorTimer.StartTimer(Math.Min(simulateActionTimer.getTimeLeft(), UnityEngine.Random.Range(0.1f, 0.5f)));
                    if (errorHasAction < actionActivationProbability)
                    {
                        var list = new List<string>(keyBindingsCaptions.Keys.ToArray());
                        list.Remove(action.name);

                        actionName = list[UnityEngine.Random.Range(0, list.Count - 1)];

                        SimmulateAction(actionName);
                        hasNoAction = false;
                    }
                    else {
                        hasNoAction = true;
                    }

                }
            }

        }

        if (!simulateWaitingTimer.isFinished())
        {
            isWaitMessage = true;
            message = MSG_WAIT + Math.Round(simulateWaitingTimer.getTimeLeft());
        }
        else
        {

            if (isWaitMessage)
            {
                worldController.AnimateTextPosition();
            }
            isWaitMessage = false;
            
        }

        worldController.UpdatePlayerRequestText(message);
    }

    private void SimmulateActionRelease(string actionName)
    {

        if (actionName.Equals("Run"))
        {
            InputSimulator.SimulateKeyUp((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyBindingsCaptions["Walk"][2]));
        }
        InputSimulator.SimulateKeyUp((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyBindingsCaptions[actionName][2]));

        isSimulatedLMouseDown = false;
        isSimulatedRMouseDown = false;
        isCrouchDown = false;
    }

    private void SimmulateAction(string actionName)
    {
        string actionCode = keyBindingsCaptions[actionName][2];

        switch (actionName)
        {
            case "Run":
                if(!IsKeyDown(keyBindingsCaptions["Walk"][2]))
                    InputSimulator.SimulateKeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyBindingsCaptions["Walk"][2]));

                if(!IsKeyDown(actionCode))
                    InputSimulator.SimulateKeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyBindingsCaptions[actionName][2]));
                break;

            case "Destroy_Block":
                if (!isSimulatedLMouseDown)
                {
                    isSimulatedLMouseDown = true;
                    MouseSimulator.ClickRightMouseButton();
                }
                break;

            case "Place_Block":
                if (!isSimulatedRMouseDown)
                {
                    isSimulatedRMouseDown = true;
                    MouseSimulator.ClickLeftMouseButton();
                }
                break;

            case "Crouch":
                if (!worldController.IsPlayerCrouched() && !IsKeyDown(actionCode))
                {
                    isCrouchDown = true;
                    InputSimulator.SimulateKeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyBindingsCaptions[actionName][2]));
                }
                break;
            case "Stand_Up":
                if (worldController.IsPlayerCrouched() && !IsKeyDown(actionCode))
                {
                    isCrouchDown = true;
                    InputSimulator.SimulateKeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyBindingsCaptions[actionName][2]));
                }
                break;
            case "Walk":
                if (!IsKeyDown(actionCode))
                    InputSimulator.SimulateKeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyBindingsCaptions[actionName][2]));
                break;

            default:
                if(!IsKeyDown(actionCode))
                    InputSimulator.SimulateKeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyBindingsCaptions[actionName][2]));
                break;
        }
    }

    bool IsKeyDown(string actionCode)
    {
        return InputSimulator.IsKeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), actionCode));
    }

    //***************************
    //Action Mode
    //***************************
    bool isWaitMessage = false;
    private void UpdateActionMode()
    {
        ActionSequence.Action action = actionSequence.get();
        string action_msg = action.name.Replace("_", MSG_PADDING);
        string message = MSG_INSTRUCTION.Replace("%1", action_msg);

        string binding = "";
        if (!worldController.UsingVR())
            binding = keyBindingsCaptions[action.name][0].Replace("_", MSG_PADDING);
        else
            binding = keyBindingsCaptions[action.name][1].Replace("_", MSG_PADDING);

        message = message.Replace("%2", binding);

        if (!currentEntry.actionName.Equals(action.name))
        {
            currentEntry.actionName = action.name;
            currentEntry.messageTimestamp = GetCurrentUnixTimestampMillis();
        }

        if (timerTriggerRelease)
            message += "..." + (Math.Round(timeLeftRelease)).ToString();

        if (timerTriggerWaiting)
        {
            message = MSG_WAIT + "  " + (Math.Round(timeLeftWaiting)).ToString();
            isWaitMessage = true;
        }
        else
        {
            if (isWaitMessage)
            {
                worldController.AnimateTextPosition();
            }
            isWaitMessage = false;
            if (actionSequence.isFinished())
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
                else
                {

                    if (!actionSequence.isLast() && !timerTriggerRelease && !shouldReleaseKey && actionFinished)
                    {
                        StartWaitingTimer(action.duration);
                        message = "Wait..." + (Math.Round(timeLeftWaiting)).ToString();
                        actionFinished = true;
                        currentEntry = new DataEntry();
                    }

                    if (actionSequence.isLast())
                    {
                        actionSequence.advance();
                        message = "Experiment complete";
                    }
                }
            }
        }

        //Debug.Log("printing");


        worldController.UpdatePlayerRequestText(message);
    }

    //***************************
    //Control Handling
    //***************************

    private void PrepareForAction(string actionName)
    {

        switch (actionName)
        {
            case "Stand_Up":
                if (!worldController.IsPlayerCrouched())
                {
                    worldController.TogglePlayerCrouch();
                }
                break;
            case "Crouch":
                if (worldController.IsPlayerCrouched())
                {
                    worldController.TogglePlayerCrouch();
                }
                break;
            default:
                break;
        }

    }

    private bool GetButtonDown(string actionName)
    {
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
                if (!isRunning && Input.GetAxis(actionName) == 1 && (GetButtonDown("Walk") || GetButton("Walk")))
                {
                    isRunning = true;
                    return true;
                }
                break;
            case "Stand_Up":
                return GetButtonDown("Crouch");
            case "Crouch":
                if (!isCrouchDown && Input.GetAxis("Crouch") == 1)
                {
                    isCrouchDown = true;
                    return true;
                }
                break;
            case "Turn_Left":
                
                if (!isTurningLeft && Input.GetAxis("Mouse X") < 0)
                {
                    isTurningLeft = true;
                    return true;
                }
                break;
            case "Turn_Right":

                if (!isTurningRight && Input.GetAxis("Mouse X") > 0)
                {
                    isTurningRight = true;
                    return true;
                }
                break;
            default:
                return Input.GetButtonDown(actionName);
        }

        return false;
    }

    private bool GetButtonUp(string actionName)
    {
        
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
                if (isRunning && (Input.GetAxis(actionName) <= 0 || GetButtonUp("Walk")))
                {
                    isRunning = false;
                    return true;
                }
                break;
            case "Stand_Up":
                return GetButtonUp("Crouch");
            case "Crouch":
                if (isCrouchDown && Input.GetAxis("Crouch") <= 0)
                {
                    isCrouchDown = false;
                    return true;
                }
                break;
            case "Turn_Left":
                if (isTurningLeft && Input.GetAxis("Mouse X") >= 0)
                {
                    isTurningLeft = false;
                    return true;
                }
                break;
            case "Turn_Right":
                if (isTurningRight && Input.GetAxis("Mouse X") <= 0)
                {
                    isTurningRight = false;
                    return true;
                }
                break;
            default:
                return Input.GetButtonUp(actionName);
        }

        return false;
    }


    private bool GetButton(string actionName)
    {

        switch (actionName)
        {
            case "Walk":
                if (Input.GetAxis("Vertical") > 0)
                {
                    return true;
                }
                break;
            case "Run":
                return Input.GetAxis(actionName) == 1 && GetButton("Walk");
            case "Stand_Up":
                return GetButton("Crouch");
            case "Crouch":
                if (Input.GetAxis("Crouch") == 1)
                {
                    return true;
                }
                break;
            case "Turn_Left":
                if (Input.GetAxis("Mouse X") < 0)
                {
                    return true;
                }
                break;
            case "Turn_Right":
                if (Input.GetAxis("Mouse X") > 0)
                {
                    return true;
                }
                break;
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
        WriteData(filePathOutput, currentEntry.ToString());
        
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

            string[] controls = new string[3];
            controls[0] = entry[1];
            controls[1] = entry[2];
            controls[2] = entry[3];

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
