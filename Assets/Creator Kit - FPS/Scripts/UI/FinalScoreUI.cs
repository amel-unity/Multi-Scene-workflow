using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalScoreUI : MonoBehaviour
{
    public static FinalScoreUI Instance { get; private set; }
    
    public Text TargetDestroyed;
    public Text Penalty;
    public Text TimeSpent;
    public Text FinalTime;
    public Text FinalScore;
    
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Display()
    {
        gameObject.SetActive(true);
        
        float time = GameSystem.Instance.RunTime;
        int targetDestroyed = GameSystem.Instance.DestroyedTarget;
        int totalTarget = GameSystem.Instance.TargetCount;
        int missedTarget = totalTarget - targetDestroyed;
        float penaltyAmount = GameSystem.Instance.TargetMissedPenalty * missedTarget;
            
        TargetDestroyed.text = targetDestroyed + "/" + totalTarget;
        TimeSpent.text = time.ToString("N2") + "s";
        Penalty.text = missedTarget +"*"+ GameSystem.Instance.TargetMissedPenalty.ToString("N2")+"s = "+ penaltyAmount.ToString("N2") + "s";
        FinalTime.text = (time + penaltyAmount).ToString("N2") + "s";

        FinalScore.text = GameSystem.Instance.Score.ToString("N");
    }
}
