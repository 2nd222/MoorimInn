using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshTrailStruct
{
    public GameObject Container;

    public MeshFilter BodyMeshFilter;
    public MeshFilter HeadMeshFilter;

    public Mesh bodyMesh;
    public Mesh headMesh;
}

public class GhostEffect : MonoBehaviour
{
    #region Variables & Initializer
    [Header("[PreRequisite]")]
    [SerializeField] private SkinnedMeshRenderer SMR_Body;
    [SerializeField] private SkinnedMeshRenderer SMR_Head;

    private Transform TrailContainer;
    [SerializeField] private GameObject MeshTrailPrefab;
    private List<MeshTrailStruct> MeshTrailStructs = new List<MeshTrailStruct>();

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> posMemory = new List<Vector3>();
    private List<Quaternion> rotMemory = new List<Quaternion>();

    [Header("[Trail Info]")]
    [SerializeField] private int TrailCount;
    [SerializeField] private float TrailGap = 0.2f;
    [SerializeField] [ColorUsage(true,true)] private Color frontColor;
    [SerializeField] [ColorUsage(true, true)] private Color backColor;
    [SerializeField] [ColorUsage(true, true)] private Color frontColor_Inner;
    [SerializeField] [ColorUsage(true, true)] private Color backColor_Inner;
    #endregion

    #region MotionTrail
    void Start()
    {
        TrailContainer = new GameObject("TrailContainer").transform;
        
        for(int i = 0; i < TrailCount; i++)
        {
            // 원하는 TrailCount만큼 생성
            MeshTrailStruct pss = new MeshTrailStruct();
            pss.Container = Instantiate(MeshTrailPrefab, TrailContainer);
            
            // Prefab의 자식 순서에 주의하세요. (0: Body, 1: Head)
            pss.BodyMeshFilter = pss.Container.transform.GetChild(0).GetComponent<MeshFilter>();
            pss.HeadMeshFilter = pss.Container.transform.GetChild(1).GetComponent<MeshFilter>();

            pss.HeadMeshFilter.transform.localPosition += new Vector3(0, 1.7f, 0.333f);
            
            pss.bodyMesh = new Mesh();
            pss.headMesh = new Mesh();

            // 각 mesh에 원하는 skinnedMeshRenderer Bake
            SMR_Body.BakeMesh(pss.bodyMesh);
            SMR_Head.BakeMesh(pss.headMesh);

            // 각 MeshFilter에 알맞은 Mesh 할당
            pss.BodyMeshFilter.mesh = pss.bodyMesh;
            pss.HeadMeshFilter.mesh = pss.headMesh;

            MeshTrailStructs.Add(pss);
            bodyParts.Add(pss.Container);

            // Material 속성 세팅
            float alphaVal = (1f - (float)i / TrailCount) * 0.5f;
            pss.BodyMeshFilter.GetComponent<MeshRenderer>().material.SetFloat("_Alpha", alphaVal);
            pss.HeadMeshFilter.GetComponent<MeshRenderer>().material.SetFloat("_Alpha", alphaVal);

            Color tmpColor = Color.Lerp(frontColor, backColor, (float)i / TrailCount);
            pss.BodyMeshFilter.GetComponent<MeshRenderer>().material.SetColor("_FresnelColor", tmpColor);
            pss.HeadMeshFilter.GetComponent<MeshRenderer>().material.SetColor("_FresnelColor", tmpColor);

            Color tmpColor_Inner = Color.Lerp(frontColor_Inner, backColor_Inner, (float)i / TrailCount);
            pss.BodyMeshFilter.GetComponent<MeshRenderer>().material.SetColor("_BaselColor", tmpColor_Inner);
            pss.HeadMeshFilter.GetComponent<MeshRenderer>().material.SetColor("_BaselColor", tmpColor_Inner);
        }

        StartCoroutine("BakeMeshCoroutine");
    }
    
    IEnumerator BakeMeshCoroutine()
    {
        for (int i = MeshTrailStructs.Count - 2; i >= 0; i--)
        {
            MeshTrailStructs[i + 1].bodyMesh.vertices = MeshTrailStructs[i].bodyMesh.vertices;
            MeshTrailStructs[i + 1].headMesh.vertices = MeshTrailStructs[i].headMesh.vertices;

            MeshTrailStructs[i + 1].bodyMesh.triangles = MeshTrailStructs[i].bodyMesh.triangles;
            MeshTrailStructs[i + 1].headMesh.triangles = MeshTrailStructs[i].headMesh.triangles;
        }

        SMR_Body.BakeMesh(MeshTrailStructs[0].bodyMesh);
        SMR_Head.BakeMesh(MeshTrailStructs[0].headMesh);

        posMemory.Insert(0, transform.position);
        rotMemory.Insert(0, transform.rotation);

        if (posMemory.Count > TrailCount)
            posMemory.RemoveAt(posMemory.Count - 1);
        if (rotMemory.Count > TrailCount)
            rotMemory.RemoveAt(rotMemory.Count - 1);
            
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].transform.position = posMemory[Mathf.Min(i, posMemory.Count - 1)];
            bodyParts[i].transform.rotation = rotMemory[Mathf.Min(i, rotMemory.Count - 1)];
        }

        yield return new WaitForSeconds(TrailGap);
        StartCoroutine("BakeMeshCoroutine");
    }
    #endregion
}