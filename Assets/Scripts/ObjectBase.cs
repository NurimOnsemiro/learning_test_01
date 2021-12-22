using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectBase : MonoBehaviour
{
    //INFO: 이동 속도
    public float moveSpeed = 1.0f;
    //INFO: 좌우 회전 속도
    public float rightRotateSpeed = 10.0f;
    //INFO: 상하 회전 속도
    public float upRotateSpeed = 10.0f;
    //INFO: 바운딩 박스의 점들
    private GameObject[] boxPoints;
    //INFO: 바운딩 박스 꼭지점 개수
    private const int numBoxPoints = 8;
    //INFO: 화면에 표출되는 점들 이미지
    public GameObject[] boxImgs;
    //INFO: 점들 색상
    public Color boxColor;
    //INFO: 스크린에서의 사각형 정보
    public Rect rectInfo;
    //INFO: 라벨 번호
    public int label;
    //INFO: 이미지 캡처 중인지 여부
    private bool isChecking = false;
    //INFO: 최소 거리
    public float minDist = 5f;
    //INFO: 최대 거리
    public float maxDist = 40f;
    //INFO: 박스 점 표출 여부
    public static bool showDots = false;

    private void Awake()
    {
        boxPoints = new GameObject[numBoxPoints];
        for (int i = 0; i < numBoxPoints; i++)
        {
            boxPoints[i] = new GameObject();
        }

        BoxCollider boxCol = GetComponent<BoxCollider>();
        //INFO: 바운딩 박스의 크기
        Vector3 boxSize = boxCol.size;
        //INFO: 바운딩 박스의 중심점
        Vector3 centerPos = boxCol.transform.position;

        //INFO: 박스길이의 반길이
        float halfX = boxSize.x * 0.5f;
        float halfY = boxSize.y * 0.5f;
        float halfZ = boxSize.z * 0.5f;

        boxPoints[0].transform.position = new Vector3(centerPos.x - halfX, centerPos.y + halfY, centerPos.z - halfZ);
        boxPoints[1].transform.position = new Vector3(centerPos.x + halfX, centerPos.y + halfY, centerPos.z - halfZ);
        boxPoints[2].transform.position = new Vector3(centerPos.x - halfX, centerPos.y - halfY, centerPos.z - halfZ);
        boxPoints[3].transform.position = new Vector3(centerPos.x + halfX, centerPos.y - halfY, centerPos.z - halfZ);
        boxPoints[4].transform.position = new Vector3(centerPos.x - halfX, centerPos.y + halfY, centerPos.z + halfZ);
        boxPoints[5].transform.position = new Vector3(centerPos.x + halfX, centerPos.y + halfY, centerPos.z + halfZ);
        boxPoints[6].transform.position = new Vector3(centerPos.x - halfX, centerPos.y - halfY, centerPos.z + halfZ);
        boxPoints[7].transform.position = new Vector3(centerPos.x + halfX, centerPos.y - halfY, centerPos.z + halfZ);

        for (int i = 0; i < numBoxPoints; i++)
        {
            boxPoints[i].transform.parent = transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (showDots == false)
        {
            for (int i = 0; i < boxImgs.Length; i++)
            {
                boxImgs[i].SetActive(false);
            }
        }

        StartCoroutine(ChangeSpeedTimer());
        StartCoroutine(ChangeDistTimer());
    }

    private IEnumerator ChangeSpeedTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);
            if (isChecking)
            {
                continue;
            }
            rightRotateSpeed = Random.Range(30f, 100f);
            upRotateSpeed = Random.Range(30f, 100f);
        }
    }

    private IEnumerator ChangeDistTimer()
    {
        ChangeDirection();
        ChangeDist();

        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            if (isChecking)
            {
                continue;
            }

            ChangeDist();
        }
    }

    public void ChangeDist()
    {
        //INFO: 객체 거리 랜덤
        //INFO: Z 거리 범위
        float randomZ = Random.Range(minDist, maxDist);
        //INFO: 카메라는 60도 지만 반으로 나누면 30도
        float maxXRange = randomZ * Mathf.Tan(Mathf.Deg2Rad * 30);
        float randomX = Random.Range(-maxXRange, maxXRange);
        float maxYRange = randomZ * Mathf.Tan(Mathf.Deg2Rad * 30);
        float randomY = Random.Range(-maxYRange, maxYRange);

        transform.position = new Vector3(randomX, randomY, randomZ);
    }

    //INFO: 임의의 방향으로 변경
    public void ChangeDirection()
    {
        transform.rotation = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
    }

    public int GetLabel()
    {
        return label;
    }

    public void SetRectInfo(Rect rect)
    {
        rectInfo = rect;
        if(rect.width < GameManager.minBoxSize || rect.height < GameManager.minBoxSize)
        {
            if (boxColor == Color.red) return;
            boxColor = Color.red;
            Debug.Log("box color is changed to red");
        }
        else
        {
            if (boxColor == Color.yellow) return;
            boxColor = Color.yellow;
            Debug.Log("box color is changed to yellow");
        }

        for(int i = 0; i < boxImgs.Length; i++)
        {
            boxImgs[i].GetComponent<Image>().color = boxColor;
        }
    }

    //INFO: 사각형이 화면 내에 점이 하나라도 있는지 검사
    public bool CheckRectInScreen()
    {
        float minBoxSize = GameManager.minBoxSize;
        Rect rect = rectInfo;
        //INFO: 크기가 너무 작은 객체는 분석하지 않음
        if (rect.width < minBoxSize || rect.height < minBoxSize) return false;

        float halfX = rect.width * 0.5f;
        float halfY = rect.height * 0.5f;

        Vector2[] pos = new Vector2[4];
        pos[0] = new Vector2(rect.x - halfX, rect.y + halfY);
        pos[1] = new Vector2(rect.x + halfX, rect.y + halfY);
        pos[2] = new Vector2(rect.x - halfX, rect.y - halfY);
        pos[3] = new Vector2(rect.x + halfX, rect.y - halfY);

        //for (int i = 0; i < 4; i++)
        //{
        //    if (pos[i].x < 0 || pos[i].x > Screen.width) continue;
        //    if (pos[i].y < 0 || pos[i].y > Screen.height) continue;
        //    return true;
        //}

        //INFO: 하나라도 벗어나면 false
        for (int i = 0; i < 4; i++)
        {
            if (pos[i].x < 0 || pos[i].x > Screen.width) return false;
            if (pos[i].y < 0 || pos[i].y > Screen.height) return false;
        }

        PrintRectInfo();

        return true;
    }

    public void PrintRectInfo()
    {
        Rect rect = rectInfo;

        float halfX = rect.width * 0.5f;
        float halfY = rect.height * 0.5f;

        Vector2[] pos = new Vector2[4];
        pos[0] = new Vector2(rect.x - halfX, rect.y + halfY);
        pos[1] = new Vector2(rect.x + halfX, rect.y + halfY);
        pos[2] = new Vector2(rect.x - halfX, rect.y - halfY);
        pos[3] = new Vector2(rect.x + halfX, rect.y - halfY);

        //Debug.Log("[GSHONG] rect info");
        //Debug.Log(pos[0] + "," + pos[1] + "," + pos[2] + "," + pos[3]);
    }

    //INFO: 특정 객체가 밑면을 보여주고 있지 않은지 검사
    public bool CheckObjectBottom(Camera cam)
    {
        //INFO: 카메라에서 객체를 보는 방향
        Vector3 dir = (cam.transform.position - transform.position).normalized;
        Vector3 bottomDir = GetBottomDirection();

        float angleDeg = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(dir, bottomDir));
        //Debug.Log("[GSHONG] rect bottom check");
        //Debug.Log("Angle : " + angleDeg);
        if (angleDeg > -84 && angleDeg < 84)
        {
            return false;
        }
        return true;
    }

    public Rect AdjustRect()
    {
        Rect rect = rectInfo;
        float halfX = rect.width * 0.5f;
        float halfY = rect.height * 0.5f;

        Vector2[] pos = new Vector2[4];
        pos[0] = new Vector2(rect.x - halfX, rect.y + halfY);
        pos[1] = new Vector2(rect.x + halfX, rect.y + halfY);
        pos[2] = new Vector2(rect.x - halfX, rect.y - halfY);
        pos[3] = new Vector2(rect.x + halfX, rect.y - halfY);

        for (int i = 0; i < 4; i++)
        {
            if (pos[i].x < 0) pos[i].x = 0;
            else if (pos[i].x > Screen.width) pos[i].x = Screen.width;
            if (pos[i].y < 0) pos[i].y = 0;
            else if (pos[i].y > Screen.height) pos[i].y = Screen.height;
        }

        rect.x = (pos[0].x + pos[1].x) * 0.5f;
        rect.y = (pos[0].y + pos[2].y) * 0.5f;
        rect.width = pos[1].x - pos[0].x;
        rect.height = pos[0].y - pos[2].y;

        rect = rectInfo;
        return rect;
    }

    public Rect GetRectInfo()
    {
        return rectInfo;
    }

    public GameObject[] getBoxImgs()
    {
        return boxImgs;
    }

    public GameObject[] getBoxPoints()
    {
        return boxPoints;
    }

    public void SetChecking(bool value)
    {
        isChecking = value;
    }

    // Update is called once per frame
    void Update()
    {
        if (isChecking)
        {
            return;
        }
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * Time.deltaTime * rightRotateSpeed);
        transform.Rotate(Vector3.right * Time.deltaTime * upRotateSpeed);
    }

    //INFO: 밑면의 방향을 반환
    public Vector3 GetBottomDirection()
    {
        //INFO: 세 점으로 방향 구하기
        Vector3 pos1 = boxPoints[2].transform.position;
        Vector3 pos2 = boxPoints[3].transform.position;
        Vector3 pos3 = boxPoints[6].transform.position;

        Vector3 dir1 = (pos2 - pos1).normalized;
        Vector3 dir2 = (pos3 - pos1).normalized;
        Vector3 perp = Vector3.Cross(dir1, dir2);

        return perp;
    }

    private void OnTriggerStay(Collider other)
    {
        if(transform.position.z > other.transform.position.z)
        {
            //INFO: 본 객체가 뒤에 있는 경우
            ChangeDist();
        } 
    }

    public void SetVisible(bool isVisible)
    {
        Renderer[] items = GetComponentsInChildren<Renderer>();
        foreach(var item in items)
        {
            item.enabled = isVisible;
        }

        Image[] imgs = GetComponentsInChildren<Image>();
        foreach (var img in imgs)
        {
            img.enabled = isVisible;
        }
    }

    //INFO: 미리 생성한 직육면체의 8개 점을 통해 바운딩 박스를 구하는 방법
    public Rect GetRectWithObjectByAABB()
    {
        GameObject[] boxPoints = getBoxPoints();

        //INFO: 바운딩 박스 여덟개의 점
        Vector3[] boxPosList = new Vector3[8];
        for (int j = 0; j < 8; j++)
        {
            boxPosList[j] = Camera.main.WorldToScreenPoint(boxPoints[j].transform.position);
            boxPosList[j].z = 0;
        }

        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        for (int j = 0; j < 8; j++)
        {
            if (boxPosList[j].x < minX)
            {
                minX = boxPosList[j].x;
            }
            if (boxPosList[j].x > maxX)
            {
                maxX = boxPosList[j].x;
            }
            if (boxPosList[j].y < minY)
            {
                minY = boxPosList[j].y;
            }
            if (boxPosList[j].y > maxY)
            {
                maxY = boxPosList[j].y;
            }
        }

        Rect bbox = new Rect(minX, minY, maxX - minX, maxY - minY);
        return bbox;
    }

    //INFO: 객체의 모든 버텍스를 계산하여 바운딩 박스를 그리는 방식 (정확도 높음)
    public Rect GetRectWithObjectByAllVertex()
    {
        if (GetComponentsInChildren<MeshFilter>().Length != 0)
        {
            var meshFilters = GetComponentsInChildren<MeshFilter>();
            float x1 = float.MaxValue, y1 = float.MaxValue, x2 = 0.0f, y2 = 0.0f;

            foreach (var meshFilter in meshFilters)
            {
                Vector3[] vertices = meshFilter.mesh.vertices;
                foreach (Vector3 vert in vertices)
                {
                    //Vector2 tmp = WorldToGUIPoint(transform.TransformPoint(vert));
                    Vector2 tmp = Camera.main.WorldToScreenPoint(meshFilter.transform.TransformPoint(vert));
                    if (tmp.x < x1) x1 = tmp.x;
                    if (tmp.x > x2) x2 = tmp.x;
                    if (tmp.y < y1) y1 = tmp.y;
                    if (tmp.y > y2) y2 = tmp.y;
                }
            }

            Rect bbox = new Rect(x1, y1, x2 - x1, y2 - y1);
            return bbox;
        }
        else if (GetComponentsInChildren<SkinnedMeshRenderer>().Length != 0)
        {
            var transforms = GetComponentsInChildren<Transform>();
            float x1 = float.MaxValue, y1 = float.MaxValue, x2 = 0.0f, y2 = 0.0f;

            foreach (var tf in transforms)
            {
                if(tf.gameObject.GetComponent<RectTransform>() != null)
                {
                    continue;
                }
                if(tf.childCount != 0)
                {
                    continue;
                }

                Vector2 tmp = Camera.main.WorldToScreenPoint(tf.position);
                if (tmp.x < x1) x1 = tmp.x;
                if (tmp.x > x2) x2 = tmp.x;
                if (tmp.y < y1) y1 = tmp.y;
                if (tmp.y > y2) y2 = tmp.y;
            }

            Rect bbox = new Rect(x1, y1, x2 - x1, y2 - y1);
            return bbox;

            //var meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            //float x1 = float.MaxValue, y1 = float.MaxValue, x2 = 0.0f, y2 = 0.0f;

            //foreach (var meshRenderer in meshRenderers)
            //{
            //    Vector3[] vertices = meshRenderer.sharedMesh.vertices;
            //    foreach (Vector3 vert in vertices)
            //    {
            //        //Vector2 tmp = WorldToGUIPoint(transform.TransformPoint(vert));
            //        Vector2 tmp = Camera.main.WorldToScreenPoint(meshRenderer.transform.TransformPoint(vert));
            //        if (tmp.x < x1) x1 = tmp.x;
            //        if (tmp.x > x2) x2 = tmp.x;
            //        if (tmp.y < y1) y1 = tmp.y;
            //        if (tmp.y > y2) y2 = tmp.y;
            //    }
            //}

            //Rect bbox = new Rect(x1, y1, x2 - x1, y2 - y1);
            //return bbox;
        }
        else
        {
            return GetRectWithObjectByAABB();
        }
    }

    public Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }
}
