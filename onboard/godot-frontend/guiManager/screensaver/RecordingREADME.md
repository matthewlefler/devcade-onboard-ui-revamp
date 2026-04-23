# Video
### Recording

The current display's resolution is **2160 by 3840**.  
So make sure that the game is running at the correct resolution or can be cropped, converted, etc. to a better resolution.  

It is recommended to use high-quality or lossless video formats so when converting the video the least quality is dropped. 

### Converting

This is a command to convert an input file of a given type to the **.ogv** format that Godot supports using **ffmpeg**. <br>
***-q:v*** changes the quality of the video and ranges from 1-10<br>
A Lower quality is recomended as high quality video runs poorly, as always test it first.<br>
***-q:a*** changes the quality of the audio and ranges from 1-10<br>
A quality of 1 is recomended as no audio is played anyways.<br>
***-vf*** specifies video filter options:
- **scale** changes the output resolution the width:height of the video
- **force_original_aspect_ratio** scales the video while keeping the original aspect ratio
- **crop** crops the video to a specific width:height
- **fps** sets the frame rate in frames per second.  

```Bash
ffmpeg -i input_file.type -vf "scale=1080:1920:force_original_aspect_ratio=increase,crop=1280:720,fps=30" -q:v 4 -q:a 1 output.ogv
```
The reason for the half scale and lower frame rate is because the .ogv format only supports cpu sided decoding and is extremly laggy on the DCU which has an older i5-8500 cpu.
