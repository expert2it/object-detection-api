import os
from flask import Flask, request, send_from_directory, jsonify
from werkzeug import secure_filename
from pprint import pprint
import json
import cv2 as cv
from cilibot_yolo import ObjectHandler
from sign_detection import TextRecognition

# Initialize the Flask application
app = Flask(__name__)

# This is the path to the upload directory
app.config['UPLOAD_FOLDER'] = './uploads/'

# This route will show a form to perform an AJAX request
# jQuery is loaded to execute the request and update the
# value of the operation
@app.route('/')
# Route that will process the file upload
@app.route('/detect', methods=['POST'])
def detect():
    # Get the name of the uploaded files
    uploaded_files = request.json['name']
    print(uploaded_files)
    filenames = uploaded_files
    result = {}
    # result['images'] = []
    result['objects'] = []
    for file in uploaded_files:
        if file:
            #result['objects'].append({file.rpartition('\\')[2]: ObjectHandler.Detect(ObjectHandler, file)})
            handler = ObjectHandler(cv)
            result['objects'].append({file.rpartition('\\')[2]: handler.Detect(file)})
    print('Result::: ', result)
    # return the face detected file to the client
    return json.dumps(result) 
    # jsonify(result)
    # return '{} Uploaded'.format(filenames)

@app.route('/camera', methods=['POST'])
def camera():
    # Get the name of the uploaded files
    camera_src = request.json['name']
    print(camera_src)
    # filenames = uploaded_files
    result = {}
    result['objects'] = []
    
    handler = ObjectHandler(cv)
    handler.Detect(None, None, camera_src[0])
    result['objects'].append({'url': camera_src[0]})
    print('Camera Result: ', result)
    # return the face detected file to the client
    return json.dumps(result) 

@app.route('/video', methods=['GET'])
def video():
    result = {}
    result['objects'] = []
    handler = ObjectHandler(cv)
    result['objects'].append({"File": handler.Detect(None, "table.mp4")})
    print('Result::: ', result)
    # return the face detected file to the client
    return json.dumps(result) 
    # jsonify(result)
    # return '{} Uploaded'.format(filenames)

@app.route('/ocr', methods=['GET', 'POST'])
def ocr():
    src = request.args.get('src')
    if not src:
        src = request.json['name'][0]
    result = {}
    result['texts'] = []
    handler = TextRecognition(cv)
    result['texts'].append({"File": handler.Ocr(0.0, src)})
    print('Text Found: ', result)
    # return the face detected file to the client
    return json.dumps(result) 
    # jsonify(result)
    # return '{} Uploaded'.format(filenames)


# Please enter the Server IP and Port number
if __name__ == '__main__':
    app.run(
        host="0.0.0.0",
        port=int("5100"),
        debug=True
    )
