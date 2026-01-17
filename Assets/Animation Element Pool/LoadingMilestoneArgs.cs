using System;
using System.Collections.Generic;

namespace NamPhuThuy.AnimateWithScripts;

public class LoadingMilestoneArgs : IAnimationArgs
{
    public float initialProgress = 0f;
    public float targetProgress = 1f;
    public float duration = 1f;
    public Action onComplete;
    public List<float> milestones; // Normalized values (0 to 1)
    public AnimationType Type { get; }
    public Action OnComplete { get; set; }
}