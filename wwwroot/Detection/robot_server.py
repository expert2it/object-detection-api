import os
from flask import Flask, request, send_from_directory, jsonify
from werkzeug import secure_filename
from pprint import pprint
import json
import cv2 as cv
from robot_yolo import MovementHandler

# Initialize the Flask application
app = Flask(__name__)

# This route will show a form to perform an AJAX request
# jQuery is loaded to execute the request and update the
# value of the operation
@app.route('/')

@app.route('/camera', methods=['GET'])
def camera():
    # Get the name of the uploaded files
    camera_src = request.args.get('camera')
    print(camera_src)
    # filenames = uploaded_files
    result = {}
    result['objects'] = []
    
    handler = MovementHandler(cv)
    handler.Detect(None, None, camera_src)
    result['objects'].append({'url': camera_src})
    print('Camera Result: ', result)
    # return the face detected file to the client
    return json.dumps(result) 

# Please enter the Server IP and Port number
if __name__ == '__main__':
    app.run(
        host="0.0.0.0",
        port=int("5200"),
        debug=True
    )
