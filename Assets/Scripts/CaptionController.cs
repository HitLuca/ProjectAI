using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CaptionController : MonoBehaviour
{
    const string MESSAGE = "Please  <b>%1</b>  by  pressing  <b>%2</b>.";

    const string ACTION_NONE = "none";
    const string ACTION_WALK = "walk";

    float action_time = 3;

    float timeLeft = -1;
    bool timerTrigger = false;
    string currentAction = ACTION_NONE;

    ActionController PlayerScript;

    public bool VREnabled;
    public string filePathActions;
    public string filePathBindings;
    ArrayList actions;
    int currentActionIndex = 0;
    Dictionary<string, string> keyBindings =
            new Dictionary<string, string>();

    // Use this for initialization3
    void Start()
    {
        actions = LoadActions(filePathActions);
        keyBindings = LoadBindings(filePathBindings);

        if (VREnabled)
        {
            PlayerScript = GameObject.Find("OVRPlayerController").GetComponent<ActionController>();
        }
        else
        {
            PlayerScript = GameObject.Find("FPSController").GetComponent<ActionController>();
        }
    }

    // Update is called once per frame
    void Update()
    {

        UpdateTimer();
        UpdateMessage();

    }

    private void UpdateMessage()
    {

        string action = actions[currentActionIndex].ToString();
        string action_msg = action.Replace('_', ' ');
        string message = MESSAGE.Replace("%1", action_msg);
        message = message.Replace("%2", keyBindings[action]);

        if (timerTrigger)
            message = "Hold on..." + (System.Math.Round(timeLeft)).ToString();

        //string keyCode = GetKeyCode(entry[1]);
        if (Input.GetKeyDown(GetKeyCode(keyBindings[action])))
        {
            StopTimer();
            StartTimer(action_time);
        }
        else if (Input.GetKeyUp(GetKeyCode(keyBindings[action])))
        {
            StopTimer();
        }


        PlayerScript.UpdateText(message);

        Debug.Log(message);
    }



    private KeyCode GetKeyCode(string keyStr)
    {
        KeyCode keyCode = KeyCode.W;
        switch (keyStr)
        {
            case "W":
                keyCode = KeyCode.W;
                break;
            case "A":
                keyCode = KeyCode.A;
                break;
            case "D":
                keyCode = KeyCode.D;
                break;
            case "Q":
                keyCode = KeyCode.E;
                break;
            case "E":
                keyCode = KeyCode.E;
                break;
            case "Z":
                keyCode = KeyCode.Z;
                break;
            case "Spacebar":
                keyCode = KeyCode.Space;
                break;
        }

        return keyCode;
    }

    //***************************
    //TIMER
    //***************************


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
        Debug.Log("Done");
        currentActionIndex++;
    }


    //***************************
    //ACTION FILE READING
    //***************************

    private Dictionary<string, string> LoadBindings(string fileName)
    {
        ArrayList parsed = Load(fileName);
        Dictionary<string, string> messages = new Dictionary<string, string>();
        
        foreach (string[] entry in parsed)
        {
            string action = entry[0];
            string action_msg = action.Replace('_', ' ');
            //entry.RemoveAt(0);
            //String.Join(" ", entry)


            messages.Add(action, entry[1]);
        }

        return messages;
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
        // If anything broke in the try block, we throw an exception with information
        // on what didn't work
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
