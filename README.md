# 2dUnityChess
 2d unity chess based on Epitome's great unity tutorial on how to make chess like and subscribe to his content it's really great. I pretty much followed the tutorial pretty closly except for making the design of board to follow 2d principles versus 3d because that's how people are used to playing chess on a screen. I have now also gone through and added, draw conditions, promotion ui, move notation and fixed the glitch around being able to castle king through attacked squares. I added Fen generation and got it working with this cloud python stockfish example i built on hugging face https://huggingface.co/spaces/nakas/Stockfish_board_eval Super close to getting the ai fully working.  Needs a little bit of testing but it's feature complete. 
 https://www.youtube.com/watch?v=qVhG6ZWqD-o&list=PLmcbjnHce7SeAUFouc3X9zqXxiPbCz8Zp

# Licensing

## Code

All scripts and scene files are distributed under the [MIT license](LICENSE.md).  
Copyright held by Andrew Nakas.

## Stock fish code on huggingface space is licensed gpl3 
https://huggingface.co/spaces/nakas/Stockfish_board_eval

## Chess Piece Assets

Anarchy Chess Pieces are from https://sharechess.github.io/ 
and made by caderek and CC BY-NC-SA 4.0 here is a link to the license. 
https://sharechess.github.io/stylus/pieces/anarchy.user.css
Don't use them commercially, get your own god damn pieces
Thanks caderek!

# Next Steps
This project is currently at the state full two player chess with all the rules, Cloud stockfish and ai eval 90% complete

Up next is test conditions, make sure it works and looks good on mobile, time control, minimax ai. 

Eventually i'm going to do the multiplayer tutorial part, make some chess varients and local bluetooth multiplayer. The project will be forked and closed source for the bluetooth part and maybe some complex variants as I'm thinking of releasing a chess app.

I have a crazy idea for a new variant, frog chess! 