using System;
using UnityEngine;

public class ConstantStimuli
{
    // input parameters
    public float[] StimulusLevels { get; private set; } // unique stimulus levels to be tested
    public int RepetitionsPerLevel { get; private set; } // how often each stimulus level?
    public bool RandomizeOrder { get; private set; } // should the order be randomized

    // internal state variables
    public float[] TrialSequence { get; private set; } // 
    public int TrialIndex { get; private set; }
    public bool IsFinished { get; private set; }
    public int TotalTrials => TrialSequence.Length;

    public ConstantStimuli(float[] stimulusLevels, int repetitionsPerLevel, bool randomizeOrder = true)
    {
        if (stimulusLevels == null || stimulusLevels.Length == 0)
            throw new ArgumentException("stimulusLevels cannot be empty.");

        if (repetitionsPerLevel <= 0)
            throw new ArgumentException("repetitionsPerLevel must be ≥ 1.");

        StimulusLevels = stimulusLevels;
        RepetitionsPerLevel = repetitionsPerLevel;
        RandomizeOrder = randomizeOrder;

        BuildTrialSequence();

        TrialIndex = 0;
        IsFinished = false;
    }

    private void BuildTrialSequence()
    {
        int total = StimulusLevels.Length * RepetitionsPerLevel;
        TrialSequence = new float[total];


        // TODO: create trial sequence
        // every stimulus level should be present
        // according to the RepetitionsPerLevel
        // for this purpose fill the Array "TrialSequence"

        // optional shuffle
        if (RandomizeOrder)
            ShuffleArray(TrialSequence);
    }

    private void ShuffleArray(float[] array)
    {

        // TODO: shuffle array e.g. Fisher Yates Shuffle
    }

    public float GetStimulusLevel()
    {
        // TODO: return the stimulus level

        return -1f; // dummy return
    }

    public void SubmitResponse(bool correct)
    {
        // TODO: handle submitted response
        // increase trial index
        // check if finished
    }

    public int GetTrialNumber() => TrialIndex;
}
