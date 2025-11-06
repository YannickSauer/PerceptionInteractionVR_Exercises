using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualAcuityExpManager : MonoBehaviour
{
    public GameObject stimulus;
    public float viewingDistance = 4.0f; // in meters
    public float[] stimulusLevels = new float[] {1.2f, 1.1f, 1.0f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f, 0.2f, 0.1f, 0.0f, -0.1f, -0.2f}; // stimulus levels in logMAR units
    public int maxTrials = 30;
    public string responseFileName = "VisualAcuityResponses.csv";
    private Staircase staircase;
    private int currentRotation; // current rotation index (0-7)
    
    // Start is called before the first frame update
    void Start()
    {
        if(stimulus == null) // GameObject for stimulus not assigned
        {
            Debug.LogError("Stimulus GameObject is not assigned!");
            return;
        }

        stimulus.transform.localPosition = new Vector3(0, 0, viewingDistance);

        staircase = new Staircase(stimulusLevels, maxTrials);
        PresentStimulus();
        
        // remove existing output file with same name
        ClearExistingOutputFile();
    }

    void ClearExistingOutputFile()
    {
        string outputDir = System.IO.Path.Combine(Application.dataPath, "..","Measurements");
        if (!System.IO.Directory.Exists(outputDir)) // create Measurements directory if it doesn't exist
        {
            System.IO.Directory.CreateDirectory(outputDir);
        }
        string filePath = System.IO.Path.Combine(outputDir, responseFileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }
    
    void Update()
    {
        CheckResponse();    
    }

    private void PresentStimulus()
    {
        //TODO: calculate/retrieve the stimulus scale and random rotation and apply them
    }

    float SizeFromLogMAR(float logMAR, float distanceMeters)
    {
        //TODO: implement conversion from logMAR to size in meters at given distance
        float sizeMeters = 1.0f; // dummy size, replace with actual calculation

        return sizeMeters;
    }

    private void CheckResponse()
    {
        // TODO: check for user input (numpad keys 1-8)
        //...
        //...
        
        // check correctness
        bool correct = false; // TODO: replace with actual correctness check
        
        // write response to file
        WriteResponseToFile(correct);
        
        // submit response to staircase
        staircase.SubmitResponse(correct);


        // check if staircase is finished
        if (staircase.IsFinished)
        {
            Debug.Log("Experiment finished.");
            stimulus.SetActive(false);
            return;
        }

        PresentStimulus(); // Present the next stimulus
    }

    private void WriteResponseToFile(bool correct)
    {
        // add a new line to the output file with columns for trial, stimulus level, rotation, and correctness
        string outputDir = System.IO.Path.Combine(Application.dataPath, "..","Measurements");
        if (!System.IO.Directory.Exists(outputDir))
        {
            System.IO.Directory.CreateDirectory(outputDir);
        }

        string line = $"{staircase.GetTrialCount()},{staircase.GetStimulusLevel()},{currentRotation},{(correct ? 1 : 0)}\n";
        System.IO.File.AppendAllText(System.IO.Path.Combine(outputDir, responseFileName), line);
    }
}