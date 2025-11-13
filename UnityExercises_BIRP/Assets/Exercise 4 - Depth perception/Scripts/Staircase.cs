using System;

public class Staircase
{
    // input parameters
    private float[] stimulusLevels;
    private int maxTrials;
    // state variables
    private int currentIndex; // index for the current stimulus level
    private int trialCount; 
    private int correctCount;
    private int wrongCount;
    private bool fastMode = true; // one-down mode until first wrong answer
    private bool finished;

    public bool IsFinished => finished;


    // Constructor - used to initialize the staircase
    public Staircase(float[] stimulusLevelRange, int maxTrials)
    {
        if (stimulusLevelRange == null || stimulusLevelRange.Length == 0)
            throw new ArgumentException("stimulusLevelRange cannot be empty.");

        this.stimulusLevels = stimulusLevelRange;
        this.maxTrials = maxTrials;

        currentIndex = 0; // start at the first stimulus size, could be optimized probably by starting somewhere in the middle
        trialCount = 0;
        correctCount = 0;
        wrongCount = 0;
        finished = false;
    }

    // Submit a response for the current trial: true for correct, false for incorrect
    public void SubmitResponse(bool correct)
    {
        if (finished) return; // do not process if staircase is finished
        
        trialCount++; // increment trial count

        // TODO: add the staircase logic here:
        // one-down until first mistake, then three-down one-up
        // change the stimulus level (currentStimulusIndex) accordingly
        if (correct)
        {
            correctCount++;

            if (fastMode)
            {
                // one correct is enough to go down until the first mistake
                StepDown();
            }
            else if (correctCount >= 3)
            {
                StepDown();
                correctCount = 0;
            }
        }
        else
        {
            wrongCount++;
            correctCount = 0;

            if (fastMode)
                fastMode = false; // switch to 3-down 1-up mode

            StepUp();

            if (wrongCount >= 4)
                finished = true;
        }

        if (trialCount >= maxTrials)
            finished = true;
    }

    private void StepDown()
    {
        if (currentIndex < stimulusLevels.Length - 1)
            currentIndex++;
    }

    private void StepUp()
    {
        if (currentIndex > 0)
            currentIndex--;
    }

    public float GetStimulusLevel()
    {
        return stimulusLevels[currentIndex];
    }

    public int GetTrialCount()
    {
        return trialCount;
    }
}
