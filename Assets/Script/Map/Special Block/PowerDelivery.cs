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


    void Awake()
    {
        foreach(GameObject gameObject in objectsToActivate)
        {
            if(gameObject.TryGetComponent<IPowerRequire>(out IPowerRequire receiver))
            {
                powerReceivers.Add(receiver);
            }

            GameObject newLine = Instantiate(LinePrefab, this.transform);
            LineRenderer wire = newLine.GetComponent<LineRenderer>();
            Wires.Add(wire);
        }
    }

    void Start()
    {
        for(int i = 0; i < objectsToActivate.Count; i++)
        {
            ConnectWire(objectsToActivate[i], Wires[i]);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ElectricBox"))
        {
            foreach(var receiver in powerReceivers)
            {
                ChangePowerState(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ElectricBox"))
        {
            foreach(var receiver in powerReceivers)
            {
                ChangePowerState(false);
            }
        }
    }

    private void ChangePowerState(bool state)
    {
        foreach(var receiver in powerReceivers)
        {
            receiver.SetPowerState(state);
        }

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
