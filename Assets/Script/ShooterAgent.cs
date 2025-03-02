using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using InfimaGames.LowPolyShooterPack;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;

public class ShooterAgent : Agent
{
    public GameManager gameManager;
    [SerializeField] private GameObject goal;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 100f;

    private Character playerCharacter;
    private WeaponBehaviour equippedWeapon;
    private List<MonsterController> monsters = new List<MonsterController>();

    private int currentEpisode = 0;
    private float cumulativeReward = 0f;

    public override void Initialize()
    {
        currentEpisode = 0;
        cumulativeReward = 0f;
        playerCharacter = GetComponent<Character>();
        gameObject.transform.position = new Vector3(83.8f, 1.8f, 13.8f);
    }

    public override void OnEpisodeBegin()
    {
        currentEpisode++;
        cumulativeReward = 0f;

        ResetAgent();
        SpawnObjects();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Observe your own position
        sensor.AddObservation(transform.localPosition);
        //Observe the end point
        sensor.AddObservation(goal.transform.localPosition);
        //Observe the enemy's position
        int maxEnemies = 5;
        for (int i = 0; i < maxEnemies; i++)
        {
            if (i < monsters.Count)
            {
                sensor.AddObservation(monsters[i].transform.localPosition);
            }
            else
            {

                sensor.AddObservation(Vector3.zero);
            }
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Action processing
        float moveX = actions.ContinuousActions[0];
        Debug.Log(moveX);
        float moveZ = actions.ContinuousActions[1];
        Debug.Log(moveZ);
        gameObject.transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        //rotate continue
        float rotationInput = actions.ContinuousActions[2];
        transform.Rotate(0, rotationInput * rotationSpeed * Time.deltaTime, 0);
        Debug.Log("why");

        //Shoot Discrete
        int fireAction = actions.DiscreteActions[0];
        if (fireAction == 1)
        {
            playerCharacter.AgentFire();
            Debug.Log("fire");
        }

        // Hit the wall -1

        // Action -0.01
        AddReward(-0.01f);

        // Hit the enemy 10 
        foreach (var monster in monsters)
        {
            if (monster.hitCount > 0)
            {
                AddReward(10f);
                Debug.Log("hitEenmy+10");
                monster.hitCount = 0;
            }
        }

        // Receive hit
        // public void TakeDamage(int damage)
        //{
        //    currentHealth -= damage;
        //    ShooterAgent agent = GetComponent<ShooterAgent>();
        //    agent.AddReward(-5f * damage);
        //    Debug.Log("Player took damage: " + damage + ", Current Health: " + currentHealth);
        //    if (currentHealth <= 0)
        //    {
        //        Die();
        //    }
        //}

        // Die 
        //Maybe I should write it in Die()
        if (playerCharacter.currentHealth <= 0)
        {
            AddReward(-100f);
            Debug.Log("die -100");
            EndEpisode();
        }

        // Reach the target 100
    }

    private void ResetAgent()
    {
        gameObject.transform.position = new Vector3(83.8f, 1.8f, 13.8f); 
        playerCharacter.currentHealth = 10; 
    }

    private void SpawnObjects()
    {
        gameManager.SpawnMonsters(Vector3.zero);
        monsters.AddRange(Object.FindObjectsByType<MonsterController>(FindObjectsSortMode.None));
    }

    private void GoalReached ()
    {
        AddReward(100f);
        cumulativeReward = GetCumulativeReward();

        EndEpisode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            GoalReached();
            Debug.Log("goal +100");
        }
        if (other.CompareTag("Wall")) 
        {
            AddReward(-1f);
            Debug.Log("Hit wall -1");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = 0.0f; // moveX
        continuousActions[1] = 0.0f; // moveZ
        continuousActions[2] = 0.0f; // rotation

        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0; // fire
    }



}
