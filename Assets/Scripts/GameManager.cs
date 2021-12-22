using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //INFO: 캡처할 타겟 클래스 객체
    private GameObject[] target;
    //INFO: 월드에 있는 클래스 객체 정보
    public GameObject[] classObjectList;
    //INFO: 객체 표출용 카메라
    public Camera cam;
    //INFO: 배경 표출용 카메라
    public Camera subCam;
    //INFO: 빛의 방향과 세기 조절
    public Light light;
    //INFO: 여기 효과 (현재 미사용)
    public ParticleSystem smoke;
    //INFO: 현재 검사 중인지 여부 (스크린샷)
    private bool isChecking = false;
    //INFO: 박스가 너무 작으면 저장하지 않음
    public static float minBoxSize = 15f;
    //INFO: 생성할 이미지 개수
    private int numDestImages = 100;
    //INFO: 생성할 이미지 시작 번호
    private int startCnt = 0;
    //INFO: 디버그 모드 여부
    private bool isDebug = false;
    //INFO: 배경 렌더러
    public SpriteRenderer background;
    //INFO: 추상 배경 이미지 목록
    public Sprite[] spriteList;
    //INFO: 도로 배경 이미지 목록
    public Sprite[] spriteRoadList;
    //INFO: 바다 배경 이미지 목록
    public Sprite[] spriteSeaList;
    private HashSet<int> vehicleSet = new HashSet<int>();
    private HashSet<int> peopleSet = new HashSet<int>();
    private HashSet<int> vesselSet = new HashSet<int>();
    private HashSet<int> animalSet = new HashSet<int>();
    //INFO: 타겟의 방향과 위치를 변경함에 따라 Update()에서 다시한번 바운더리 계산을 해주어야 함
    private bool hasUpdated = true;
    //INFO: 현재 대상 객체
    private GameObject targetObj = null;

    private void InitSet()
    {
        vehicleSet.Add(0);
        vehicleSet.Add(1);
        vehicleSet.Add(2);
        vehicleSet.Add(4);
        vehicleSet.Add(5);
        vehicleSet.Add(6);
        vehicleSet.Add(7);
        vehicleSet.Add(16);

        peopleSet.Add(3);

        vesselSet.Add(9);
        vesselSet.Add(10);
        vesselSet.Add(14);
        vesselSet.Add(15);
        vesselSet.Add(17);

        animalSet.Add(11);
        animalSet.Add(12);
        animalSet.Add(13);
    }

    private int GetLabelType(int label)
    {
        if (vehicleSet.Contains(label))
        {
            return 0;
        } 
        else if (peopleSet.Contains(label))
        {
            return 1;
        }
        else if (vesselSet.Contains(label))
        {
            return 2;
        }
        else if (animalSet.Contains(label))
        {
            return 3;
        }
        else
        {
            return -1;
        }
    }

    private void Awake()
    {
        InitSet();
#if DEBUG
        isDebug = true;
#else
        isDebug = false;
#endif
        //Screen.SetResolution(416, 416, false);
        Screen.SetResolution(800, 800, false);
        Directory.CreateDirectory("./trains/images");
        Directory.CreateDirectory("./trains/labels");

        StreamReader reader = new StreamReader("./analysis.config");
        while (!reader.EndOfStream)
        {
            string data = reader.ReadLine();
            Debug.Log(data);
            string[] keyValues = data.Split("="[0]);
            if(keyValues.Length != 2)
            {
                continue;
            }

            if(keyValues[0] == "targetClass")
            {
                //INFO: 분석할 클래스 목록
                string[] classStrList = keyValues[1].Split(","[0]);

                int activeObjCnt = 0;

                //INFO: config에 입력된 모든 클래스들에 대해 반복
                for(int i = 0; i < classStrList.Length; i++)
                {
                    //INFO: 입력한 클래스 값
                    int targetClass = int.Parse(classStrList[i]);
                    for (int j = 0; j < classObjectList.Length; j++)
                    {
                        GameObject targetObject = classObjectList[j];
                        ObjectBase objBase = targetObject.GetComponent<ObjectBase>();
                        //INFO: 현재 객체가 입력한 클래스가 아니면 continue
                        if (targetClass != objBase.GetLabel())
                        {
                            continue;
                        }

                        targetObject.SetActive(true);
                        activeObjCnt++;
                    }
                }

                Debug.Log("activeObjCnt : " + activeObjCnt);

                //INFO: 객체 개수만큼 공간 할당
                target = new GameObject[activeObjCnt];

                int targetCnt = 0;
                for(int i = 0; i < classObjectList.Length; i++)
                {
                    GameObject targetObject = classObjectList[i];
                    if (targetObject.activeSelf == false)
                    {
                        continue;
                    }

                    target[targetCnt++] = targetObject;
                    ObjectBase objBase = targetObject.GetComponent<ObjectBase>();
                    objBase.SetVisible(false);
                }
            }
            else if(keyValues[0] == "numDestImages")
            {
                //INFO: 저장할 이미지 개수
                numDestImages = int.Parse(keyValues[1]);
            }
            else if (keyValues[0] == "startCnt")
            {
                //INFO: 저장 이미지 시작 번호
                startCnt = int.Parse(keyValues[1]);
            }
            else if (keyValues[0] == "showDots")
            {
                //INFO: 이미지에 바운딩 박스 점 표출 여부
                if(int.Parse(keyValues[1]) == 1)
                {
                    ObjectBase.showDots = true;
                }
                else
                {
                    ObjectBase.showDots = false;
                }
            }
        }
        reader.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        //smoke.Stop();
        Debug.Log("width: " + Screen.width + ", height: " + Screen.height);
        StartCoroutine(ScreenshotTimer(0.5f));
        //StartCoroutine(SmokeTimer());
    }

    //INFO: 연기 효과 함수
    private IEnumerator SmokeTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            if(Random.value > 0.7f)
            {
                smoke.startColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
                smoke.Play();
                Debug.Log("Start smoke");
            } else
            {
                smoke.Stop();
                Debug.Log("Stop smoke");
            }
        }
    }

    private IEnumerator ScreenshotTimer(float time)
    {
        //INFO: 시작 번호
        int saveCnt = startCnt;
        //INFO: 마지막 번호
        int destCnt = startCnt + numDestImages;
        while (true)
        {
            //INFO: 원하는 개수만 처리했으면 멈추기
            if (saveCnt >= destCnt)
            {
                Debug.Break();
                Application.Quit();
                break;
            }

            yield return new WaitForSeconds(time);

            SetChecking(true);

            yield return new WaitForSeconds(0.1f);

            //INFO: 조명 색상 및 강도 랜덤 (빛의 세기로 대체)
            //light.color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            //INFO: 빛의 세기 변경
            light.intensity = Random.Range(0f, 3f);
            //INFO: 빛의 방향 변경
            light.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(-90f, 90f), Random.Range(-90f, 90f), 0));

            //INFO: 카메라 배경색상 랜덤
            subCam.backgroundColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));

            //INFO: 저장할 파일 이름
            string imgFilename = (isDebug ? "": ".") + "./trains/images/test (" + saveCnt + ").jpg";
            //INFO: 저장할 라벨 파일 이름
            string metaFilename = "./trains/labels/test (" + saveCnt + ").txt";

            //INFO: 실제 이미지에 표출되는 개체 정보 목록
            List<string> resList = new List<string>();

            //INFO: 표출할 객체 랜덤으로 선정
            int showTargetIdx = Random.Range(0, target.Length);
            int labelType = 0;
            Debug.Log("showTargetIdx : " + showTargetIdx);
            ObjectBase objBase;

            {
                targetObj = target[showTargetIdx];
                objBase = targetObj.GetComponent<ObjectBase>();
                objBase.SetVisible(true);
                labelType = GetLabelType(objBase.GetLabel());
                UpdateObjPoints(targetObj);
            }

            {
                //INFO: 배경 변경
                if (labelType == 0 || labelType == 1)
                {
                    //INFO: 사람 또는 자동차의 경우 도로, 도보 배경을 사용함
                    background.sprite = spriteRoadList[Random.Range(0, spriteRoadList.Length)];
                }
                else if (labelType == 2)
                {
                    //INFO: 배의 경우 바다 배경을 사용함
                    background.sprite = spriteSeaList[Random.Range(0, spriteSeaList.Length)];
                }

                //INFO: 배경이 단색인 경우 색상 변경
                //background.color = new Color(Random.Range(125f, 255f), Random.Range(125f, 255f), Random.Range(125f, 255f));
            }
            //objBase.AdjustRect();
            int changeCnt = 0;
            while (true)
            {
                //yield return new WaitForSeconds(0.5f);
                yield return new WaitForEndOfFrame();
                {
                    hasUpdated = false;
                    while (hasUpdated == false)
                    {
                        //INFO: 여기서 대기하지 않으면 객체 라벨 정보가 갱신되지 않음
                        yield return new WaitForSeconds(0.1f);
                    }
                }

                if (objBase.CheckRectInScreen() == false)
                {
                    Debug.Log("[GSHONG] rect is out");
                    changeCnt++;
                    objBase.ChangeDist();
                    objBase.ChangeDirection();
                    //INFO: 객체가 이동하거나 회전했다면 점을 다시 그려야 한다
                    UpdateObjPoints(targetObj);
                    resList.Clear();
                    continue;
                }

                Debug.Log("[GSHONG] rect in check done");
                //Debug.Break();

                if (objBase.CheckObjectBottom(cam) == false)
                {
                    Debug.Log("[GSHONG] rect is bottom");
                    changeCnt++;
                    objBase.ChangeDist();
                    objBase.ChangeDirection();
                    //Debug.Log("ChangeDirection " + showTargetIdx);
                    //INFO: 객체가 이동하거나 회전했다면 점을 다시 그려야 한다
                    UpdateObjPoints(targetObj);
                    resList.Clear();
                    continue;
                }

                Debug.Log("[GSHONG] rect bottom check done");
                //Debug.Break();

                break;
            }

            Debug.Log("[GSHONG] pos check done, change cnt: " + changeCnt);
            //objBase.PrintRectInfo();

            //Debug.Break();

            {
                if (changeCnt != 0)
                {
                    hasUpdated = false;
                    while (hasUpdated == false)
                    {
                        //INFO: 여기서 대기하지 않으면 객체 라벨 정보가 갱신되지 않음
                        yield return new WaitForSeconds(0.1f);
                    }
                }

                Rect rect = objBase.AdjustRect();

                {//INFO: 실제 스크린샷에 표시되는 라벨 정보
                    rect.x = rect.x / Screen.width;
                    rect.y = (Screen.height - rect.y) / Screen.height;
                    rect.width = rect.width / Screen.width;
                    rect.height = rect.height / Screen.width;
                    string res = objBase.GetLabel() + " " + rect.x + " " + rect.y + " " + rect.width + " " + rect.height;
                    resList.Add(res);
                    Debug.Log(res);
                }
            }

            Debug.Log(imgFilename + ", object count : " + resList.Count);

            if (resList.Count != 0)
            {
                //STEP 1: 화면을 캡처한다.
                ScreenCapture.CaptureScreenshot(imgFilename);

                //STEP 2: 화면에 있는 객체의 좌표 및 크기를 저장한다.
                StreamWriter writer = new StreamWriter(metaFilename, false);
                foreach (var res in resList)
                {
                    writer.WriteLine(res);
                }
                writer.Close();

                
                if (isDebug)
                {
                    Debug.Break();
                }

                //INFO: 여기서 대기하지 않으면 객체가 이미지 캡처되지 않음
                yield return new WaitForSeconds(0.1f);

                //Debug.Log(imgFilename);
                saveCnt++;
            }
            else
            {
                Debug.Log("No Object exist!");
                //Debug.Break();
            }

            //INFO: 객체 표출 후 다시 숨김
            targetObj.GetComponent<ObjectBase>().SetVisible(false);
            SetChecking(false);

            targetObj = null;
        }
    }

    private void SetChecking(bool value)
    {
        isChecking = value;
        for(int i = 0; i < target.Length; i++)
        {
            ObjectBase objBase = target[i].GetComponent<ObjectBase>();
            objBase.SetChecking(value);
        }
        Debug.Log("isChecking: " + value);
    }

    //INFO: 특정 객체가 화면 내에 가리지는 않는지 검사 (현재는 한개씩 표출하므로 미사용)
    private bool checkObjectInSight(GameObject obj)
    {
        //INFO: 카메라에서 객체를 보는 방향
        Vector3 dir = (obj.transform.position - cam.transform.position).normalized;
        RaycastHit hitInfo;
        if(Physics.Raycast(cam.transform.position, dir, out hitInfo))
        {
            //INFO: 다른 객체가 선에 맞은 경우
            if(hitInfo.collider.gameObject.GetInstanceID() != obj.GetInstanceID())
            {
                Debug.Log("Other Object hit; obj id : " + obj.GetInstanceID() + ", other id : " + hitInfo.collider.gameObject.GetInstanceID());
                return false;
            }

            Debug.Log("Hit correct");
            return true;
            //ObjectBase targetObjBase = obj.GetComponent<ObjectBase>();
            //if (hitInfo.transform == null)
            //{
            //    //Debug.Log("hitInfo is null");
            //    //Debug.Break();
            //    return false;
            //}

            //GameObject hitObj = hitInfo.transform.gameObject;
            //ObjectBase hitObjBase = hitObj.GetComponent<ObjectBase>();
            ////INFO: 다른 객체가 있으면 false
            //if (hitObjBase.GetInstanceID() != targetObjBase.GetInstanceID())
            //{
            //    return false;
            //}

            //return true;
        }
        else
        {
            Debug.Log("Hit object is empty");
            //INFO: 해당 객체로 향한 선에 맞은 객체가 하나도 없는 경우
            return false;
        }

        //if(hitObjBase.GetLabel() == targetObjBase.GetLabel())
        //{
        //    return true;
        //}
        //else
        //{
        //    return false;
        //}
    }

    private void UpdateObjPoints(GameObject target, bool isUpdate = false)
    {
        ObjectBase objBase = target.GetComponent<ObjectBase>();

        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;

        Rect bbox;
        if (isUpdate)
        {
            bbox = objBase.GetRectWithObjectByAABB();
        }
        else
        {
            bbox = objBase.GetRectWithObjectByAllVertex();
        }
        minX = bbox.x;
        minY = bbox.y;
        maxX = minX + bbox.width;
        maxY = minY + bbox.height;

        //if (minX < 0) minX = 0;
        //else if (minX > Screen.width) minX = Screen.width;
        //if (maxX < 0) maxX = 0;
        //else if (maxX > Screen.width) maxX = Screen.width;
        //if (minY < 0) minY = 0;
        //else if (minY > Screen.height) minY = Screen.height;
        //if (maxY < 0) maxY = 0;
        //else if (maxY > Screen.height) maxY = Screen.height;

        GameObject[] boxImgs = objBase.getBoxImgs();
        {
            RectTransform rt = boxImgs[0].GetComponent<RectTransform>();
            rt.transform.position = new Vector3(minX, minY, 1.0f);
        }
        {
            RectTransform rt = boxImgs[1].GetComponent<RectTransform>();
            rt.transform.position = new Vector3(minX, maxY, 1.0f);
        }
        {
            RectTransform rt = boxImgs[2].GetComponent<RectTransform>();
            rt.transform.position = new Vector3(maxX, minY, 1.0f);
        }
        {
            RectTransform rt = boxImgs[3].GetComponent<RectTransform>();
            rt.transform.position = new Vector3(maxX, maxY, 1.0f);
        }

        //INFO: 저장할 메타 데이터를 작성한다
        Rect rect = new Rect();
        rect.x = (maxX + minX) * 0.5f;
        rect.y = (maxY + minY) * 0.5f;
        rect.width = maxX - minX;
        rect.height = maxY - minY;

        objBase.SetRectInfo(rect);
    }

    // Update is called once per frame
    void Update()
    {
        bool hasToUpdate = false;
        if(hasUpdated == false)
        {
            hasToUpdate = true;
        }

        //INFO: 카메라의 위치는 무조건 (0,0,0) 이고, 바라보는 방향은 (0,0,1)로 가정 (시야각은 60도)
        //for (int i = 0; i < target.Length; i++)
        //{
        //    //UpdateObjPoints(target[i]);
        //}
        if(targetObj != null)
        {
            UpdateObjPoints(targetObj);
        }

        if (hasToUpdate)
        {
            hasUpdated = true;
        }
    }
}
