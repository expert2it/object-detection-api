# This code is written at BigVision LLC. It is based on the OpenCV project. It is subject to the license terms in the LICENSE file found in this distribution and at http://opencv.org/license.html

# Usage example:  python3 object_detection_yolo.py --video=run.mp4
#                 python3 object_detection_yolo.py --image=bird.jpg

import argparse
import sys
import numpy as np
import os.path
import socket
import time

class MovementHandler:
    def __init__(self, cv):
        # print("Current Directory: ", os.path.dirname(os.path.abspath(__file__)))
        self.cv = cv
        self.detected = False
        directory = os.path.dirname(os.path.abspath(__file__))
        # Initialize the parameters
        self.confThreshold = 0.30  #Confidence threshold
        self.nmsThreshold = 0.4   #Non-maximum suppression threshold
        self.inpWidth = 416       #Width of network's input image
        self.inpHeight = 416      #Height of network's input image

        parser = argparse.ArgumentParser(description='Object Detection using YOLO in OPENCV')
        parser.add_argument('--image', help='Path to image file.')
        parser.add_argument('--video', help='Path to video file.')
        args = parser.parse_args()
        
        # Load names of classes
        self.classesFile = directory + "/obj.names" #"cards.names";
        self.classes = None
        with open(self.classesFile, 'rt') as f:
            self.classes = f.read().rstrip('\n').split('\n')

        # Give the configuration and weight files for the model and load the network using them.
        self.modelConfiguration = directory + "/yolo3-detect.cfg" #"yolov3-cards.cfg";
        self.modelWeights = directory + "/yolo-obj_final.weights" #"yolov3-obj_2500.weights";

        self.net = cv.dnn.readNetFromDarknet(self.modelConfiguration, self.modelWeights)
        self.net.setPreferableBackend(cv.dnn.DNN_BACKEND_OPENCV)
        self.net.setPreferableTarget(cv.dnn.DNN_TARGET_CPU)
        #return super().__init__()
   

    def Detect(self, path, video = None, camera = None):
        print("Initiating MovementHandler Class...")
    # Get the names of the output layers
        def getOutputsNames(net):
            # Get the names of all the layers in the network
            layersNames = net.getLayerNames()
            # Get the names of the output layers, i.e. the layers with unconnected outputs
            return [layersNames[i[0] - 1] for i in net.getUnconnectedOutLayers()]

        # Draw the predicted bounding box
        def drawPred(classId, conf, left, top, right, bottom):
            # Draw a bounding box.
            self.cv.rectangle(frame, (left, top), (right, bottom), (255, 178, 50), 2)
            # print("Bounding Box:: ", (left, top), (right, bottom))
            label = '%.2f' % conf
        
            # Get the label for the class name and its confidence
            if self.classes:
                assert(classId < len(self.classes))
                label = '%s:%s' % (self.classes[classId], label)
            if (camera):
                message.append(label)
                # print("message: ", message)
            else:
                labels.append(label)
            #Display the label at the top of the bounding box
            labelSize, baseLine = self.cv.getTextSize(label, self.cv.FONT_HERSHEY_SIMPLEX, 0.5, 1)
            top = max(top, labelSize[1])
            self.cv.rectangle(frame, (left, top - round(1.5*labelSize[1])), (left + round(1.5*labelSize[0]), top + baseLine), (255, 255, 255), self.cv.FILLED)
            self.cv.putText(frame, label, (left, top), self.cv.FONT_HERSHEY_SIMPLEX, 0.75, (0,0,0), 1)

        # Remove the bounding boxes with low confidence using non-maxima suppression
        def postprocess(frame, outs):
            frameHeight = frame.shape[0]
            frameWidth = frame.shape[1]

            classIds = []
            confidences = []
            boxes = []
            # Scan through all the bounding boxes output from the network and keep only the
            # ones with high confidence scores. Assign the box's class label as the class with the highest score.
            classIds = []
            confidences = []
            boxes = []
            for out in outs:
                for detection in out:
                    scores = detection[5:]
                    classId = np.argmax(scores)
                    confidence = scores[classId]
                    if confidence > self.confThreshold:
                        center_x = int(detection[0] * frameWidth)
                        center_y = int(detection[1] * frameHeight)
                        width = int(detection[2] * frameWidth)
                        height = int(detection[3] * frameHeight)
                        left = int(center_x - width / 2)
                        top = int(center_y - height / 2)
                        classIds.append(classId)
                        confidences.append(float(confidence))
                        boxes.append([left, top, width, height])

            # Perform non maximum suppression to eliminate redundant overlapping boxes with
            # lower confidences.
            indices = self.cv.dnn.NMSBoxes(boxes, confidences, self.confThreshold, self.nmsThreshold)
            for i in indices:
                i = i[0]
                box = boxes[i]
                left = box[0]
                top = box[1]
                width = box[2]
                height = box[3]
                drawPred(classIds[i], confidences[i], left, top, left + width, top + height)
            # print(classIds)
            if classIds:
                self.detected = True
                # print("detected!")
            else:
                self.detected = False
                #print("not detected!")
        # Process inputs
        winName = 'Robot'
        # self.cv.namedWindow(winName, self.cv.WINDOW_NORMAL)

        labels = []
        message = []
        if (camera):
            # Webcam input
            print("Running camerra source: " + camera)
            cap = self.cv.VideoCapture(0)
            # TODO: Create a TCP/IP socket
            # sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            # Connect the socket to the port where the server is listening
            # server_address = ('127.0.0.1', 11111)
            # print(sys.stderr, 'connecting to %s port %s' % server_address)
            # sock.connect(server_address)
            # set time out 2 minute
            #timeout = time.time() + 60*2   # 2 minutes from now
            #print("Timeout is 2 minute: " + str(timeout))
        # Get the video writer initialized to save the output video
        # Encoders: DIVX, XVID, MJPG, X264, WMV1, WMV2
                
        while True: # cv.waitKey(1) < 0:
            # print("6- Line: 135" + str(labels))
            # get frame from the video
            hasFrame, frame = cap.read()
    
            # Stop the program if reached end of video
            if not hasFrame:
                print("Done processing !!!")
                # cv.waitKey(10000)
                break

            # Show image
            #small = self.cv.resize(frame, (0,0), fx=0.5, fy=0.5) 
            #print("img size to show: ", small.size)
            #self.cv.imshow(winName, small)

            # Create a 4D blob from a frame.
            blob = self.cv.dnn.blobFromImage(frame, 1/255, (self.inpWidth, self.inpHeight), [0,0,0], 1, crop=False)

            # Sets the input to the network
            self.net.setInput(blob)

            # Runs the forward pass to get output of the output layers
            outs = self.net.forward(getOutputsNames(self.net))

            # Remove the bounding boxes with low confidence
            postprocess(frame, outs)

            # Put efficiency information. The function getPerfProfile returns the overall time for inference(t) and the timings for each of the layers(in layersTimes)
            t, _ = self.net.getPerfProfile()
            label = 'Detected in %.3f Seconds' % (t * 1.0 / self.cv.getTickFrequency())
            self.cv.putText(frame, label, (0, 15), self.cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 255))
            # Write the frame with the detection boxes
            if (path):
                self.cv.imwrite(outputFile, frame.astype(np.uint8));

            if (camera):
                # print("Broadcasting camera:" + camera)
                try:                   
                    # if labels != []:
                    #    message = labels[-1]  # get the last index =>> labels[-1]
                   
                    print('Detected -- > %s' % message)
                    #if self.detected: # message
                    #    # print('%s -- Detected' % message)
                    #    sock.sendall(str(message).encode())
                    #    data = sock.recv(1024)
                    #    # clear the previous message array
                    message.clear()
                    #    # print("Socker recv: ", repr(data))
                    #    if repr(data).find("<EOF>") > 0:
                    #        sock.close()
                    #        break
                    
                except Exception as exc:
                    print("Exception: ".format(exc)) 
        if (camera):
            # print(sys.stderr, 'closing socket')
            # sock.close()
            cap.release()
            self.cv.destroyAllWindows()
        return labels

