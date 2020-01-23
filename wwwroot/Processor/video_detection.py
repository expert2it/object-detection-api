class FileVideoStream:
    def __init__(self, path, queueSize=128):
	    # initialize the file video stream along with the boolean
	    # used to indicate if the thread should be stopped or not
	    self.stream = cv2.VideoCapture(path)
	    self.stopped = False
 
	    # initialize the queue used to store frames read from
	    # the video file
	    self.Q = Queue(maxsize=queueSize)