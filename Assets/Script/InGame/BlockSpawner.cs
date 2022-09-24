
using UnityEngine;
using Photon.Pun;

public class BlockSpawner : MonoBehaviour
{
  public static BlockSpawner instance;

  public GameObject fallingBlockPrefab;
  public Transform blockHolder;
  public Vector2 blockSizeMinMax;
  public float spawnAngleMax;
  public Vector2 secondsBetweenSpawnMinMax;
  float nextSpawnTime;
  Vector2 screenHalfSize;
  public bool isSpawn;

  private void Awake()
  {
    if (instance == null) instance = this;
    else if (instance != this)
    {
      Destroy(instance.gameObject);
      instance = this;
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    isSpawn = true;
    screenHalfSize = new Vector2(Camera.main.aspect * Camera.main.orthographicSize, Camera.main.orthographicSize);
  }

  // Update is called once per frame
  void Update()
  {
    if (!isSpawn) return;

    if (PhotonNetwork.IsMasterClient)
    {
      if (Time.time > nextSpawnTime)
      {
        float secondsBetweenSpawns = Mathf.Lerp(secondsBetweenSpawnMinMax.y, secondsBetweenSpawnMinMax.x, Difficulty.GetDifficultyPercent());
        nextSpawnTime = Time.time + secondsBetweenSpawns;
        float blockSize = Random.Range(blockSizeMinMax.x, blockSizeMinMax.y);
        float spawnAngle = Random.Range(-spawnAngleMax, spawnAngleMax);

        Vector2 spawnPosition = new Vector2(Random.Range(-screenHalfSize.x, screenHalfSize.x), screenHalfSize.y + blockSize);

        Vector3 force = new Vector3(Random.Range(-2.5f, 2.5f), -5, 0) * 1.5f;
        object[] instantiationData = { force, blockSize };

        GameObject newBlock = PhotonNetwork.InstantiateRoomObject(fallingBlockPrefab.name, spawnPosition, Quaternion.Euler(Vector3.forward * spawnAngle), 0, instantiationData);
        newBlock.transform.parent = blockHolder;
      }
    }
  }
}
