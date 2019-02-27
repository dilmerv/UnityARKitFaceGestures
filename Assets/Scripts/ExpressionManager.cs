using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using System.Linq;

public class ExpressionManager : MonoBehaviour
{
    public ExpressionConfiguration[] ExpresionConfigurations;
    private bool IsConfigured = false;

    public string FORMAT_DEBUG_TEXT = "({0}){1}(min={2} max={3}) -> val={4}";

    #region ARKIT Type Variables
    private UnityARSessionNativeInterface m_session;
    private Dictionary<string, float> currentBlendShapes;
    private Dictionary<string, Text> currentBlendShapeUIs;

    private bool blendShapesEnabled = false;

    public GameObject expressionIndicator;

    public float delayFor = 1.0f;
    private float delayTimer = 0;

    [SerializeField]
    private GameObject eyePrefab;
    private GameObject eyeLeft, eyeRight;
    private bool toggleEyeColor = false;

    [SerializeField]
    private GameObject headPrefab;
    private GameObject headGameObject;

    [SerializeField]
    private MeshFilter meshFilter;
    private Mesh faceMesh;
    
    #endregion
    void Start()
    {
        m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

		Application.targetFrameRate = 60;
		ARKitFaceTrackingConfiguration config = new ARKitFaceTrackingConfiguration();
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.enableLightEstimation = true;

		if (config.IsSupported)
		{
			m_session.RunWithConfig (config);
			UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
			UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
			UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
		}

        if(ExpresionConfigurations == null || ExpresionConfigurations.Length == 0){
            Debug.Log("You must set at least one expression to use the ExpressionManager.cs");    
        }
        IsConfigured = true;

        CreateDebugOverlays();    

        CleanUp();  

        if(headPrefab != null){
            headGameObject = Instantiate(headPrefab) as GameObject;
        }
        if(eyePrefab != null){
            eyeLeft = Instantiate(eyePrefab) as GameObject;
            eyeRight = Instantiate(eyePrefab) as GameObject;
        }
    }

    void CleanUp()
    {
        foreach (var configuration in ExpresionConfigurations)
        {
            foreach (var range in configuration.BlendShapeRanges)
            {
                range.DetectionCount = 0;
            }
        }
    }
    void CreateDebugOverlays()
    {
        currentBlendShapeUIs = new Dictionary<string, Text>();
        Transform overlay = GameObject.Find("Overlay").transform;
        foreach (var configuration in ExpresionConfigurations)
        {
            foreach (var range in configuration.BlendShapeRanges)
            {
                GameObject rangeGo = new GameObject(range.BlendShape.ToString());
                rangeGo.AddComponent<Text>();
                Text rangeGoText = rangeGo.GetComponent<Text>();
                currentBlendShapeUIs.Add(range.BlendShape.ToString(), rangeGoText);
                rangeGoText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                rangeGoText.color = Color.white;
                rangeGoText.fontSize = 30;
                rangeGoText.text = string.Format(FORMAT_DEBUG_TEXT, 0, range.BlendShape, range.LowBound, range.UpperBound, string.Empty);
                rangeGo.transform.parent = overlay;        
            }
        }
    }

    void FaceAdded (ARFaceAnchor anchorData)
    {
        currentBlendShapes = anchorData.blendShapes;
        blendShapesEnabled = true;
        faceMesh = new Mesh ();
        UpdateMesh(anchorData);
        meshFilter.mesh = faceMesh;
        UpdatePositionAndRotation(anchorData);
    }

    void FaceUpdated (ARFaceAnchor anchorData) 
    {
        currentBlendShapes = anchorData.blendShapes;
        UpdateMesh(anchorData);
        UpdatePositionAndRotation(anchorData);
    }

