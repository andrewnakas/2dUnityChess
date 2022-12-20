using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX,int tileCountY){
      List<Vector2Int> r = new List <Vector2Int>();

      int direction = (team == 0)  ? 1 :-1; 
      if (currentY == 7 || currentY == 0){
          return r;
      }
      if (board[currentX,currentY+ direction] == null){
          r.Add(new Vector2Int(currentX, currentY + direction));
      }

      if (board[currentX,currentY+ direction] == null){
         if(team == 0 && currentY ==1 && board[currentX,currentY + (direction *2)] == null){
            r.Add (new Vector2Int(currentX,currentY + (direction *2)));   
         }
           if(team == 1 && currentY ==6 && board[currentX,currentY + (direction *2)] == null){
            r.Add (new Vector2Int(currentX,currentY + (direction *2)));   
         }

        
      }
       //killmove
         if (currentX!= tileCountX -1 ){
             if (board [currentX +1,currentY +direction ] != null && board[currentX +1, currentY +direction].team !=team) {
                 r.Add(new Vector2Int(currentX+1, currentY +direction));
             }
         }
        if (currentX!= 0 ){
            if (board [currentX -1,currentY +direction ] != null && board[currentX -1, currentY +direction].team !=team) {
                 r.Add(new Vector2Int(currentX-1, currentY +direction));
             }
         }


      return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves, int tileCountX, int tileCountY){
   
        int direction = (team== 0 )? 1 : -1;
        //promotion
        if ((team == 0 && currentY == 6) || (team == 1 && currentY == 1)) {
            Debug.Log ("promotion");
            return SpecialMove.Promotion;

        }

     //En Passant 
        if (moveList.Count > 0 ){

            Vector2Int[] lastMove = moveList[moveList.Count-1]; 
            //if last piece was a pawn
            if (board[lastMove[1].x,lastMove[1].y].type == ChessPieceType.Pawn){
                //was last move plus two
                //check other team
                if (Mathf.Abs (lastMove[0].y - lastMove[1].y) == 2){

                    if (board[lastMove[1].x, lastMove[1].y].team != team){
                        
                        if (lastMove[1].y == currentY) {
                        //land right or left
                        if (lastMove[1].x == currentX-1){
                            availableMoves.Add(new Vector2Int(currentX-1, currentY + direction ));
                           Debug.Log ("Enpassnt" + lastMove[1].x  + " "+ " " + (currentX-1 ));
                            return SpecialMove.EnPassant;
                        }
                         if (lastMove[1].x == currentX+1){
                            availableMoves.Add(new Vector2Int(currentX+1, currentY + direction ));
                            Debug.Log ("Enpassnt1");

                            return SpecialMove.EnPassant;

                        }
                        }
                    }
                }
            }
        }
        return SpecialMove.None;
    }

}
