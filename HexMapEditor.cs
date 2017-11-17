using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{

    public Color[] colors;

    public HexGrid hexGrid;

    int activeElevation;
    int activeWaterLevel;

    int activeUrbanLevel, activeFarmLevel, activePlantLevel;

    Color activeColor;

    int brushSize;

    bool applyColor;
    bool applyElevation = true;
    bool applyWaterLevel = true;

    bool applyUrbanLevel, applyFarmLevel, applyPlantLevel;

    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    OptionalToggle riverMode, roadMode;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
    }

    public void SetApplyUrbanLevel(bool toggle)
    {
        applyUrbanLevel = toggle;
    }

    public void SetUrbanLevel(float level)
    {
        activeUrbanLevel = (int)level;
    }

    public void SetApplyFarmLevel(bool toggle)
    {
        applyFarmLevel = toggle;
    }

    public void SetFarmLevel(float level)
    {
        activeFarmLevel = (int)level;
    }

    public void SetApplyPlantLevel(bool toggle)
    {
        applyPlantLevel = toggle;
    }

    public void SetPlantLevel(float level)
    {
        activePlantLevel = (int)level;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    void Awake()
    {
        SelectColor(0);
    }

    void Update()
    {
        if (
            Input.GetMouseButton(0) &&
            !EventSystem.current.IsPointerOverGameObject()
        )
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
        updateCellsState();
    }

    void updateCellsState()
    {
        HexCell temp;
        float sample = randomSample();
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                HexCoordinates coord = new HexCoordinates(i, j);
                temp = hexGrid.GetCell(coord);
                if (temp.PlantLevel != 0 || temp.FarmLevel != 0 || temp.UrbanLevel != 0)
                {
                    temp.UpdateProbabilities();
                    checkRandomEvents(temp, sample);
                }
            }
        }
    }

    void checkRandomEvents(HexCell cell, float sample)
    {
        if (cell.IsCloud == false)
        {
            if (sample < cell.ProbabilityCloud)
            {
                createRain(cell);
            }
        }
        else
        {
            updateCloudStatus(cell);
            if (cell.IsRaining == false)
            {
                if (sample < cell.ProbabilityRain)
                {
                    createRain(cell);
                }
            }
            else
            {
                updateRainStatus(cell);
            }
        }
    }

    void updateCloudStatus(HexCell cell)
    {
        // write time 
        /*
        if (cell.CloudStartTimeStamp + cell.CloudDurationCycles > time)
        {
            cell.IsCloud = false;
        }
        */
    }
    
    void updateRainStatus(HexCell cell)
    {
        //time
        /*
        if (cell.RainStartTimeStamp + cell.RainDurationCycles > time)
        {
            cell.IsRaining = false;
        }
        */
    }

    void createRain(HexCell cell)
    {
        cell.IsRaining = true;
    }

    void createCloud(HexCell cell)
    {
        cell.IsCloud = true;
    }

    float randomSample()
    {
        return Random.Range(0, 1);
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell);
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }

    void ValidateDrag(HexCell currentCell)
    {
        for (
            dragDirection = HexDirection.NE;
            dragDirection <= HexDirection.NW;
            dragDirection++
        )
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    void EditCell(HexCell cell)
    {
        if (cell)
        {
            if (applyColor)
            {
                cell.Color = activeColor;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if (applyWaterLevel)
            {
                cell.WaterLevel = activeWaterLevel;
            }
            if (applyUrbanLevel)
            {
                cell.UrbanLevel = activeUrbanLevel;
                cell.UpdateCell();
            }
            if (applyFarmLevel)
            {
                cell.FarmLevel = activeFarmLevel;
                cell.UpdateCell();
            }
            if (applyPlantLevel)
            {
                cell.PlantLevel = activePlantLevel;
                cell.UpdateCell();
            }
            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if (roadMode == OptionalToggle.No)
            {
                cell.RemoveRoads();
            }
            if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if (riverMode == OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if (roadMode == OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
    }
}