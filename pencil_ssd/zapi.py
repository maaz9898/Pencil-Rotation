import zmq
import cv2
import numpy as np
from inference import run_inference
import json

# Create a ZMQ context, and bind a socket to the port
context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:8002")

while True:
    # Wait for next request from client
    message = socket.recv()
    print("Received request")

    # Decode the recieved message to an uint8 image
    image = cv2.imdecode(np.frombuffer(message, np.uint8), cv2.IMREAD_ANYCOLOR)
    # Run inference on the image
    prediction = run_inference(image)
    # print(prediction)
    
    # Convert the prediction output to a dictionary
    data = {'location': prediction[0].tolist(), 'label': prediction[1], 'score': float(prediction[2])}
    print(data)
    encodedData = json.dumps(data).encode('utf-8') # encode to utf-8 after converting to json
    #  Send reply back to client
    socket.send(encodedData)