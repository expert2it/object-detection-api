from imutils.video import VideoStream
#from imutils.video import FPS
from imutils.object_detection import non_max_suppression
import numpy as np
import argparse
import imutils
import time
import pytesseract
import socket
import sys

class TextRecognition:
	def __init__(self, cv):
		self.cv2 = cv
		self.min_confidence=0.5
		self.result = []
		self.checkList = ['STOP', 'GO', 'LEFT', 'RIGHT', 'UP', 'DOWN', 'PIC', 'FORWARD', 'BACK']
		self.detected = False

	def decode_predictions(self, scores, geometry):
		# grab the number of rows and columns from the scores volume, then
		# initialize our set of bounding box rectangles and corresponding
		# confidence scores
		(numRows, numCols) = scores.shape[2:4]
		rects = []
		confidences = []

		# loop over the number of rows
		for y in range(0, numRows):
			# extract the scores (probabilities), followed by the
			# geometrical data used to derive potential bounding box
			# coordinates that surround text
			scoresData = scores[0, 0, y]
			xData0 = geometry[0, 0, y]
			xData1 = geometry[0, 1, y]
			xData2 = geometry[0, 2, y]
			xData3 = geometry[0, 3, y]
			anglesData = geometry[0, 4, y]

			# loop over the number of columns
			for x in range(0, numCols):
				# if our score does not have sufficient probability,
				# ignore it
				if scoresData[x] < self.min_confidence:
					continue

				# compute the offset factor as our resulting feature
				# maps will be 4x smaller than the input image
				(offsetX, offsetY) = (x * 4.0, y * 4.0)

				# extract the rotation angle for the prediction and
				# then compute the sin and cosine
				angle = anglesData[x]
				cos = np.cos(angle)
				sin = np.sin(angle)

				# use the geometry volume to derive the width and height
				# of the bounding box
				h = xData0[x] + xData2[x]
				w = xData1[x] + xData3[x]

				# compute both the starting and ending (x, y)-coordinates
				# for the text prediction bounding box
				endX = int(offsetX + (cos * xData1[x]) + (sin * xData2[x]))
				endY = int(offsetY - (sin * xData1[x]) + (cos * xData2[x]))
				startX = int(endX - w)
				startY = int(endY - h)

				# add the bounding box coordinates and probability score
				# to our respective lists
				rects.append((startX, startY, endX, endY))
				confidences.append(scoresData[x])

		# return a tuple of the bounding boxes and associated confidences
		return (rects, confidences)

	def Ocr(self, padding, path):
		print("Ocr initiated at: ", path)
		east="frozen_east_text_detection.pb"
		# resized image size (should be multiple of 32)"
		width=320
		height=320
		# initialize the original frame dimensions, new frame dimensions,
		# and ratio between the dimensions
		(W, H) = (None, None)
		(newW, newH) = (width, height)
		(rW, rH) = (None, None)

		# define the two output layer names for the EAST detector model that
		# we are interested -- the first is the output probabilities and the
		# second can be used to derive the bounding box coordinates of text
		layerNames = [
			"feature_fusion/Conv_7/Sigmoid",
			"feature_fusion/concat_3"]

		# load the pre-trained EAST text detector
		print("[INFO] loading EAST text detector...")
		net = self.cv2.dnn.readNet(east)

		if path:
			# Create a TCP/IP socket
			sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			# Connect the socket to the port where the server is listening
			server_address = ('127.0.0.1', 11112)
			# print(sys.stderr, 'connecting to %s port %s' % server_address)
			sock.connect(server_address)			
			#sock.setblocking(1)
			# if a video path was not supplied, grab the reference to the web cam
			# "http://192.168.7.152:8080/?action=stream"
			vs = VideoStream(src=path).start()
			time.sleep(1.0)

		# ******** start the FPS throughput estimator *************
		# fps = FPS().start()
		# loop over frames from the video stream

		while True: 
				# grab the current frame, then handle if we are using a
				# VideoStream or VideoCapture object
				frame = vs.read()
				#frame = frame[1] if args.get("video", False) else frame

				# check to see if we have reached the end of the stream
				if frame is None:
					break

				# resize the frame, maintaining the aspect ratio
				frame = imutils.resize(frame, width=1000)
				orig = frame.copy()

				# if our frame dimensions are None, we still need to compute the
				# ratio of old frame dimensions to new frame dimensions
				if W is None or H is None:
					(H, W) = frame.shape[:2]
					rW = W / float(newW)
					rH = H / float(newH)

				# resize the frame, this time ignoring aspect ratio
				frame = self.cv2.resize(frame, (newW, newH))

				# construct a blob from the frame and then perform a forward pass
				# of the model to obtain the two output layer sets
				blob = self.cv2.dnn.blobFromImage(frame, 1.0, (newW, newH),
					(123.68, 116.78, 103.94), swapRB=True, crop=False)
				net.setInput(blob)
				(scores, geometry) = net.forward(layerNames)

				# decode the predictions, then  apply non-maxima suppression to
				# suppress weak, overlapping bounding boxes
				(rects, confidences) = self.decode_predictions(scores, geometry)
				boxes = non_max_suppression(np.array(rects), probs=confidences)
				if rects:
					self.detected = True
				# loop over the bounding boxes
				for (startX, startY, endX, endY) in boxes:
					# scale the bounding box coordinates based on the respective
					# ratios
					startX = int(startX * rW)
					startY = int(startY * rH)
					endX = int(endX * rW)
					endY = int(endY * rH)

					# in order to obtain a better OCR of the text we can potentially
					# apply a bit of padding surrounding the bounding box -- here we
					# are computing the deltas in both the x and y direction
					dX = int((endX - startX) * 0.02)
					dY = int((endY - startY) * 0.02)

					# apply padding to each side of the bounding box, respectively
					startX = max(0, startX - dX)
					startY = max(0, startY - dY)
					endX = min(W, endX + (dX * 2))
					endY = min(H, endY + (dY * 2))

					# extract the actual padded ROI
					roi = orig[startY:endY, startX:endX]


					# draw the bounding box on the frame
					self.cv2.rectangle(orig, (startX, startY), (endX, endY), (0, 255, 0), 2)
					# Text Recognition
					# in order to apply Tesseract v4 to OCR text we must supply
					# (1) a language, (2) an OEM flag of 4, indicating that the we
					# wish to use the LSTM neural net model for OCR, and finally
					# (3) an OEM value, in this case, 7 which implies that we are
					# treating the ROI as a single line of text
					config = ("-l eng --oem 1 --psm 7")
					text = pytesseract.image_to_string(roi, config=config)
					# print("Text: ", text)
					# ******** Filtering Response Messages being send to socket clients ********
					if any(txt in text.upper() for txt in self.checkList):
						self.result.append(text)
						print("Result:", self.result)
				# update the FPS counter
				#fps.update()

				# *********** show the output frame ***************
				#self.cv2.imshow("Text Detection", orig)
				#key = self.cv2.waitKey(1) & 0xFF
				## if the `q` key was pressed, break from the loop
				#if key == ord("q"):
				#	break

				if self.detected:
						sock.sendall(str(self.result).encode())
						# clear the previous message array
						self.result.clear()
						data = sock.recv(1024)
						# print("Socker recv: ", repr(data))
						if repr(data).find("<EOF>") > 0:
							sock.close()
							break
				
				self.detected = False
 
        #return results, texts
		# stop the timer and display FPS information
		#fps.stop()
		#print("[INFO] elasped time: {:.2f}".format(fps.elapsed()))
		#print("[INFO] approx. FPS: {:.2f}".format(fps.fps()))
		vs.stop()
		sock.close()
		self.cv2.destroyAllWindows()
		return "Done"