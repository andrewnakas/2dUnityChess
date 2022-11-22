using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum SpecialMove{

    None = 0,
    EnPassant =1,
    Castling = 2,
    Promotion = 3
}
public class ThreeFoldCheckClass {
           
    public ChessPiece[,] ChessPiecesTF;
    public bool HasLeftBlackCastles;
    public bool HasRightBlackCastles;
       public bool HasLeftWhiteCastles;
    public bool HasRightWhiteCastles;
    public bool HasEnPassant;
           
    public ThreeFoldCheckClass (ChessPiece[,] chessPiecesTF, bool hasLeftWhiteCastles, bool hasRightWhiteCastles,bool hasLeftBlackCastles, bool hasRightBlackCastles, bool hasEnPassant ){
        ChessPiecesTF = chessPiecesTF;
        HasLeftBlackCastles = hasLeftBlackCastles;  
        HasRightBlackCastles = hasRightBlackCastles; 
        HasLeftWhiteCastles = hasLeftWhiteCastles;  
        HasRightWhiteCastles = hasRightWhiteCastles; 
        HasEnPassant = hasEnPassant;       
      
            }
          }
        
public class ChessBoard : MonoBehaviour
{
    public AiManager AiCTR;
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
          
          
          //castle check mainly needed for threefold draw check
          private bool hasForfietedWhiteCastleKingsSize;
               
        private bool hasForfietedWhiteCastleQueensSize;
        private bool hasForfietedBlackCastleKingsSize;
        private bool hasForfietedBlackCastleQueensSize;

        //promotion wait
       [SerializeField]private GameObject promotionSelection;
        private bool isSelectingPromotion;

     
       //notation 
       //this one is for X
       private bool isInCheck;
        private string lastMoveNotation;
        private bool didLastMoveCapture;
        private bool didCastleKingsSize;
        private bool didCastleQueensSize;
        //this is used for a promotion ui to delay the next turn untill they perdict the 
        private bool didSelectPromotion;
        private bool didPromoteQueen;
        private bool didPromoteRook;
        private bool didPromoteBishop;
        private bool didPromoteKnight;
        //ok now we need to add all the sprites so we can reasign the buttons
        [SerializeField]private Sprite WhiteQueenImage;
        [SerializeField]private Sprite WhiteRookImage;
        [SerializeField]private Sprite WhiteBishopImage;
        [SerializeField]private Sprite WhiteKnightImage;

        [SerializeField]private Sprite BlackQueenImage;
        [SerializeField]private Sprite BlackRookImage;
        [SerializeField]private Sprite BlackBishopImage;
        [SerializeField]private Sprite BlackKnightImage;

        //stockfish ai 
        private bool WhiteStockFishToMove = false;
        private bool BlackStockFishToMove = false;
        private string previousFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";





       //this is called either if white castles or forfiets castle rights
       
        private bool threeFoldCastle;
        private bool threeFoldEnPassant;

        //this is tracking en passant on fen string, is so it can cordinate with y
        private int pawnDoubleJump = -1;

         public List <ThreeFoldCheckClass> threeFoldChessPiecesCheck = new List <ThreeFoldCheckClass>();  