    private void UpdateMesh(ARFaceAnchor anchorData)
    {
        if(faceMesh != null)
        {
            gameObject.transform.localPosition = UnityARMatrixOps.GetPosition (anchorData.transform);
			gameObject.transform.localRotation = UnityARMatrixOps.GetRotation (anchorData.transform);
            faceMesh.vertices = anchorData.faceGeometry.vertices;
            faceMesh.uv = anchorData.faceGeometry.textureCoordinates;
            faceMesh.triangles = anchorData.faceGeometry.triangleIndices;
            faceMesh.RecalculateBounds();
            faceMesh.RecalculateNormals();
        }
    }

    void FaceRemoved (ARFaceAnchor anchorData) 
    {
        blendShapesEnabled = false;
        meshFilter.mesh = null;
		faceMesh = null;
    }

    private void UpdatePositionAndRotation(ARFaceAnchor anchorData)
    {
        if(headPrefab != null){
            // head position and rotation
            headGameObject.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            headGameObject.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        }

        if(eyePrefab != null){
            // eyes position and rotation
            eyeLeft.transform.position = anchorData.leftEyePose.position;
            eyeLeft.transform.rotation = anchorData.leftEyePose.rotation;

            eyeRight.transform.position = anchorData.rightEyePose.position;
            eyeRight.transform.rotation = anchorData.rightEyePose.rotation;
        }
    }

    void Update()
    {
        if(!IsConfigured || !blendShapesEnabled){
            Debug.Log("Not configured or blendshapes are not enabled");
            return;
        }

        if(delayTimer >= delayFor){
            DetectExpressions();
            delayTimer = 0;
        }
        else {
            delayTimer += Time.deltaTime * 1.0f;
        }
    }

    void DetectExpressions()
    {
        foreach (var configuration in ExpresionConfigurations)
        {
            foreach (var range in configuration.BlendShapeRanges)
            {
                string blendshapeName = range.BlendShape.ToString();
                if(currentBlendShapes.ContainsKey(blendshapeName))
                {
                    Text currentBlendshapeText = currentBlendShapeUIs[blendshapeName];
                    float currentBlendshapeValue = currentBlendShapes [blendshapeName];
                    
                    // offset values by sensitivity
                    float newLower = range.LowBound <= 0 ? 0 : range.LowBound;
                    float newUpper = range.UpperBound <= range.LowBound ? range.LowBound : range.UpperBound;
                    currentBlendshapeText.text = string.Format(FORMAT_DEBUG_TEXT, range.DetectionCount, range.BlendShape, newLower, newUpper, currentBlendshapeValue.ToString());
                    if(currentBlendshapeValue >= newLower && currentBlendshapeValue <= newUpper){
                        currentBlendshapeText.color = Color.red;
                        range.DetectionCount += 1;

                        if(range.Action != null && !string.IsNullOrEmpty(range.Action.MethodName)){
                            Invoke(range.Action.MethodName, range.Action.Delay);
                        }
                    }
                    else
                        currentBlendshapeText.color = Color.white;
                }
            }
            expressionIndicator.SetActive(AreAllSet(configuration));
        }
    }

    private bool AreAllSet(ExpressionConfiguration configuration)
    {
        bool areAllSet = currentBlendShapeUIs.Values.Where(v => v.color == Color.red).Count() == currentBlendShapeUIs.Keys.Count();
        if(configuration.Action != null && !string.IsNullOrEmpty(configuration.Action.MethodName)){
            Invoke(configuration.Action.MethodName, configuration.Action.Delay);
        }
        return areAllSet;
    }

    public void ChangeEyeColor()
    {
        toggleEyeColor = !toggleEyeColor;
        MeshRenderer eyeLeftMeshRenderer = eyeLeft.GetComponent<MeshRenderer>();
        MeshRenderer eyeRightMeshRenderer = eyeRight.GetComponent<MeshRenderer>();

        eyeLeftMeshRenderer.material.color = toggleEyeColor ? Color.black : Color.yellow;
        eyeRightMeshRenderer.material.color = toggleEyeColor ? Color.black : Color.yellow;
    }
}
