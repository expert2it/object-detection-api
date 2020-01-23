import os
from flask import Flask, request, send_from_directory, jsonify
from werkzeug import secure_filename
from pprint import pprint
import json
import cv2 as cv
from object_detection_yolo import ObjectHandler
from text_recognition import TextRecognition

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

@app.route('/ocr', methods=['POST'])
def ocr():
    # Get the name of the uploaded files
    uploaded_files = request.json['name']
    padding = request.json['padding']
    print("Padding: ", padding)
    result = {}
    # result['images'] = []
    result['Texts'] = []
    for file in uploaded_files:
        if file:
            handler = TextRecognition(cv)
            result['Texts'].append({file.rpartition('\\')[2]: handler.Ocr(padding, file)})
    print('OCR::: ', result)
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

# Please enter the Server IP and Port number
if __name__ == '__main__':
    app.run(
        host="127.0.0.1",
        port=int("5000"),
        debug=True
    )
