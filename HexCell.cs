using UnityEngine;

public class HexCell : MonoBehaviour
{

    public HexCoordinates coordinates;

    public RectTransform uiRect;

    public HexGridChunk chunk;

    public Transform cloudPrefab;

    public Transform rainPrefab;

    public Transform cloudInstance;

    public Transform rainInstance;


    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }

    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value)
            {
                return;
            }
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y +=
                (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            ValidateRivers();

            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                {
                    SetRoad(i, false);
                }
            }

            Refresh();
        }
    }

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
            {
                return;
            }
            waterLevel = value;
            ValidateRivers();
            Refresh();
        }
    }

    public bool IsUnderwater
    {
        get
        {
            return waterLevel > elevation;
        }
    }

    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
    }

    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }

    public HexDirection RiverBeginOrEndDirection
    {
        get
        {
            return hasIncomingRiver ? incomingRiver : outgoingRiver;
        }
    }

    public bool HasRoads
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    public HexDirection IncomingRiver
    {
        get
        {
            return incomingRiver;
        }
    }

    public HexDirection OutgoingRiver
    {
        get
        {
            return outgoingRiver;
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }


    public float StreamBedY
    {
        get
        {
            return
                (elevation + HexMetrics.streamBedElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    public float RiverSurfaceY
    {
        get
        {
            return
                (elevation + HexMetrics.waterElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    public float WaterSurfaceY
    {
        get
        {
            return
                (waterLevel + HexMetrics.waterElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    public int UrbanLevel
    {
        get
        {
            return urbanLevel;
        }
        set
        {
            if (urbanLevel != value)
            {
                urbanLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public int FarmLevel
    {
        get
        {
            return farmLevel;
        }
        set
        {
            if (farmLevel != value)
            {
                farmLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public int PlantLevel
    {
        get
        {
            return plantLevel;
        }
        set
        {
            if (plantLevel != value)
            {
                plantLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public float ProbabilityRain
    {
        get
        {
            return PrRain;
        }
        set
        {
            if (PrRain != value)
            {
                PrRain = value;
                RefreshSelfOnly();
            }
        }
    }

    public float ProbabilityCloud
    {
        get
        {
            return PrCloud;
        }
        set
        {
            if (PrCloud != value)
            {
                PrCloud = value;
                RefreshSelfOnly();
            }
        }
    }


    public bool IsRaining
    {
        get
        {
            return isRaining;
        }
        set
        {
            if (isRaining == true && value == false)
            {
                isRaining = value;
                deleteRain();
                
                }
            else if(isRaining == false && value == true)
            {
                isRaining = value;
                
                generateRain();
                rainStartTimeStamp = Time.time;

                float pr = Mathf.Pow((pressure + 0.8f) / 2.4f, 2);
                float hm = Mathf.Pow((humidity-40f)/50f, 2);
                if (temperature > 0) temperature = 0.9f * temperature;
                pressure = Mathf.Lerp(-0.8f, 1.6f, pr);
                humidity = Mathf.Lerp(40f, 90f, hm);
            }
            
        }
    }

    public bool IsCloud
    {
        get
        {
            return isCloud;
        }
        set
        {
            if (isCloud == true && value == false)
            {
                isCloud = value;
                deleteCloud();
                
            }
            else if (isCloud == false && value == true)
            {
                isCloud = true;
                generateCloud();
                cloudStartTimeStamp = Time.time;
                
            }
            
        }
    }

    public float RainStartTimeStamp
    {
        get
        {
            return rainStartTimeStamp;
        }
        set
        {
            rainStartTimeStamp = value;
        }
    }

    public float CloudStartTimeStamp
    {
        get
        {
            return cloudStartTimeStamp;
        }
        set
        {
            cloudStartTimeStamp = value;
        }
    }

    void deleteCloud() {
        Destroy(cloudInstance.gameObject);
        if (rainInstance != null)
            Destroy(rainInstance.gameObject);
    }
    void generateCloud() {
        cloudInstance = Instantiate(cloudPrefab);
        Vector3 position = Position;
        position.y += cloudInstance.localScale.y * 0.5f;
        cloudInstance.localPosition = HexMetrics.Perturb(position);
        cloudInstance.localRotation = Quaternion.Euler(0f, 360f * 0.5f, 0f);
    }
    void deleteRain() {
        Destroy(rainInstance.gameObject);
    }
    void generateRain() {
        rainInstance = Instantiate(rainPrefab);
        Vector3 position = Position;
        position.y += rainInstance.localScale.y * 15f;
        rainInstance.localPosition = HexMetrics.Perturb(position);
        rainInstance.localRotation = Quaternion.Euler(0f, 360f * 0.5f, 0f);
        
    }

    public int RainDurationCycles
    {
        set
        {
            rainDurationCycles = value;
        }
        get
        {
            return rainDurationCycles;
        }
    }

    public int CloudDurationCycles
    {
        set
        {
            cloudDurationCycles = value;
        }
        get
        {
            return cloudDurationCycles;
        }
    }

    Color color;

    int elevation = int.MinValue;
    int waterLevel;

    public float PrRain;
    public float PrCloud;
    public float PrDrought;
    public bool isVacant;
    bool isRaining;
    bool isCloud;
    int rainDurationCycles;
    int cloudDurationCycles;

    float rainStartTimeStamp;
    float cloudStartTimeStamp;

    public float temperature;  // In celsius
    public float pressure; // In kiloPascals
    public float humidity; // In percentage
    public float CO2Percentage;    // In percentage
    public float precipitation;	// in mm

    float rainDurationFactor = 10000; //max rain duration is for 1000 frames
    float cloudDurationFactor = 10000;//

    int urbanLevel, farmLevel, plantLevel;

    bool hasIncomingRiver, hasOutgoingRiver;
    HexDirection incomingRiver, outgoingRiver;

    [SerializeField]
    HexCell[] neighbors;

    [SerializeField]
    bool[] roads;

    float CalculateRainProbability()
    {
        if (isCloud == true) return 0.05f * PrCloud;
        else return 0.005f * PrCloud;
    }

    float CalculateCloudProbability()
    {
        float c;
        //c = 0.6f * (pressure * humidity) / (Mathf.Sqrt(temperature) * CO2Percentage);
        c = (5f / 12f) * (pressure + 0.8f) * (humidity - 40f) / (temperature + 60f);
        float cClamped;
        cClamped = Mathf.Lerp(0f, 1f, c);
        return cClamped;
    }

    void CalculateRainDuration()
    {
        rainDurationCycles = (int)(PrRain * rainDurationFactor);
        if (rainDurationCycles > cloudDurationCycles)
            rainDurationCycles = cloudDurationCycles;
        return;
    }

    void CalculateCloudDuration()
    {
        cloudDurationCycles = (int)(PrCloud * cloudDurationFactor);
        return;
    }

    public void UpdateProbabilities()
    {
        PrRain = CalculateRainProbability();
        PrCloud = CalculateCloudProbability();
        CalculateRainDuration();
        CalculateCloudDuration();
        UpdateParamsRandom();
    }

    void UpdateParamsRandom()
    {
        temperature +=  Random.Range(-2.0f, 2.0f);
        pressure += Random.Range(-0.005f, 0.005f);
        humidity += Random.Range(-1f, 1f);
        precipitation += Random.Range(-0.01f, 0.01f);
        CO2Percentage = Random.Range(-0.0006f, 0.0006f);
    }
    
    void UpdateParams()
    {
        /*
        temperature;  // In celsius
        pressure; // In kiloPascals
        humidity; // In percentage
        CO2Percentage;    // In percentage
        precipitation;	// in mm
        dependencies on urbanLevel, farmLevel, plantLevel
        */
        
        float temp = (0.5f + UrbanLevel/6.0f) / (Mathf.Sqrt(1 + plantLevel/6.0f) * Mathf.Sqrt(1 + FarmLevel/6.0f));
        temperature = Mathf.Lerp(-10f, 50f, temp) + Random.Range(-5.0f,5.0f);

        float press = (0.5f + UrbanLevel/6.0f) / (Mathf.Sqrt(1 + plantLevel/6.0f) * Mathf.Sqrt(1 + FarmLevel/6.0f));
        pressure = Mathf.Lerp(-0.8f, 1.6f, press) + Random.Range(-0.1f, 0.1f) ;

        float hum = (Mathf.Sqrt(Mathf.Sqrt(0.5f + plantLevel/6.0f))) * (Mathf.Sqrt(Mathf.Sqrt(0.5f + plantLevel/6.0f))) / (1 + UrbanLevel/6.0f);
        humidity = Mathf.Lerp(40f, 90f, hum) + Random.Range(-10f, 10f);

        float prec = (Mathf.Sqrt(Mathf.Sqrt(0.5f + plantLevel/3.0f))) * (Mathf.Sqrt(Mathf.Sqrt(0.5f + plantLevel/3.0f))) / Mathf.Pow(1 + UrbanLevel/3.0f, 2);
        precipitation = Mathf.Lerp(0.1f, 9f, prec);

        float CO2 = Mathf.Pow(0.5f + UrbanLevel/3.0f, 3) / (Mathf.Sqrt(1 + plantLevel/3.0f) * Mathf.Sqrt(1 + plantLevel/3.0f));
        CO2Percentage = Mathf.Lerp(0.003f, 0.07f, CO2);
        
    }

    public void UpdateCell()
    {
        UpdateParams();
        UpdateProbabilities();
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(
            elevation, neighbors[(int)direction].elevation
        );
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(
            elevation, otherCell.elevation
        );
    }

    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return
            hasIncomingRiver && incomingRiver == direction ||
            hasOutgoingRiver && outgoingRiver == direction;
    }

    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
        {
            return;
        }
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
        {
            return;
        }
        hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }

        HexCell neighbor = GetNeighbor(direction);
        if (!IsValidRiverDestination(neighbor))
        {
            return;
        }

        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }
        hasOutgoingRiver = true;
        outgoingRiver = direction;

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();

        SetRoad((int)direction, false);
    }

    public bool HasRoadThroughEdge(HexDirection direction)
    {
        return roads[(int)direction];
    }

    public void AddRoad(HexDirection direction)
    {
        if (
            !roads[(int)direction] && !HasRiverThroughEdge(direction) &&
            GetElevationDifference(direction) <= 1
        )
        {
            SetRoad((int)direction, true);
        }
    }

    public void RemoveRoads()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (roads[i])
            {
                SetRoad(i, false);
            }
        }
    }

    public int GetElevationDifference(HexDirection direction)
    {
        int difference = elevation - GetNeighbor(direction).elevation;
        return difference >= 0 ? difference : -difference;
    }

    bool IsValidRiverDestination(HexCell neighbor)
    {
        return neighbor && (
            elevation >= neighbor.elevation || waterLevel == neighbor.elevation
        );
    }

    void ValidateRivers()
    {
        if (
            hasOutgoingRiver &&
            !IsValidRiverDestination(GetNeighbor(outgoingRiver))
        )
        {
            RemoveOutgoingRiver();
        }
        if (
            hasIncomingRiver &&
            !GetNeighbor(incomingRiver).IsValidRiverDestination(this)
        )
        {
            RemoveIncomingRiver();
        }
    }

    void SetRoad(int index, bool state)
    {
        roads[index] = state;
        neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
        neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
    }

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }
}