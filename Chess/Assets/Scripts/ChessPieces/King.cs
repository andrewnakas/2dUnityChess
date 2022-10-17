using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
         public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX,int tileCountY){
        List <Vector2Int> r = new List <Vector2Int>();

        if(currentX +1 < tileCountX){
            //right
            if (board[currentX + 1, currentY]== null)
            {
                r.Add(new Vector2Int(currentX+1,currentY));
            } else if (board[currentX + 1, currentY].team != team){
                r.Add(new Vector2Int(currentX+1,currentY));
            }
            //top right
             if(currentY +1 < tileCountY){
                    if (board[currentX + 1, currentY+1]== null)
                        {
                             r.Add(new Vector2Int(currentX+1,currentY+1));
                         } else if (board[currentX + 1, currentY+1].team != team){
                            r.Add(new Vector2Int(currentX+1,currentY+1));
                         }
             }
             ///bottom right
               if(currentY -1 >= 0){
                    if (board[currentX + 1, currentY-1]== null)
                        {
                             r.Add(new Vector2Int(currentX+1,currentY-1));
                         } else if (board[currentX + 1, currentY-1].team != team){
                            r.Add(new Vector2Int(currentX+1,currentY-1));
                         }
             }
        }
            if(currentX -1 >= 0){
            //left
            if (board[currentX - 1, currentY]== null)
            {
                r.Add(new Vector2Int(currentX-1,currentY));
            } else if (board[currentX - 1, currentY].team != team){
                r.Add(new Vector2Int(currentX-1,currentY));
            }
            //top left
             if(currentY +1 < tileCountY){
                    if (board[currentX - 1, currentY+1]== null)
                        {
                             r.Add(new Vector2Int(currentX-1,currentY+1));
                         } else if (board[currentX - 1, currentY+1].team != team){
                            r.Add(new Vector2Int(currentX-1,currentY+1));
                         }
             }
             ///bottom left  
               if(currentY -1 >= 0){
                    if (board[currentX - 1, currentY-1]== null)
                        {
                             r.Add(new Vector2Int(currentX-1,currentY-1));
                         } else if (board[currentX - 1, currentY-1].team != team){
                            r.Add(new Vector2Int(currentX-1,currentY-1));
                         }
             }
        }
        if (currentY +1 < tileCountY){
        if (board[currentX,currentY +1] == null || board[currentX,currentY+1].team != team){
            r.Add (new Vector2Int (currentX, currentY+1));
        }
        }
         if (currentY -1 >= 0){
        if (board[currentX,currentY -1] == null || board[currentX,currentY-1].team != team){
            r.Add (new Vector2Int (currentX, currentY-1));
        }
        }
        return r;
        
        
        }
         public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves,int tileCountX,int tileCountY){
        //castles
        SpecialMove r = SpecialMove.None;
        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0)? 0 : 7));
        var leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0)? 0 : 7));
        var rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == 0)? 0 : 7));

        if (kingMove == null && currentX == 4 ){
            //white team
            if (team == 0){
                if (leftRook == null){
                    if (board[0,0].type == ChessPieceType.Rook){
                        if (board[0,0].team == 0){
                            if (board[3,0] == null){
                                if (board[2,0] == null){
                                    if (board[1,0] == null){
                                        // king and rook are in right positions, havent moved? and nothing in between them we can castle left
                                        //check to make sure nothing is attacking the 3 sqiares as the king cant pass over attacking pieces
                                       if (checkForWhiteLeftCastlesAttack(board,tileCountX,tileCountY) == false){
                                            availableMoves.Add(new Vector2Int(2,0));
                                            r = SpecialMove.Castling;
                                       }
                                    }
                                }
                            }
                        }

                    }
                }
                          if (rightRook == null){
                    if (board[7,0].type == ChessPieceType.Rook){
                        if (board[7,0].team == 0){
                            if (board[5,0] == null){
                                if (board[6,0] == null){
                                    if (checkForWhiteRightCastlesAttack(board,tileCountX,tileCountY) == false){
                                        // king and rook are in right positions, havent moved? and nothing in between them we can castle left
                                        availableMoves.Add(new Vector2Int(6,0));
                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }

                    }
                }
        } else {
                            if (leftRook == null){
                    if (board[0,7].type == ChessPieceType.Rook){
                        if (board[0,7].team == 1){
                            if (board[3,7] == null){
                                if (board[2,7] == null){
                                    if (board[1,7] == null){
                                          if (checkForBlackLeftCastlesAttack(board,tileCountX,tileCountY) == false){
                                        // king and rook are in right positions, havent moved? and nothing in between them we can castle left
                                        availableMoves.Add(new Vector2Int(2,7));
                                        r = SpecialMove.Castling;
                                          }
                                    }
                                }
                            }
                        }

                    }
                }
                          if (rightRook == null){
                    if (board[7,7].type == ChessPieceType.Rook){
                        if (board[7,7].team == 1){
                            if (board[5,7] == null){
                                if (board[6,7] == null){
                                        if (checkForBlackRightCastlesAttack(board,tileCountX,tileCountY) == false){
                                        // king and rook are in right positions, havent moved? and nothing in between them we can castle left
                                        availableMoves.Add(new Vector2Int(6,7));
                                        r = SpecialMove.Castling;
                                    }    
                                }
                            }
                        }

                    }
                }
        }
        }
        return r;
         }

             private bool checkForWhiteLeftCastlesAttack(ChessPiece[,] board, int tileCountX,int tileCountY){
        //check if any piece is attacking the 0,3 square as the king cant hop over attack
        //first get all the attacking pieces
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        for (int x = 0; x < tileCountX; x++ ){
           for (int y = 0; y < tileCountY; y++){
           if (board[x,y] != null ){
           if (board[x,y].team == 1){
               attackingPieces.Add(board[x,y]);
           }
           }
           }
        }
           //now that we have attacking moves check if any are targeting 0,3 square 
            for (int i = 0; i < attackingPieces.Count; i++){
                List<Vector2Int> pieceMoves = attackingPieces[i].GetAvailableMoves(ref board, tileCountX,tileCountY);
                for (int b = 0; b < pieceMoves.Count; b++){
                   if (pieceMoves[b] == new Vector2Int(3,0)){
                       return true;
                   }
                }
            }
            return false;
    }
            private bool checkForWhiteRightCastlesAttack(ChessPiece[,] board, int tileCountX,int tileCountY){
        //check if any piece is attacking the 0,3 square as the king cant hop over attack
        //first get all the attacking pieces
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        for (int x = 0; x < tileCountX; x++ ){
           for (int y = 0; y < tileCountY; y++){
           if (board[x,y] != null ){
           if (board[x,y].team == 1){
               attackingPieces.Add(board[x,y]);
           }
           }
           }
        }
           //now that we have attacking moves check if any are targeting 0,3 square 
            for (int i = 0; i < attackingPieces.Count; i++){
                List<Vector2Int> pieceMoves = attackingPieces[i].GetAvailableMoves(ref board, tileCountX,tileCountY);
                for (int b = 0; b < pieceMoves.Count; b++){
                   if (pieceMoves[b] == new Vector2Int(5,0)){
                       return true;
                   }
                }
            }
            return false;
    }
             private bool checkForBlackLeftCastlesAttack(ChessPiece[,] board, int tileCountX,int tileCountY){
        //check if any piece is attacking the 0,3 square as the king cant hop over attack
        //first get all the attacking pieces
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        for (int x = 0; x < tileCountX; x++ ){
           for (int y = 0; y < tileCountY; y++){
           if (board[x,y] != null ){
           if (board[x,y].team == 0){
               attackingPieces.Add(board[x,y]);
           }
           }
           }
        }
           //now that we have attacking moves check if any are targeting 0,3 square 
            for (int i = 0; i < attackingPieces.Count; i++){
                List<Vector2Int> pieceMoves = attackingPieces[i].GetAvailableMoves(ref board, tileCountX,tileCountY);
                for (int b = 0; b < pieceMoves.Count; b++){
                   if (pieceMoves[b] == new Vector2Int(3,7)){
                       return true;
                   }
                }
            }
            return false;
    }
            private bool checkForBlackRightCastlesAttack(ChessPiece[,] board, int tileCountX,int tileCountY){
        //check if any piece is attacking the 0,3 square as the king cant hop over attack
        //first get all the attacking pieces
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        for (int x = 0; x < tileCountX; x++ ){
           for (int y = 0; y < tileCountY; y++){
           if (board[x,y] != null ){
           if (board[x,y].team == 0){
               attackingPieces.Add(board[x,y]);
           }
           }
           }
        }
           //now that we have attacking moves check if any are targeting 0,3 square 
            for (int i = 0; i < attackingPieces.Count; i++){
                List<Vector2Int> pieceMoves = attackingPieces[i].GetAvailableMoves(ref board, tileCountX,tileCountY);
                for (int b = 0; b < pieceMoves.Count; b++){
                   if (pieceMoves[b] == new Vector2Int(5,7)){
                       return true;
                   }
                }
            }
            return false;
    }
}