        [SerializeField]private TMPro.TMP_Text notationText;
        private List<string> moveNotationList = new List<string>();
        private int moveNumber;
        private bool movedPawn;
        private bool capturedPiece;
        private int movesSincePawnMoveOrCapture;
        public int realMoveNumber;
    // Start is called before the first frame update
    void Start()
    {
    
    }
      void Awake()
    {
        isWhiteTurn =true;
        GenerateAllTiles(8,8);
        SpawnAllPieces();
        PositionAllPieces();
     int testMoves =    MoveGenerationTest(2);
     Debug.Log (testMoves);
    }
    public void GenerateAllTiles( int tileX, int tileY){
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
   //should move this to after you pick up piece but for now its ok here
   if ((isWhiteTurn && AiCTR.isWhiteStockfish == false ) || (!isWhiteTurn && AiCTR.isBlackStockfish == false)){


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
             specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList,ref availableMoves,TileCountX,TileCountY);
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
    } else {
        if (isWhiteTurn){
           if (WhiteStockFishToMove == false ){
               makeStockFishMove(previousFen);
               WhiteStockFishToMove = true;
               
           } 
        } else {
                if (BlackStockFishToMove == false ){
                makeStockFishMove(previousFen);
                WhiteStockFishToMove = true;

           } 
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

    public void makeStockFishMove(string fen){
        AiCTR.stocky.stockData="";
        AiCTR.stocky.FenToStock(fen);
        StartCoroutine (waitForStock());

    }
    public IEnumerator waitForStock(){


        yield return new WaitUntil(() =>   AiCTR.stocky.stockData != "");
        
        Debug.Log ("Stock Data is " + AiCTR.stocky.stockData );
        //Process data and do move;
        //WhiteStockFishToMove = false;
        //BlackStockFishToMove = false;
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
public IEnumerator ProcessCheck(){

    yield return new WaitForSeconds(3);
    Debug.Log ("waiting");
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
               didLastMoveCapture = true;
               //its redundant but because we reset it in different spots lets just do it
               capturedPiece = true;

               deadWhites.Add (ocp);
               ocp.SetScale (Vector3.one *deathSize);
               ocp.SetPosition (new Vector3 (8*tileSize, -tileSize/4, yOffset ) + new Vector3 (tileSize/2, 0,0) + (Vector3.up * deathSpacing) *deadWhites.Count);
           } else {
                  if (ocp.type == ChessPieceType.King ){
                   CheckMate(1);
               }
               didLastMoveCapture = true;
                capturedPiece = true;

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
        //check for moved pawn 50 move draw rule
        if (cp.type == ChessPieceType.Pawn){
            movedPawn = true;
        }
       // StartCoroutine (ProcessCheck());
      //  Debug.Log ("process Check 1");
        ProcessSpecialMove();
        //double jump for fen
        if(cp.type == ChessPieceType.Pawn){
            if (Mathf.Abs(previousPosition.y -cp.currentY ) == 2){
                pawnDoubleJump = cp.currentX;
            }

        }
       
        if (checkForCheckMate() == true){
            
            if (cp.team == 1){
            CheckMate(0);
            } else {
            CheckMate(1);
            }
        }
        if (cp.team == 0 && (hasForfietedWhiteCastleQueensSize == false ||hasForfietedWhiteCastleKingsSize == false    ) )
        {       
       CheckforCastles(cp, previousPosition);
        } 
         if ( cp.team ==1 && (hasForfietedBlackCastleQueensSize == false ||hasForfietedBlackCastleKingsSize == false))
         {
        CheckforCastles(cp, previousPosition);
       }

     
        //is selecting promotion means the turn is not over until you select your piece you want
        if (isSelectingPromotion == false ){
        previousFen = GenerateFenFromBoard(chessPieces,isWhiteTurn, !hasForfietedWhiteCastleQueensSize, !hasForfietedWhiteCastleKingsSize,!hasForfietedBlackCastleQueensSize,!hasForfietedBlackCastleKingsSize,pawnDoubleJump);
        ProcessNotation();
        //lets check for them draws
        CheckForInsufficientDraw();
        CheckForNoMovesDraw();
        CheckForThreeFoldDraw();
        CheckFor50MoveDraw();
        //Debug.Log (fen);
        isWhiteTurn = !isWhiteTurn;

       
       
        }

        
        return true;


    }
    private void ProcessNotation(){
    moveNumber ++;
     realMoveNumber= 1;
    //this his how to get the actual move number
    if (moveNumber %2 != 0){
        //its an odd number 
        realMoveNumber = (moveNumber+1 )/2;
    } else {
        realMoveNumber = moveNumber/2;
    }
    Vector2Int[] lastMove =    moveList[moveList.Count - 1];
       string s = convertMoveArrayToChessCords(lastMove);
        Debug.Log ("last move was" + lastMove[1] + "and the notation is " + s );
        moveNotationList.Add (s);
        //we are prob gonna wanna display that somewhere
       // lastMoveNotation = 
      if (moveNumber %2 != 0){
       notationText.text += realMoveNumber + ". " + s+ " "; 
      } else {
         notationText.text += s + " ";
      }
    }
    private string convertMoveArrayToChessCords(Vector2Int[] move)
    {
       
        string s = "";
        //if we castle we do none of this notation so lets skip it with an iff statement
        if (didCastleKingsSize == true || didCastleQueensSize == true){
            if (didCastleKingsSize == true){
                s += "0-0";
            } 
            if (didCastleQueensSize == true){
                s+= "0-0-0";
            }
        } else {

        //lets find the specific piece name
        // pawns are represented by nothing
        if (chessPieces[move[1].x,move[1].y].type == ChessPieceType.King)
            s += "K";
        if (chessPieces[move[1].x,move[1].y].type == ChessPieceType.Queen){
           //on Queens Rooks bishops and knights we need to check if there are another same piece type that
           //can move to the square as well and if true, we have to add the piece that moved notation
            s += "Q";  
            if (canOtherPieceMoveToSquare(move[1].x,move[1].y,chessPieces[move[1].x,move[1].y].type) == true){
                // we are supposed to only notate the full move notation if like a queen is in both diagnal and row from square but fuck it for now
                //ideally we would just return row or file only if say they are in same row or something but lets do it for now like this untill ai
                 s+= returnProperFile (move[0].x);
                 s+= (move[0].y +1).ToString();
            }
        }
        if (chessPieces[move[1].x,move[1].y].type == ChessPieceType.Knight){
            s += "N";
                  if (canOtherPieceMoveToSquare(move[1].x,move[1].y,chessPieces[move[1].x,move[1].y].type) == true){
                 s+= returnProperFile (move[0].x);
                 s+= (move[0].y +1).ToString();
            }
        }
        if (chessPieces[move[1].x,move[1].y].type == ChessPieceType.Rook){
            s += "R";
            if (canOtherPieceMoveToSquare(move[1].x,move[1].y,chessPieces[move[1].x,move[1].y].type) == true){
                 s+= returnProperFile (move[0].x);
                 s+= (move[0].y +1).ToString();
            }
        }
        if (chessPieces[move[1].x,move[1].y].type == ChessPieceType.Bishop){
            s += "B";
                if (canOtherPieceMoveToSquare(move[1].x,move[1].y,chessPieces[move[1].x,move[1].y].type) == true){
                 s+= returnProperFile (move[0].x);
                 s+= (move[0].y +1).ToString();
            }
        }

        if (didLastMoveCapture == true){
            // we captured so first we need to check if it was a pawn that captured 
           if ( chessPieces[move[1].x,move[1].y].type == ChessPieceType.Pawn){
               //if it is pawn we need to add the file it moved from
                s+= returnProperFile(move[0].x);
           }
           // ok now add an x to signal a capture on that square
           s+= "x";
        }

       s+= returnProperFile (move[1].x);
        s += (move[1].y +1).ToString();
        //check if pawn promoted
        if(didPromoteQueen == true){
            s+= "Q";
        }
        if(didPromoteRook == true){
            s+= "R";
        }
        if(didPromoteKnight == true){
            s+= "N";
        }
        if(didPromoteBishop == true){
            s+= "N";
        }
        }
        if (isInCheck == true){
            s+= "+";
        }
       //reset bools
       isInCheck = false;
        didPromoteBishop = false;
        didPromoteQueen = false;
        didPromoteRook = false;
        didPromoteKnight = false;
       didLastMoveCapture = false;
       didCastleKingsSize = false;
       didCastleQueensSize = false;
        return s;
    }
    private string returnProperFile(int i){
       //covert the ones to letters
        string s = "";
        if (i == 0)
       s += "a";
         if (i == 1)
       s += "b";
         if (i == 2)
       s += "c";
         if (i == 3)
       s += "d";
         if (i== 4)
       s += "e";
         if (i == 5)
       s += "f";
         if (i == 6)
       s += "g";
         if (i == 7)
       s += "h";
        return s;
    }
    private bool canOtherPieceMoveToSquare( int xSquare, int ySquare, ChessPieceType pieceToCheck){
       //can other pieces of the same type also move to square? this is for notation check arount if we need to add begining clarfier or specific peice
        
        List<ChessPiece> sameTeamSamePiece = new List<ChessPiece>();
        for (int x = 0; x > TileCountX; x++){
            for (int y = 0; y > TileCountY; y++){
                 if (chessPieces[x,y] != null){
                     //is on same team
                     if (chessPieces[x,y].team == chessPieces[xSquare,ySquare].team){
                    //this is to make sure we dont include the piece on the actual square
                     if (x!= xSquare && y != ySquare){
                    if (chessPieces[x,y].type == pieceToCheck){
                    //its the same piece but not the one we moved
                    sameTeamSamePiece.Add(chessPieces[x,y]);
                    }
                    }
                     }
        }
    }
    }
    for (int a = 0; a > sameTeamSamePiece.Count; a++){
        //get avalible moves for the other same pieces and if they hit the target piece square then
            List<Vector2Int> pieceMoves = sameTeamSamePiece[a].GetAvailableMoves(ref chessPieces, TileCountX,TileCountY);
                for (int b = 0; b < pieceMoves.Count; b++){
                   if (pieceMoves[b] == new Vector2Int(xSquare,ySquare)){
                       //this means another piece of the same type can hit the square
                       return true;
                   }
                }
                }

    return false;
    }
    public void ProcessPromotionUi(){
        promotionSelection.SetActive(true);
        Vector2Int[] lastMove  = moveList[moveList.Count-1];

        if (isWhiteTurn == true){
            //white team
       //set postion to that of last Pawn
       
        promotionSelection.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position; 
        promotionSelection.transform.eulerAngles = new Vector3 (0,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(0).transform.localEulerAngles = new Vector3 (0,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(1).transform.localEulerAngles = new Vector3 (0,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(2).transform.localEulerAngles = new Vector3 (0,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(3).transform.localEulerAngles = new Vector3 (0,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().sprite = WhiteQueenImage;
        promotionSelection.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().sprite = WhiteRookImage;
        promotionSelection.transform.GetChild(0).transform.GetChild(2).GetComponent<Image>().sprite = WhiteBishopImage;
        promotionSelection.transform.GetChild(0).transform.GetChild(3).GetComponent<Image>().sprite = WhiteKnightImage;

        //change 
        } else {
        promotionSelection.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position; 
      
        promotionSelection.transform.GetChild(0).transform.GetChild(0).transform.localEulerAngles = new Vector3 (180,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(1).transform.localEulerAngles = new Vector3 (180,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(2).transform.localEulerAngles = new Vector3 (180,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(3).transform.localEulerAngles = new Vector3 (180,0,0);
        promotionSelection.transform.eulerAngles = new Vector3 (180,0,0);
        promotionSelection.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().sprite = BlackQueenImage;
        promotionSelection.transform.GetChild(0).transform.GetChild(1).GetComponent<Image>().sprite = BlackRookImage;
        promotionSelection.transform.GetChild(0).transform.GetChild(2).GetComponent<Image>().sprite = BlackBishopImage;
        promotionSelection.transform.GetChild(0).transform.GetChild(3).GetComponent<Image>().sprite = BlackKnightImage;


        //for black rotate position 180
        //spawn in black pieces for button sprites
        // set position to black 
        }
    }
    public void ProcessQueenSelect(){

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
                  //eventually gonna do ui to select
                  didPromoteQueen = true;

              }
                if (targetPawn.team== 1 && lastMove[1].y == 0){
                  //change it to select promotion
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen,1);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                //eventually gonna do ui to select
                  didPromoteQueen = true;
              }
          }
          promotionSelection.SetActive(false);
          isSelectingPromotion = false;
            previousFen = GenerateFenFromBoard(chessPieces,isWhiteTurn, !hasForfietedWhiteCastleQueensSize, !hasForfietedWhiteCastleKingsSize,!hasForfietedBlackCastleQueensSize,!hasForfietedBlackCastleKingsSize,pawnDoubleJump);

            //make the process happen because we skipped it at the end. 
            ProcessNotation();
            //lets check for them draws
            CheckForInsufficientDraw();
            CheckForNoMovesDraw();
            CheckForThreeFoldDraw();
            CheckFor50MoveDraw();
            isWhiteTurn =!isWhiteTurn;
    }
        public void ProcessRookSelect(){

          Vector2Int[] lastMove  = moveList[moveList.Count-1];
          ChessPiece targetPawn = chessPieces[lastMove[1].x,lastMove[1].y];

          if (targetPawn.type == ChessPieceType.Pawn)
          {
              if (targetPawn.team== 0 && lastMove[1].y == 7){
                  //change it to select promotion
                  
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Rook,0);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                  //eventually gonna do ui to select
                  didPromoteRook = true;

              }
                if (targetPawn.team== 1 && lastMove[1].y == 0){
                  //change it to select promotion
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Rook,1);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                //eventually gonna do ui to select
                  didPromoteRook = true;
              }
          }
          promotionSelection.SetActive(false);
          isSelectingPromotion = false;
           previousFen = GenerateFenFromBoard(chessPieces,isWhiteTurn, !hasForfietedWhiteCastleQueensSize, !hasForfietedWhiteCastleKingsSize,!hasForfietedBlackCastleQueensSize,!hasForfietedBlackCastleKingsSize,pawnDoubleJump);

            //make the process happen because we skipped it at the end. 
            ProcessNotation();

            //lets check for them draws
            CheckForInsufficientDraw();
            CheckForNoMovesDraw();
            CheckForThreeFoldDraw();
            CheckFor50MoveDraw();
            isWhiteTurn =!isWhiteTurn;
    }
         public void ProcessBishopSelect(){

          Vector2Int[] lastMove  = moveList[moveList.Count-1];
          ChessPiece targetPawn = chessPieces[lastMove[1].x,lastMove[1].y];

          if (targetPawn.type == ChessPieceType.Pawn)
          {
              if (targetPawn.team== 0 && lastMove[1].y == 7){
                  //change it to select promotion
                  
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Bishop,0);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                  //eventually gonna do ui to select
                  didPromoteBishop= true;

              }
                if (targetPawn.team== 1 && lastMove[1].y == 0){
                  //change it to select promotion
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Bishop,1);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                //eventually gonna do ui to select
                  didPromoteBishop = true;
              }
          }
          promotionSelection.SetActive(false);
          isSelectingPromotion = false;
            previousFen = GenerateFenFromBoard(chessPieces,isWhiteTurn, !hasForfietedWhiteCastleQueensSize, !hasForfietedWhiteCastleKingsSize,!hasForfietedBlackCastleQueensSize,!hasForfietedBlackCastleKingsSize,pawnDoubleJump);
            //make the process happen because we skipped it at the end. 
            ProcessNotation();
            //lets check for them draws
            CheckForInsufficientDraw();
            CheckForNoMovesDraw();
            CheckForThreeFoldDraw();
            CheckFor50MoveDraw();
            isWhiteTurn =!isWhiteTurn;
    }
          public void ProcessKnightSelect(){

          Vector2Int[] lastMove  = moveList[moveList.Count-1];
          ChessPiece targetPawn = chessPieces[lastMove[1].x,lastMove[1].y];

          if (targetPawn.type == ChessPieceType.Pawn)
          {
              if (targetPawn.team== 0 && lastMove[1].y == 7){
                  //change it to select promotion
                  
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Knight,0);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                  //eventually gonna do ui to select
                  didPromoteKnight= true;

              }
                if (targetPawn.team== 1 && lastMove[1].y == 0){
                  //change it to select promotion
                  ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Knight,1);
                  newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].gameObject.transform.position;
                  Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                  chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                  PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                //eventually gonna do ui to select
                  didPromoteKnight = true;
              }
          }
          promotionSelection.SetActive(false);
          isSelectingPromotion = false;
           previousFen= GenerateFenFromBoard(chessPieces,isWhiteTurn, !hasForfietedWhiteCastleQueensSize, !hasForfietedWhiteCastleKingsSize,!hasForfietedBlackCastleQueensSize,!hasForfietedBlackCastleKingsSize,pawnDoubleJump);
            //make the process happen because we skipped it at the end. 
            ProcessNotation();
            //lets check for them draws
            CheckForInsufficientDraw();
            CheckForNoMovesDraw();
            CheckForThreeFoldDraw();
            CheckFor50MoveDraw();
            isWhiteTurn =!isWhiteTurn;
    }

    private void ProcessSpecialMove(){
     if (specialMove == SpecialMove.EnPassant){
         threeFoldEnPassant = true;
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
    isSelectingPromotion = true;
     //Debug.Log ("Promotion!!!");
      ProcessPromotionUi();

      }
      if (specialMove == SpecialMove.Castling){
          threeFoldCastle = true;

          var lastMove = moveList[moveList.Count -1];
            //left
          if (lastMove[1].x == 2 ){
              if (lastMove[1].y  == 0 ) {
                //white
                ChessPiece rook = chessPieces[0,0];
                hasForfietedWhiteCastleKingsSize = true;
                hasForfietedWhiteCastleQueensSize = true;

                chessPieces[3,0] = rook;
                PositionSinglePiece(3,0);
                chessPieces[0,0] = null;
              } else if (lastMove[1].y == 7){
                  //black
                ChessPiece rook = chessPieces[0,7];
                hasForfietedBlackCastleKingsSize = true;
                hasForfietedBlackCastleQueensSize = true;
                chessPieces[3,7] = rook;
                PositionSinglePiece(3,7);
                chessPieces[0,7] = null;
                

              }
              didCastleKingsSize = true;
              
            //right
          } else if (lastMove[1].x == 6){
                if (lastMove[1].y  == 0 ) {
                //white
                    ChessPiece rook = chessPieces[7,0];
                    hasForfietedWhiteCastleKingsSize = true;
                    hasForfietedWhiteCastleQueensSize = true;
                    chessPieces[5,0] = rook;
                    PositionSinglePiece(5,0);
                chessPieces[7,0] = null;
              } else if (lastMove[1].y == 7){
                  //black
                //threefold draw check
                hasForfietedBlackCastleKingsSize = true;
                hasForfietedBlackCastleQueensSize = true;
                ChessPiece rook = chessPieces[7,7];
                
                chessPieces[5,7] = rook;
                PositionSinglePiece(5,7);
                chessPieces[7,7] = null;

              }

            didCastleQueensSize = true;
 
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
       ChessPiece targetKing = null;
       for (int x = 0; x < TileCountX; x++ ){
           for (int y = 0; y < TileCountY; y++){
               if (chessPieces[x,y] != null){
                if (chessPieces[x,y].team == targetTeam){
                  
                    defendingPieces.Add(chessPieces[x,y]);
                    if (chessPieces[x,y].type == ChessPieceType.King){
                        targetKing = chessPieces[x,y];
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
           //for notation
            isInCheck = true;
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
      private void DisplayDraw(int drawType){
       if (drawType == 1){
        victoryText.text = "Draw: Insufficient mating material";
       } else  if (drawType == 2){
        victoryText.text = "Draw: Threefold-repetition rule";

       } else  if (drawType == 3){
        victoryText.text = "Draw: 50 move rule";

       
       }
       else  if (drawType == 4){
        victoryText.text = "Draw: Stalemate";
       
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
      notationText.text = "Move List: ";
      isWhiteTurn = true;
      

    }
    private void CheckforCastles(ChessPiece cp, Vector2Int startMove){
        Debug.Log ("check for castles" + cp.currentX + " " + cp.currentY);
if (cp.team == 0 ){
if (cp.type == ChessPieceType.King){
if (cp.currentX != 4 ||  cp.currentY != 0){
 Debug.Log ("white king moved");

    //lost both rights
    hasForfietedWhiteCastleKingsSize =true;
    hasForfietedWhiteCastleQueensSize =true;
    threeFoldCastle = true;
}
}
if (cp.type == ChessPieceType.Rook){
if (startMove == new Vector2Int(0 ,0) ){

    hasForfietedWhiteCastleKingsSize =true;
    threeFoldCastle = true;

//left castle canceled
    
}  
if (startMove == new Vector2Int(7,0) ){
    hasForfietedWhiteCastleQueensSize =true;
    threeFoldCastle = true;


//right castle canceled
    
}  
}

}
if (cp.team == 1 ){
if (cp.type == ChessPieceType.King){
if (cp.currentX != 4 ||  cp.currentY != 7){

    //lost both rights
    hasForfietedBlackCastleKingsSize =true;
    hasForfietedBlackCastleQueensSize =true;
    threeFoldCastle = true;

}
}
if (cp.type == ChessPieceType.Rook){
if (startMove == new Vector2Int(0,7) ){

     hasForfietedBlackCastleKingsSize =true;
     threeFoldCastle = true;

//left castle canceled
    
}  
if (startMove == new Vector2Int(7,7) ){
    hasForfietedBlackCastleQueensSize =true;
    threeFoldCastle = true;
//right castle canceled   
}  
}
}
  }
    private void CheckForInsufficientDraw(){
        //this draw happens when both teams run out of pieces
        //it can happen when they have
        // no pawns lone king, king and bishop, king and knight
        // or wierd one king and two knights verus lone king
        //force mate is still possible with king and two knights versus king and bishop
        //fide are madlads and still allow king and two knights versus lone king to continue because 
        //there is no forced mate, but a mate is still possible if you go in the right place
        //follow chess dot com on this one and just make it a draw

        //first lets create a list of all the pieces on both teams
        List<ChessPiece> whitePieces = new List<ChessPiece>();
        List<ChessPiece> blackPieces = new List<ChessPiece>();
             for (int x = 0; x < TileCountX; x++ ){
           for (int y = 0; y < TileCountY; y++){
               if (chessPieces[x,y] != null){
                if (chessPieces[x,y].team == 0){
                    whitePieces.Add(chessPieces[x,y]);
             } else  {
                    blackPieces.Add(chessPieces[x,y]);
              }
             }}}
            // ok now that we got all the remaning peices
            //lets check to see if we got enough material
            int whiteBishopCount = 0;
            int whiteKnightCount = 0;
            bool whiteInsufficent = false;
            bool whiteJustHasTwoKnights= false;
            bool whiteHasJustKing= false;

            for (int i = 0; i < whitePieces.Count; i++){
                if (whitePieces[i].type ==ChessPieceType.Pawn ||whitePieces[i].type ==ChessPieceType.Queen ||  whitePieces[i].type ==ChessPieceType.Rook ){
                // if its one of the three pieces just return out 
                return;
                }
                if (whitePieces[i].type ==ChessPieceType.Knight){
                whiteKnightCount++;
                } 
                if (whitePieces[i].type ==ChessPieceType.Bishop){
                whiteBishopCount++;
                } 
            }
            //ok now that we are done with our for loop and we counted what we got
            //lets check for the pieces on the team
            if (whiteBishopCount < 2 && whiteKnightCount == 0){
                whiteInsufficent = true;
            }
            if (whiteBishopCount == 0 && whiteKnightCount < 2){
                whiteInsufficent = true;
            }
            if (whiteBishopCount == 0 && whiteKnightCount == 0){
                whiteHasJustKing = true;
            }
            if (whiteKnightCount == 2 && whiteBishopCount == 0){
                whiteJustHasTwoKnights = true;
            }
            //ok now lets do the same for black
             int blackBishopCount =0;
            int blackKnightCount = 0;
            bool blackInsufficent= false;
            bool blackJustHasTwoKnights= false;
            bool blackHasJustKing= false;
               for (int i = 0; i < blackPieces.Count; i++){
                if (blackPieces[i].type ==ChessPieceType.Pawn ||blackPieces[i].type ==ChessPieceType.Queen ||  blackPieces[i].type ==ChessPieceType.Rook ){
                // if its one of the three pieces just return out 
                return;
                }
                if (blackPieces[i].type ==ChessPieceType.Knight){
                blackKnightCount++;
                } 
                if (blackPieces[i].type ==ChessPieceType.Bishop){
                blackBishopCount++;
                } 
            }
            //ok now that we are done with our for loop and we counted what we got
            //lets check for the pieces on the team
            if (blackBishopCount < 2 && blackKnightCount == 0){
                blackInsufficent = true;
            }
            if (blackBishopCount == 0 && blackKnightCount < 2){
                blackInsufficent = true;
            }
            if (blackBishopCount == 0 && blackKnightCount == 0){
                blackHasJustKing = true;
            }
            if (blackKnightCount == 2 && blackBishopCount == 0){
                blackJustHasTwoKnights = true;
            }
            //ok now based on this lets figure out what is up 
            if (blackInsufficent && whiteInsufficent){
               DisplayDraw(1); 
            }
            if (whiteJustHasTwoKnights == true && blackHasJustKing){
                DisplayDraw(1); 
            }
            if (blackJustHasTwoKnights == true && whiteHasJustKing){
                DisplayDraw(1); 
            }
    }
    
    private void CheckForThreeFoldDraw(){
        //ok this happens if a position happens three times in the same game
        //store every board ok and if three match to one another
          //use find all to find if there are three matching positions on the last board
     //good post about the topic, castling rights and en passant rights also count into the threefold
     // https://www.chessprogramming.org/Repetitions
     
        int arraySameCount = 0;
//     Debug.Log  (threeFoldChessPiecesCheck.Count + "  ");
     for (int i = 0; i < threeFoldChessPiecesCheck.Count; i++){
         bool equal = false;
       // chessPieces,hasForfietedBlackCastleKingsSize,hasForfietedBlackCastleQueensSize,threeFoldEnPassant, TileCountX,Tieck[i].ChessPiecesTF,leCount
        equal = ArePositionsTheSame(threeFoldChessPiecesCheck[i].ChessPiecesTF, threeFoldChessPiecesCheck[i].HasLeftWhiteCastles,threeFoldChessPiecesCheck[i].HasRightWhiteCastles,  threeFoldChessPiecesCheck[i].HasLeftBlackCastles,threeFoldChessPiecesCheck[i].HasRightBlackCastles,threeFoldChessPiecesCheck[i].HasEnPassant,  chessPieces ,hasForfietedWhiteCastleKingsSize, hasForfietedWhiteCastleQueensSize,hasForfietedBlackCastleKingsSize,hasForfietedBlackCastleQueensSize,threeFoldEnPassant,TileCountX,TileCountY);
         
       // Debug.Log (equal);
         if (equal == true)
            {
                arraySameCount++;
                Debug.Log ("same position" + arraySameCount);  
        } 
        if (arraySameCount == 2)
        {
            //arrays have matched twice with the current position we can call a threefold draw
        DisplayDraw(2);
        }

//        Debug.Log (arraySameCount);
     }
    
  
     //copy the instance of the board at time or else old references will always return true;
     ChessPiece[,] simulation = new ChessPiece[TileCountX,TileCountY];
          //List<ChessPiece> simulationAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TileCountX; x++ ){
                for (int y = 0; y < TileCountY; y++){
                    if (chessPieces[x,y]!= null){
                        simulation [x,y] = chessPieces[x,y];
                    }
                }
            }
            
            //before we add, we check if en passant or castlerightsforfieted;
               // add current position to the board
//               Debug.Log (threeFoldCastle  + " " +threeFoldEnPassant +  " arraySameCount" + arraySameCount );
  //          Debug.Log ("added board to threefold checl list");
           
             ThreeFoldCheckClass theeFolder = new ThreeFoldCheckClass(simulation, hasForfietedWhiteCastleKingsSize,hasForfietedWhiteCastleQueensSize, hasForfietedBlackCastleKingsSize,hasForfietedBlackCastleQueensSize,threeFoldEnPassant);
            threeFoldChessPiecesCheck.Add(theeFolder);
            
           
            // now reset them should happen 4 times for castles and upto 8 times for 
           threeFoldEnPassant = false;
           threeFoldCastle = false;
  
    }
    private void CheckForNoMovesDraw(){
        // get all the pieces, if the team to move team has no moves then its a draw
       
       
          List<ChessPiece> whitePieces = new List<ChessPiece>();
        List<ChessPiece> blackPieces = new List<ChessPiece>();
             for (int x = 0; x < TileCountX; x++ ){
           for (int y = 0; y < TileCountY; y++){
               if (chessPieces[x,y] != null){
                if (chessPieces[x,y].team == 0){
                    whitePieces.Add(chessPieces[x,y]);
             } else  {
                    blackPieces.Add(chessPieces[x,y]);
              }
             }}}
             if(isWhiteTurn == false){
                 //black just went check for white moves
            int TotalWhiteMoves = 0;
            for (int a = 0; a < whitePieces.Count; a++){
             //get avalible moves for the other same pieces and if they hit the target piece square then
            List<Vector2Int> pieceMoves = whitePieces[a].GetAvailableMoves(ref chessPieces, TileCountX,TileCountY);
           
             TotalWhiteMoves +=  pieceMoves.Count; 

                }
              //  Debug.Log ("This many avalible moves "+ TotalWhiteMoves);


                if (TotalWhiteMoves == 0){
                    DisplayDraw(4);
                }
             } else 
             {

                 //white just went check for black stalemate
                   int TotalBlackMoves = 0;
            for (int a = 0; a < blackPieces.Count; a++){
             //get avalible moves for the other same pieces and if they hit the target piece square then
            List<Vector2Int> pieceMoves = blackPieces[a].GetAvailableMoves(ref chessPieces, TileCountX,TileCountY);
             TotalBlackMoves +=  pieceMoves.Count; 
               //  Debug.Log ("found the moves" + pieceMoves.Count);
                }
               // Debug.Log ("This many avalible moves "+ TotalBlackMoves);

                if (TotalBlackMoves == 0){
                    DisplayDraw(4);
                }

             }
    }
    private void CheckFor50MoveDraw(){
        //50 moves without a captured piece of pawn moved
      
        movesSincePawnMoveOrCapture++;
       // its 100 because there two moves in a move number black and white
        if (movesSincePawnMoveOrCapture >= 100){
            DisplayDraw(3);

        }
          //50 moves without a captured piece of pawn moved
        if (capturedPiece == true || movedPawn == true){
            //reset
            movesSincePawnMoveOrCapture = 0;
        }
        capturedPiece = false;
        movedPawn = false;
    }

    public void onExit(){

    }
   private bool ArePositionsTheSame(ChessPiece[,] a,  bool hasLeftWhiteCastles, bool hasRightWhiteCastles, bool hasLeftBlackCastles, bool hasRightBlackCastles,bool hasEnPassant ,ChessPiece[,] b, bool leftWhiteCastles, bool rightWhiteCastles, bool leftBlackCastles, bool rightBlackCastles, bool enPassant, int width,int height) {
  for (int x=0; x<width; x++) {
   for (int y=0; y<height; y++) {
   // Debug.Log (a[x,y] +" " + x +" " + y + "   " + "current position " +b[x,y]);

    if (a[x,y] != b[x,y]) return false;
   }
  }

  if (hasLeftWhiteCastles == leftWhiteCastles &&  hasRightWhiteCastles  == rightWhiteCastles &&hasLeftBlackCastles == leftBlackCastles &&  hasRightBlackCastles  == rightBlackCastles  && hasEnPassant ==enPassant  ){
   // Debug.Log ("position the same and castles events match up");
    return true;
  } else {
   // Debug.Log ("position the same but castles dont match");

      return false;
  }
 }
//chess ai
int MoveGenerationTest(int depth){
int numMoves = 0;
//Get all peices
       List<ChessPiece> whitePieces = new List<ChessPiece>();
        List<ChessPiece> blackPieces = new List<ChessPiece>();
        List<ChessPiece[,]> possibleBoardPositions  = new List<ChessPiece[,]>();

             for (int x = 0; x < TileCountX; x++ ){
           for (int y = 0; y < TileCountY; y++){
               if (chessPieces[x,y] != null){
                if (chessPieces[x,y].team == 0){
                    whitePieces.Add(chessPieces[x,y]);
             } else  {
                    blackPieces.Add(chessPieces[x,y]);
              }
             }}} 
// now simulate all of whites moves
  for (int a = 0; a < whitePieces.Count; a++){
             //get avalible moves for the other same pieces and if they hit the target piece square then
            List<Vector2Int> pieceMoves = whitePieces[a].GetAvailableMoves(ref chessPieces, TileCountX,TileCountY);
    //lets now get the total board positions
    possibleBoardPositions.AddRange ( SimulateMoveForSinglePieceAI(whitePieces[a],pieceMoves ));
    numMoves += pieceMoves.Count;
return 1;
  } 
  
  
  // for (int a = 0; a < blackPieces.Count; a++){
             //get avalible moves for the other same pieces and if they hit the target piece square then
    //        List<Vector2Int> pieceMoves = whitePieces[a].GetAvailableMoves(ref chessPieces, TileCountX,TileCountY);
     // numMoves += pieceMoves.Count;
  //} 

return numMoves;

}

    private List<ChessPiece[,]> SimulateMoveForSinglePieceAI(ChessPiece cp,  List<Vector2Int> moves){
        //this section could be make to make a really basic ai, with a second or third or however mayny more simulated set of check, for defending and attacking
        //save the current values to reset after function 
       List<ChessPiece[,]> possibleBoardPositions  = new List<ChessPiece[,]>();
        int actualX = cp.currentX;
        int actualY = cp.currentY;
      //  List<Vector2Int> movesToRemove = new List<Vector2Int>();
        //going through all the moves, simualite them and check if we are in check
        for (int i =0; i < moves.Count; i++){
            int simX = moves[i].x;
            int simY = moves[i].y;

          //  Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX,targetKing.currentY);
            //did we simulate the kings move?
           // if (cp.type == ChessPieceType.King){
             //   kingPositionThisSim = new Vector2Int(simX,simY);
           // }
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
                //where to add evaluation
                simulationAttackingPieces.Remove(deadPiece);
            }
                //is the king in trouble if so remove the mov
           
            possibleBoardPositions.Add (simulation);
         }
        //remove from the current list avalible. 
        return possibleBoardPositions;
    }


    private void RecusiveMiniMax(){


    }

        //should make class that is chess boards and also returns lists of move lists for special moves and also lists of eval
        private List<ChessPiece[,]> SimulateMoveForEntireBoardsAI(List<ChessPiece[,]> currentBoards, List<List<Vector2Int[]>> currentMoveLists, int isWhiteTurn){
        //this section could be make to make a really basic ai, with a second or third or however mayny more simulated set of check, for defending and attacking
        //save the current values to reset after function 
       List<ChessPiece[,]> possibleBoardPositions  = new List<ChessPiece[,]>();
         List<List<Vector2Int[]>> possibleMoveLists  = new  List<List<Vector2Int[]>>();
        for (int i =0; i < currentBoards.Count; i++){   
            ChessPiece[,] simulation = new ChessPiece[TileCountX,TileCountY];
            List<ChessPiece> simulationAttackingPieces = new List<ChessPiece>();
            List<ChessPiece> simulationFriendlyPieces = new List<ChessPiece>();
            List<Vector2Int[]> simulationMoveList = new List<Vector2Int[]>();
            for (int x = 0; x < TileCountX; x++ ){
                for (int y = 0; y < TileCountY; y++){
                    if (currentBoards[i][x,y]!= null){
                        simulation [x,y] = currentBoards[i][x,y];
                        if (simulation[x,y].team != isWhiteTurn){
                            simulationAttackingPieces.Add (simulation[x,y]);
                        } else {
                            simulationFriendlyPieces.Add (simulation[x,y]);
                        }
                    }
                }
            }

            //set the simulation move list so it works as a ref
            simulationMoveList =   currentMoveLists[i];
        
            for (int x = 0; x < simulationFriendlyPieces.Count; x++ ){
            //simulate the moves
               List<Vector2Int> pieceMoves = simulationFriendlyPieces[x].GetAvailableMoves(ref simulation, TileCountX,TileCountY);
              //add the special moves to the get avalible moves list in order to get all the moves
              //because it takes takes the list piece moves and returns special move check we can use it to handle a lot of logic
               SpecialMove specialPieceMoves = simulationFriendlyPieces[x].GetSpecialMoves(ref simulation,ref simulationMoveList,ref pieceMoves, TileCountX,TileCountY);

        //lets now get the total board positions
    
    //possibleBoardPositions.AddRange ( SimulateMoveForSinglePieceAI(whitePieces[a],pieceMoves ));
            int currentX = simulationFriendlyPieces[x].currentX;
            int currentY = simulationFriendlyPieces[x].currentY;
            for (int t = 0; t < pieceMoves.Count; t++){
                //ok we need to store a extra reference of the simulation attacking pieces because if we take then we need to still have orginal reference
                //it may be faster to do another way but lets jsut do it this way
                List<ChessPiece> movesimulationAttackingPieces =new List<ChessPiece>(simulationAttackingPieces); 
               //dont do this for array just set it back after the move
                //ChessPiece[,] moveSimulation = simulation;
                int simX = pieceMoves[t].x;
                int simY = pieceMoves[t].y;
                //track special moves with bools
                bool didSimCapture = false;
               //done with special move
                bool didQueenCastle = false;
                bool didKingCastle = false;
                bool didEnPassant = false;
                bool didPromote = false;
                //if special move happens we need to keep a refereence to the other chesspiece for reseting back
                ChessPiece CapturedPiece = null;
                ChessPiece QueenRook = null;
                ChessPiece KingRook = null;
                ChessPiece EnPassantPeopos = null;
             

            
                simulation[currentX,currentY] = null;
                simulationFriendlyPieces[x].currentX = simX;
                simulationFriendlyPieces[x].currentY = simY;
                simulation[simX,simY] =  simulationFriendlyPieces[x];
                // add to current board move list so we can simulate all the actual moves
                //rememver to remove the lists at the end of the for loop after we returned the custom game object 
              simulationMoveList.Add (new Vector2Int[] {new Vector2Int(currentX,currentY) , new Vector2Int(simX,simY) } ); 
            //did one of pieces get taken down during simulation move
            var deadPiece = simulationAttackingPieces.Find (c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null){
                //where to add evaluation
               int eval = returnCaptureMinMaxEval(deadPiece.type,isWhiteTurn);
                simulationAttackingPieces.Remove(deadPiece);
                didSimCapture = true;
                CapturedPiece = deadPiece;
            }
            //ok now that we moved in the simulation lets manually check to see if we moved on the special moves
            //if move is a special move than we need to place the other piece in the correct place
            //also may eventually want to add in a little extra eval boost for castling and a +9 eval boost for promotion
            //the way special moves work is that its going to only allow it as a real move if the get avalible moves
            //castling
            if (simulationFriendlyPieces[x].type  == ChessPieceType.King){

                //check for last move position and current space
                if (isWhiteTurn == 0){
                if (currentX == 4 &&  currentY == 0){
                    if (simX == 2){
                        //queen castle move rook to 3
                        //find queen rook
                        foreach (ChessPiece cp in simulationFriendlyPieces){
                            if (cp.type ==ChessPieceType.Rook ){
                                if (cp.currentX == 0){
                                    //queenside rook
                                    simulation[cp.currentX,cp.currentY] = null;
                                   cp.currentX = 3;
                                    cp.currentY = 0;
                                    simulation[3,0] = cp;
                                    QueenRook = cp;
                                    //didQueenCastle = true;
                                }
                            }
                        }
                    }
                    if (simX == 6){
                        //king side castle
                          foreach (ChessPiece cp in simulationFriendlyPieces){
                            if (cp.type ==ChessPieceType.Rook ){
                                if (cp.currentX == 7){
                                    //kingside rook
                                    simulation[cp.currentX,cp.currentY] = null;
                                   cp.currentX = 5;
                                    cp.currentY = 0;
                                    simulation[5,0] = cp;
                                    KingRook = cp;
                                    didKingCastle = true;
                                }
                            }
                        }
                    }
                } 
                } else {
                    //black castles
                       if (currentX == 4 &&  currentY == 7){
                    if (simX == 2){
                        //queen castle move rook to 3
                        //find queen rook
                        foreach (ChessPiece cp in simulationFriendlyPieces){
                            if (cp.type ==ChessPieceType.Rook ){
                                if (cp.currentX == 0){
                                    //queenside rook
                                    simulation[cp.currentX,cp.currentY] = null;
                                   cp.currentX = 3;
                                    cp.currentY = 7;
                                    simulation[3,7] = cp;
                                    QueenRook = cp;
                                    didQueenCastle = true;
                                }
                            }
                        }
                    }
                    if (simX == 6){
                        //king side castle
                          foreach (ChessPiece cp in simulationFriendlyPieces){
                            if (cp.type ==ChessPieceType.Rook ){
                                if (cp.currentX == 7){
                                    //kingside rook
                                    simulation[cp.currentX,cp.currentY] = null;
                                   cp.currentX = 5;
                                    cp.currentY = 7;
                                    simulation[5,7] = cp;
                                    KingRook = cp;
                                    didKingCastle = true;
                                }
                            }
                        }
                    }
                } 
                }
            }
            //promotion and en passant
             if (simulationFriendlyPieces[x].type  == ChessPieceType.Pawn){
                 if (isWhiteTurn == 0){
                     if (simulationFriendlyPieces[x].currentY == 7)
                     {
                         //peopos ready to promote
                        //due to this just being a simulation and no graphics lets just change the piece type
                        simulationFriendlyPieces[x].type = ChessPieceType.Queen;
                        didPromote = true;               
                     }  
                 //white passant 

                 }

            }
            
            
            //here is the other eval boosting things like keeping pawns formation, taking with less good peices, 
            //add in current move list positions as well

            possibleBoardPositions.Add (simulation);
            possibleMoveLists.Add (simulationMoveList);

            // now lets reset the simulation board and simulation attacking piece

            }
            
           
            }
         }
        //remove from the current list avalible. 
        return possibleBoardPositions;
    }
    public int returnCaptureMinMaxEval(ChessPieceType type, int team){
        int eval = 0;
        if (type == ChessPieceType.Pawn){
            eval = 100;
        }
        if (type == ChessPieceType.Bishop){
            eval = 300;
        }
         if (type == ChessPieceType.Knight){
            eval = 300;
        }
        if (type == ChessPieceType.Rook){
            eval = 500;
        }
        if (type == ChessPieceType.Queen){
            eval = 900;
        }
        if (type == ChessPieceType.King){
            eval = 1000000;
        }
        if (team ==1 ){
            eval = eval * -1;
        }
        return eval;
    }
    //everything we need to start the next depth level
    public class AIChessObject
    {
        public List<ChessPiece[,]> chessBoards;
        //this is for getting the updated move lists to process special moves
        public List<List<Vector2Int[]>> currentMoveLists;
        //position evaluation
        public List<int> evals;
        
        public void AiChessObj(List<ChessPiece[,]> chesB, List<List<Vector2Int[]>> moveL,  List<int> ev)
        {
            chessBoards = chesB;
            currentMoveLists = moveL;
            evals = ev;
        }
    }

    //end minimax ai, this is for stockfish ai/ eval/ analysis and testing 
    public string GenerateFenFromBoard(ChessPiece[,] board,bool isWhiteTurn,bool whiteQueenCastle, bool whiteKingCastle, bool blackQueenCastle, bool blackKingCastle, int enPassant){
        string s = "";
        int spacesSinceLastPiece = 0;
        for(int y = 7; y >= 0; y--){
            for(int x = 0; x < TileCountX; x++){
                if (board[x,y] != null){
                   if (board[x,y].team == 0 ){
                      if (spacesSinceLastPiece !=0){
                          s+= spacesSinceLastPiece;
                          spacesSinceLastPiece =0;
                      }
                       s+= processFenCharFromTypeAndTeam(board[x,y].type,board[x,y].team);  
                   }  else {
                         if (spacesSinceLastPiece !=0){
                          s+= spacesSinceLastPiece;
                          spacesSinceLastPiece =0;
                      }
                       s+= processFenCharFromTypeAndTeam(board[x,y].type,board[x,y].team);  
                   } 
                } else {
                    spacesSinceLastPiece++;
                }
                if (x == 7){
                    if (spacesSinceLastPiece != 0 ){
                        s+= spacesSinceLastPiece;
                        spacesSinceLastPiece=0;
                    }
                    if (y!=0){
                    s+= "/";
                    }
                }
            }
        }
                // now that we have the pieces sorted out we add a space and then a w or b for who is playing next
                s+= " ";
                if (isWhiteTurn  == false){
                    s+= "w";
                }else {
                    s+= "b";
                }
                s+= " ";
                     //ok now we do castling rights
               
                if (whiteKingCastle == true){
                    s+= "K";
                }
                 if (whiteQueenCastle == true){
                    s+= "Q";
                }
               
                if (blackKingCastle == true){
                    s+= "k";
                }
                if (blackQueenCastle == true){
                    s+= "q";
                } 
                if (whiteKingCastle == false && whiteQueenCastle == false && blackKingCastle == false && blackQueenCastle == false){
                    s+= "-";
                }
                s+= " ";
               
                //next thing if a pawn double jumps we need to label the en passant square 
                //interesting how it's like this as offical versus checking surrounding pawns probably easier to compute 
                //easier to promote knight passant an subvarients i guess
                //bolean that will flip if double jumped in move to function.
                if (pawnDoubleJump != -1){
                    //there is a double jump
                     s+=  getColFromFile(pawnDoubleJump);
                    if (isWhiteTurn == true){
                     s+="3";
                    } else {
                     s+= "6";
                    }
                } else {
                    s+= "-";
                }
                pawnDoubleJump = -1;
                // ok now we do that half moon and full moon, which we just snag     from process notation 
                s+= " ";
                s+= movesSincePawnMoveOrCapture;
                s+= " "; 
                s+= realMoveNumber + 1;



        
              
        return s;
    }
    //helper functions for fen generation and board spawn
    public string getColFromFile(int i){
        string s ="";
        if (i == 0){
            s= "a";
        }
        if (i == 1){
            s= "b";
        }
        if (i == 2){
            s= "c";
        }
        if (i == 3){
            s= "d";
        }
        if (i == 4){
            s= "e";
        }
          if (i == 5){
            s= "f";
        }
         if (i == 6){
            s= "g";
        }
           if (i == 7){
            s= "h";
        }
        return s;
    }
    public string processFenCharFromTypeAndTeam(ChessPieceType type, int team){
       string s = "";
        if (team == 0){
            if (type == ChessPieceType.Pawn){
                s = "P";
            }
              if (type == ChessPieceType.Rook){
                s = "R";
            }
              if (type == ChessPieceType.Knight){
                s = "N";
            }
              if (type == ChessPieceType.Bishop){
                s = "B";
            }
              if (type == ChessPieceType.King){
                s = "K";
            }
              if (type == ChessPieceType.Queen){
                s = "Q";
            }
        } else {
                if (type == ChessPieceType.Pawn){
                s = "p";
            }
              if (type == ChessPieceType.Rook){
                s = "r";
            }
              if (type == ChessPieceType.Knight){
                s = "n";
            }
              if (type == ChessPieceType.Bishop){
                s = "b";
            }
              if (type == ChessPieceType.King){
                s = "k";
            }
              if (type == ChessPieceType.Queen){
                s = "q";
            }
        }
        return s;
    }
    
    
}
