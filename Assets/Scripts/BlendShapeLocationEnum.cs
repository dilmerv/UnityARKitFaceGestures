using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BlendShapeRange {
    public BlendShapeLocationEnum BlendShape = BlendShapeLocationEnum.NoSet;
    public float LowBound = 0;
    public float UpperBound = 0;
    public int DetectionCount = 0;
    public ActionExecutor Action;
}

[Serializable]
public class ActionExecutor 
{
    public string MethodName;
    public float Delay;
}

[Serializable]
public enum BlendShapeLocationEnum {
    NoSet,
    browDown_L,
    browDown_R,
    browInnerUp,
    browOuterUp_L,
    browOuterUp_R,
    cheekPuff,
    cheekSquint_L,
    cheekSquint_R,
    eyeBlink_L,
    eyeBlink_R,
    eyeLookDown_L,
    eyeLookDown_R,
    eyeLookIn_L,
    eyeLookIn_R,
    eyeLookOut_L,
    eyeLookOut_R,
    eyeLookUp_L,
    eyeLookUp_R,
    eyeSquint_L,
    eyeSquint_R,
    eyeWide_L,
    eyeWide_R,
    jawForward,
    jawLeft,
    jawOpen,
    jawRight,
    mouthClose,
    mouthDimple_L,
    mouthDimple_R,
    mouthFrown_L,
    mouthFrown_R,
    mouthFunnel,
    mouthLeft,
    mouthLowerDown_L,
    mouthLowerDown_R,
    mouthPress_L,
    mouthPress_R,
    mouthPucker,
    mouthRight,
    mouthRollLower,
    mouthRollUpper,
    mouthShrugLower,
    mouthShrugUpper,
    mouthSmile_L,
    mouthSmile_R,
    mouthStretch_L,
    mouthStretch_R,
    mouthUpperUp_L,
    mouthUpperUp_R,
    noseSneer_L,
    noseSneer_R,
    tongueOut
}