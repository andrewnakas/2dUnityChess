    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SpecialMove{

    None = 0,
    EnPassant =1,
    Castling = 2,
    Promotion = 3
}

public class ChessBoard : MonoBehaviour
{
    public Material tileMaterial;
    public Material darktileMaterial;
    public ChessPiece currentlyDragging;
    public List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    private bool isWhiteTurn;
    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    private const int TileCountY = 8;
        private const int TileCountX = 8;
        private int tileCount;
        public GameObject[,] tiles;
        private Camera currentCamera;
        private Vector2Int currentHover;
         bool flipTiles= false;
         int yOffset =0;
         float tileSize = 1f;
         private SpecialMove specialMove;
         public ChessPiece[,] chessPieces;  
         [SerializeField]private float deathSize= .1f;
        [SerializeField]private float deathSpacing= .5f;
       [SerializeField] private float dragOffset = 0.75f;
        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private TMPro.TMP_Text victoryText;




    // Start is called before the first frame update
    void Start()
    {
      
        
    }
      void Awake()
    {
        isWhiteTurn =true;
        generateAllTiles(8,8);
        SpawnAllPieces();
        PositionAllPieces();
    }
    public void generateAllTiles( int tileX, int tileY){
        tiles = new GameObject[TileCountX,TileCountY];
        for(int x = 0; x < TileCountX; x++){
                 for(int y = 0; y < TileCountY; y++){
                tiles[x,y] = GenerateSingleTile(x,y);

                 }
        }

    }
    private GameObject GenerateSingleTile( int x, int y){
        tileCount++;
        
        GameObject tileObject = new GameObject(string.Format("X;{0}, Y:{1}", x,y));
        tileObject.transform.parent = transform;
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
       
        //this is code to alternate between chessboard
        if (tileCount %2 == 0){
            if (flipTiles == false){
            tileObject.AddComponent<MeshRenderer>().material = tileMaterial;
            } else {
            tileObject.AddComponent<MeshRenderer>().material = darktileMaterial; 
            }
        } else {
            if (flipTiles == false){
        tileObject.AddComponent<MeshRenderer>().material = darktileMaterial;
            } else {
            tileObject.AddComponent<MeshRenderer>().material = tileMaterial;
            }

        }
        if (tileCount %8 == 0){
        flipTiles = !flipTiles; 
        }
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3 (x * tileSize,  y* tileSize,0);
        vertices[1] = new Vector3 (x * tileSize,  (y+1)* tileSize,0);
        vertices[2] = new Vector3 ((x+1) * tileSize, y* tileSize,0);
        vertices[3] = new Vector3 ((x+1) * tileSize, (y+1)* tileSize,0);
        int [] tris = new int[]{0,1,2,1,3,2};
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Update is called once per frame
   private void Update()
    {
        if(!currentCamera)
    {
        currentCamera = Camera.main;
        return;
    }
    RaycastHit info;
     Ray  ray = currentCamera.ScreenPointToRay(Input.mousePosition);
    if ( Physics.Raycast(ray, out info,100,LayerMask.GetMask("Tile", "Hover","Highlight")))
    {
        Vector2Int hitPosition = LookupTileIndex (info.transform.gameObject);
                   // Debug. Log("hit" + hitPosition);

        if(currentHover == -Vector2Int.one){
            Debug. Log("hit" + hitPosition);
            currentHover = hitPosition;
            tiles [hitPosition.x,hitPosition.y].layer = LayerMask.NameToLayer("Hover");
        }
        if (currentHover != hitPosition){
             tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");;
            currentHover = hitPosition;
            tiles [hitPosition.x,hitPosition.y].layer = LayerMask.NameToLayer("Hover");
        }
    
        if (Input.GetMouseButtonDown(0)){

        if (chessPieces[hitPosition.x, hitPosition.y] != null){

            //is it our turn
            if((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && isWhiteTurn == false) ){

                currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
             availableMoves =  currentlyDragging.GetAvailableMoves(ref chessPieces, TileCountX,TileCountY);
             specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList,ref availableMoves);
             PreventCheck(); 
            HighlightTiles();
            }

        }


    }
     if (Input.GetMouseButtonUp(0)){
         if (currentlyDragging != null){
             Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX,currentlyDragging.currentY);
             bool validMove = MoveTo(currentlyDragging, hitPosition.x,hitPosition.y);

             if (!validMove){
                 currentlyDragging.SetPosition( getTileCenter(previousPosition.x,previousPosition.y));
                
             }
              currentlyDragging = null;
            RemoveHighlightTiles();
         }
        
    }
    }  else {
            if (currentHover != -Vector2Int.one)
                {
                 tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
                 }

                 if (currentlyDragging &&Input.GetMouseButtonUp(0) ){
                    currentlyDragging.SetPosition( getTileCenter(currentlyDragging.currentX,currentlyDragging.currentY));

                    currentlyDragging = null;
                    RemoveHighlightTiles();


                 }

    }
// if we are dragging a peice
    if (currentlyDragging){

        Plane HorizontalPlane = new Plane (Vector3.forward, Vector3.up * 0);
        float distance = 0.0f;
        if (HorizontalPlane.Raycast(ray, out distance )){
            currentlyDragging.SetPosition (  ray.GetPoint (distance) + (-Vector3.forward * dragOffset));


        }

    }

    }

    private void SpawnAllPieces(){
        chessPieces = new ChessPiece[TileCountX, TileCountY];
        int whiteTeam = 0;
        int blackTeam =1 ;
        //white team
        chessPieces[0,0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1,0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2,0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[4,0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[3,0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[5,0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6,0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7,0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[0,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        chessPieces[1,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        chessPieces[2,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        chessPieces[3,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        chessPieces[4,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        chessPieces[5,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        chessPieces[6,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        chessPieces[7,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        chessPieces[0,7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1,7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2,7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[4,7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[3,7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[5,7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6,7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7,7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[0,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        chessPieces[1,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        chessPieces[2,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        chessPieces[3,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        chessPieces[4,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        chessPieces[5,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        chessPieces[6,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        chessPieces[7,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);


    }

    private void PositionAllPieces(){
        for (int x = 0; x < TileCountX; x ++ ){
            for (int y = 0; y < TileCountY; y ++ ){
            if (chessPieces[x,y] != null){
                PositionSinglePiece(x,y,true);
            }

            }
            }

    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x,y].currentX = x;
        chessPieces[x,y].currentY = y;
        chessPieces[x,y].SetPosition(getTileCenter(x,y),force);

    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team){
        if (team == 0){
    ChessPiece cp = Instantiate(prefabs[(int)type -1], transform).GetComponent<ChessPiece>() ;
    cp.type = type;
    cp.team = team;
    return cp;
        } else {
      ChessPiece cp = Instantiate(prefabs[(int)type +5], transform).GetComponent<ChessPiece>() ;
    cp.type = type;
    cp.team = team;
    return cp;
        }
    }
    private Vector3 getTileCenter(int x, int y){
        return new Vector3 (x*tileSize, y * tileSize,yOffset) +  new Vector3 (tileSize/2, tileSize/2, 0);
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo){
         for(int x = 0; x < TileCountX; x++){
                 for(int y = 0; y < TileCountY; y++){

                 if (tiles[x,y] ==hitInfo)
                 return new Vector2Int(x,y);

                 
                 }}
                 return -Vector2Int.one; //invalid
    }
    private void HighlightTiles(){

        for (int i = 0; i < availableMoves.Count; i++)
        {
           tiles[availableMoves[i].x, availableMoves[i].y].layer =  LayerMask.NameToLayer("Highlight");
        }
    }
        private void RemoveHighlightTiles(){

        for (int i = 0; i < availableMoves.Count; i++)
        {
           tiles[availableMoves[i].x, availableMoves[i].y].layer =  LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
    }

private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos){
    for (int i = 0; i < moves.Count; i++)
    {
        if (moves[i].x == pos.x && moves[i].y == pos.y){
            return true;
        }
    }
    return false;
}
    private bool MoveTo(ChessPiece cp ,int x, int y){
        Vector2Int previousPosition = new Vector2Int(cp.currentX,cp.currentY);
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x,y)))
        {
            return false;
        }
        //is there another piece on the target position?
        if (chessPieces[x,y]  != null){
           ChessPiece ocp = chessPieces[x,y];
           if (cp.team == ocp.team){
               return false;
           }


           if (ocp.team == 0  ){
               if (ocp.type == ChessPieceType.King ){
                   CheckMate(0);
               }

               deadWhites.Add (ocp);
               ocp.SetScale (Vector3.one *deathSize);
               ocp.SetPosition (new Vector3 (8*tileSize, -tileSize/4, yOffset ) + new Vector3 (tileSize/2, 0,0) + (Vector3.up * deathSpacing) *deadWhites.Count);
           } else {
                  if (ocp.type == ChessPieceType.King ){
                   CheckMate(1);
               }
               deadBlacks.Add (ocp);
               ocp.SetScale (Vector3.one *deathSize);
               ocp.SetPosition (new Vector3 (-tileSize, -tileSize/4, yOffset ) + new Vector3 (tileSize/2, 0,0) + (Vector3.up * deathSpacing) *deadBlacks.Count);

           }
        }
        chessPieces[x,y] = cp;
        chessPieces[previousPosition.x,previousPosition.y] = null;
        PositionSinglePiece(x,y);
        //add to move list 
        moveList.Add (new Vector2Int[] {previousPosition , new Vector2Int(x,y) } );
        ProcessSpecialMove();

        if (checkForCheckMate() == true){
            
            if (cp.team == 1){
            CheckMate(0);
            } else {
            CheckMate(1);

            }


        }
        isWhiteTurn = !isWhiteTurn;
        return true;


    }
    private void ProcessSpecialMove(){
     if (specialMove == SpecialMove.EnPassant){
         var newMove = moveList[moveList.Count -1];
         ChessPiece winningPawn = chessPieces[newMove[1].x,newMove[1].y];
         var targetPawnPosition = moveList[moveList.Count -2];
         ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x,targetPawnPosition[1].y];
         if (winningPawn.currentX == enemyPawn.currentX){
             if (winningPawn.currentY == enemyPawn.currentY-1 || winningPawn.currentY == enemyPawn.currentY+1 ){
                 if (enemyPawn.team == 0){
                     deadWhites.Add(enemyPawn);
                     enemyPawn.SetScale (Vector3.one *deathSize);
                     enemyPawn.SetPosition (new Vector3 (8*tileSize, -tileSize/4, yOffset ) + new Vector3 (tileSize/2, 0,0) + (Vector3.up * deathSpacing) *deadWhites.Count);
                 } else {
                      deadBlacks.Add(enemyPawn);
                     enemyPawn.SetScale (Vector3.one *deathSize);
                     enemyPawn.SetPosition (new Vector3 (-tileSize, -tileSize/4, yOffset ) + new Vector3 (tileSize/2, 0,0) + (Vector3.up * deathSpacing) *deadBlacks.Count);
                 }
                 chessPieces[enemyPawn.currentX,enemyPawn.currentY] = null;
             }

         }
      }

      if (specialMove == SpecialMove.Promotion)
      {
          Vector2Int[] lastMove  = moveList[moveList.Count-1];
          ChessPiece targetPawn = chessPieces[lastMove[1].x,lastMove[1].y];

          if (targetPawn.type == ChessPieceType.Pawn)
          {
              if (targetPawn.team== 0 && lastMove[1].y == 7){
                  //change it to select promotion
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen,0);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
              }
                if (targetPawn.team== 1 && lastMove[1].y == 0){
                  //change it to select promotion
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen,1);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
              }
          }

      }
      if (specialMove == SpecialMove.Castling){
          var lastMove = moveList[moveList.Count -1];
            //left
          if (lastMove[1].x == 2 ){
              if (lastMove[1].y  == 0 ) {
                //white
                ChessPiece rook = chessPieces[0,0];
                chessPieces[3,0] = rook;
                PositionSinglePiece(3,0);
                chessPieces[0,0] = null;
              } else if (lastMove[1].y == 7){
                  //black
                ChessPiece rook = chessPieces[0,7];
                chessPieces[3,7] = rook;
                PositionSinglePiece(3,7);
                chessPieces[0,7] = null;

              }
            //right
          } else if (lastMove[1].x == 6){
                if (lastMove[1].y  == 0 ) {
                //white
                    ChessPiece rook = chessPieces[7,0];
                    chessPieces[5,0] = rook;
                    PositionSinglePiece(5,0);
                chessPieces[7,0] = null;
              } else if (lastMove[1].y == 7){
                  //black
                ChessPiece rook = chessPieces[7,7];
                chessPieces[5,7] = rook;
                PositionSinglePiece(5,7);
                chessPieces[7,7] = null;

              }


          }

      }  
    }
    private void PreventCheck(){
       ChessPiece targetKing = null;
       for (int x = 0; x < TileCountX; x++ ){
           for (int y = 0; y < TileCountY; y++){
               if (chessPieces[x,y] != null){
               if (chessPieces[x,y].type == ChessPieceType.King){
                   if (chessPieces[x,y].team == currentlyDragging.team){
                       targetKing = chessPieces[x,y];
                       //delete moves in avalible moves that will put us in check
                       SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves,targetKing);
                   }
               }
               }
           }
       }
        
    }
    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing){
        //this section could be make to make a really basic ai, with a second or third or however mayny more simulated set of check, for defending and attacking
        //save the current values to reset after function 
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();
        //going through all the moves, simualite them and check if we are in check
        for (int i =0; i < moves.Count; i++){
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX,targetKing.currentY);
            //did we simulate the kings move?
            if (cp.type == ChessPieceType.King){
                kingPositionThisSim = new Vector2Int(simX,simY);
            }
            //copy the chesspiece board for simulation 
            ChessPiece[,] simulation = new ChessPiece[TileCountX,TileCountY];
            List<ChessPiece> simulationAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TileCountX; x++ ){
                for (int y = 0; y < TileCountY; y++){
                    if (chessPieces[x,y]!= null){
                        simulation [x,y] = chessPieces[x,y];
                        if (simulation[x,y].team != cp.team){
                            simulationAttackingPieces.Add (simulation[x,y]);
                        }
                    }
                }
            }
            //simulate the move
            simulation[actualX,actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX,simY] = cp;

            //did one of pieces get taken down during simulation
            var deadPiece = simulationAttackingPieces.Find (c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null){
                simulationAttackingPieces.Remove(deadPiece);
            }

            //get all the simulated attacking pieces moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a< simulationAttackingPieces.Count; a++){
                var pieceMoves = simulationAttackingPieces[a].GetAvailableMoves(ref simulation, TileCountX,TileCountY);
                for (int b = 0; b < pieceMoves.Count; b++){
                    simMoves.Add (pieceMoves[b]);
                }

                //is the king in trouble if so remove the move
                if (ContainsValidMove(ref simMoves, kingPositionThisSim) ){

                   movesToRemove.Add (moves[i]); 
                }
                //Restore actual cp data
                cp.currentX = actualX;
                cp.currentY = actualY;
            }
        }
        //remove from the current list avalible. 
        for (int i = 0; i < movesToRemove.Count; i++){
            moves.Remove(movesToRemove[i]);
        }
    }

    private void CheckMate(int team){

        DisplayVictory(team);
    }

    private bool checkForCheckMate(){
        //hacky way to find last pieces team
        var lastMove = moveList[moveList.Count-1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0)? 1 : 0;
       List<ChessPiece> attackingPieces = new List<ChessPiece>();
       List<ChessPiece> defendingPieces = new List<ChessPiece>();
        //Debug.Log("target team" +targetTeam);
       ChessPiece targetKing = null;
       for (int x = 0; x < TileCountX; x++ ){
           for (int y = 0; y < TileCountY; y++){
               if (chessPieces[x,y] != null){
                if (chessPieces[x,y].team == targetTeam){
                  
                    defendingPieces.Add(chessPieces[x,y]);
                    if (chessPieces[x,y].type == ChessPieceType.King){
                        targetKing = chessPieces[x,y];
                        Debug.Log (x + y);
                    }
                } else {
                    attackingPieces.Add(chessPieces[x,y]);

                }
               }
           }
       }
       //is the king attacked right now
       List<Vector2Int> currentAvailibleMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++){
        var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, TileCountX,TileCountY);
                for (int b = 0; b < pieceMoves.Count; b++){
                    currentAvailibleMoves.Add (pieceMoves[b]);
                }
        }
        //are we in check rn
         List<Vector2Int> defendingMoveAvailible = new List<Vector2Int>(); 

        if (ContainsValidMove(ref currentAvailibleMoves, new Vector2Int( targetKing.currentX, targetKing.currentY ))){
            // king is under attack can we move something to help?
           // Debug.Log ("moves avalalible" +defendingMoveAvailible.Count + " " + defendingPieces.Count );

           for (int i = 0; i < defendingPieces.Count; i++ ){
           defendingMoveAvailible = defendingPieces[i].GetAvailableMoves(ref chessPieces, TileCountX, TileCountY);

           SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoveAvailible, targetKing);
           // Debug.Log ("moves avalalible" +defendingMoveAvailible.Count + " " + defendingPieces.Count );
            if (defendingMoveAvailible.Count != 0){
               return false;
           } 
           }
           return true;
           }
           // Debug.Log ("moves avalalible" +defendingMoveAvailible.Count );

          
           
         return false;


      
    }
    private void DisplayVictory(int team){
      if (team == 0){
        victoryText.text = "Black Wins";
      } else  {
        victoryText.text = "White Wins";
      }
      victoryScreen.SetActive(true);

    }
    public void onReset(){
      victoryScreen.SetActive(false);
      //fields reset
      currentlyDragging = null;
      availableMoves.Clear();
      moveList.Clear();
      //clean up
      for (int x = 0; x < TileCountX; x++){
          for (int y = 0; y < TileCountY; y++){
              if (chessPieces[x,y] != null){
                  Destroy(chessPieces[x,y].gameObject );
              chessPieces[x,y] = null;
              }       
      }
      }
      for  (int i =0; i < deadWhites.Count; i++){
          Destroy(deadWhites[i].gameObject);
      }
         for  (int i =0; i < deadBlacks.Count; i++){
          Destroy(deadBlacks[i].gameObject);
      }
      deadWhites.Clear();
      deadBlacks.Clear();

      //spawn stuff
      SpawnAllPieces();
      PositionAllPieces();
      isWhiteTurn = true;

    }
    public void onExit(){

    }
}
