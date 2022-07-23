using FrogNinja.LevelGeneration.Configs;
using FrogNinja.Platforms;
using System.Collections.Generic;
using UnityEngine;

namespace FrogNinja.LevelGeneration
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private List<PlatformConfiguration> platformConfigurations;
        [SerializeField] private Transform playerSpawnPosition;
        [SerializeField] Vector2 minMaxVariation = new Vector2(0f, 1f);
        [SerializeField] int maxCountOfTheSamePlatformInRow = 1;

        private BasePlatform lastSpawnedPlatform;
        private int samePlatformInRow = 0;

        private float lastSpawnedY;
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            CreateLevel();

            EventManager.EnterGameplay += EventManager_EnterGameplay;
        }
        private void OnDestroy()
        {
            EventManager.EnterGameplay -= EventManager_EnterGameplay;
            EventManager.PlayerPositionUpdate -= EventManager_PlayerPositionUpdate;
            EventManager.PlayerFallenOff -= EventManager_PlayerFallenOff;
        }

        private void EventManager_EnterGameplay()
        {
            EventManager.PlayerPositionUpdate += EventManager_PlayerPositionUpdate;
            EventManager.PlayerFallenOff += EventManager_PlayerFallenOff;
        }

        private void EventManager_PlayerFallenOff()
        {
            EventManager.PlayerPositionUpdate -= EventManager_PlayerPositionUpdate;
            EventManager.PlayerFallenOff -= EventManager_PlayerFallenOff;
        }

        private void EventManager_PlayerPositionUpdate(Vector2 obj)
        {
            if (obj.y > lastSpawnedY / 2)
            {
                SpawnPlatform();
            }
        }

        private void CreateLevel()
        {
            lastSpawnedY = playerSpawnPosition.position.y;

            for (int i = 0; i < 100; i++)
            {
                SpawnPlatform();
            }
        }

        private void SpawnPlatform()
        {
            BasePlatform platformToSpawn = DifferentiatePlatforms(out PlatformConfiguration configToUse);
            Vector3 spawnPosition = new Vector3(GetRandomXPosition(), GetRandomYPosition(configToUse), 0);

            GameObject.Instantiate<BasePlatform>(platformToSpawn, spawnPosition, Quaternion.identity, transform);

            lastSpawnedY = spawnPosition.y;
        }

        private BasePlatform DifferentiatePlatforms(out PlatformConfiguration configToUse)
        {
            configToUse = platformConfigurations[Random.Range(0, platformConfigurations.Count)];

            BasePlatform platformToSpawn = configToUse.GetRandomPlatform();

            if (platformConfigurations.Count == 1)
            {
                if (platformConfigurations[0].Platforms.Count == 1)
                    return platformToSpawn;
            }

            if (platformToSpawn == lastSpawnedPlatform)
            {
                samePlatformInRow++;

                if (samePlatformInRow >= maxCountOfTheSamePlatformInRow)
                {
                    while (platformToSpawn == lastSpawnedPlatform)
                    {
                        configToUse = platformConfigurations[Random.Range(0, platformConfigurations.Count)];
                        platformToSpawn = configToUse.GetRandomPlatform();
                    }

                    samePlatformInRow = 0;
                }
            }
            else
            {
                samePlatformInRow = 0;
            }

            lastSpawnedPlatform = platformToSpawn;
            return platformToSpawn;
        }


        private float GetRandomXPosition()
        {
            float randomValue = Random.Range(0f, 1f);

            Vector3 resultPosition = mainCamera.ViewportToWorldPoint(new Vector3(randomValue, 0));

            return resultPosition.x;
        }
        private float GetRandomYPosition(PlatformConfiguration configToUse)
        {
            return lastSpawnedY + configToUse.DefaultYIncrease + Random.Range(minMaxVariation.x, minMaxVariation.y);
        }

    }
}
