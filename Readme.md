## Pencil Rotation Project Readme

1. The project aims to identify a single complete rotation of a pen(cil)-like shaped object from a vertically upright position, either clockwise or anticlockwise, back to it's original position.

2. The project utilizes a unity scene and a python script that runs a deep learning model for identifying the pencil within an image

### Setup

1. Clone the repository, making sure to download any additional relevant files that are provided.

#### Python
1. Install python (version 3.10.5), using either the standard python (available on the official python website, or the Microsoft store), or using the pyenv tool (recommended).

2. Ensure all the necessary dependencies have been installed for the python project (pip install -r requirements.txt).

3. Ensure the model works by testing the test.ipynb notebook locally

4. Start the ZeroMQ Server by running the zapi.py file

#### Unity
1. Install the correct unity version (2020.3.25f1) from the unity website (archived versions).

2. Install the opencv package by downloading the archive, extracting and  copying the folder within the assets.
##### Get OpenCV for Unity
https://drive.google.com/file/d/1Bx3IUakJKFsG3LGaAPmTdSj3dqM3xz6X/view
#####
3. After package has been loaded, run the import tool within the opencv dropdown, and move the StreamingAssets folder up one directory (Assets/)

4. Install NuGet package manager by importing the package into unity.

5. Use Package manager to install ZeroMQ package.

6. Ensure python server is running and start the Main scene.

### Howto
Prequisites:
1. Python and Unity projects are Setup
2. Python zapi.py server is running.
3. Ensure camera is connected.
Steps:
1. Start the scene within the unity editor
2. A Show pencil text will popup along with the current angle state, overlaying the camera stream
3. Bring the pencil to the camera
4. Bounding boxes will now be visible around the pencil provided it is sufficiently visible
5. Rotate the pencil in one direction
6. As the pencil rotates, the state will update from 315-45 -> 45-135 -> 135-225 -> 225-315
7. Once a complete rotation is performed and the state loops back to 315-45, the center text will change to 'Successful'
8. Congratulations, the pencil has now completed one rotation cycle and the application can now be restarted to perform the activity again if needed. Note that the server does not need to be restarted in this case.

