using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;


public class FovRendExpManager : MonoBehaviour
{
    public string participantAlias;
    private string outputFileName;
    private FoveatedRendering foveatedRendering;
    private ConstantStimuli stimulusController; // class that decides on next stimulus level for us

    [Header("Stimulus levels")]
    public float[] delayStimulusLevels; // which delays should we test in the experiment
    public int repetitionsPerLevel = 5; // how often should we test each stimulus level
    
    [Header("Timing settings")]

    public float presentationTime = 2f;
    public float interStimInterval = 0.5f;

    private bool runningExperiment = false;
    private Queue<GazeData> gazeBuffer = new Queue<GazeData>(); // used for storing gaze samples that can be choosen for artifical delay stimulation
    private GameObject scene;
    private int participantAnswer; // will store the current answer of the participant


    void Start()
    {
        foveatedRendering = Camera.main.GetComponent<FoveatedRendering>();

        stimulusController = new ConstantStimuli(delayStimulusLevels,repetitionsPerLevel);
        scene = GameObject.Find("Scene");
        if(scene == null)
        {
            Debug.LogError("Scene GameObject not found.");
        }

    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !runningExperiment)
        {
            StartCoroutine(StartExperiment());
        }
    }

    IEnumerator StartExperiment()
    {
        runningExperiment = true;
        Debug.Log("Experiment started");
        yield return null; // dummy yield return

        // TODO:
        // Start trials, until all stimulis are measured
        // After each trial, wait for answer
        // write answer to file
        // submit answer to the stimulusController

        // stop execution of program
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;   // stop play mode
        #else
            Application.Quit();                                // close built game
        #endif    
    }

    IEnumerator PresentTrial(int foveatedInterval,float delay)
    {
        // fov render on with delay

        // TODO: implement the procedure of one trial
        // foveatedInterval is 0 or 1. If 0, then the first intrevall is with foveated rendering
        // present each intervall for 3 seconds
        // deactivate the scene and wait between the intervalls 0.5 seconds
        // deactivate the scene after the trial 
        // to activate/deactive scene: scene.SetActive(true/false)

        // save the response to file

        // to activate fveated rendering: foveatedRendering.active=true
        // simulated delay of foveated rendering: foveatedRendering.SetDelay(float delay)

        yield return null; // dummy yield return
    }
    
    IEnumerator WaitForAnswer()
    {

        // TODO: implement the answer
        // left arrow: first intervall
        // right arrow: second intervall

        yield return null; // dummy yield return
    }

    

}
