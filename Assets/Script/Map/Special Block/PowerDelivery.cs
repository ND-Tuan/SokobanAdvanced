using System.Collections.Generic;
using System.Linq;
using ObserverPattern;
using UnityEngine;

public class PowerDelivery : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToActivate;
    private List<IPowerRequire> powerReceivers = new() { };
    public GameObject LinePrefab ;
    [SerializeField] private List<LineRenderer> Wires = new() { };
    [SerializeField] private FxAudioDataSO PowerOnAudioData;
    [SerializeField] private FxAudioDataSO PowerOffAudioData;


    void Awake()
    {
        // Tìm tất cả các thiết bị cần cấp nguồn
        foreach(GameObject gameObject in objectsToActivate)
        {
            // Thêm vào danh sách thiết bị nhận nguồn
            if(gameObject.TryGetComponent<IPowerRequire>(out IPowerRequire receiver))
            {
                powerReceivers.Add(receiver);
            }

            // Tạo dây điện
            GameObject newLine = Instantiate(LinePrefab, this.transform);
            LineRenderer wire = newLine.GetComponent<LineRenderer>();
            Wires.Add(wire);
        }
    }

    void Start()
    {
        // Kết nối dây điện ban đầu
        for(int i = 0; i < objectsToActivate.Count; i++)
        {
            ConnectWire(objectsToActivate[i], Wires[i]);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ElectricBox"))
        {
            //Báo cáo nhiệm vụ
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.SupplyPower, 1, true});

            // Cấp nguồn
            ChangePowerState(true);

            // Chạy âm thanh hiệu ứng cấp nguồn
            Observer.PostEvent(EvenID.PlayFX, PowerOnAudioData);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ElectricBox"))
        {
            ChangePowerState(false);

            // Chạy âm thanh hiệu ứng ngắt nguồn
            Observer.PostEvent(EvenID.PlayFX, PowerOffAudioData);
        }
    }

    /// Thay đổi trạng thái nguồn điện cho các thiết bị nhận
    private void ChangePowerState(bool state)
    {
        foreach(var receiver in powerReceivers)
        {
            receiver.SetPowerState(state);
        }


        // Cập nhật màu dây dẫn
        foreach(LineRenderer wire in Wires)
        {
            Color color = state? new Color(1,0.627451f,0, 1) : new Color(0.7568628f, 0.7882354f, 0.8039216f, 1);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(color.a, 1f)
                }
            );

            wire.colorGradient = gradient;
        }
    }


    // Kết nối dây điện từ nguồn đến thiết bị nhận
    private void ConnectWire(GameObject receiver, LineRenderer line)
    {
        
        line.positionCount = 3;
        line.SetPosition(0, this.transform.position);
        line.SetPosition(1, new Vector3(this.transform.position.x, receiver.transform.position.y, 0));
        line.SetPosition(2, receiver.transform.position);
        Wires.Add(line);
    }
    

    #if UNITY_EDITOR
    private void OnValidate()
    {
        
        
    }

    #endif
}
