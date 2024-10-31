using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BalloonInflator : XRGrabInteractable
{
    [Header("Balloon Data")]
    public Transform attachPoint;
    public Balloon balloonPrefab;
    public float shakeThreshold = 2.5f; // กำหนดค่าความเร็วที่ต้องการให้บอลลูนหลุดออก

    private Balloon m_BalloonInstance; // สำหรับเก็บบอลลูนที่สร้างขึ้นใหม่
    private XRBaseController m_Controller; // เก็บ Controller ที่ใช้จับ

    // สร้างบอลลูนเมื่อหยิบ Inflator ขึ้นมา
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // สร้างอินสแตนซ์ของบอลลูนที่ attachPoint
        m_BalloonInstance = Instantiate(balloonPrefab, attachPoint);

        // ตรวจสอบว่าเป็น Controller ไหนที่จับ Inflator
        var controllerInteractor = args.interactorObject as XRBaseControllerInteractor;
        if (controllerInteractor != null)
        {
            m_Controller = controllerInteractor.xrController;
            m_Controller.SendHapticImpulse(0.5f, 0.1f); // ส่งการสั่นเล็กน้อยเมื่อหยิบ
        }
    }

    // ลบบอลลูนเมื่อปล่อย Inflator
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // ลบอินสแตนซ์ของบอลลูน
        if (m_BalloonInstance != null)
        {
            Destroy(m_BalloonInstance.gameObject);
            m_BalloonInstance = null;
        }

        // ล้างตัวแปร Controller
        m_Controller = null;
    }

    // ปรับขนาดของบอลลูนตามแรงกดของทริกเกอร์ใน Controller
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (isSelected && m_Controller != null && m_BalloonInstance != null)
        {
            // รับค่าแรงกดทริกเกอร์
            float triggerValue = m_Controller.activateInteractionState.value;

            // ใช้ Lerp ให้บอลลูนขยาย/หดตัวอย่างนุ่มนวลตามค่าแรงกดทริกเกอร์
            float targetScale = Mathf.Lerp(1.0f, 4.0f, triggerValue);
            m_BalloonInstance.transform.localScale = Vector3.Lerp(
                m_BalloonInstance.transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * 5
            );

            // เพิ่ม Haptic Feedback ตามแรงกดทริกเกอร์
            m_Controller.SendHapticImpulse(triggerValue, 0.1f);

            // ตรวจสอบค่าความเร็ว (velocity) เพื่อเช็คว่าถูกเขย่าหรือไม่
            Vector3 velocity = m_Controller.inputDevice.velocity;
            if (velocity.magnitude > shakeThreshold)
            {
                DetachBalloon();
            }
        }
    }

    // ฟังก์ชันสำหรับการปล่อยบอลลูนให้ลอยขึ้น
    private void DetachBalloon()
    {
        if (m_BalloonInstance != null)
        {
            m_BalloonInstance.Detach(); // เรียกใช้ฟังก์ชัน Detach ของบอลลูน
            m_BalloonInstance = null;
        }
    }
}
