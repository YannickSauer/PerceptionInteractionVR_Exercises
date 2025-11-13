using System.Collections;
using UnityEngine;
using System.IO;

public class DepthTest_ExpManager : MonoBehaviour
{

    public string participantAlias = "test";
    public float IPD = 0.064f; // inter-pupillary distance in meters

    public enum TestCondition
    {
        Disparity,
        Size,
        Parallax,
        None
    }
    public TestCondition condition = TestCondition.Disparity;
    public GameObject stimulus; // parent object for all stimuli (rings + background)
    private GameObject[] ringStimuli; // array to hold individual ring stimuli
    private float viewingDistance = 15.0f; // in meters
    private float[] stimulusLevels = new float[] { 40f, 20f, 10f, 8f, 6f, 4f, 2f, 1f, 0.8f, 0.6f, 0.4f, 0.2f, 0.1f, 0.08f, 0.06f, 0.04f, 0.02f}; // stimulus levels in angle of dissparity (arcmin)
    private float horizontalOffset = 2.625f; // horizontal offset of left and right rings from center ring (in meters) at the viewing distance 15 meters
    public int maxTrials = 30;
    public float isiDuration = 0.5f;
    private bool isiActive = false;
    public bool IsiActive => isiActive;

    
    private Staircase staircase;
    private int outlierStimulus; // index 0,1,2 for left, center, right
    private string outputFileName = "";
    private MonoVRRenderer monoEye; // script usef for overwriting eye rendering to simulate mono vision (no binocular disparity)
    
    void Start()
    {
        if(stimulus == null) // GameObject for stimulus not assigned
        {
            Debug.LogError("Stimulus GameObject is not assigned!");
            return;
        }
        ringStimuli = new GameObject[3];
        GameObject ringParent = stimulus.transform.Find("Rings").gameObject;
        ringStimuli[0] = ringParent.transform.Find("Ring_Left").gameObject;
        ringStimuli[1] = ringParent.transform.Find("Ring_Center").gameObject;
        ringStimuli[2] = ringParent.transform.Find("Ring_Right").gameObject;
        stimulus.transform.localPosition = new Vector3(0, 0, viewingDistance);
        
        // default ring positions
        ringStimuli[0].transform.localPosition = new Vector3(-horizontalOffset, 0, 0);
        ringStimuli[1].transform.localPosition = new Vector3(0, 0, 0);
        ringStimuli[2].transform.localPosition = new Vector3(horizontalOffset, 0, 0);
        
        staircase = new Staircase(stimulusLevels, maxTrials);
        SetOutputFileName();
        monoEye = Camera.main.GetComponent<MonoVRRenderer>();


        // TODO:
        // set monoEye.active and other settings based on condition

        // set texture of background to random dot texture
        Texture2D backgroundTexture = TextureGenerator.Randot(8 / 3 * 128, 128);
        stimulus.transform.Find("Background").GetComponent<Renderer>().material.mainTexture = backgroundTexture;
        StartISI(); // start the first "trial break", which will present the first stimulus after the 0.5s break

    }

    
    void Update()
    {
        if (!isiActive) // don't check for response during trial break
            CheckResponse();    
    }

    public void StartISI()
    {
        if (!isiActive)
            StartCoroutine(InterStimulusInterval());
    }

    private IEnumerator InterStimulusInterval()
    {
        isiActive = true;

        // TODO:
        // dummy return: change this part
        yield return 0; 
        // - hide stimulus
        // - wait for isiDuration
        // - show stimulus again
        
        

        isiActive = false;
        PresentStimulus();
    }


    private void PresentStimulus()
    {
        float currentLevel = staircase.GetStimulusLevel();
        float distance = DistanceFromDisparity(currentLevel, viewingDistance);

        // TODO:
        // - get a random outlier position (left, center, right)
        // - set ring position and scaling (based on condition and currentLevel)
        
    }

    float DistanceFromDisparity(float disparityArcmin, float viewingDistanceMeters)
    {
        // TODO: calculate ring offset based on disparity and viewing distance
        float offset = 1f; // dummy offset
        return offset;
    }

    private void CheckResponse()
    {

        
        // TODO: check for user input and evaluate correctness
        return; // dummy return, remove this line when implementing
        // dummy response:
        bool correct = false; // TODO adjust based on user input


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
        StartISI();        // start the trial break
    }

    void SetOutputFileName()
    {
        string outputDir = Path.Combine(Application.dataPath, "..","Measurements");
        if (!Directory.Exists(outputDir)) // create Measurements directory if it doesn't exist
        {
            Directory.CreateDirectory(outputDir);
        }
        outputFileName = $"DepthTest_{participantAlias}_{condition}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(outputDir, outputFileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private void WriteResponseToFile(bool correct)
    {
        // add a new line to the output file with columns for trial, stimulus level, rotation, and correctness
        string outputDir = Path.Combine(Application.dataPath, "..", "Measurements");
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        string line = $"{staircase.GetTrialCount()},{staircase.GetStimulusLevel()},{outlierStimulus},{(correct ? 1 : 0)}\n";
        File.AppendAllText(Path.Combine(outputDir, outputFileName), line);
    }
}