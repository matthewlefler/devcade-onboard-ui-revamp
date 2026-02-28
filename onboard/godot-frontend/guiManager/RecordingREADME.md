# Video
### Recording

The current display's resolution is **2160 by 3840**.<br>
So make sure that the game is running at the correct resolution or can be cropped, converted, etc. to a better resolution.

It is recommended to use high-quality or lossless video formats so when converting the video the least quality is dropped. 
### Converting

This is a command to convert an input file of a given type to the **.ogv** format that Godot supports using **ffmpeg**. <br>
***-q:v*** changes the quality of the video and ranges from 1-10<br>
A Lower quality is recomended as high quality video runs poorly, as always test it first.<br>
***-q:a*** changes the quality of the audio and ranges from 1-10<br>
A quality of 1 is recomended as no audio is played anyways.<br>
***-vf*** specifies video filter options: **scale** changes the output resolution, the width:height of the video and **fps** sets the frame rate.
```Bash
ffmpeg -i input_file.type -vf "scale=1080:1920,fps=30" -q:v 4 -q:a 1 output.ogv
```

## Videos **MUST** be under **100MB** or else GitHub will reject the Commit
And fixing it is annoying