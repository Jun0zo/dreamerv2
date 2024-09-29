using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
// 코드로 RenderTexture를 생성하는 예시
public class MLAgent : Agent
{
    public PlayerController playerController;
    public TracksControler trackController;

    private Rigidbody rb;
    private Vector3 lastAngularVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();  // Rigidbody 컴포넌트 가져오기
        lastAngularVelocity = rb.angularVelocity;  // 초기 각속도 저장
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);  // Vector3, 3개의 값

        // 에이전트의 현재 회전 (3개의 값: x, y, z)
        sensor.AddObservation(transform.localEulerAngles);  // Vector3, 3개의 값

        // 새로운 떨림 관측 (각속도 변화: roll과 pitch만 확인)
        float rollChange = Mathf.Abs(rb.angularVelocity.x - lastAngularVelocity.x);  // roll 축 (x축)
        float pitchChange = Mathf.Abs(rb.angularVelocity.z - lastAngularVelocity.z);  // pitch 축 (z축)

        // 떨림 정보를 관측에 추가
        sensor.AddObservation(rollChange);  // roll 축 변화량
        sensor.AddObservation(pitchChange);  // pitch 축 변화량


        // debug all observation
        // Debug.Log("localPosition: " + transform.localPosition);
        // Debug.Log("localEulerAngles: " + transform.localEulerAngles);
        // Debug.Log("Abs x: " + Mathf.Abs(transform.localEulerAngles.x));
        // Debug.Log("Abs z: " + Mathf.Abs(transform.localEulerAngles.z));

        // 마지막 각속도 업데이트
        lastAngularVelocity = rb.angularVelocity;
    

    }

    
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 행동 값을 받아옵니다 (연속적 행동 예시)
        float turn = actions.ContinuousActions[0]; // 좌우 회전
        float moveVertical = actions.ContinuousActions[1]; // 전후 이동

        // Pass the values to the TracksController
        playerController.turnInput = turn;
        playerController.moveInput = moveVertical;
        trackController.turnInput = turn;
        trackController.moveInput = moveVertical;


        // 떨림이 적은 경우 보상 부여
        float rollDelta = Mathf.Abs(rb.angularVelocity.x - lastAngularVelocity.x);
        float pitchDelta = Mathf.Abs(rb.angularVelocity.z - lastAngularVelocity.z);

        if (rollDelta < 0.1f && pitchDelta < 0.1f)
        {
            AddReward(0.2f);  // 떨림이 적을 때 보상 부여
        }

        // 마지막 각속도 업데이트
        lastAngularVelocity = rb.angularVelocity;
    }


    

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Debug.Log("Heuristic");
        // playerController.turn = Input.GetAxis("Horizontal");
        // playerController.moveVertical = Input.GetAxis("Vertical");

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("OnCollisionEnter");
            SetReward(-1.0f);
            EndEpisode();
            playerController.InitializeAgent();
        }
        
    }

    public override void OnEpisodeBegin() {
        Debug.Log("Init ep");
    }


}