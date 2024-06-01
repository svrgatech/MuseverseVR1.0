using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TileManager : MonoBehaviour {

   #region Static Properties
   public static TileManager Instance { get; set; }
   #endregion

   [SerializeField]
   private UnityEngine.Object
      tileObject;

    public RectTransform gridParent;

   public Tile[,] Tiles { get; set; }

   public int nSlots = 16;
   public int nChannels = 8;
   private bool[,] tileStates;
   private string stateKey = "gridState";

   void Awake() {
      if (TileManager.Instance == null)
         TileManager.Instance = this;
   }

    // Use this for initialization
    void Start()
    {
        // TODO: initialize the grid
        nChannels = SampleManager.Instance.numChannels;

        // intialize the notes array.
        Tiles = new Tile[nChannels, nSlots];
        tileStates = new bool[nChannels, nSlots];

        float parentWidth = gridParent.GetComponent<RectTransform>().rect.width;
        float parentHeight = gridParent.GetComponent<RectTransform>().rect.height;

        int marginRatio = 0; // tile to margin ratio, e.g. 5:1
        float unitX = parentWidth / (nSlots);
        float unitY = parentHeight / (nChannels * (marginRatio + 1) + 1);
        float tileW = gridParent.GetComponent<GridLayoutGroup>().cellSize.x;
        float tileH = gridParent.GetComponent<GridLayoutGroup>().cellSize.y;

        //inform location bar of the layout specifics
        LocationBar.Instance.SetDisplayRange(unitX - 36.25f, tileW, parentWidth);

        if (gridParent != null)
        {
            int totalTiles = nChannels * nSlots;

            for (int i = 0; i < nChannels; i++)
            {
                for (int j = 0; j < nSlots; j++)
                {
                    GameObject tile = (GameObject)Instantiate(tileObject); // , new Vector3(xBase + j * xSpacing, yBase + i * ySpacing, 0), Quaternion.identity);
                    tile.GetComponent<Tile>().channel = i;
                    tile.GetComponent<Tile>().slot = j;
                    tile.transform.SetParent(gridParent, false);
                    Tiles[i, j] = tile.GetComponent<Tile>();
                    Tiles[i, j].OnTileChanged += HandleOnTileChanged;
                    tileStates[i, j] = false;
                }
            }

        }

        // restore prev state of tiles
        RestoreState();
    }

    void OnDestroy() {
      for (int i = 0; i < nChannels; i++) {
         for (int j = 0; j < nSlots; j++) {
            Tiles[i, j].OnTileChanged -= HandleOnTileChanged;
         }
      }
   }

   // get current state of tiles as list of strings
   public List<string> GetStateList() {
      List<string> localList = new List<string>();
      for (int i = 0; i < nChannels; i++) {
         string localString = "";
         for (int j = 0; j < nSlots; j++) {
            if (tileStates[i, j]) {
               localString += "1";
            }
            else {
               localString += "0";
            }
         }
         localList.Add(localString);
      }
      return localList;
   }

   // save current state of tiles
   private void SaveState() {
      List<string> localList = new List<string>();
      for (int i = 0; i < nChannels; i++) {
         string localString = "";
         for (int j = 0; j < nSlots; j++) {
            if (tileStates[i, j]) {
               localString += "1";
            }
            else {
               localString += "0";
            }
         }
         localList.Add(localString);
      }
      PlayerPrefs.SetString(stateKey, MiniJSON.Json.Serialize(localList));
   }

   // restore prev state of tiles
   public void RestoreState() {
      if (String.IsNullOrEmpty(PlayerPrefs.GetString(stateKey)))
         return;
      
      var stateInfo = MiniJSON.Json.Deserialize(PlayerPrefs.GetString(stateKey)) as List<object>;

      // inAsyncRestore = false;
      if (stateInfo != null && stateInfo.Count > 0) {
         SetGridStateFromListObject(stateInfo, false);
      }
   }
   
   void HandleOnTileChanged(object sender, System.EventArgs e) {
      Tile.TileChangedEventArgs args = e as Tile.TileChangedEventArgs;
      tileStates[args.channel, args.slot] = args.state;
      SaveState();
   }

   // set entire grid data from remix playback data
   public void SetGridStateFromListObject(object data_o, bool saveState) {
      // create List from input object
      List<string> triggerList = new List<string>();
      foreach (object elem in data_o as List<object>) {
         triggerList.Add(elem as string);
      }
      // pad out if less data rows than channels
      if (triggerList.Count < nChannels) {
         for (int i = triggerList.Count-1; i < nChannels; i++) {
            triggerList.Add("");
         }
      }
      // truncate if more
      while (triggerList.Count>nChannels) 
         triggerList.RemoveAt(triggerList.Count - 1);

      // Set all grid cells present in the data
      char triggerChar = '1';
      int currChannel = 0;
      foreach (string triggerString in triggerList) {
         for (int i = 0; i < nSlots; i++) {
            bool state = false;
            if (triggerString.Length > 0 && i < triggerString.Length && triggerString[i] == triggerChar)
               state = true;
            Tiles[currChannel, i].SetPlayState(state);
            tileStates[currChannel, i] = state;
         }
         
         currChannel++;
      }
      if (saveState) 
         SaveState();
   }
   
   // Update is called once per frame
   void Update() {
   
   }

}
